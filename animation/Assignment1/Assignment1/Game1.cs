using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace Assignment1
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

        ModelContainer cube;

        BasicEffect effect;

        double MAX_TIME = 20;
        double animationTime;

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

            base.Initialize();
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
            world = Matrix.Identity;
            view = Matrix.CreateLookAt(camPos, camTarget, Vector3.Up);
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, nearPlane, farPlane);

            initEffect();

            cube = new ModelContainer(Content.Load<Model>(@"cube"));
            cube.Scale = 10;
            cube.SetEffect(effect);

        }

        private void initEffect()
        {
            effect = new BasicEffect(GraphicsDevice, new EffectPool());

            effect.World = world;
            effect.View = view;
            effect.Projection = projection;

            effect.EnableDefaultLighting();
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
                animationTime = 0;

            // TODO: Add your update logic here

            if(animationTime < MAX_TIME) {
                // X position = 5t (t is time in sec)
                // Y position = 5t (t is time in sec)
                // Z position = constant
                // Rotation around Y axis = 2t (t is time in sec)
                // Rotation around X and Z axis = 0.
                cube.Position = new Vector3(5 * (float)animationTime, 5 * (float)animationTime, 0);
                cube.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, MathHelper.ToRadians((2 * (float)animationTime)));
                animationTime += gameTime.ElapsedGameTime.TotalSeconds;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            effect.World = cube.Transform;
            cube.Draw();
            effect.World = world;

            base.Draw(gameTime);
        }
    }
}
