using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace RayTracer
{
    /// <summary>
    /// A point light source that has position and color.
    /// </summary>
    public struct Light
    {
        public Vector4 LightColor;
        public Vector3 Position;
    }
}
