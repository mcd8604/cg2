using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RayTracerXNA
{
    class MaterialCheckered : Material
    {

        private Vector4 red = Color.Red.ToVector4();
        private Vector4 yellow = Color.Yellow.ToVector4();

        public override Vector4 calculateAmbient(Vector4 ambientLight, float u, float v)
        {
            //TODO
            if (u % 1 < 0.5)
            {
                if (v % 1 < 0.5)
                {
                    //red
                    return ambient * ambientColor * red * ambientLight;
                }
                else
                {
                    //yellow
                    return ambient * ambientColor * yellow * ambientLight;
                }
            }
            else
            {
                if (v % 1 < 0.5)
                {
                    //yellow
                    return ambient * yellow * ambientLight;
                }
                else
                {
                    //red
                    return ambient * red * ambientLight;
                }
            }
        }
    }
}
