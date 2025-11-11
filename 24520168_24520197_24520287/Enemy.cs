using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Media;
using System.Windows.Forms;

namespace _24520168_24520197_24520287
{
    public class Enemy : GameObject
    {
        private const float MoveSpeed = 2f;           // Tốc độ di chuyển
        private const float Gravity = 0.5f;           // Trọng lực
        private const float MaxFallSpeed = 10f;       // Tốc độ rơi tối đa
        private const float DetectionRange = 200f;    // Khoảng phát hiện (fix typo)
        private const float JumpForce = -10f;         // Lực nhảy

        private Player target;
        private static readonly Random rng = new Random();
        private float idleTimer;                      // Thời gian đứng yên
        private bool facingRight;

        // Ảnh kẻ địch
        private static Image cactusImage;
        public static int cactusWidth;
        public static int cactusHeight;

        // Âm thanh của địch
        private static SoundPlayer hitSound;
        private static SoundPlayer deathSound;

        // Máu
        public int MaxHealth { get; private set; }
        public int Health { get; private set; }

        public bool isSelfKilled { get; set; }
        public bool isDead { get; set; }

        // Tải tài nguyên tĩnh
        static Enemy()
        {
            // Ảnh
            try
            {
                var imgPath = Path.Combine(Application.StartupPath, "Assets", "Monster", "monster.png");
                cactusImage = File.Exists(imgPath) ? new Bitmap(imgPath) : null;
                if (cactusImage != null)
                {
                    cactusWidth = cactusImage.Width;
                    cactusHeight = cactusImage.Height;
                }
                else
                {
                    cactusWidth = 35;
                    cactusHeight = 50;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Không thể tải ảnh Enemy: {ex.Message}");
                cactusImage = null;
                cactusWidth = 35;
                cactusHeight = 50;
            }

            // Âm thanh
            try
            {
                var hurtPath = Path.Combine(Application.StartupPath, "resources", "Enemy Hurt.wav");
                var deathPath = Path.Combine(Application.StartupPath, "resources", "Enemy Death.wav");

                if (File.Exists(hurtPath))
                {
                    hitSound = new SoundPlayer(hurtPath);
                    hitSound.Load();
                }
                if (File.Exists(deathPath))
                {
                    deathSound = new SoundPlayer(deathPath);
                    deathSound.Load();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading enemy sound: " + ex.Message);
                hitSound = null;
                deathSound = null;
            }
        }

        public Enemy(float x, float y, Player player)
        {
            X = x;
            Y = y;
            Width = Math.Max(1, cactusWidth);
            Height = Math.Max(1, cactusHeight);
            target = player;
            idleTimer = 0f;
            facingRight = true;

            MaxHealth = 50;
            Health = MaxHealth;
            isDead = false;
        }

        public void TakeDamage(int amount)
        {
            if (isDead) return;

            Health -= amount;

            if (hitSound != null && !Form1.SfxMuted)
            {
                try { hitSound.Play(); } catch { /* ignore */ }
            }

            if (Health <= 0)
            {
                Health = 0;
                isDead = true;

                if (deathSound != null)
                {
                    try { deathSound.Play(); } catch { /* ignore */ }
                }
            }
        }

        public override void Draw(Graphics g)
        {
            if (isDead) return;

            if (cactusImage != null)
            {
                var destRect = new Rectangle((int)X, (int)Y, (int)Width, (int)Height);
                if (facingRight)
                {
                    g.DrawImage(cactusImage, destRect);
                }
                else
                {
                    var flippedRect = new Rectangle((int)(X + Width), (int)Y, -(int)Width, (int)Height);
                    g.DrawImage(cactusImage, flippedRect);
                }
            }
            else
            {
                g.FillRectangle(Brushes.Red, X, Y, Width, Height);
            }

            // Thanh máu
            float barW = Width, barH = 5f, barX = X, barY = Y - 8f;
            g.FillRectangle(Brushes.Red, barX, barY, barW, barH);
            float ratio = (MaxHealth > 0) ? (float)Health / MaxHealth : 0f;
            g.FillRectangle(Brushes.LimeGreen, barX, barY, barW * ratio, barH);
            g.DrawRectangle(Pens.Black, barX, barY, barW, barH);
        }

        public override void Update(List<Platform> platforms)
        {
            if (isDead) return;

            // Trọng lực
            VelocityY += Gravity;
            if (VelocityY > MaxFallSpeed) VelocityY = MaxFallSpeed;

            // AI đuổi theo player
            float distanceToPlayer = (target != null) ? Math.Abs(target.X - X) : float.MaxValue;
            float heightDifference = (target != null) ? (target.Y - Y) : 0f;

            if (distanceToPlayer < DetectionRange)
            {
                if (target.X < X) { VelocityX = -MoveSpeed; facingRight = false; }
                else if (target.X > X) { VelocityX = MoveSpeed; facingRight = true; }

                // Nhảy nếu player ở trên và đang đứng đất
                if (heightDifference < -20 && IsOnGround && rng.Next(100) < 3)
                {
                    VelocityY = JumpForce;
                }
            }
            else
            {
                // Đi lang thang (idle)
                idleTimer += 0.0167f; // ~1 frame ở 60fps
                if (idleTimer > 2f)
                {
                    int step = rng.Next(3) - 1; // -1, 0, 1
                    VelocityX = step * MoveSpeed;
                    idleTimer = 0f;
                }
            }

            // Di chuyển và va chạm
            X += VelocityX;
            HandleHorizontalCollision(platforms);

            Y += VelocityY;
            HandleVerticalCollision(platforms);

            // Rơi khỏi map -> xem như chết do tự rơi
            if (Y > 1000)
            {
                isDead = true;
                isSelfKilled = true;
            }
        }

        private void HandleVerticalCollision(List<Platform> platforms)
        {
            IsOnGround = false;
            if (platforms == null || platforms.Count == 0) return;

            for (int i = 0; i < platforms.Count; i++)
            {
                var platform = platforms[i];
                RectangleF enemyBounds = GetBounds();
                if (enemyBounds.IntersectsWith(platform.GetBounds()))
                {
                    if (VelocityY > 0)
                    {
                        Y = platform.Y - Height;
                        VelocityY = 0;
                        IsOnGround = true;
                    }
                    else if (VelocityY < 0)
                    {
                        Y = platform.Y + platform.Height;
                        VelocityY = 0;
                    }
                }
            }
        }

        private void HandleHorizontalCollision(List<Platform> platforms)
        {
            if (platforms == null || platforms.Count == 0) return;

            for (int i = 0; i < platforms.Count; i++)
            {
                var platform = platforms[i];
                RectangleF enemyBounds = GetBounds();
                if (enemyBounds.IntersectsWith(platform.GetBounds()))
                {
                    if (VelocityX > 0)
                    {
                        X = platform.X - Width;
                        VelocityX = -VelocityX;
                        facingRight = false;
                    }
                    else if (VelocityX < 0)
                    {
                        X = platform.X + platform.Width;
                        VelocityX = -VelocityX;
                        facingRight = true;
                    }
                }
            }
        }
    }
}
