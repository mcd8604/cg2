using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Assignment5
{
    class Emitter : KeyFrameable
    {
        private double emissionTime = 0;
        private double generationRate = 0;
        /// <summary>
        /// Emissions per second
        /// </summary>
        private double GenerationRate 
        {
            get { return generationRate; }
            set
            {
                generationRate = value;
                this.emissionTime = 1 / generationRate;
            }
        }

        private double curTime = 0;

        private int maxParticles;
        public int MaxParticles
        {
            get { return maxParticles; }
            set { maxParticles = value; }
        }

        private double maxParticleLifetime = 1f;
        public double MaxParticleLifetime
        {
            get { return maxParticleLifetime; }
            set { maxParticleLifetime = value; }
        }

        private float maxForce = 1f;
        public float MaxForce
        {
            get { return maxForce; }
            set { maxForce = value; }
        }

        private List<Particle> particles;
        public List<Particle> Particles
        {
            get { return particles; }
        }

        private bool active = false;
        public bool Active
        {
            get { return active; }
            set { active = value; }
        }

        private Color initialColor = Color.Azure;
        public Color InitialColor
        {
            get { return initialColor; }
            set { initialColor = value; }
        }

        private Color finalColor = new Color(0f, 0f, 1f, 0f);
        public Color FinalColor
        {
            get { return finalColor; }
            set { finalColor = value; }
        }

        private float radius = 0;
        public float Radius
        {
            get { return radius; }
            set { radius = value; }
        }

        private Vector3 spawnDirection = Vector3.Zero;
        /// <summary>
        /// Direction, as unit vector, to spawn particles. Default is Vector3.Zero.
        /// </summary>
        public Vector3 SpawnDirection
        {
            get { return spawnDirection; }
            set
            {
                spawnDirection = value; 
                thetaX = Math.Atan2(spawnDirection.Y, spawnDirection.X);
                thetaZ = Math.Atan2(spawnDirection.Z, spawnDirection.X);
            }
        }

        private double thetaX = 0;
        private double thetaZ = 0;

        private double spawnAngle = MathHelper.Pi;
        /// <summary>
        /// Angle from SpawnDirection, in radians, to limit particle spawning. Default is Pi.
        /// </summary>
        public double SpawnAngle
        {
            get { return spawnAngle; }
            set { spawnAngle = value; }
        }

        /// <summary>
        /// Index of next particle to spawn.
        /// </summary>
        private int spawnIndex = 0;

        /// <summary>
        /// Creates a new emitter.
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="generationRate">Emissions per second</param>
        /// <param name="maxParticles">Maximum number of particles to hold.</param>
        public Emitter(Game game, double generationRate, int maxParticles)
           : base(game) 
        {
            this.generationRate = generationRate;
            this.emissionTime = 1 / generationRate;
            this.maxParticles = maxParticles;
        }

        public override void Initialize()
        {
            particles = new List<Particle>(maxParticles);
            for (int i = 0; i < maxParticles; ++i)
            {
                particles.Add(new Particle());
            }

            active = true;

            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            curTime += gameTime.ElapsedGameTime.TotalSeconds;

            if (curTime > emissionTime)
            {
                for(int i = 0; i < curTime / emissionTime; ++i)
                    emit();

                curTime = 0;
            }

            List<Particle> toRemove = new List<Particle>();

            foreach (Particle p in particles)
            {
                p.Update(gameTime);
            }

            foreach (Particle p in toRemove)
            {
                particles.Remove(p);
            }

            base.Update(gameTime);
        }

        private static Random rand = new Random();

        private void emit()
        {
            if (++spawnIndex >= maxParticles)
                spawnIndex = 0;

            Vector3 offset = Vector3.Normalize(new Vector3((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble())) * radius;

            particles[spawnIndex].Spawn(
                this.position + offset,
                getRandomDirection() * (float)(rand.NextDouble() * maxForce),
                (float)rand.NextDouble(),
                initialColor,
                finalColor,
                rand.NextDouble() * maxParticleLifetime
            );
        }

        private Vector3 getRandomDirection()
        {
            if (spawnDirection == Vector3.Zero)
                return Vector3.Zero;

            double dTheta = (rand.NextDouble() - 0.5) * spawnAngle * 2;

            double thetaX2 = thetaX + dTheta;
            double thetaZ2 = thetaZ + dTheta;

            return Vector3.Normalize(new Vector3((float)Math.Cos(thetaX2), (float)Math.Sin(thetaX2), (float)Math.Sin(thetaZ2)));
        }
    }
}
