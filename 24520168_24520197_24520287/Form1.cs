using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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

        // Projectiles
        private List<Projectile> projectiles;

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
            // Random platform generation
            var rand = new Random();
            platforms = new List<Platform>();

            int groundHeight = 50;
            platforms.Add(new Platform(0, this.ClientSize.Height - groundHeight, this.ClientSize.Width, groundHeight));

            platforms = new List<Platform>
            {
                new Platform(0, 550, 800, 50),        // Nền đất
                new Platform(100, 450, 150, 20),      // Platform trên trời
                new Platform(300, 350, 150, 20),
                new Platform(500, 450, 150, 20),
                new Platform(200, 250, 100, 20),
                new Platform(600, 300, 120, 20)
            };


            // Player spawns above ground
            player = new Player(100, this.ClientSize.Height - groundHeight - 60);

            // Enemies and spawner
            enemies = new List<Enemy>();
            spawner = new EnemySpawner(player, enemies, platforms);

            // Projectiles
            projectiles = new List<Projectile>();

            // Game loop
            gameTimer = new Timer();
            gameTimer.Interval = 16; // ~60 FPS
            gameTimer.Tick += GameLoop;
            gameTimer.Start();

            spawner.Start();
        }

        private void GameLoop(object sender, EventArgs e)
        {
            // Update player
            player.Update(platforms);

            // Update enemies
            foreach (var enemy in enemies.ToList())
            {
                enemy.Update(platforms);

                // Player - enemy collision
                if (player.GetBounds().IntersectsWith(enemy.GetBounds()))
                {
                    if (player.VelocityY > 0 && player.Y + player.Height - 10 < enemy.Y + enemy.Height / 2)
                    {
                        enemy.isDead = true;
                        player.VelocityY = -8; // Bounce
                    }
                    else
                    {
                        // contact damage (small per tick)
                        player.TakeDamage(1);
                    }
                }
            }

            // Update projectiles
            foreach (var proj in projectiles.ToList())
            {
                proj.Update();

                // Remove out-of-bounds
                if (proj.X < -50 || proj.X > this.ClientSize.Width + 50 || proj.Y < -50 || proj.Y > this.ClientSize.Height + 50)
                    proj.IsDead = true;

                if (proj.IsDead) continue;

                // Player projectile -> hit enemies
                if (proj.Owner is Player)
                {
                    foreach (var enemy in enemies)
                    {
                        if (enemy.isDead) continue;
                        if (proj.GetBounds().IntersectsWith(enemy.GetBounds()))
                        {
                            enemy.TakeDamage(proj.Damage);
                            proj.IsDead = true;
                            break;
                        }
                    }
                }
                // Enemy projectile -> hit player (not implemented for enemies in current code, but kept for future)
                else if (proj.Owner is Enemy)
                {
                    if (proj.GetBounds().IntersectsWith(player.GetBounds()))
                    {
                        player.TakeDamage(proj.Damage);
                        proj.IsDead = true;
                    }
                }
            }

            // Cleanup
            projectiles.RemoveAll(p => p.IsDead);
            enemies.RemoveAll(e2 => e2.isDead);

            // Check player death -> return to main menu
            if (player.Health <= 0)
            {
                EndGame();
                return;
            }

            Invalidate();
        }

        private void EndGame()
        {
            // stop timers and spawner cleanly then close the form so menu reappears
            try
            {
                gameTimer?.Stop();
                spawner?.Stop();
            }
            catch { }
            this.Close();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            // Fire on Space
            if (e.KeyCode == Keys.Space)
            {
                var proj = player.Fire();
                if (proj != null)
                    projectiles.Add(proj);
            }

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

            // Background
            g.Clear(Color.SkyBlue);

            // Platforms
            foreach (var platform in platforms)
                platform.Draw(g);

            // Player
            player.Draw(g);

            // Enemies
            foreach (var enemy in enemies)
            {
                if (!enemy.isDead)
                    enemy.Draw(g);
            }

            // Projectiles
            foreach (var proj in projectiles)
                proj.Draw(g);

            // HUD / debug
            g.DrawString($"Enemies: {enemies.Count(enemy => !enemy.isDead)}", this.Font, Brushes.Black, 10, 10);
            g.DrawString($"On Ground: {player.IsOnGround}", this.Font, Brushes.Black, 10, 30);
            g.DrawString($"Player HP: {player.Health}/{player.MaxHealth}", this.Font, Brushes.Black, 10, 50);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            try
            {
                gameTimer?.Stop();
                gameTimer?.Dispose();
                spawner?.Stop();
                spawner?.Dispose();
            }
            catch { }
        }

        public void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
