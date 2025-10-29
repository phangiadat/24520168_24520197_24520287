using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace _24520168_24520197_24520287
{
    public class Player : GameObject
    {
        private const float Gravity = 0.5f; //Trọng lực tác động lên nhân vật
        private const float MoveSpeed = 5f; //Tốc độ di chuyển ngang
        private const float JumpForce = -12f; //Lực nhảy của nhân vật
        private const float MaxFallSpeed = 15f; //Tốc độ rơi tối đa

        private Dictionary<Keys, bool> keyStates; //Trạng thái phím bấm
        private bool canJump; //Kiểm tra xem nhân vật có thể nhảy hay không

        public Player(float x, float y)
        {
            X = x;
            Y = y;
            Width = 40;
            Height = 60;
            VelocityX = 0;
            VelocityY = 0;
            IsOnGround = false;
            keyStates = new Dictionary<Keys, bool>
            {
                { Keys.Left, false },//Phím di chuyển ngang
                { Keys.Right, false },//Phím di chuyển ngang
                { Keys.Up, false }, //Phím nhảy
            };
        }
        public void SetKeyState(Keys key, bool isPressed)
        {
            if (keyStates.ContainsKey(key))
            {
                keyStates[key] = isPressed;
            }
        }


        public override void Draw(Graphics g)
        {
            g.FillRectangle(Brushes.Blue, X, Y, Width, Height);
            g.DrawRectangle(Pens.Black, X, Y, Width, Height);

            g.FillEllipse(Brushes.White, X + 10, Y + 15, 8, 8); // Mắt trái
            g.FillEllipse(Brushes.White, X + 22, Y + 15, 8, 8); // Mắt phải
        }

        public override void Update(List<Platform> platforms)
        {
            VelocityX = 0;
            if(keyStates[Keys.Left])
            {
                VelocityX = -MoveSpeed;
            }
            if(keyStates[Keys.Right])
            {
                VelocityX = MoveSpeed;
            }
            if(keyStates[Keys.Up] && IsOnGround && canJump)
            {
                VelocityY = JumpForce;
                IsOnGround = false;
                canJump = false;
            }
            if(!keyStates[Keys.Up])
            {
                canJump = true;
            }
            VelocityY += Gravity;
            if(VelocityY > MaxFallSpeed)
            {
                VelocityY = MaxFallSpeed;
            }
            X += VelocityX;
            HandleHorizontalCollisions(platforms);
            Y += VelocityY;
            HandleVerticalCollisions(platforms);
        }
        private void HandleHorizontalCollisions(List<Platform> platforms)
        {
            RectangleF playerBounds = GetBounds();
            foreach (var platform in platforms)
            {
                if(playerBounds.IntersectsWith(platform.GetBounds()))
                {
                    if(VelocityX > 0)
                    {
                        X = platform.X - Width;
                    }
                    else if(VelocityX < 0)
                    {
                        X = platform.X + platform.Width;
                    }
                    VelocityX = 0;
                }
            }
        }
        private void HandleVerticalCollisions(List<Platform> platforms)
        {
            RectangleF playerBounds = GetBounds();
            IsOnGround = false;
            foreach(var platform in platforms)
            {
                if(playerBounds.IntersectsWith(platform.GetBounds()))
                {
                    if(VelocityY > 0) //Đang rơi xuống
                    {
                        Y = platform.Y - Height;
                        IsOnGround = true;
                    }
                    else if(VelocityY < 0) //Đang nhảy lên
                    {
                        Y = platform.Y + platform.Height;
                    }
                    VelocityY = 0;
                }
            }
        }
    }
}
