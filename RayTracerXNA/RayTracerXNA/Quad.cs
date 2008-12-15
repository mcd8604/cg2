using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace RayTracerXNA
{
    class Square : Primitive
    {
        private Plane plane;

        protected BoundingBox boundingBox;
        public BoundingBox MyBoundingBox
        {
            get { return boundingBox; }
            set { boundingBox = value; }
        }

        public override Vector3 Center
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public Square(Vector3 pt1, Vector3 pt2, Vector3 pt3, Vector3 pt4)
        {
            plane = new Plane(pt1, pt2, pt3);
            List<Vector3> points = new List<Vector3>();
            points.Add(pt1);
            points.Add(pt2);
            points.Add(pt3);
            points.Add(pt4);

            boundingBox = BoundingBox.CreateFromPoints(points);
        }

        public override float? Intersects(Ray ray)
        {
            if (ray.Intersects(plane) != null)
                return ray.Intersects(boundingBox);

            return null;
        }

        public override Vector3 GetIntersectNormal(Vector3 intersectPoint)
        {
            return plane.Normal;
        }
    }
}
