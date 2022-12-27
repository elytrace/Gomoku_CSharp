using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using static Gomoku.GameOverChecking;
using static Gomoku.UtilityTool;
using Timer = System.Timers.Timer;

namespace Gomoku
{
    public partial class Form1 : Form
    {
        // SCREEN SETTINGS
        public const int size = 20;
        private const int tileSize = 16;

        private const int WIDTH = size * tileSize + tileSize * 9;
        private const int HEIGHT = size * tileSize + tileSize * 2;

        // ALGORITHM SETTINGS
        public static int[,] board = new int[size, size];
        private static Button[,] btnList = new Button[size, size];

        // LOGIC GAMES
        private const int NONE = 0;
        public const int WHITE = 1;
        public const int BLACK = 2;
        public static int currentTurn = WHITE;

        private static int whiteScore;
        private static int blackScore;

        private const int timeThinking = 30;
        private static Timer counter;

        // GUI
        private static Image whiteSymbol = Image.FromFile(Path.GetFullPath("Materials\\X.png"));
        private static Image blackSymbol = Image.FromFile(Path.GetFullPath("Materials\\O.png"));
        private static Label scoreLabel = new Label();
        public static ProgressBar pb = new ProgressBar();
        public static Label turnIndication = new Label();
        
        public Form1()
        {
            InitializeComponent();
            InitBoard();
            InitGUI();
            InitCounter();
            this.ClientSize = new Size(WIDTH, HEIGHT);

            whiteSymbol = ScaleImage(whiteSymbol, tileSize, tileSize);
            blackSymbol = ScaleImage(blackSymbol, tileSize, tileSize);
        }

        private void InitBoard()
        {
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    Button btn = new Button();
                    btn.Location = new Point(tileSize + tileSize * j, tileSize + tileSize * i);
                    btn.Width = tileSize;
                    btn.Height = tileSize;
                    btn.Click += btn_Click;

                    board[j, i] = NONE;
                    btnList[j, i] = btn;
                    this.Controls.Add(btn);
                }
            }
        }

        private void InitGUI()
        {
            Panel panel = new Panel();
            panel.Location = new Point(WIDTH - tileSize * 7, tileSize);
            panel.Size = new Size(tileSize * 6, tileSize * 7 / 2);
            panel.BorderStyle = BorderStyle.Fixed3D;

            Label scoreBoard = new Label();
            scoreBoard.AutoSize = false;
            scoreBoard.Text = @"SCORE BOARD";
            scoreBoard.Font = new Font("Arial", 10, FontStyle.Bold);
            scoreBoard.TextAlign = ContentAlignment.MiddleCenter;
            scoreBoard.Dock = DockStyle.Top;

            PictureBox whiteIcon = new PictureBox();
            whiteIcon.Location = new Point(panel.Location.X + tileSize / 2, panel.Location.Y + tileSize * 3 / 2);
            whiteIcon.Size = new Size(tileSize * 3 / 2, tileSize * 3 / 2);
            whiteIcon.SizeMode = PictureBoxSizeMode.StretchImage;
            whiteIcon.Image = ScaleImage(whiteSymbol, tileSize * 2, tileSize * 2);

            PictureBox blackIcon = new PictureBox();
            blackIcon.Location = new Point(panel.Location.X + tileSize * 4, panel.Location.Y + tileSize * 3 / 2);
            blackIcon.Size = new Size(tileSize * 3 / 2, tileSize * 3 / 2);
            blackIcon.SizeMode = PictureBoxSizeMode.StretchImage;
            blackIcon.Image = ScaleImage(blackSymbol, tileSize * 2, tileSize * 2);

            scoreLabel.Location = new Point(panel.Location.X + tileSize * 2, panel.Location.Y + tileSize * 3 / 2);
            scoreLabel.Size = new Size(tileSize * 2, tileSize * 3 / 2);
            scoreLabel.Text = whiteScore + @" - " + blackScore;
            scoreLabel.TextAlign = ContentAlignment.MiddleCenter;
            
            this.Controls.Add(whiteIcon);
            this.Controls.Add(blackIcon);
            this.Controls.Add(scoreLabel);
            panel.Controls.Add(scoreBoard);
            
            this.Controls.Add(panel);
        }

        private void InitCounter()
        {
            Panel panel = new Panel();
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
            this.Controls.Add(pb);
            this.Controls.Add(panel);

            counter = new Timer(100);
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
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    btnList[i, j].Image = null;
                    board[i, j] = NONE;
                    currentTurn = WHITE;
                }
            }
            counter.Start();
        }

        void btn_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            if (btn.Image != null) return;
            btn.Image = currentTurn == WHITE ? whiteSymbol : blackSymbol;
            
            int j = (btn.Location.X - tileSize) / tileSize;
            int i = (btn.Location.Y - tileSize) / tileSize;
            board[j, i] = currentTurn;

            if (IsWon())
            {
                MessageBox.Show(@"Player " + (currentTurn == WHITE ? "1" : "2") + @" wins");
                if (currentTurn == WHITE) whiteScore++;
                else blackScore++;
                counter.Stop();
                RepaintBoard();
                return;
            }
            ChangeTurn();
        }
    }
}
