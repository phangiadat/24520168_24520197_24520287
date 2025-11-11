using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Windows.Media; // MediaPlayer (WPF) – đã có PresentationCore trong .csproj

namespace _24520168_24520197_24520287
{
    public partial class Form1 : Form
    {
        private Player player;
        private List<Enemy> enemies;
        private List<Platform> platforms;
        private EnemySpawner spawner;
        private Timer gameTimer;

        // Camera (từ HEAD)
        private float cameraX = 0f;

        // Pause menu + âm thanh (từ nhánh Binh)
        private bool isPaused = false;
        private Panel panelPauseMenu;
        private Label labelPausedTitle;
        private Button btnResume;
        private Button btnQuit;
        private TrackBar trackBarMusic;
        private Label labelMusicVolume;
        private CheckBox chkSfxMute;
        private MediaPlayer musicPlayer;
        public static bool SfxMuted = false;

        // Projectiles
        private List<Projectile> projectiles;

        private readonly Random rand = new Random();
        private float generationEdgeX = 0;     // X xa nhất đã tạo platform
        private float lastPlatformY = 550;     // Y của platform cuối cùng
        private const float DespawnBuffer = 500;

        private int enemiesKilled = 0;

        public Form1()
        {
            InitializeComponent();

            // Thiết lập form trước để panel pause căn giữa đúng
            this.DoubleBuffered = true;
            this.KeyPreview = true;
            this.ClientSize = new Size(800, 600);

            InitializePauseMenu(); // tạo control (trackBarMusic…) trước khi init game để set volume
            InitializeGame();
        }

        private void InitializePauseMenu()
        {
            panelPauseMenu = new Panel
            {
                Name = "panelPauseMenu",
                Size = new Size(400, 300),
                Location = new Point((this.ClientSize.Width - 400) / 2, (this.ClientSize.Height - 300) / 2),
                BackColor = System.Drawing.Color.FromArgb(200, System.Drawing.Color.Black),
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false
            };

            labelPausedTitle = new Label
            {
                Name = "labelPausedTitle",
                Text = "PAUSED",
                Font = new Font("Arial", 24, FontStyle.Bold),
                ForeColor = System.Drawing.Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(panelPauseMenu.Width, 50),
                Location = new Point(0, 20)
            };

            btnResume = new Button
            {
                Name = "btnResume",
                Text = "Resume",
                Size = new Size(200, 40),
                Location = new Point((panelPauseMenu.Width - 200) / 2, 80),
                BackColor = System.Drawing.Color.Gray,
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnResume.Click += (sender, e) => TogglePause();

            btnQuit = new Button
            {
                Name = "btnQuit",
                Text = "Quit to Menu",
                Size = new Size(200, 40),
                Location = new Point((panelPauseMenu.Width - 200) / 2, 130),
                BackColor = System.Drawing.Color.Gray,
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnQuit.Click += (sender, e) => EndGame();

            chkSfxMute = new CheckBox
            {
                Name = "chkSfxMute",
                Text = "Mute SFX",
                ForeColor = System.Drawing.Color.White,
                Size = new Size(100, 30),
                Location = new Point(50, 200)
            };
            chkSfxMute.CheckedChanged += (sender, e) => { SfxMuted = chkSfxMute.Checked; };

            labelMusicVolume = new Label
            {
                Name = "labelMusicVolume",
                Text = "Music Volume:",
                ForeColor = System.Drawing.Color.White,
                Size = new Size(120, 30),
                Location = new Point(50, 240)
            };

            trackBarMusic = new TrackBar
            {
                Name = "trackBarMusic",
                Minimum = 0,
                Maximum = 100,
                Value = 50,                 // âm lượng mặc định
                TickFrequency = 10,
                Size = new Size(250, 45),
                Location = new Point(170, 235)
            };
            trackBarMusic.ValueChanged += TrackBarMusic_ValueChanged;

            panelPauseMenu.Controls.Add(labelPausedTitle);
            panelPauseMenu.Controls.Add(btnResume);
            panelPauseMenu.Controls.Add(btnQuit);
            panelPauseMenu.Controls.Add(chkSfxMute);
            panelPauseMenu.Controls.Add(labelMusicVolume);
            panelPauseMenu.Controls.Add(trackBarMusic);

            this.Controls.Add(panelPauseMenu);
        }

        private void InitializeGame()
        {
            // --- Platforms ---
            platforms = new List<Platform>();
            int groundHeight = 50;
            platforms.Add(new Platform(0, 550, 800, groundHeight)); // đất nền

            lastPlatformY = 550;
            generationEdgeX = 800;

            // --- Player ---
            player = new Player(100, this.ClientSize.Height - groundHeight - 60); // đứng trên đất

            // --- Enemies & spawner ---
            enemies = new List<Enemy>();
            spawner = new EnemySpawner(player, enemies, platforms);

            // --- Projectiles ---
            projectiles = new List<Projectile>();

            // --- Nhạc nền ---
            try
            {
                musicPlayer = new MediaPlayer();
                string musicPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources", "attack.mp3");
                if (File.Exists(musicPath))
                {
                    musicPlayer.Open(new Uri(musicPath));
                    musicPlayer.Volume = trackBarMusic.Value / 100.0; // sync với UI
                    musicPlayer.MediaEnded += (sender, e) =>
                    {
                        musicPlayer.Position = TimeSpan.Zero;
                        musicPlayer.Play();
                    };
                    musicPlayer.Play();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading background music: " + ex.Message);
            }

            // --- Game loop ---
            gameTimer = new Timer { Interval = 16 }; // ~60 FPS
            gameTimer.Tick += GameLoop;

            gameTimer.Start();
            spawner.Start();
        }

        private void TrackBarMusic_ValueChanged(object sender, EventArgs e)
        {
            if (musicPlayer != null)
            {
                musicPlayer.Volume = trackBarMusic.Value / 100.0;
            }
        }

        private void TogglePause()
        {
            isPaused = !isPaused;

            if (isPaused)
            {
                gameTimer?.Stop();
                spawner?.Stop();
                musicPlayer?.Pause();
                panelPauseMenu.Visible = true;
                panelPauseMenu.BringToFront();
            }
            else
            {
                gameTimer?.Start();
                spawner?.Start();
                musicPlayer?.Play();
                panelPauseMenu.Visible = false;
            }
        }

        private void GameLoop(object sender, EventArgs e)
        {
            // Update player
            player.Update(platforms);

            // Platforms
            UpdatePlatforms();

            // Enemies
            foreach (var enemy in enemies.ToList())
            {
                enemy.Update(platforms);

                // Player - enemy collision
                if (player.GetBounds().IntersectsWith(enemy.GetBounds()))
                {
                    if (player.VelocityY > 0 && player.Y + player.Height - 10 < enemy.Y + enemy.Height / 2)
                    {
                        enemy.isDead = true;
                        player.VelocityY = -8; // nảy lên
                    }
                    else
                    {
                        player.TakeDamage(1);
                    }
                }
            }

            // Projectiles
            foreach (var proj in projectiles.ToList())
            {
                proj.Update();

                float cameraLeftEdge = cameraX - 100;
                float cameraRightEdge = cameraX + this.ClientSize.Width + 100;

                if (proj.X < cameraLeftEdge || proj.X > cameraRightEdge || proj.Y < -50 || proj.Y > this.ClientSize.Height + 50)
                {
                    proj.IsDead = true;
                }
                if (proj.IsDead) continue;

                if (proj.Owner is Player)
                {
                    foreach (var enemy in enemies)
                    {
                        if (proj.GetBounds().IntersectsWith(enemy.GetBounds()))
                        {
                            enemy.TakeDamage(proj.Damage);
                            proj.IsDead = true;
                            break;
                        }
                    }
                }
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
            int killed = enemies.RemoveAll(e2 => (e2.isDead && !e2.isSelfKilled));
            enemiesKilled += killed;

            // Check player death
            if (player.Health <= 0)
            {
                EndGame();
                return;
            }

            Invalidate();
        }

        private void EndGame()
        {
            try
            {
                gameTimer?.Stop();
                spawner?.Stop();
                musicPlayer?.Stop();
                musicPlayer?.Close();
            }
            catch { }
            this.Close();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            // Toggle pause
            if (e.KeyCode == Keys.Escape)
            {
                TogglePause();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }

            if (isPaused) return;

            // Bắn
            if (e.KeyCode == Keys.A)
            {
                var proj = player.Fire();
                if (proj != null) projectiles.Add(proj);
            }

            player.SetKeyState(e.KeyCode, true);
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (isPaused && e.KeyCode != Keys.Escape) return;

            player.SetKeyState(e.KeyCode, false);
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;

            // Nền
            g.Clear(Color.SkyBlue);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Camera follow X
            float targetX = player.X - (this.ClientSize.Width / 2f);
            cameraX += (targetX - cameraX) * 0.1f;
            g.TranslateTransform(-cameraX, 0);

            // Draw world
            foreach (var platform in platforms) platform.Draw(g);
            foreach (var enemy in enemies) if (!enemy.isDead) enemy.Draw(g);
            foreach (var proj in projectiles) proj.Draw(g);
            player.Draw(g);

            // Reset transform cho HUD
            g.ResetTransform();

            // HUD
            g.DrawString($"Enemies: {enemies.Count(enemy => !enemy.isDead)}", this.Font, Brushes.Black, 10, 10);
            g.DrawString($"On Ground: {player.IsOnGround}", this.Font, Brushes.Black, 10, 30);
            g.DrawString($"Player HP: {player.Health}/{player.MaxHealth}", this.Font, Brushes.Black, 10, 50);
            g.DrawString($"Enemies Killed: {enemiesKilled}", this.Font, Brushes.Black, 10, 70);
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
                musicPlayer?.Stop();
                musicPlayer?.Close();
            }
            catch { }
        }

        // ===== Platform management =====

        private void UpdatePlatforms()
        {
            DespawnOldPlatforms();
            GenerateNewPlatforms();
        }

        private void DespawnOldPlatforms()
        {
            float despawnEdge = cameraX - DespawnBuffer;
            // giữ lại đất nền Y=550 (nếu bạn muốn)
            platforms.RemoveAll(p => (p.X + p.Width) < despawnEdge && p.Y < 540);
        }

        private void GenerateNewPlatforms()
        {
            float generationTriggerEdge = cameraX + this.ClientSize.Width + 200;

            while (generationEdgeX < generationTriggerEdge)
            {
                int gap = rand.Next(60, 140);
                int width = rand.Next(100, 250);

                int yChange = rand.Next(-100, 100);
                float newY = lastPlatformY + yChange;
                newY = Math.Max(newY, 250);
                newY = Math.Min(newY, 550);

                float newX = generationEdgeX + gap;
                platforms.Add(new Platform(newX, newY, width, 20));

                if (rand.Next(100) < 60) // 60% xuất hiện enemy (comment gốc ghi 30% nhưng code dùng 60)
                {
                    float enemyX = newX + (width / 2f);
                    float enemyY = newY - Enemy.cactusHeight;
                    enemies.Add(new Enemy(enemyX, enemyY, player));
                }

                lastPlatformY = newY;
                generationEdgeX = newX + width;
            }
        }

        public void Form1_Load(object sender, EventArgs e)
        {
            Console.WriteLine("Game Started");
        }
    }
}
