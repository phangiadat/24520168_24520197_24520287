using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _24520168_24520197_24520287
{
    public partial class Form1 : Form
    {
        private Player player;
        private List<Enemy> enemies;
        private List<Platform> platforms;
        private EnemySpawner spawner;
        private Timer gameTimer;
        public Form1()
        {
            InitializeComponent();

            this.DoubleBuffered = true;
            this.KeyPreview = true;
            this.ClientSize = new Size(800, 600);

            InitializeGame();
        }
        private void InitializeGame()
        {
            platforms = new List<Platform>
            {
                new Platform(0, 550, 800, 50),        // Nền đất
                new Platform(100, 450, 150, 20),      // Platform trên trời
                new Platform(300, 350, 150, 20),
                new Platform(500, 450, 150, 20),
                new Platform(200, 250, 100, 20),
                new Platform(600, 300, 120, 20)
            };
            // Khởi tạo player
            player = new Player(100, 400);

            // Khởi tạo enemy system
            enemies = new List<Enemy>();
            spawner = new EnemySpawner(player, enemies, platforms);

            // Game loop
            gameTimer = new Timer();
            gameTimer.Interval = 16; // ~60 FPS
            gameTimer.Tick += GameLoop;
            gameTimer.Start();

            spawner.Start();
        }

        private void GameLoop(object sender, EventArgs e)
        {
            player.Update(platforms);

            foreach (var enemy in enemies.ToList())
            {
                enemy.Update(platforms);

                // Check va chạm player - enemy
                if (player.GetBounds().IntersectsWith(enemy.GetBounds()))
                {
                    // Xử lý va chạm (game over, mất máu, etc.)
                    // Tạm thời: nếu player nhảy lên đầu enemy -> enemy chết
                    if (player.VelocityY > 0 && player.Y + player.Height - 10 < enemy.Y + enemy.Height / 2)
                    {
                        enemy.isDead = true;
                        player.VelocityY = -8; // Bounce
                    }
                }
            }
            Invalidate();
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            player.SetKeyState(e.KeyCode, true);
            e.Handled = true;
            e.SuppressKeyPress = true; 
        }
        protected override void OnKeyUp(KeyEventArgs e)
        {
            player.SetKeyState(e.KeyCode, false);
            e.Handled = true;
            e.SuppressKeyPress = true;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Vẽ background
            g.Clear(Color.SkyBlue);

            // Vẽ platforms
            foreach (var platform in platforms)
                platform.Draw(g);

            // Vẽ player
            player.Draw(g);

            // Vẽ enemies
            foreach (var enemy in enemies)
            {
                if (!enemy.isDead)
                    enemy.Draw(g);
            }

            // Vẽ debug info
            g.DrawString($"Enemies: {enemies.Count(enemy => !enemy.isDead)}",
                this.Font, Brushes.Black, 10, 10);
            g.DrawString($"On Ground: {player.IsOnGround}",
                this.Font, Brushes.Black, 10, 30);
        }
    }
}
