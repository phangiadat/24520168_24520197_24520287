using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Media;
using System.Windows.Forms;

namespace _24520168_24520197_24520287
{
    public enum PlayerState
    {
        Idle,
        Run,
        Jump,
        Fall,
        Shoot,
        Hurt,
        Death,
    }

    public class Player : GameObject
    {
        private const float Gravity = 0.5f;     // Trọng lực
        private const float MoveSpeed = 5f;     // Tốc độ ngang
        private const float JumpForce = -12f;   // Lực nhảy
        private const float MaxFallSpeed = 15f; // Tốc độ rơi tối đa

        private readonly Dictionary<Keys, bool> keyStates; // Trạng thái phím
        private bool canJump = true; // Cho phép nhảy ngay từ đầu

        // Hướng và bắn
        private int facing = 1; // 1: phải, -1: trái
        private float fireCooldown = 0f;
        private const float FireRate = 0.35f;

        // Âm thanh player bị thương
        private static SoundPlayer hitPlayer;

        // Máu
        public int MaxHealth { get; private set; }
        public int Health { get; private set; }
        public event EventHandler PlayerDied;

        // Animation / sprite sheet
        private Bitmap spriteSheet;
        private int currentFrame = 0;
        private float frameTimer = 0f;
        private bool isHurt = false;
        private bool isShooting = false;

        private const int FrameWidth = 64;
        private const int FrameHeight = 64;
        private const int FramesPerRow = 11;

        public struct Animation
        {
            public int StartFrame;
            public int FrameCount;
            public float FrameDuration;
        }

        private PlayerState currentState = PlayerState.Idle;
        private Dictionary<PlayerState, Animation> animations;

        // ----- Static ctor: load âm thanh một lần -----
        static Player()
        {
            try
            {
                var hurtPath = Path.Combine(Application.StartupPath, "resources", "Player Hurt.wav");
                if (File.Exists(hurtPath))
                {
                    hitPlayer = new SoundPlayer(hurtPath);
                    hitPlayer.Load();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading player sound: " + ex.Message);
                hitPlayer = null;
            }
        }

        public Player(float x, float y)
        {
            X = x;
            Y = y;
            VelocityX = 0;
            VelocityY = 0;
            IsOnGround = false;

            keyStates = new Dictionary<Keys, bool>
            {
                { Keys.Left,  false },
                { Keys.Right, false },
                { Keys.Up,    false },
            };

            MaxHealth = 100;
            Health = MaxHealth;

            Width = FrameWidth;
            Height = FrameHeight;

            // Load sprite sheet an toàn đường dẫn
            try
            {
                var sheetPath = Path.Combine(Application.StartupPath, "Assets", "Player", "GandalfHardcoreArchersheet.png");
                if (File.Exists(sheetPath))
                {
                    spriteSheet = new Bitmap(sheetPath);
                }
                else
                {
                    Console.WriteLine("Không tìm thấy sprite sheet: " + sheetPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Không thể tải Sprite Sheet: {ex.Message}");
            }

            animations = new Dictionary<PlayerState, Animation>
            {
                { PlayerState.Idle,  new Animation { StartFrame = 0,  FrameCount = 4, FrameDuration = 0.15f } },
                { PlayerState.Shoot, new Animation { StartFrame = 11, FrameCount = 8, FrameDuration = 0.02f } },
                { PlayerState.Run,   new Animation { StartFrame = 22, FrameCount = 4, FrameDuration = 0.10f } },
                { PlayerState.Hurt,  new Animation { StartFrame = 33, FrameCount = 4, FrameDuration = 0.10f } },
                { PlayerState.Death, new Animation { StartFrame = 51, FrameCount = 4, FrameDuration = 0.20f } },
                { PlayerState.Jump,  new Animation { StartFrame = 2,  FrameCount = 1, FrameDuration = 1.00f } },
                { PlayerState.Fall,  new Animation { StartFrame = 2,  FrameCount = 1, FrameDuration = 1.00f } },
            };
        }

        public void SetKeyState(Keys key, bool isPressed)
        {
            if (keyStates.ContainsKey(key)) keyStates[key] = isPressed;
        }

        public void TakeDamage(int amount)
        {
            if (Health <= 0) return;

            Health -= amount;

            if (hitPlayer != null && !Form1.SfxMuted)
            {
                try { hitPlayer.Play(); } catch { /* ignore */ }
            }

            if (Health <= 0)
            {
                Health = 0;
                OnPlayerDied();
            }
            else
            {
                isHurt = true;
                currentFrame = 0;
                frameTimer = 0f;
            }
        }

        protected virtual void OnPlayerDied()
        {
            PlayerDied?.Invoke(this, EventArgs.Empty);
        }

        public Projectile Fire()
        {
            if (fireCooldown > 0f) return null;

            float px = X + Width / 2f;
            float py = Y + Height / 2f;
            float speed = 10f;
            float vx = speed * facing;
            float vy = 0f;
            int damage = 20;

            var proj = new Projectile(px, py + 6f, vx, vy, 5f, damage, this);

            fireCooldown = FireRate;
            isShooting = true;
            return proj;
        }

        public override void Draw(Graphics g)
        {
            if (spriteSheet != null && animations.ContainsKey(currentState))
            {
                Animation currentAnim = animations[currentState];

                int absoluteFrameIndex = currentAnim.StartFrame + currentFrame;
                int frameCol = absoluteFrameIndex % FramesPerRow;
                int frameRow = absoluteFrameIndex / FramesPerRow;

                int sourceX = frameCol * FrameWidth;
                int sourceY = frameRow * FrameHeight;

                Rectangle srcRect = new Rectangle(sourceX, sourceY, FrameWidth, FrameHeight);
                Rectangle destRect = new Rectangle((int)X, (int)Y, (int)Width, (int)Height);

                if (facing == 1)
                {
                    g.DrawImage(spriteSheet, destRect, srcRect, GraphicsUnit.Pixel);
                }
                else
                {
                    var flippedDestRect = new Rectangle((int)(X + Width), (int)Y, -(int)Width, (int)Height);
                    g.DrawImage(spriteSheet, flippedDestRect, srcRect, GraphicsUnit.Pixel);
                }
            }
            else
            {
                g.FillRectangle(Brushes.Blue, X, Y, Width, Height);
            }

            // Thanh máu
            float barW = Width, barH = 6f, barX = X, barY = Y - barH - 2f;
            g.FillRectangle(Brushes.Red, barX, barY, barW, barH);
            float ratio = (MaxHealth > 0) ? (float)Health / MaxHealth : 0f;
            g.FillRectangle(Brushes.LimeGreen, barX, barY, barW * ratio, barH);
            g.DrawRectangle(Pens.Black, barX, barY, barW, barH);
        }

        public override void Update(List<Platform> platforms)
        {
            // Bắn cooldown
            if (fireCooldown > 0f) fireCooldown -= 0.016f;

            // Điều khiển
            VelocityX = 0f;
            if (keyStates[Keys.Left])  { VelocityX = -MoveSpeed; facing = -1; }
            if (keyStates[Keys.Right]) { VelocityX =  MoveSpeed; facing =  1; }

            if (keyStates[Keys.Up] && IsOnGround && canJump)
            {
                VelocityY = JumpForce;
                IsOnGround = false;
                canJump = false;
            }
            if (!keyStates[Keys.Up]) canJump = true;

            // Trọng lực
            VelocityY += Gravity;
            if (VelocityY > MaxFallSpeed) VelocityY = MaxFallSpeed;

            // Di chuyển + va chạm
            X += VelocityX;
            HandleHorizontalCollisions(platforms);

            Y += VelocityY;
            HandleVerticalCollisions(platforms);

            // Chọn state
            PlayerState newState;
            if (Health <= 0) newState = PlayerState.Death;
            else if (isHurt) newState = PlayerState.Hurt;
            else if (isShooting) newState = PlayerState.Shoot;
            else
            {
                if (!IsOnGround) newState = (VelocityY < 0) ? PlayerState.Jump : PlayerState.Fall;
                else newState = (Math.Abs(VelocityX) > 0.1f) ? PlayerState.Run : PlayerState.Idle;
            }

            if (newState != currentState)
            {
                currentState = newState;
                currentFrame = 0;
                frameTimer = 0f;
            }

            // Cập nhật frame
            Animation anim = animations[currentState];
            if (anim.FrameCount > 1)
            {
                frameTimer += 0.016f;
                if (frameTimer >= anim.FrameDuration)
                {
                    frameTimer = 0f;
                    currentFrame++;

                    if (currentState == PlayerState.Shoot)
                    {
                        if (currentFrame >= anim.FrameCount)
                        {
                            currentFrame = 0;
                            isShooting = false;
                        }
                    }
                    else if (currentState == PlayerState.Hurt)
                    {
                        if (currentFrame >= anim.FrameCount)
                        {
                            currentFrame = 0;
                            isHurt = false;
                        }
                    }
                    else if (currentState == PlayerState.Death)
                    {
                        if (currentFrame >= anim.FrameCount)
                        {
                            currentFrame = anim.FrameCount - 1; // dừng ở frame cuối
                            frameTimer = anim.FrameDuration;
                        }
                    }
                    else
                    {
                        currentFrame %= anim.FrameCount; // Idle/Run lặp
                    }
                }
            }

            // Ràng giới hạn
            if (X < 0) { X = 0; VelocityX = 0; }
            if (Y < 0) { Y = 0; VelocityY = 0; }

            // Rơi khỏi map => chết
            if (Y > 1000 && Health > 0)
            {
                TakeDamage(MaxHealth);
            }
        }

        private void HandleHorizontalCollisions(List<Platform> platforms)
        {
            RectangleF playerBounds = GetBounds();
            foreach (var platform in platforms)
            {
                if (playerBounds.IntersectsWith(platform.GetBounds()))
                {
                    if (VelocityX > 0) X = platform.X - Width;
                    else if (VelocityX < 0) X = platform.X + platform.Width;
                    VelocityX = 0;
                    playerBounds = GetBounds(); // cập nhật bounds sau khi chỉnh X
                }
            }
        }

        private void HandleVerticalCollisions(List<Platform> platforms)
        {
            RectangleF playerBounds = GetBounds();
            IsOnGround = false;
            foreach (var platform in platforms)
            {
                if (playerBounds.IntersectsWith(platform.GetBounds()))
                {
                    if (VelocityY > 0)      { Y = platform.Y - Height; IsOnGround = true; }
                    else if (VelocityY < 0) { Y = platform.Y + platform.Height; }
                    VelocityY = 0;
                    playerBounds = GetBounds(); // cập nhật bounds sau khi chỉnh Y
                }
            }
        }
    }
}
