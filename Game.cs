using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using static Gomoku.GameOverChecking;
using static Gomoku.UtilityTool;
using Timer = System.Timers.Timer;

namespace Gomoku
{
    public partial class Game : Form
    {
        // SCREEN SETTINGS
        public const int size = 20;    // number of blocks
        private const int tileSize = 24;    // blocks' edge width

        private const int WIDTH = size * tileSize + tileSize * 9;
        private const int HEIGHT = size * tileSize + tileSize * 2;

        // END GAME CHECKING
        private static readonly Button[,] btnList = new Button[size, size];

        // LOGIC GAMES
        public static readonly int[,] board = new int[size, size];

        public const int NONE = 0;
        public const int WHITE = 1;
        public const int BLACK = 2;
        public static int currentTurn = WHITE;

        private static int whiteScore;
        private static int blackScore;
        
        private static Timer counter;
        private const float timeThinking = 15; // seconds
        private bool start;

        // GUI
        private static Image whiteIcon = Image.FromFile(Path.GetFullPath("Materials\\X.png"));
        private static Image blackIcon = Image.FromFile(Path.GetFullPath("Materials\\O.png"));
        private static readonly Label scoreLabel = new Label();
        private static readonly ProgressBar pb = new ProgressBar();
        private static readonly ProgressBar whitePb = new ProgressBar();
        private static readonly ProgressBar blackPb = new ProgressBar();
        private static readonly Label turnIndication = new Label();

        private static readonly Label logcat = new Label();

        // SERVER
        private readonly bool isSinglePlayer;
        private readonly Minimax bot;
        
        private readonly Socket socket;
        private readonly BackgroundWorker messageReceiver = new BackgroundWorker();
        private readonly TcpListener server;
        private readonly TcpClient client;

        // MOVE TRACKING
        private readonly List<Tuple<int, int>> undoMoves = new List<Tuple<int, int>>();
        private readonly List<Tuple<int, int>> redoMoves = new List<Tuple<int, int>>();
        private readonly Button btnUndo = new Button();
        private readonly Button btnRedo = new Button();

        public Game(bool isSinglePlayer, bool isHost, int level = 0)
        {
            this.isSinglePlayer = isSinglePlayer;
            
            InitializeComponent();

            InitPlayground();
            InitScoreBoard();
            InitCounter();

            this.ClientSize = new Size(WIDTH, HEIGHT);
            whiteIcon = ScaleImage(whiteIcon, tileSize, tileSize);
            blackIcon = ScaleImage(blackIcon, tileSize, tileSize);
            bot = new Minimax(level);
            
            if (!isSinglePlayer)
            {
                messageReceiver.DoWork += WaitForOpponent; // Coroutine
                CheckForIllegalCrossThreadCalls = false;

                if (isHost)
                {
                    IPAddress ipAddress = IPAddress.Parse("192.168.1.11");
                    server = new TcpListener(ipAddress, 8888);
                    server.Start();
                    socket = server.AcceptSocket();
                }
                else
                {
                    try
                    {
                        client = new TcpClient("192.168.1.11", 8888);
                        socket = client.Client;
                        messageReceiver.RunWorkerAsync();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                        Close();
                    }
                }
            }
            else
            {
                InitTracker();
            }
        }

        #region GAME FLOW LOGIC

        private void FreezeBoard()
        {
            foreach (var btn in btnList) btn.Click -= MakeMove;
        }
        private void UnfreezeBoard()
        {
            foreach (var btn in btnList) btn.Click += MakeMove;
        }

        private void MakeMove(object sender, EventArgs e)
        {
            if (!isSinglePlayer && !start)
            {
                start = true;
                counter.Start();
            }

            var btn = (Button)sender;
            if (btn.Image != null) return;
            btn.Image = currentTurn == WHITE ? whiteIcon : blackIcon;

            var j = (btn.Location.X - tileSize) / tileSize;
            var i = (btn.Location.Y - tileSize) / tileSize;
            board[j, i] = currentTurn;

            if (!isSinglePlayer)
            {
                byte[] num = { (byte)(i + 1), (byte)(j + 1) };
                socket.Send(num);
            }

            if (IsWon())
            {
                DisplayWinner();
                return;
            }

            ChangeTurn();
            if (!isSinglePlayer)
                messageReceiver.RunWorkerAsync();
            else
            {
                undoMoves.Add(new Tuple<int, int>(j, i));
                btnUndo.Visible = true;
                BotMove();
            }
        }

        private void ChangeTurn()
        {
            currentTurn = 3 - currentTurn;

            if (!isSinglePlayer)
            {
                turnIndication.Text = @"Player " + (currentTurn == WHITE ? "1" : "2") + @" turn";
                pb.Value = pb.Maximum;
                whitePb.Value = whitePb.Maximum;
                blackPb.Value = blackPb.Maximum;
            }
            else
            {
                turnIndication.Text = (currentTurn == WHITE ? "Player" : "Computer") + @" turn";
            }
        }
        private void DisplayWinner()
        {
            if (!isSinglePlayer)
            {
                counter.Stop();
                start = false;
                MessageBox.Show(@"Player " + (currentTurn == WHITE ? "1" : "2") + @" wins");
            }
            else
                MessageBox.Show((currentTurn == WHITE ? "Player" : "Computer") + @" wins");

            if (currentTurn == WHITE) whiteScore++;
            else blackScore++;
            RepaintBoard();
        }

        #endregion
        
        #region SINGLEPLAYER METHODS

        private async void BotMove()
        {
            await Task.Run(() =>
            {
                FreezeBoard();
                var (j, i) = bot.best_move(board, currentTurn);
                board[j, i] = currentTurn;
                btnList[j, i].Image = currentTurn == WHITE ? whiteIcon : blackIcon;
                undoMoves.Add(new Tuple<int, int>(j, i));
                ChangeTurn();
                if (IsWon())
                {
                    DisplayWinner();
                    return;
                }
                UnfreezeBoard();
                Task.Delay(100).Wait();
            });
        }

        #endregion

        #region MULTIPLAYER METHODS

        private void WaitForOpponent(object sender, DoWorkEventArgs e)
        {
            FreezeBoard();
            ReceiveMove();
            if(!IsWon()) UnfreezeBoard();
        }
        
        private void ReceiveMove()
        {
            var buffer = new byte[2];
            socket.Receive(buffer);
            if (!start)
            {
                start = true;
                counter.Start();
            }

            if (buffer[0] != 0 && buffer[1] != 0)
            {
                btnList[buffer[1]-1, buffer[0]-1].Image = currentTurn == WHITE ? whiteIcon : blackIcon;
                board[buffer[1]-1, buffer[0]-1] = currentTurn;
                
                if (IsWon())
                {
                    DisplayWinner();
                    return;
                }
            }
            
            ChangeTurn();
        }
        
        private void Counting(object o, ElapsedEventArgs e)
        {
            pb.Value = (int)(pb.Value > pb.Maximum / timeThinking ? pb.Value - pb.Maximum / timeThinking : 0);
            if (currentTurn == WHITE)
            {
                whitePb.Value = (int)(whitePb.Value > whitePb.Maximum / timeThinking ? whitePb.Value - whitePb.Maximum / timeThinking : 0);
                if (whitePb.Value <= 0)
                {
                    byte[] num = { 0, 0 };
                    socket.Send(num);
                    ChangeTurn();
                    messageReceiver.RunWorkerAsync();
                }
            }
            else
            {
                blackPb.Value = (int)(blackPb.Value > blackPb.Maximum / timeThinking ? blackPb.Value - blackPb.Maximum / timeThinking : 0);
                if (blackPb.Value <= 0)
                {
                    byte[] num = { 0, 0 };
                    socket.Send(num);
                    ChangeTurn();
                    messageReceiver.RunWorkerAsync();
                }
            }
            // logcat.Text = pb.Value + @" " + whitePb.Value + @" " + blackPb.Value;
        }

        #endregion

        #region Initiation

        private void InitPlayground()
        {
            for (var i = 0; i < size; i++)
            for (var j = 0; j < size; j++)
            {
                btnList[j, i] = new Button();
                btnList[j, i].Location = new Point(tileSize + tileSize * j, tileSize + tileSize * i);
                btnList[j, i].Width = tileSize;
                btnList[j, i].Height = tileSize;
                btnList[j, i].Click += MakeMove;

                board[j, i] = NONE;
                Controls.Add(btnList[j, i]);
            }
        }

        private void InitScoreBoard()
        {
            var panel = new Panel();
            panel.Location = new Point(WIDTH - tileSize * 7, tileSize);
            panel.Size = new Size(tileSize * 6, tileSize * 7 / 2);
            panel.BorderStyle = BorderStyle.Fixed3D;

            var scoreBoard = new Label();
            scoreBoard.AutoSize = false;
            scoreBoard.Text = @"SCORE BOARD";
            scoreBoard.Font = new Font("Arial", 10, FontStyle.Bold);
            scoreBoard.TextAlign = ContentAlignment.MiddleCenter;
            scoreBoard.Dock = DockStyle.Top;

            var whiteSymbol = new PictureBox();
            whiteSymbol.Location = new Point(panel.Location.X + tileSize / 2, panel.Location.Y + tileSize * 3 / 2);
            whiteSymbol.Size = new Size(tileSize * 3 / 2, tileSize * 3 / 2);
            whiteSymbol.SizeMode = PictureBoxSizeMode.StretchImage;
            whiteSymbol.Image = ScaleImage(whiteIcon, tileSize * 2, tileSize * 2);

            var blackSymbol = new PictureBox();
            blackSymbol.Location = new Point(panel.Location.X + tileSize * 4, panel.Location.Y + tileSize * 3 / 2);
            blackSymbol.Size = new Size(tileSize * 3 / 2, tileSize * 3 / 2);
            blackSymbol.SizeMode = PictureBoxSizeMode.StretchImage;
            blackSymbol.Image = ScaleImage(blackIcon, tileSize * 2, tileSize * 2);

            scoreLabel.Location = new Point(panel.Location.X + tileSize * 2, panel.Location.Y + tileSize * 3 / 2);
            scoreLabel.Size = new Size(tileSize * 2, tileSize * 3 / 2);
            scoreLabel.Text = whiteScore + @" - " + blackScore;
            scoreLabel.TextAlign = ContentAlignment.MiddleCenter;

            Controls.Add(whiteSymbol);
            Controls.Add(blackSymbol);
            Controls.Add(scoreLabel);
            panel.Controls.Add(scoreBoard);

            Controls.Add(panel);
        }

        private void InitCounter()
        {
            var panel = new Panel();
            panel.Location = new Point(WIDTH - tileSize * 7, tileSize * 5);
            panel.Size = new Size(tileSize * 6, tileSize * 7 / 2);
            panel.BorderStyle = BorderStyle.Fixed3D;
            
            pb.Location = new Point(panel.Location.X + tileSize, panel.Location.Y + tileSize * 2);
            pb.Size = new Size(tileSize * 4, tileSize);
            pb.Maximum = tileSize * 4;
            pb.Value = pb.Maximum;

            whitePb.Maximum = tileSize * 4;
            whitePb.Value = whitePb.Maximum;
            blackPb.Maximum = tileSize * 4;
            blackPb.Value = blackPb.Maximum;

            turnIndication.Location = new Point(panel.Location.X + tileSize, panel.Location.Y + tileSize);
            turnIndication.AutoSize = false;
            turnIndication.Font = new Font("Arial", 10, FontStyle.Bold);
            turnIndication.TextAlign = ContentAlignment.BottomCenter;
            turnIndication.Dock = DockStyle.Top;
            if(!isSinglePlayer)
                turnIndication.Text = @"Player " + (currentTurn == WHITE ? "1" : "2") + @" turn";
            else 
                turnIndication.Text = (currentTurn == WHITE ? "Player" : "Computer") + @" turn";

            logcat.Location = new Point(WIDTH - tileSize * 7, tileSize * 9);
            this.Controls.Add(logcat);

            if (!isSinglePlayer)
            {
                Controls.Add(pb);
                counter = new Timer(1000);
                counter.Elapsed += Counting;
                counter.AutoReset = true;
            }
            
            panel.Controls.Add(turnIndication);
            Controls.Add(panel);
        }

        private void InitTracker()
        {
            var undoIcon = Image.FromFile(Path.GetFullPath("Materials\\undo.png"));
            undoIcon = ScaleImage(undoIcon, tileSize / 2, tileSize / 2);
            var redoIcon = new Bitmap(undoIcon);
            redoIcon.RotateFlip(RotateFlipType.RotateNoneFlipX);

            btnUndo.Location = new Point(WIDTH - tileSize * 6, tileSize * 10);
            btnUndo.Size = new Size(tileSize, tileSize);
            btnUndo.ImageAlign = ContentAlignment.MiddleCenter;
            btnUndo.Image = undoIcon;
            btnUndo.Visible = false;
            btnUndo.Click += (sender, args) =>
            {
                for (int index = 0; index < 2; index++)
                {
                    var j = undoMoves[undoMoves.Count - 1].Item1;
                    var i = undoMoves[undoMoves.Count - 1].Item2;
                    btnList[j, i].Image = null;
                    board[j, i] = NONE;
                    redoMoves.Add(undoMoves[undoMoves.Count - 1]);
                    undoMoves.RemoveAt(undoMoves.Count - 1);
                    if (undoMoves.Count == 0) btnUndo.Visible = false;
                    btnRedo.Visible = true;
                    ChangeTurn();
                }
            };
            
            btnRedo.Location = new Point(WIDTH - tileSize * 3, tileSize * 10);
            btnRedo.Size = new Size(tileSize, tileSize);
            btnRedo.ImageAlign = ContentAlignment.MiddleCenter;
            btnRedo.Image = redoIcon;
            btnRedo.Visible = false;
            btnRedo.Click += (sender, args) =>
            {
                for (int index = 0; index < 2; index++)
                {
                    ChangeTurn();
                    var j = redoMoves[redoMoves.Count - 1].Item1;
                    var i = redoMoves[redoMoves.Count - 1].Item2;
                    btnList[j, i].Image = currentTurn == WHITE ? whiteIcon : blackIcon;
                    board[j, i] = currentTurn;
                    undoMoves.Add(redoMoves[redoMoves.Count - 1]);
                    redoMoves.RemoveAt(redoMoves.Count - 1);
                    if (redoMoves.Count == 0) btnRedo.Visible = false;
                    btnUndo.Visible = true;
                }
            };

            this.Controls.Add(btnUndo);
            this.Controls.Add(btnRedo);
        }

        private void RepaintBoard()
        {
            scoreLabel.Text = whiteScore + @" - " + blackScore;
            for (var i = 0; i < size; i++)
            for (var j = 0; j < size; j++)
            {
                btnList[i, j].Image = null;
                board[i, j] = NONE;
            }
            currentTurn = WHITE;

            if (!isSinglePlayer)
            {
                pb.Value = pb.Maximum;
                whitePb.Value = whitePb.Maximum;
                blackPb.Value = blackPb.Maximum;
            }
        }

        #endregion

        private void Game_OnClosed(object sender, EventArgs e)
        {
            if (!isSinglePlayer)
            {
                messageReceiver.WorkerSupportsCancellation = true;
                messageReceiver.CancelAsync();
                if (server != null) server.Stop();
                start = false;
            }
            RepaintBoard();
            whiteScore = 0;
            blackScore = 0;
        }
    }
}