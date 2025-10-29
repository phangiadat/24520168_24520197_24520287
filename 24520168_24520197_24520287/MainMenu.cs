using System;
using System.Drawing;
using System.Windows.Forms;

namespace _24520168_24520197_24520287
{
    public class MainMenu : Form
    {
        private Button btnPlay;
        private Button btnInstruction;
        private Label lblTitle;

        public MainMenu()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Main Menu";
            this.ClientSize = new Size(420, 300);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(40, 40, 48);

            var titleFont = new Font("Segoe UI", 20f, FontStyle.Bold);
            lblTitle = new Label();
            lblTitle.Text = "My Platformer";
            lblTitle.Font = titleFont;
            lblTitle.ForeColor = Color.White;
            var titleSize = TextRenderer.MeasureText(lblTitle.Text, lblTitle.Font);
            lblTitle.Size = titleSize;
            lblTitle.Location = new Point((this.ClientSize.Width - titleSize.Width) / 2, 30);
            this.Controls.Add(lblTitle);

            btnPlay = new Button();
            btnPlay.Text = "Play";
            btnPlay.Font = new Font("Segoe UI", 12f, FontStyle.Regular);
            btnPlay.BackColor = Color.FromArgb(28, 151, 234);
            btnPlay.Size = new Size(200, 42);
            btnPlay.Location = new Point((ClientSize.Width - btnPlay.Width) / 2, 110);
            btnPlay.Click += BtnPlay_Click;
            this.Controls.Add(btnPlay);

            btnInstruction = new Button();
            btnInstruction.Text = "Instructions";
            btnInstruction.Font = new Font("Segoe UI", 12f, FontStyle.Regular);
            btnInstruction.BackColor = Color.FromArgb(28, 151, 234);
            btnInstruction.Size = new Size(200, 42);
            btnInstruction.Location = new Point((ClientSize.Width - btnInstruction.Width) / 2, 170);
            btnInstruction.Click += BtnInstruction_Click;
            this.Controls.Add(btnInstruction);
        }

        private void BtnPlay_Click(object sender, EventArgs e)
        {
            using (var game = new Form1())
            {
                this.Hide();
                game.ShowDialog(); // returns when Form1 closes (player died or window closed)
                this.Show();
            }
        }

        private void BtnInstruction_Click(object sender, EventArgs e)
        {
            var msg =
                "Controls:\n" +
                "- Left / Right: move\n" +
                "- Up: jump\n" +
                "- A: fire projectile\n\n" +
                "Goal:\n" +
                "- Kill enemies with projectiles or jump on their head.\n" +
                "- If HP reaches 0 you'll return to this menu.";
            MessageBox.Show(msg, "Instructions", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}