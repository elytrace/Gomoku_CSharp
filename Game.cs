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
        public const int size = 20;
        private const int tileSize = 32;

        private const int WIDTH = size * tileSize + tileSize * 9;
        private const int HEIGHT = size * tileSize + tileSize * 2;

        // LOGIC GAMES
        private const int NONE = 0;

        private const int timeThinking = 15;

        // ALGORITHM SETTINGS
        public static int[,] board = new int[size, size];
        private static readonly Button[,] btnList = new Button[size, size];
        public static int WHITE = 1;
        public static int BLACK = 2;
        public static int currentTurn = WHITE;

        private static int whiteScore;
        private static int blackScore;
        private static Timer counter;

        // GUI
        private static Image whiteSymbol = Image.FromFile(Path.GetFullPath("Materials\\X.png"));
        private static Image blackSymbol = Image.FromFile(Path.GetFullPath("Materials\\O.png"));
        private static readonly Label scoreLabel = new Label();
        public static ProgressBar pb = new ProgressBar();
        public static Label turnIndication = new Label();
        
        // Server
        private readonly TcpClient client;
        private readonly BackgroundWorker messageReceiver = new BackgroundWorker();
        private readonly TcpListener server;

        // SOCKET
        private readonly Socket socket;

        public Game(bool isHost, string ip = null)
        {
            InitializeComponent();
            messageReceiver.DoWork += MessageReceiver_DoWork;
            CheckForIllegalCrossThreadCalls = false;

            InitPlayground();
            InitScoreBoard();
            InitCounter();
            ClientSize = new Size(WIDTH, HEIGHT);

            whiteSymbol = ScaleImage(whiteSymbol, tileSize, tileSize);
            blackSymbol = ScaleImage(blackSymbol, tileSize, tileSize);

            if (isHost)
            {
                WHITE = 1;
                BLACK = 2;
                server = new TcpListener(IPAddress.Any, 20000);
                server.Start();
                socket = server.AcceptSocket();
            }
            else
            {
                WHITE = 2;
                BLACK = 1;
                try
                {
                    client = new TcpClient(ip, 25000);
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

        private void MessageReceiver_DoWork(object sender, DoWorkEventArgs e)
        {
            if (IsWon()) return;
            FreezeBoard();
            turnIndication.Text = @"Player " + (currentTurn == WHITE ? "1" : "2") + @"turn";
            ReceiveMove();
            turnIndication.Text = @"Player " + (currentTurn == WHITE ? "1" : "2") + @"turn";
            UnfreezeBoard();
        }

        private void MakeMove(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            if (btn.Image != null) return;
            btn.Image = currentTurn == WHITE ? whiteSymbol : blackSymbol;

            var j = (btn.Location.X - tileSize) / tileSize;
            var i = (btn.Location.Y - tileSize) / tileSize;
            board[j, i] = currentTurn;
            byte[] num = { (byte)i, (byte)j };
            socket.Send(num);
            messageReceiver.RunWorkerAsync();

            if (IsWon())
            {
                counter.Stop();
                MessageBox.Show(@"Player " + (currentTurn == WHITE ? "1" : "2") + @" wins");
                if (currentTurn == WHITE) whiteScore++;
                else blackScore++;
                RepaintBoard();
                return;
            }

            ChangeTurn();
        }

        private void FreezeBoard()
        {
            foreach (var btn in btnList) btn.Enabled = false;
        }

        private void UnfreezeBoard()
        {
            foreach (var btn in btnList) btn.Enabled = true;
        }

        private void ReceiveMove()
        {
            var buffer = new byte[2];
            socket.Receive(buffer);
            btnList[buffer[1], buffer[0]].Image = currentTurn == WHITE ? whiteSymbol : blackSymbol;
            ChangeTurn();
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
                pb.Value -= pb.Maximum / timeThinking;
                if (pb.Value <= 0) ChangeTurn();
            };
            counter.AutoReset = true;
            counter.Enabled = true;
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

            counter.Start();
        }

        #endregion
    }
}