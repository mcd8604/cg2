using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RayTracerXNA
{
    class Material
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

        protected Vector4 ambientColor = Vector4.Zero;
        public Vector4 AmbientColor
        {
            get { return ambientColor; }
            set { ambientColor = value; }
        }

        protected Vector4 diffuseColor = Vector4.Zero;
        public Vector4 DiffuseColor
        {
            get { return diffuseColor; }
            set { diffuseColor = value; }
        }

        protected Vector4 specularColor = Vector4.One;
        public Vector4 SpecularColor
        {
            get { return specularColor; }
            set { specularColor = value; }
        }

        public virtual Vector4 calculateAmbient(Vector4 ambientLight)
        {
            return ambient * ambientColor * ambientLight;
        }        

        public virtual Vector4 calculateAmbient(Vector4 ambientLight, float u, float v)
        {
            return ambient * ambientColor * ambientLight;
        }

        public Vector4 calculateDiffuse(Vector3 intersection, Vector3 normal, Light l, Vector3 lightVector)
        {
            Vector4 diffuse = l.LightColor * diffuseColor;

            float diffuseAmount = Math.Abs(Vector3.Dot(lightVector, normal));

            return Vector4.Multiply(diffuse, diffuseAmount);
        }

        public Vector4 calculateSpecular(Vector3 intersection, Vector3 normal, Light l, Vector3 lightVector, Vector3 viewVector)
        {
            Vector4 specular = l.LightColor * specularColor;

            Vector3 reflectedVector = Vector3.Reflect(lightVector, normal);

            double dot = (double)Vector3.Dot(reflectedVector, viewVector);

            if (dot >= 0)
                return Vector4.Zero;

            float specularAmount = (float)Math.Pow(dot, exponent);

            return Vector4.Multiply(specular, specularAmount);
        }
    }
}
