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

        protected Vector3 cameraPos;
        public Vector3 CameraPosition
        {
            get { return cameraPos; }
            set { cameraPos = value; }
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

        #endregion
    }
}
