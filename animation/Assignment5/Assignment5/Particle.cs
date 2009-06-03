using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Assignment5
{
    class Particle
    {
        protected Vector3 position;
        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        protected Vector3 velocity;
        public Vector3 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }

        protected Vector3 accel;
        protected Vector3 netForce;

        protected float mass = 1f;
        public float Mass
        {
            get { return mass; }
            set { mass = value; }
        }

        protected float size;
        public float Size
        {
            get { return size; }
            set { size = value; }
        }

        protected Color color;
        public Color ParticleColor
        {
            get { return color; }
            set { color = value; }
        }

        protected Vector4 initialColor;
        public Vector4 InitialColor
        {
            get { return initialColor; }
            set { initialColor = value; }
        }

        protected Vector4 finalColor;
        public Vector4 FinalColor
        {
            get { return finalColor; }
            set { finalColor = value; }
        }

        protected double initialLifetime;
        protected double lifetime;
        public double Lifetime
        {
            get { return lifetime; }
            set { lifetime = value; }
        }

        private bool active = false;
        public bool Active
        {
            get { return active; }
            set { active = value; }
        }

        public void Spawn(Vector3 initialPosition, Vector3 initialForce, float initialSize, Color initialColor, Color finalColor, double lifetime)
        {
            this.position = initialPosition;
            this.netForce = initialForce;
            this.accel = initialForce / mass;
            this.velocity = Vector3.Zero;
            this.size = initialSize;
            this.color = initialColor;
            this.initialColor = initialColor.ToVector4();
            this.finalColor = finalColor.ToVector4();
            this.lifetime = this.initialLifetime = lifetime;
            this.active = true;
        }

        internal void Update(GameTime gameTime)
        {
            float t = (float)gameTime.ElapsedGameTime.TotalSeconds;
            lifetime -= t;
            if (lifetime <= 0)
            {
                active = false;
            }
            else
            {
                //byte i = (byte)((lifetime / initialLifetime) * 256);
                //byte f = (byte)(256 - f);
                float i = (float)(lifetime / initialLifetime);
                float f = 1 - i;
                this.color.R = (byte)(((initialColor.X * i) + (finalColor.X * f)) * 255);
                this.color.G = (byte)(((initialColor.Y * i) + (finalColor.Y * f)) * 255);
                this.color.B = (byte)(((initialColor.Z * i) + (finalColor.Z * f)) * 255);
                this.color.A = (byte)(((initialColor.W * i) + (finalColor.W * f)) * 255);

                this.accel = netForce / mass;

                // integrate 
                this.velocity += this.accel * t;
                this.position += this.velocity * t;
            }
        }
    }
}
