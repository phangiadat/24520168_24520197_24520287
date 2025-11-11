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

        float cameraX = 0;

        // Projectiles
        private List<Projectile> projectiles;

        private Random rand = new Random(); // Để tạo số ngẫu nhiên
        private float generationEdgeX = 0; // Vị trí X xa nhất đã tạo platform
        private float lastPlatformY = 550; // Vị trí Y của platform cuối cùng
        private const float DespawnBuffer = 500;

        private int enemiesKilled = 0;

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
            platforms.Add(new Platform(0, 550, 800, groundHeight)); // Platform đất dài 800px

            // 2. Cập nhật các biến theo dõi
            lastPlatformY = 550; // Vị trí Y của platform đất
            generationEdgeX = 800; // Bắt đầu tạo platform mới sau platform đất

            // Player spawns above ground
            player = new Player(100, 550 - 60); // Đứng trên platform đất


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

            // Update platform
            UpdatePlatforms();

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

                float cameraLeftEdge = cameraX - 100; // Thêm 100px đệm
                float cameraRightEdge = cameraX + this.ClientSize.Width + 100;

                // Remove out-of-bounds (so với camera)
                if (proj.X < cameraLeftEdge || proj.X > cameraRightEdge || proj.Y < -50 || proj.Y > this.ClientSize.Height + 50)
                {
                    proj.IsDead = true;
                }

                if (proj.IsDead) continue;

                // Player projectile -> hit enemies
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
            int killed = enemies.RemoveAll(e2 => (e2.isDead && !e2.isSelfKilled));
            enemiesKilled += killed;

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
            player.SetKeyState(e.KeyCode, false);
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            g.Clear(Color.SkyBlue);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            //Camera
            float targetX = player.X - (this.ClientSize.Width / 2);
            cameraX += (targetX - cameraX) * 0.1f;
            g.TranslateTransform(-cameraX, 0);

            foreach (Platform platform in platforms)
            {
                platform.Draw(g);
            }

            foreach (Enemy enemy in enemies)
            {
                if (!enemy.isDead)
                    enemy.Draw(g);
            }

            foreach (var proj in projectiles)
            {
                proj.Draw(g);
            }

            player.Draw(g);
            g.ResetTransform();

            // HUD / debug
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
            }
            catch { }
        }

        // Thêm 3 hàm mới này vào Form1.cs

        /// <summary>
        /// Được gọi mỗi frame trong GameLoop để quản lý platforms
        /// </summary>
        private void UpdatePlatforms()
        {
            // 1. Xóa các platform cũ đã ra khỏi màn hình
            DespawnOldPlatforms();

            // 2. Tạo các platform mới ở phía trước
            GenerateNewPlatforms();
        }

        /// <summary>
        /// Xóa các platform ở quá xa về bên trái camera
        /// </summary>
        private void DespawnOldPlatforms()
        {
            // Vị trí xóa (cách bên trái camera 500px)
            float despawnEdge = cameraX - DespawnBuffer;

            // Xóa tất cả platform mà cạnh phải của nó < vị trí xóa
            // (Không xóa platform mặt đất đầu tiên nếu bạn muốn)
            platforms.RemoveAll(p => (p.X + p.Width) < despawnEdge && p.Y < 540); // (Giữ lại đất nền Y=550)
        }

        /// <summary>
        /// Tạo platform mới khi camera di chuyển đến gần cạnh đã tạo
        /// </summary>
        private void GenerateNewPlatforms()
        {
            // Vị trí kích hoạt (cách bên phải màn hình 200px)
            float generationTriggerEdge = cameraX + this.ClientSize.Width + 200;

            // Liên tục tạo platform cho đến khi "generationEdgeX" vượt qua "generationTriggerEdge"
            while (generationEdgeX < generationTriggerEdge)
            {
                // --- 1. Tính toán vị trí platform mới ---

                // Khoảng cách ngang ngẫu nhiên so với platform trước
                int gap = rand.Next(60, 140); // Khoảng trống
                int width = rand.Next(100, 250); // Độ rộng platform

                // Thay đổi độ cao Y ngẫu nhiên (để có thể nhảy tới)
                int yChange = rand.Next(-100, 100); // Thay đổi Y
                float newY = lastPlatformY + yChange;

                // Giới hạn Y (để không bay quá cao hoặc quá thấp)
                newY = Math.Max(newY, 250); // Không cao hơn 250
                newY = Math.Min(newY, 550); // Không thấp hơn 550 (mặt đất)

                // --- 2. Tạo platform mới ---
                float newX = generationEdgeX + gap;
                platforms.Add(new Platform(newX, newY, width, 20)); // Tạo platform

                if (rand.Next(100) < 60) // 30% chance
                {
                    // Tính toán vị trí X (giữa platform) và Y (trên platform)
                    float enemyX = newX + (width / 2);
                    float enemyY = newY - Enemy.cactusHeight; // Dùng DesiredHeight từ Enemy.cs

                    enemies.Add(new Enemy(enemyX, enemyY, player));
                }

                // --- 3. Cập nhật biến theo dõi ---
                lastPlatformY = newY; // Lưu lại Y cho lần tạo sau
                generationEdgeX = newX + width; // Cập nhật cạnh xa nhất
            }
        }

        public void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
