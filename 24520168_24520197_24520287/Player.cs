using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Media;

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

        //Hướng quay mặt và bắn
        private int facing = 1; //1: phải, -1: trái/
        private float fireCooldown = 0f;
        private const float FireRate = 0.35f;
        //Âm thanh player hurt
        private static SoundPlayer hitPlayer;
        //Máu
        public int MaxHealth { get; private set; }
        public int Health { get; private set; }

        public event EventHandler PlayerDied;

        //Khởi tạo âm thanh player hurt
        static Player()
        {
            try
            {
                hitPlayer = new SoundPlayer(Application.StartupPath + "\\resources\\Player Hurt.wav");
                hitPlayer.Load(); //tải âm thanh khi nhận sát thương
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error loading player sound: " + ex.Message);
                hitPlayer = null;
            }
        }

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

            MaxHealth = 100;
            Health = MaxHealth;
        }
        public void SetKeyState(Keys key, bool isPressed)
        {
            if (keyStates.ContainsKey(key))
            {
                keyStates[key] = isPressed;
            }
        }

        public void TakeDamage(int amount)
        {
            Health -= amount;
            if(hitPlayer != null && !Form1.SfxMuted) // âm thanh player bị thương
            {
                hitPlayer.Play();
            }

            if (Health < 0)
            {
                Health = 0;
                OnPlayerDied();
            }
        }

        protected virtual void OnPlayerDied()
        {
            PlayerDied?.Invoke(this, EventArgs.Empty);
        }

        public Projectile Fire()
        {
            if (fireCooldown > 0) return null;
            float px = X + Width / 2f;
            float py = Y + Height / 2f;
            float speed = 10f; 
            float vx = speed * facing;
            float vy = 0;
            int damage = 20;
            var proj = new Projectile(px, py, vx, vy, 5f, damage, this);

            fireCooldown = FireRate;
            return proj;
        }

        public override void Draw(Graphics g)
        {
            g.FillRectangle(Brushes.Blue, X, Y, Width, Height);
            g.DrawRectangle(Pens.Black, X, Y, Width, Height);

            g.FillEllipse(Brushes.White, X + 10, Y + 15, 8, 8); // Mắt trái
            g.FillEllipse(Brushes.White, X + 22, Y + 15, 8, 8); // Mắt phải

            //Vẽ thanh máu cho player nha
            float barW = Width;
            float barH = 6;
            float barX = X;
            float barY = Y - barH - 2;
            g.FillRectangle(Brushes.Red, barX, barY, barW, barH);
            float ratio = (MaxHealth > 0) ? (float)Health / MaxHealth : 0;
            g.FillRectangle(Brushes.LimeGreen, barX, barY, barW * ratio, barH);
            g.DrawRectangle(Pens.Black, barX, barY, barW, barH);
        }

        public override void Update(List<Platform> platforms)
        {
            if (fireCooldown > 0f)
                fireCooldown -= 0.016f;

            VelocityX = 0;
            if(keyStates[Keys.Left])
            {
                VelocityX = -MoveSpeed;
                facing = -1;
            }
            if(keyStates[Keys.Right])
            {
                VelocityX = MoveSpeed;
                facing = 1;
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
