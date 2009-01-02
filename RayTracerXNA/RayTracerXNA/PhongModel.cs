using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace RayTracerXNA
{
    class PhongModel
    {
        protected List<Light> lights = new List<Light>();
        public List<Light> Lights
        {
            get { return lights; }
            set { lights = value; }
        }        

        protected Vector4 ambientLight;
        public Vector4 AmbientLight
        {
            get { return ambientLight; }
            set { ambientLight = value; }
        }

        protected Vector3 intersection;
        public Vector3 Intersection
        {
            get { return intersection; }
            set { intersection = value; }
        }
        
        #region ILightingModel Members

        public Color CalculateLighting(Primitive p, Vector3 intersection)
        {
            Vector4 diffuseTotal = Vector4.Zero;
            Vector4 specularTotal = Vector4.Zero;
            foreach (Light light in lights)
            {
                diffuseTotal += calculateDiffuse(p, intersection, light);
                specularTotal += calculateSpecular();
            }

            Vector4 colorVector = calculateAmbient(p) + 
                Vector4.Multiply(diffuseTotal, p.DiffuseStrength) +
                Vector4.Multiply(specularTotal, p.SpecularStrength);

            return new Color(colorVector);
        }

        private Vector4 calculateAmbient(Primitive p)
        {
            return p.AmbientStrength * p.MaterialColor * ambientLight;
        }

        private Vector4 calculateDiffuse(Primitive p, Vector3 intersection, Light l)
        {
            Vector4 diffuseColor = l.LightColor * p.MaterialColor;
            Vector3 lightVector = Vector3.Normalize(intersection - l.Position);
            float diffuseAmount = Vector3.Dot(lightVector, p.GetIntersectNormal(intersection));
            return Vector4.Multiply(diffuseColor, diffuseAmount);
        }

        private Vector4 calculateSpecular()
        {
            return Vector4.Zero;
        }

        #endregion
    }
}
