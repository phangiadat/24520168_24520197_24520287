using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _24520168_24520197_24520287
{
    public abstract class GameObject
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float VelocityX { get; set; }
        public float VelocityY { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public bool IsOnGround { get; set; }
        public abstract void Update(List<Platform> platforms);
        public abstract void Draw(Graphics g);
        public RectangleF GetBounds()
        {
            return new RectangleF(X, Y, Width, Height);
        }
        public bool checkCollision(GameObject other)
        {
            return GetBounds().IntersectsWith(other.GetBounds());
        }
    }
}
