using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Microsoft.Xna.Framework;

namespace RayTracerXNA
{
    abstract class Primitive
    {

#region Phong parameters
    
        protected float ambient;
        public float AmbientStrength
        {
            get { return ambient; }
            set { ambient = value; }
        }

        protected float diffuseStrength;
        public float DiffuseStrength
        {
            get { return diffuseStrength; }
            set { diffuseStrength = value; }
        }

        protected float specularStrength;
        public float SpecularStrength
        {
            get { return specularStrength; }
            set { specularStrength = value; }
        }

        protected double exponent;
        public double Exponent
        {
            get { return exponent; }
            set { exponent = value; }
        }
        
#endregion

        protected Vector4 materialColor = Vector4.Zero;
        public Vector4 MaterialColor
        {
            get { return materialColor; }
            set { materialColor = value; }
        }

        protected Vector4 specularColor = Vector4.One;
        public Vector4 SpecularColor
        {
            get { return specularColor; }
            set { specularColor = value; }
        }

        public abstract Vector3 Center { get; set; }

        public abstract float? Intersects(Ray ray);

        public abstract Vector3 GetIntersectNormal(Vector3 intersectPoint);

    }
}
