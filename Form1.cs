using System.Drawing;
using System.Windows.Forms;

namespace Gomoku
{
    public partial class Form1 : Form
    {
        private Label gameTitle = new Label();
        
        // INITIATION STATE
        private Button btnSingle = new Button();
        private Button btnMultiple = new Button();
        private Button btnSettings = new Button();
        
        // SINGLE PLAYER LEVEL CHOICES STATE
        private Button btnEasy = new Button();
        private Button btnHard = new Button();

        // MULTIPLE PLAYER TYPE CHOICES STATE
        private Button btnHost = new Button();
        private Button btnJoin = new Button();

        private Button btnBack = new Button();
        
        public Form1()
        {
            InitializeComponent();
            InitGUI();
            GotoMainState();
            HandleOnClickEvents();
        }

        private void InitGUI()
        {
            gameTitle.Text = @"GOMOKU";
            gameTitle.TextAlign = ContentAlignment.MiddleCenter;
            gameTitle.Location = new Point(ClientSize.Width / 10, ClientSize.Height / 10);
            gameTitle.Size = new Size(ClientSize.Width * 8 / 10, ClientSize.Height / 10);
            gameTitle.Font = new Font("Lucida Handwriting", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);

            // ------------------------------------------------------------------------------------------
            
            btnSingle.Text = @"Single Player";
            btnSingle.TextAlign = ContentAlignment.MiddleCenter;
            btnSingle.Location = new Point(ClientSize.Width / 5, ClientSize.Height * 3 / 10);
            btnSingle.Size = new Size(ClientSize.Width * 3 / 5, ClientSize.Height * 3 / 20);
            
            btnMultiple.Text = @"Multiple Player";
            btnMultiple.TextAlign = ContentAlignment.MiddleCenter;
            btnMultiple.Location = new Point(ClientSize.Width / 5, ClientSize.Height * 5 / 10);
            btnMultiple.Size = new Size(ClientSize.Width * 3 / 5, ClientSize.Height * 3 / 20);
            
            btnSettings.Text = @"Settings";
            btnSettings.TextAlign = ContentAlignment.MiddleCenter;
            btnSettings.Location = new Point(ClientSize.Width / 5, ClientSize.Height * 7 / 10);
            btnSettings.Size = new Size(ClientSize.Width * 3 / 5, ClientSize.Height * 3 / 20);

            // -----------------------------------------------------------------------------------------
            
            btnEasy.Text = @"Easy";
            btnEasy.TextAlign = ContentAlignment.MiddleCenter;
            btnEasy.Location = new Point(ClientSize.Width / 5, ClientSize.Height * 3 / 10);
            btnEasy.Size = new Size(ClientSize.Width * 3 / 5, ClientSize.Height * 3 / 20);

            btnHard.Text = @"Hard";
            btnHard.TextAlign = ContentAlignment.MiddleCenter;
            btnHard.Location = new Point(ClientSize.Width / 5, ClientSize.Height * 5 / 10);
            btnHard.Size = new Size(ClientSize.Width * 3 / 5, ClientSize.Height * 3 / 20);

            // -----------------------------------------------------------------------------------------
            
            btnHost.Text = @"Host";
            btnHost.TextAlign = ContentAlignment.MiddleCenter;
            btnHost.Location = new Point(ClientSize.Width / 5, ClientSize.Height * 3 / 10);
            btnHost.Size = new Size(ClientSize.Width * 3 / 5, ClientSize.Height * 3 / 20);

            btnJoin.Text = @"Join";
            btnJoin.TextAlign = ContentAlignment.MiddleCenter;
            btnJoin.Location = new Point(ClientSize.Width / 5, ClientSize.Height * 5 / 10);
            btnJoin.Size = new Size(ClientSize.Width * 3 / 5, ClientSize.Height * 3 / 20);
            
            // -----------------------------------------------------------------------------------------
            
            btnBack.Text = @"Back";
            btnBack.TextAlign = ContentAlignment.MiddleCenter;
            btnBack.Location = new Point(ClientSize.Width / 5, ClientSize.Height * 7 / 10);
            btnBack.Size = new Size(ClientSize.Width * 3 / 5, ClientSize.Height * 3 / 20);

            // -----------------------------------------------------------------------------------------

            this.Controls.Add(gameTitle);

            this.Controls.Add(btnSingle);
            this.Controls.Add(btnMultiple);
            this.Controls.Add(btnSettings);
            
            this.Controls.Add(btnEasy);
            this.Controls.Add(btnHard);
            this.Controls.Add(btnHost);
            this.Controls.Add(btnJoin);
            
            this.Controls.Add(btnBack);
        }

        private void HandleOnClickEvents()
        {
            btnSingle.Click += (sender, args) => GotoSingleState();
            btnMultiple.Click += (sender, args) => GotoMultipleState();
            btnBack.Click += (sender, args) => GotoMainState();

            btnEasy.Click += (sender, args) =>
            {
                Game newGame = new Game(true, false, null, 3);
                this.Visible = false;
                if (!newGame.IsDisposed)
                    newGame.ShowDialog();
                this.Visible = true;
            };
            
            btnHard.Click += (sender, args) =>
            {
                Game newGame = new Game(true, false, null, 4);
                this.Visible = false;
                if (!newGame.IsDisposed)
                    newGame.ShowDialog();
                this.Visible = true;
            };
            
            btnHost.Click += (sender, args) =>
            {
                Game newGame = new Game(false, true);
                this.Visible = false;
                if (!newGame.IsDisposed)
                    newGame.ShowDialog();
                this.Visible = true;
            };

            btnJoin.Click += (sender, args) =>
            {
                Game newGame = new Game(false, false, "");
                Visible = false;
                if (!newGame.IsDisposed)
                    newGame.ShowDialog();
                Visible = true;
            };
        }

        private void GotoMainState()
        {
            btnSingle.Visible = true;
            btnMultiple.Visible = true;
            btnSettings.Visible = true;

            btnEasy.Visible = false;
            btnHard.Visible = false;

            btnHost.Visible = false;
            btnJoin.Visible = false;

            btnBack.Visible = false;
        }
        
        private void GotoSingleState()
        {
            btnSingle.Visible = false;
            btnMultiple.Visible = false;
            btnSettings.Visible = false;

            btnEasy.Visible = true;
            btnHard.Visible = true;

            btnHost.Visible = false;
            btnJoin.Visible = false;

            btnBack.Visible = true;
        }
        
        private void GotoMultipleState()
        {
            btnSingle.Visible = false;
            btnMultiple.Visible = false;
            btnSettings.Visible = false;

            btnEasy.Visible = false;
            btnHard.Visible = false;

            btnHost.Visible = true;
            btnJoin.Visible = true;

            btnBack.Visible = true;
        }
    }
}
