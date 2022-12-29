using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
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
        private const int tileSize = 16;    // blocks' edge width

        private const int WIDTH = size * tileSize + tileSize * 9;
        private const int HEIGHT = size * tileSize + tileSize * 2;

        // END GAME CHECKING
        private static readonly Button[,] btnList = new Button[size, size];

        // LOGIC GAMES
        public static readonly int[,] board = new int[size, size];

        private const int NONE = 0;
        private static int WHITE = 1;
        private static int BLACK = 2;
        public static int currentTurn = WHITE;

        private static int whiteScore;
        private static int blackScore;
        
        private static Timer counter;
        private const int timeThinking = 15; // seconds
        private bool start = false;

        // GUI
        private static Image whiteSymbol = Image.FromFile(Path.GetFullPath("Materials\\X.png"));
        private static Image blackSymbol = Image.FromFile(Path.GetFullPath("Materials\\O.png"));
        private static readonly Label scoreLabel = new Label();
        private static readonly ProgressBar pb = new ProgressBar();
        private static readonly ProgressBar whitePb = new ProgressBar();
        private static readonly ProgressBar blackPb = new ProgressBar();
        private static readonly Label turnIndication = new Label();

        // SERVER
        private readonly Socket socket;
        private readonly BackgroundWorker messageReceiver = new BackgroundWorker();
        private readonly TcpListener server;
        private readonly TcpClient client;

        public Game(bool isHost, string ip = null)
        {
            InitializeComponent();

            InitPlayground();
            InitScoreBoard();
            InitCounter();

            this.ClientSize = new Size(WIDTH, HEIGHT);
            whiteSymbol = ScaleImage(whiteSymbol, tileSize, tileSize);
            blackSymbol = ScaleImage(blackSymbol, tileSize, tileSize);

            messageReceiver.DoWork += WaitForOpponent;  // Coroutine
            CheckForIllegalCrossThreadCalls = false;

            if (isHost)
            {
                server = new TcpListener(IPAddress.Any, 5555);
                server.Start();
                socket = server.AcceptSocket();
            }
            else
            {
                try
                {
                    client = new TcpClient(ip, 5555);
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

        private void WaitForOpponent(object sender, DoWorkEventArgs e)
        {
            FreezeBoard();
            ReceiveMove();
            if(!IsWon()) UnfreezeBoard();
        }
        
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
            if (!start)
            {
                start = true;
                counter.Start();
            }
            var btn = (Button)sender;
            if (btn.Image != null) return;
            btn.Image = currentTurn == WHITE ? whiteSymbol : blackSymbol;

            var j = (btn.Location.X - tileSize) / tileSize;
            var i = (btn.Location.Y - tileSize) / tileSize;
            board[j, i] = currentTurn;
            byte[] num = { (byte)(i+1), (byte)(j+1) };
            socket.Send(num);

            if (IsWon())
            {
                DisplayWinner();
                return;
            }

            ChangeTurn();
            messageReceiver.RunWorkerAsync();
        }

        private void ReceiveMove()
        {
            var buffer = new byte[2];
            socket.Receive(buffer);
            counter.Start();
            if (buffer[0] != 0 && buffer[1] != 0)
            {
                btnList[buffer[1]-1, buffer[0]-1].Image = currentTurn == WHITE ? whiteSymbol : blackSymbol;
                board[buffer[1]-1, buffer[0]-1] = currentTurn;
            }

            if (IsWon())
            {
                DisplayWinner();
                return;
            }
            ChangeTurn();
        }

        private static void ChangeTurn()
        {
            currentTurn = 3 - currentTurn;
            turnIndication.Text = @"Player " + (currentTurn == WHITE ? "1" : "2") + @" turn";
            pb.Value = pb.Maximum;
            whitePb.Value = whitePb.Maximum;
            blackPb.Value = blackPb.Maximum;
        }

        private void DisplayWinner()
        {
            counter.Stop();
            start = false;
            MessageBox.Show(@"Player " + (currentTurn == WHITE ? "1" : "2") + @" wins");
            if (currentTurn == WHITE) whiteScore++;
            else blackScore++;
            RepaintBoard();
        }
        
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

            var whiteIcon = new PictureBox();
            whiteIcon.Location = new Point(panel.Location.X + tileSize / 2, panel.Location.Y + tileSize * 3 / 2);
            whiteIcon.Size = new Size(tileSize * 3 / 2, tileSize * 3 / 2);
            whiteIcon.SizeMode = PictureBoxSizeMode.StretchImage;
            whiteIcon.Image = ScaleImage(whiteSymbol, tileSize * 2, tileSize * 2);

            var blackIcon = new PictureBox();
            blackIcon.Location = new Point(panel.Location.X + tileSize * 4, panel.Location.Y + tileSize * 3 / 2);
            blackIcon.Size = new Size(tileSize * 3 / 2, tileSize * 3 / 2);
            blackIcon.SizeMode = PictureBoxSizeMode.StretchImage;
            blackIcon.Image = ScaleImage(blackSymbol, tileSize * 2, tileSize * 2);

            scoreLabel.Location = new Point(panel.Location.X + tileSize * 2, panel.Location.Y + tileSize * 3 / 2);
            scoreLabel.Size = new Size(tileSize * 2, tileSize * 3 / 2);
            scoreLabel.Text = whiteScore + @" - " + blackScore;
            scoreLabel.TextAlign = ContentAlignment.MiddleCenter;

            Controls.Add(whiteIcon);
            Controls.Add(blackIcon);
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
            
            pb.Maximum = tileSize * 4;
            pb.Value = pb.Maximum;
            pb.Location = new Point(panel.Location.X + tileSize, panel.Location.Y + tileSize * 2);
            pb.Size = new Size(tileSize * 4, tileSize);

            whitePb.Maximum = tileSize * 4;
            whitePb.Value = whitePb.Maximum;
            blackPb.Maximum = tileSize * 4;
            blackPb.Value = blackPb.Maximum;

            turnIndication.Location = new Point(panel.Location.X + tileSize, panel.Location.Y + tileSize);
            turnIndication.AutoSize = false;
            turnIndication.Font = new Font("Arial", 10, FontStyle.Bold);
            turnIndication.Text = @"Player " + (currentTurn == WHITE ? "1" : "2") + @" turn";
            turnIndication.TextAlign = ContentAlignment.BottomCenter;
            turnIndication.Dock = DockStyle.Top;

            panel.Controls.Add(turnIndication);
            Controls.Add(pb);
            Controls.Add(panel);

            counter = new Timer(1000);
            counter.Elapsed += (o, e) =>
            {
                pb.Value = pb.Value > pb.Maximum / timeThinking ? pb.Value - pb.Maximum / timeThinking : 0;
                if (currentTurn == WHITE)
                {
                    whitePb.Value = whitePb.Value > whitePb.Maximum / timeThinking ? whitePb.Value - whitePb.Maximum / timeThinking : 0;
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
                    blackPb.Value = blackPb.Value > blackPb.Maximum / timeThinking ? blackPb.Value - blackPb.Maximum / timeThinking : 0;
                    if (blackPb.Value <= 0)
                    {
                        byte[] num = { 0, 0 };
                        socket.Send(num);
                        ChangeTurn();
                        messageReceiver.RunWorkerAsync();
                    }
                }
            };
            counter.AutoReset = true;
        }

        private void RepaintBoard()
        {
            scoreLabel.Text = whiteScore + @" - " + blackScore;
            for (var i = 0; i < size; i++)
            for (var j = 0; j < size; j++)
            {
                btnList[i, j].Image = null;
                board[i, j] = NONE;
                currentTurn = WHITE;
            }
            pb.Value = pb.Maximum;
            whitePb.Value = whitePb.Maximum;
            blackPb.Value = blackPb.Maximum;
        }

        #endregion

        private void Game_OnClosed(object sender, EventArgs e)
        {
            messageReceiver.WorkerSupportsCancellation = true;
            messageReceiver.CancelAsync();
            if(server != null) server.Stop();
        }
    }
}