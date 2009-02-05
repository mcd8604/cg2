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
using RayTracer;

#if DEBUG
using System.Diagnostics;
#endif

namespace RayTracerXNA
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

#if DEBUG
        double fps;
        int frameCount;
        const double SAMPLE_TIME_FRAME = 1f;
        double sampleTime;
        SpriteFont font;

        double rayTime;
        Stopwatch sw = new Stopwatch();
#endif
        RayTraceable floor;
        RayTraceable sphere1;
        RayTraceable sphere2;

        RayTracer.RTManager rayTracer;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            rayTracer = new RayTracer.RTManager(this);

            rayTracer.NearPlaneDistance = 0.1f;
            rayTracer.FarPlaneDistance = 100.0f;

            rayTracer.CameraPosition = new Vector3(3f, 4f, 15f);
            rayTracer.CameraTarget = new Vector3(3f, 0f, -70f);

            rayTracer.RecursionDepth = 5;

            rayTracer.BackgroundColor = Color.CornflowerBlue.ToVector4();

            Components.Add(rayTracer);
            Components.Add(new ScreenCapture(this));
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
            InitializeLighting();

            base.Initialize();
        }

        private void InitializeWorld()
        {
            floor = new Quad(new Vector3(8, 0, 16), new Vector3(-8, 0, -16), new Vector3(8, 0, -16), new Vector3(-8, 0, -16));
            Material floorMat = new MaterialCheckered();
            //Material floorMat = new MaterialCircleGradient(.5f, Color.White.ToVector4(), Color.Green.ToVector4());
            //Material floorMat = new MaterialBitmap((System.Drawing.Bitmap)System.Drawing.Bitmap.FromFile(@"mtgcard.jpg"));
            floorMat.AmbientStrength = 1f;
            floorMat.DiffuseStrength = 1f;
            floor.Material1 = floorMat;
            floor.MaxU = 10;
            floor.MaxV = 15;
            rayTracer.WorldObjects.Add(floor);

            sphere1 = new Sphere(new Vector3(3f, 4f, 11f), 1f);
            Material glass = new Material();
            glass.AmbientStrength = 0.075f;
            glass.DiffuseStrength = 0.075f;
            glass.SpecularStrength = 0.2f;
            //s1Mat.AmbientStrength = 1f;
            //s1Mat.DiffuseStrength = 1f;
            //s1Mat.SpecularStrength = 1f;
            glass.Exponent = 20;
            glass.setAmbientColor(new Vector4(1f, 1f, 1f, 1f));
            glass.setDiffuseColor(new Vector4(1f, 1f, 1f, 1f));
            glass.setSpecularColor(Vector4.One);
            glass.ReflectionCoef = .01f;
            glass.Transparency = .95f;
            glass.RefractionIndex = .99f;
            sphere1.Material1 = glass;
            rayTracer.WorldObjects.Add(sphere1);

            sphere2 = new Sphere(new Vector3(1.5f, 3f, 9f), 1f);
            Material mirror = new Material();
            mirror.AmbientStrength = 0.15f;
            mirror.DiffuseStrength = 0.25f;
            //s2Mat.AmbientStrength = 1f;
            //s2Mat.DiffuseStrength = 1f;
            mirror.SpecularStrength = 1f;
            mirror.Exponent = 20;
            mirror.setAmbientColor(new Vector4(0.7f, 0.7f, 0.7f, 1f));
            mirror.setDiffuseColor(new Vector4(0.7f, 0.7f, 0.7f, 1f));
            mirror.setSpecularColor(Vector4.One);
            mirror.ReflectionCoef = .75f;
            sphere2.Material1 = mirror;
            rayTracer.WorldObjects.Add(sphere2);
        }

        private void InitializeLighting()
        {
            Light l1 = new Light();
            l1.LightColor = new Vector4(1f, 1f, 1f, 1f);
            l1.Position = new Vector3(5f, 8f, 15f);
            rayTracer.Lights.Add(l1);

            //Light l2 = new Light();
            //l2.LightColor = new Vector4(1, 1f, 1f, 1f);
            //l2.Position = new Vector3(-5f, 8f, 15f);
            //rayTracer.Lights.Add(l2);
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
            font = Content.Load<SpriteFont>(@"font");
            InitializeWorld();
      
            GraphicsDevice.VertexDeclaration = new VertexDeclaration(GraphicsDevice, VertexPositionNormalTexture.VertexElements);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        //readonly float degree = MathHelper.ToRadians(1);
        //readonly Vector3 rotVec = Vector3.Normalize(Vector3.One);
        //float theta = 0;
        ////    theta += degree;
        ////    if (theta >= MathHelper.TwoPi) 
        ////        theta -= MathHelper.TwoPi;

        KeyboardState lastKeyState = Keyboard.GetState();

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

            // TODO: Add your update logic here
            MouseState mouseState = Mouse.GetState();
            KeyboardState curKeyState = Keyboard.GetState();

            if (lastKeyState.IsKeyUp(Keys.W) && curKeyState.IsKeyDown(Keys.W))
            {
                sphere1.Center = new Vector3(sphere1.Center.X, sphere1.Center.Y, sphere1.Center.Z - 1);
                sphere2.Center = new Vector3(sphere2.Center.X, sphere2.Center.Y, sphere2.Center.Z - 1);
            }

            lastKeyState = curKeyState;

            base.Update(gameTime);
        }



        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(new Color(rayTracer.BackgroundColor));

#if DEBUG
            sw.Reset();
            sw.Start();
#endif
            rayTracer.Draw(gameTime);
#if DEBUG
            sw.Stop();
#endif


#if DEBUG
            //sampleTime += gameTime.ElapsedGameTime.TotalSeconds;
            //if (sampleTime >= SAMPLE_TIME_FRAME)
            //{
            //    fps = sampleTime / frameCount;
            //    sampleTime = 0;
            //    frameCount = 0;
            //}

            rayTime = sw.Elapsed.TotalSeconds;
#endif

            ++frameCount;

            base.Draw(gameTime);

            spriteBatch.Begin();
            spriteBatch.DrawString(font, "Raytrace time: " + rayTime, Vector2.Zero, Color.White);
            spriteBatch.End();
        }
    }
}
