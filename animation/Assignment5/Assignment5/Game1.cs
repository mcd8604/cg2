using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace Assignment5
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Vector3 camPos = new Vector3(50f, 50f, 200f);
        Vector3 camTarget = new Vector3(50f, 50f, 0f);
        float nearPlane = 0.1f;
        float farPlane = 250f;

        Matrix world;
        Matrix view;
        Matrix projection;

        Emitter emitter;
        List<VertexPositionColor> vertices;
        VertexDeclaration vertDecl;

        Effect effect;
        Model model;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            emitter = new Emitter(this, 20000, 50000);
            emitter.Radius = 1f;

            // cone parameters
            emitter.SpawnDirection = Vector3.Normalize(new Vector3(-1, -1, 1));
            emitter.SpawnAngle = MathHelper.Pi / 8;
            
            // particle parameters
            emitter.MaxForce = 300f;
            emitter.MaxParticleLifetime = 1;

            emitter.KeyFrames = Util.LoadKeyFrames("keyframe-input.txt");

            Components.Add(emitter);

            vertices = new List<VertexPositionColor>();

            InitializeMatrices();

            InitializeEffect();

            base.Initialize();
        }

        private void InitializeEffect()
        {
            //effect = new BasicEffect(GraphicsDevice, new EffectPool());

            //effect.World = world;
            //effect.View = view;
            //effect.Projection = projection;

            ////effect.EnableDefaultLighting();
            //effect.VertexColorEnabled = true;

            effect = Content.Load<Effect>("Simple");

            effect.Parameters["World"].SetValue(world);
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);
        }

        private void InitializeMatrices()
        {
            world = Matrix.Identity;
            view = Matrix.CreateLookAt(camPos, camTarget, Vector3.Up);
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, nearPlane, farPlane);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            vertDecl = new VertexDeclaration(GraphicsDevice, VertexPositionColor.VertexElements);
            GraphicsDevice.VertexDeclaration = vertDecl;

            model = Content.Load<Model>("sphere");
            model.Meshes[0].MeshParts[0].Effect = effect;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            else if (Keyboard.GetState().IsKeyDown(Keys.Space))
                emitter.Reset();

            // TODO: Add your update logic here
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here

            if (emitter.Active)
            {
                vertices.Clear();
                foreach (Particle p in emitter.Particles)
                {
                    if (p.Active)
                    {
                        vertices.Add(new VertexPositionColor(p.Position, p.ParticleColor));
                    }
                }
                //effect.World = Matrix.CreateTranslation(p.Position);
                //effect.DiffuseColor = p.ParticleColor.ToVector3();
                //effect.Alpha = p.Alpha;
                //model.Meshes[0].Draw();
                if (vertices.Count > 0)
                {
                    effect.Begin();
                    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Begin();
                        GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.PointList, vertices.ToArray(), 0, vertices.Count);
                        pass.End();
                    }
                    effect.End();
                }
            }
            base.Draw(gameTime);
        }
    }
}
