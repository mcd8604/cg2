using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BilliardsSimulation
{
    class Ball
    {
        // mass (kg)
        private const float m = 0.5f;
        private const float radius = 0.04f;
        public float Radius
        {
            get { return radius; }
        }

        // position (meters)
        private Vector3 s = Vector3.Zero;
        public Vector3 Position
        {
            get { return s; }
            set { s = value; }
        }

        // accel gravity (m/s^2)
        private const float g = 9.8f;
        // normal force
        private readonly float n = m * g;
        
        // ball-cloth coefficient of rolling resistance (m): 0.01
        private const float rMu = 0.01f;
        // rolling friction
        private readonly float rF;

        // ball-cloth coefficient of sliding friction (m): 0.2
        private const float sMu = 0.2f;
        // sliding friction
        private readonly float sF;

        // velocity (m/s)
        private Vector3 v = Vector3.Zero;
        public Vector3 Velocity
        {
            get { return v; }
            set { v = value; }
        }

        // momentum
        private Vector3 p = Vector3.Zero;

        // net force
        private Vector3 f = Vector3.Zero;
        public Vector3 Force
        {
            get { return f; }
            set { f = value; }
        }

        public Ball()
        {
            rF = n * rMu;
            sF = n * sMu;
        }

        public void UpdateVelocity()
        {
            v = (p / m); // +jV;
        }

        public void UpdateMomentum(float dT)
        {
            p = f * dT;
        }

        public void UpdatePosition(float dT)
        {
            s += v * dT;
        }

        public void ApplyFriction()
        {
            Vector3 fric = Vector3.Negate(f);
            fric.Normalize();
            fric *= rF;
            if (fric.Length() <= f.Length())
            {
                f += fric;
            }
            else
            {
                f = Vector3.Zero;
            }
        }

        public void ApplyForce(Vector3 force)
        {
            this.f += force;
        }

        public void Collide(Vector3 normal)
        {
            f = Vector3.Reflect(f, normal);
            p = Vector3.Reflect(p, normal);
        }

        /// <summary>
        /// Detects a collision between this ball and another.
        /// </summary>
        /// <param name="other"></param>
        /// <param name="perc"></param>
        /// <param name="dT"></param>
        public void CheckCollision(Ball other, float dT)
        {
            // net velocity of two moving balls
            Vector3 netV = (s - other.Velocity) * dT;

            // distance between balls at current positions
            float dist = Vector3.Distance(s, other.Position);

            // sum of the radii
            float radSum = radius + other.Radius;

            // if collision isn't possible, break
            if (netV.Length() < dist - radSum) return;

            Vector3 N = netV;
            N.Normalize();

            // position difference vector
            Vector3 C = other.Position - s;

            double D = Vector3.Dot(N, C);

            // if not moving towards each other, break
            if (D <= 0) return;

            double F = C.LengthSquared() - (D * D);

            // If the balls never come close enough, break
            double sumRadiiSquared = radSum * radSum;
            if (F >= sumRadiiSquared) return;

            double T = sumRadiiSquared - F;
            if (T < 0) return;

            // find interpolated distance
            double distance = D - Math.Sqrt(T);

            // if distance is greater than net velocity, break
            if (N.Length() < distance) return;

            // set N to interpolated position
            N.Normalize();
            N = N * (float)distance;

            // interpolated percentage
            float perc = N.Length() / netV.Length();

            handleCollision(other, dT, perc);
        }

        /// <summary>
        /// Handles a collision with a given ball, time slice, and interpolation percentage
        /// </summary>
        private void handleCollision(Ball other, float dT, float perc)
        {
            Vector3 v1 = v;
            if (v != Vector3.Zero)
            {
                v1.Normalize();
                v1 *= perc;
            }

            Vector3 v2 = other.Velocity;
            if (v2 != Vector3.Zero)
            {
                v2.Normalize();
                v2 *= perc;
            }

            dT *= perc;

            Vector3 nV = s - other.Position;
            nV.Normalize();

            float a1 = Vector3.Dot(v1, nV);
            float a2 = Vector3.Dot(v2, nV);

            float o = (2f * (a1 - a2)) / (2f * m);

            Vector3 myV = v1 - o * m * nV;
            Vector3 otherV = v2 + o * m * nV;

            Vector3 myP = myV * m;
            Vector3 otherP = otherV * m;

            f = myP / dT;
            other.Force = otherP / dT;
        }
    }
}
