using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Gomoku.GameOverChecking;

namespace Gomoku
{
    public partial class Form1 : Form
    {
        public static int numOfRows = 20;
        private static int tileSize = 32;

        private static int WIDTH = numOfRows * tileSize + tileSize * 8;
        private static int HEIGHT = numOfRows * tileSize + tileSize * 2;

        public static int[,] board = new int[numOfRows, numOfRows];
        private static Button[,] btnList = new Button[numOfRows, numOfRows];

        private const int NONE = 0;
        private const int WHITE = 1;
        private const int BLACK = 2;
        public static int currentTurn = WHITE;

        private static Image whiteSymbol = Image.FromFile(Path.GetFullPath("Materials\\X.png"));
        private static Image blackSymbol = Image.FromFile(Path.GetFullPath("Materials\\O.png"));

        public Form1()
        {
            InitializeComponent();
            InitBoard();
            InitGUI();
            this.ClientSize = new Size(WIDTH, HEIGHT);
        }

        private void InitBoard()
        {
            for (int i = 0; i < numOfRows; i++)
            {
                for (int j = 0; j < numOfRows; j++)
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
            panel.Location = new Point(WIDTH - tileSize * 6, tileSize);
            panel.Size = new Size(tileSize * 6, HEIGHT - tileSize * 2);
            panel.BackColor = Color.Silver;
            panel.Visible = true;

            Label player1 = new Label();
            player1.Location = new Point(WIDTH - tileSize * 6 + tileSize / 2, tileSize + tileSize / 2);
            // player1.Size = new Size(tileSize, tileSize / 2);
            player1.Text = "X";
            
            Label player2 = new Label();
            player2.Location = new Point(WIDTH - tileSize * 5 + tileSize / 2, tileSize + tileSize / 2);
            // player2.Size = new Size(tileSize, tileSize / 2);
            player2.Text = "O";
            
            Controls.Add(player1);
            Controls.Add(player2);
            
            this.Controls.Add(panel);
        }

        private void ClearBoard()
        {
            for (int i = 0; i < numOfRows; i++)
            {
                for (int j = 0; j < numOfRows; j++)
                {
                    // btnList[i, j].Text = "";
                    btnList[i, j].Image = null;
                    board[i, j] = NONE;
                    currentTurn = WHITE;
                }
            }
        }

        void btn_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            if (btn.Text != "") return;
            // btn.Text = currentTurn == WHITE ? "X" : "O";
            btn.Image = currentTurn == WHITE ? whiteSymbol : blackSymbol;

            int j = (btn.Location.X - tileSize) / tileSize;
            int i = (btn.Location.Y - tileSize) / tileSize;

            board[j, i] = currentTurn;

            if (IsWon())
            {
                MessageBox.Show("Player " + (currentTurn == WHITE ? "1" : "2") + " wins");
                ClearBoard();
                return;
            }

            currentTurn = 3 - currentTurn;
        }


        
    }
}
