using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace _24520168_24520197_24520287
{
    public class Enemy : GameObject
    {
        private const float MoveSpeed = 2f; // Tốc độ di chuyển của kẻ địch
        private const float Gravity = 0.5f; // Trọng lực tác động lên kẻ địch
        private const float MaxFallSpeed = 10f; // Tốc độ rơi tối đa của kẻ địch
        private const float DectectionRange = 200f; // Khoảng cách phát hiện người chơi
        private const float JumpForce = -10f; // Lực nhảy của kẻ địch

        private Player target;
        private Random random;
        private float idleTimer; //Thời gian đứng yên
        private bool facingRight;

        private static Image cactusImage;
        public static int cactusWidth;
        public static int cactusHeight;

        //Máu
        public int MaxHealth { get; private set; }
        public int Health { get; private set; }

        public bool isSelfKilled { get; set; }
        public bool isDead { get; set; }

        static Enemy()
        {
            try
            {
                // Thay "Assets\\cactus.png" bằng đường dẫn chính xác của bạn
                cactusImage = new Bitmap("Assets\\Monster\\monster.png");
                cactusWidth = cactusImage.Width;
                cactusHeight = cactusImage.Height;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Không thể tải ảnh Enemy (cactus): {ex.Message}");
                cactusImage = null;
                cactusWidth = 35; // Kích thước dự phòng
                cactusHeight = 50; // Kích thước dự phòng
            }
        }

        public Enemy(float x, float y, Player player)
        {
            X = x;
            Y = y;
            Width = cactusWidth;
            Height = cactusHeight;
            target = player;
            random = new Random();
            idleTimer = 0;
            facingRight = true;

            MaxHealth = 50;
            Health = MaxHealth;
            isDead  = false;
        }

        public void TakeDamage(int amount)
        {
            Health -= amount;
            if (Health <= 0)
            {
                Health = 0;
                isDead = true;
            }
        }

        public override void Draw(Graphics g)
        {
            if (isDead) return; // Không vẽ nếu đã chết (hoặc bạn có thể vẽ animation chết)

            if (cactusImage != null)
            {
                // Vẽ ảnh cây xương rồng
                Rectangle destRect = new Rectangle((int)X, (int)Y, (int)Width, (int)Height);

                if (facingRight)
                {
                    // Vẽ ảnh bình thường (hướng phải)
                    g.DrawImage(cactusImage, destRect);
                }
                else
                {
                    // Lật ảnh (hướng trái)
                    Rectangle flippedRect = new Rectangle((int)X + (int)Width, (int)Y, -(int)Width, (int)Height);
                    g.DrawImage(cactusImage, flippedRect);
                }
            }
            else
            {
                // Dự phòng: Vẽ hình chữ nhật nếu không tải được ảnh
                g.FillRectangle(Brushes.Red, X, Y, Width, Height);
            }

            //Vẽ thanh máu
            float barW = Width;
            float barH = 5f;
            float barX = X;
            float barY = Y - 8f;
            g.FillRectangle(Brushes.Red, barX, barY, barW, barH);
            float ratio = (MaxHealth > 0) ? (float)Health / MaxHealth : 0f;
            g.FillRectangle(Brushes.LimeGreen, barX, barY, barW * ratio, barH);
            g.DrawRectangle(Pens.Black, barX, barY, barW, barH);

        }

        public override void Update(List<Platform> platforms)
        {
            if (isDead) return;

            // Áp dụng trọng lực
            VelocityY += Gravity;
            if (VelocityY > MaxFallSpeed)
                VelocityY = MaxFallSpeed;

            // AI đuổi theo player
            float distanceToPlayer = Math.Abs(target.X - X);
            float heightDifference = target.Y - Y;

            if (distanceToPlayer < DectectionRange)
            {
                // Đuổi theo player
                if (target.X < X)
                {
                    VelocityX = -MoveSpeed; 
                    facingRight = false;
                }
                else if (target.X > X)
                {
                    VelocityX = MoveSpeed;
                    facingRight = true;
                }

                // Thử nhảy nếu player ở trên và có nền
                if (heightDifference < -20 && IsOnGround && random.Next(100) < 3)
                {
                    VelocityY = JumpForce;
                }
            }
            else
            {
                // Idle - đi lung tung
                idleTimer += 0.016f;
                if (idleTimer > 2f)
                {
                    VelocityX = random.Next(3) - 1; // -1, 0, hoặc 1
                    idleTimer = 0;
                }
            }

            // Di chuyển
            X += VelocityX;
            HandleHorizontalCollision(platforms);

            Y += VelocityY;
            HandleVerticalCollision(platforms);

            // Rơi xuống hố -> chết
            if (Y > 1000)
            {
                isDead = true;
                isSelfKilled = true;
            }
        }

        private void HandleVerticalCollision(List<Platform> platforms)
        {
            RectangleF enemyBounds = GetBounds();
            IsOnGround = false;

            foreach (Platform platform in platforms)
            {
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
            RectangleF enemyBounds = GetBounds();

            foreach (Platform platform in platforms)
            {
                if (enemyBounds.IntersectsWith(platform.GetBounds()))
                {
                    if (VelocityX > 0)
                    {
                        X = platform.X - Width;
                        VelocityX = -VelocityX; // Đổi hướng khi chạm tường
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
