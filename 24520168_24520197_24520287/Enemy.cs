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

        public bool isDead { get; set; }
        public Enemy(float x, float y, Player player)
        {
            X = x;
            Y = y;
            Width = 35;
            Height = 50;
            target = player;
            random = new Random();
            idleTimer = 0;
            facingRight = true;
        }
        public override void Draw(Graphics g)
        {
            // Vẽ body
            g.FillRectangle(Brushes.Red, X, Y, Width, Height);
            g.DrawRectangle(Pens.DarkRed, X, Y, Width, Height);

            // Vẽ mắt theo hướng
            if (facingRight)
            {
                g.FillEllipse(Brushes.Yellow, X + 20, Y + 15, 8, 8);
            }
            else
            {
                g.FillEllipse(Brushes.Yellow, X + 7, Y + 15, 8, 8);
            }
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
                isDead = true;
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
