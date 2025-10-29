using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _24520168_24520197_24520287
{
    public class Platform
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public Platform(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public RectangleF GetBounds()
        {
            return new RectangleF(X, Y, Width, Height);
        }

        public void Draw(Graphics g)
        {
            g.FillRectangle(Brushes.Brown, X, Y, Width, Height);
            g.DrawRectangle(Pens.Black, X, Y, Width, Height);
        }
    }
}
