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
        }

        public void Update()
        {
            X += VelocityX;
            Y += VelocityY;
        }

        public void Draw(Graphics g)
        {
            var rect = new RectangleF(X - Radius, Y - Radius, Radius * 2, Radius * 2);
            g.FillEllipse(Brushes.Orange, rect);
            g.DrawEllipse(Pens.DarkOrange, rect);
        }

        public RectangleF GetBounds()
        {
            return new RectangleF(X - Radius, Y - Radius, Radius * 2, Radius * 2);
        }
    }
}