using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Media;
using System.Windows.Media;
using System.IO;


namespace _24520168_24520197_24520287
{
    public partial class Form1 : Form
    {
        private Player player;
        private List<Enemy> enemies;
        private List<Platform> platforms;
        private EnemySpawner spawner;
        private Timer gameTimer;

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

        public Form1()
        {
            InitializeComponent();
            InitializePauseMenu();

            this.DoubleBuffered = true;
            this.KeyPreview = true;
            this.ClientSize = new Size(800, 600);

            InitializeGame();
        }


        private void InitializePauseMenu()
        {
            // 1. Tạo Panel chính cho menu
            panelPauseMenu = new Panel
            {
                Name = "panelPauseMenu",
                Size = new Size(400, 300),
                Location = new Point((this.ClientSize.Width - 400) / 2, (this.ClientSize.Height - 300) / 2),
                BackColor = System.Drawing.Color.FromArgb(200, System.Drawing.Color.Black), // Sửa ở đây
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false
            };

            // 2. Tiêu đề "Paused"
            labelPausedTitle = new Label
            {
                Name = "labelPausedTitle",
                Text = "PAUSED",
                Font = new Font("Arial", 24, FontStyle.Bold),
                ForeColor = System.Drawing.Color.White, // Sửa ở đây
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(panelPauseMenu.Width, 50),
                Location = new Point(0, 20)
            };

            // 3. Nút Resume
            btnResume = new Button
            {
                Name = "btnResume",
                Text = "Resume",
                Size = new Size(200, 40),
                Location = new Point((panelPauseMenu.Width - 200) / 2, 80),
                BackColor = System.Drawing.Color.Gray, // Sửa ở đây
                ForeColor = System.Drawing.Color.White, // Sửa ở đây
                FlatStyle = FlatStyle.Flat
            };
            btnResume.Click += (sender, e) => TogglePause();

            // 4. Nút Quit
            btnQuit = new Button
            {
                Name = "btnQuit",
                Text = "Quit to Menu",
                Size = new Size(200, 40),
                Location = new Point((panelPauseMenu.Width - 200) / 2, 130),
                BackColor = System.Drawing.Color.Gray, // Sửa ở đây
                ForeColor = System.Drawing.Color.White, // Sửa ở đây
                FlatStyle = FlatStyle.Flat
            };
            btnQuit.Click += (sender, e) => EndGame();

            // 5. Checkbox tắt SFX
            chkSfxMute = new CheckBox
            {
                Name = "chkSfxMute",
                Text = "Mute SFX",
                ForeColor = System.Drawing.Color.White, // Sửa ở đây
                Size = new Size(100, 30),
                Location = new Point(50, 200)
            };
            chkSfxMute.CheckedChanged += (sender, e) => { SfxMuted = chkSfxMute.Checked; };

            // 6. Thanh trượt Âm lượng Nhạc nền
            labelMusicVolume = new Label
            {
                Name = "labelMusicVolume",
                Text = "Music Volume:",
                ForeColor = System.Drawing.Color.White, // Sửa ở đây
                Size = new Size(100, 30),
                Location = new Point(50, 240)
            };

            // ... (phần code còn lại của trackBarMusic, Add Controls...)
            // ...
            trackBarMusic = new TrackBar
            {
                Name = "trackBarMusic",
                Minimum = 0,
                Maximum = 100,
                Value = 50, // Âm lượng mặc định 50%
                TickFrequency = 10,
                Size = new Size(250, 45),
                Location = new Point(100, 235)
            };
            trackBarMusic.ValueChanged += TrackBarMusic_ValueChanged;

            // 7. Thêm tất cả controls vào Panel
            panelPauseMenu.Controls.Add(labelPausedTitle);
            panelPauseMenu.Controls.Add(btnResume);
            panelPauseMenu.Controls.Add(btnQuit);
            panelPauseMenu.Controls.Add(chkSfxMute);
            panelPauseMenu.Controls.Add(labelMusicVolume);
            panelPauseMenu.Controls.Add(trackBarMusic);

            // 8. Thêm Panel vào Form
            this.Controls.Add(panelPauseMenu);
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

            // Khởi tạo nhạc nền
            try
            {
                musicPlayer = new MediaPlayer();
                // Đường dẫn tới file nhạc. Đảm bảo bạn có file "music.mp3" trong thư mục "Assets"
                string musicPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources", "attack.mp3");
                musicPlayer.Open(new Uri(musicPath));

                // Đặt âm lượng ban đầu từ thanh trượt
                musicPlayer.Volume = trackBarMusic.Value / 100.0;

                // Sự kiện để lặp lại nhạc khi kết thúc
                musicPlayer.MediaEnded += (sender, e) =>
                {
                    musicPlayer.Position = TimeSpan.Zero;
                    musicPlayer.Play();
                };

                musicPlayer.Play();
            }
            catch (Exception ex)
            {
                // Ghi lại lỗi nếu không tìm thấy file nhạc
                Console.WriteLine("Error loading background music: " + ex.Message);
            }

            // Game loop
            gameTimer = new Timer();
            gameTimer.Interval = 16; // ~60 FPS
            gameTimer.Tick += GameLoop;
            gameTimer.Start();

            spawner.Start();
        }

        // Phương thức này được gọi bởi thanh trượt âm lượng
        private void TrackBarMusic_ValueChanged(object sender, EventArgs e)
        {
            if (musicPlayer != null)
            {
                // Chuyển giá trị (0-100) của TrackBar sang (0.0 - 1.0) cho MediaPlayer
                musicPlayer.Volume = trackBarMusic.Value / 100.0;
            }
        }

        // Phương thức Bật/Tắt Pause
        private void TogglePause()
        {
            isPaused = !isPaused; // Đảo ngược trạng thái pause

            if (isPaused)
            {
                gameTimer.Stop();
                spawner.Stop(); // Tạm dừng spawner
                musicPlayer.Pause();
                panelPauseMenu.Visible = true;
                panelPauseMenu.BringToFront(); // Đưa menu lên trên cùng
            }
            else
            {
                gameTimer.Start();
                spawner.Start(); // Chạy lại spawner
                musicPlayer.Play();
                panelPauseMenu.Visible = false;
            }
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
                musicPlayer?.Stop(); // <-- Dừng nhạc
                musicPlayer?.Close(); // <-- Giải phóng file
            }
            catch { }
            this.Close();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            // Bắt phím Escape để mở/tắt menu
            if (e.KeyCode == Keys.Escape)
            {
                TogglePause();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return; // Không xử lý gì thêm
            }

            // Nếu đang pause, không nhận phím game
            if (isPaused) return;

            // Fire on Space
            if (e.KeyCode == Keys.A)
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
            // Nếu đang pause, không nhận phím
            if (isPaused && e.KeyCode != Keys.Escape) return;

     

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
            g.Clear(System.Drawing.Color.SkyBlue);

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
            g.DrawString($"Enemies: {enemies.Count(enemy => !enemy.isDead)}", this.Font, System.Drawing.Brushes.Black, 10, 10);
            g.DrawString($"On Ground: {player.IsOnGround}", this.Font, System.Drawing.Brushes.Black, 10, 30);
            g.DrawString($"Player HP: {player.Health}/{player.MaxHealth}", this.Font, System.Drawing.Brushes.Black, 10, 50);
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
                musicPlayer?.Stop(); // <-- Dừng nhạc
                musicPlayer?.Close(); // <-- Giải phóng file
            }
            catch { }
        }

        public void Form1_Load(object sender, EventArgs e)
        {
            Console.WriteLine("Game Started");
        }
    }
}
