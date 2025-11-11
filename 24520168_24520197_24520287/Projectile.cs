using System;
using System.Drawing;

namespace _24520168_24520197_24520287
{
    public class Projectile
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float VelocityX { get; set; }
        public float VelocityY { get; set; }
        public float Radius { get; set; }
        public int Damage { get; set; }
        public object Owner { get; set; } // can be Player or Enemy
        public bool IsDead { get; set; }

        private static Image arrowImage;
        private static int arrowWidth = 16;  
        private static int arrowHeight = 8;

        private float lifetime;
        private const float DeltaTime = 0.016f;

        static Projectile()
        {
            try
            {
                arrowImage = new Bitmap("Assets\\Player\\arrow.png");
                if (arrowImage != null)
                {
                    arrowWidth = arrowImage.Width;
                    arrowHeight = arrowImage.Height;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Không thể tải ảnh mũi tên: {ex.Message}");
                arrowImage = null;
            }
        }

        public Projectile(float x, float y, float vx, float vy, float radius, int damage, object owner)
        {
            X = x;
            Y = y;
            VelocityX = vx;
            VelocityY = vy;
            Radius = radius;
            Damage = damage;
            Owner = owner;
            IsDead = false;

            lifetime = 2;
        }

        public void Update()
        {
            X += VelocityX;
            Y += VelocityY;
            lifetime -= DeltaTime;
            if (lifetime <= 0f)
            {
                IsDead = true;
            }
        }

        public void Draw(Graphics g)
        {

            var drawWidth = arrowWidth;
            var drawHeight = arrowHeight;

            var rect = new RectangleF(X - drawWidth / 2f, Y - drawHeight / 2f, drawWidth, drawHeight);

            if (arrowImage != null)
            {
                // Nếu ảnh được tải thành công
                if (VelocityX > 0) // Đạn bay sang phải
                {
                    g.DrawImage(arrowImage, rect);
                }
                else // Đạn bay sang trái (lật ảnh)
                {
                    // Tạo một hình chữ nhật lật để vẽ
                    var flippedRect = new RectangleF(rect.X + rect.Width, rect.Y, -rect.Width, rect.Height);
                    g.DrawImage(arrowImage, flippedRect);
                }
            }
            else
            {
                // Dự phòng: Nếu không tải được ảnh, vẽ hình tròn cam
                g.FillEllipse(Brushes.Orange, rect);
                g.DrawEllipse(Pens.DarkOrange, rect);
            }
        }

        public RectangleF GetBounds()
        {
            return new RectangleF(X - Radius, Y - Radius, Radius * 2, Radius * 2);
        }

        

    }
}