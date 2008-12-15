using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Microsoft.Xna.Framework;

namespace RayTracerXNA
{
    abstract class Primitive
    {
        protected Color color;
        public Color MyColor
        {
            get { return color; }
            set { color = value; }
        }

        public abstract Vector3 Center { get; set; }

        public abstract float? Intersects(Ray ray);

        public abstract Vector3 GetIntersectNormal(Vector3 intersectPoint);

    }
}
