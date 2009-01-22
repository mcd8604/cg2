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
#endif
        Primitive floor;
        Primitive sphere1;
        Primitive sphere2;

        RayTracer.RayTracer rayTracer;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            rayTracer = new RayTracer.RayTracer(this);

            rayTracer.NearPlaneDistance = 0.1f;
            rayTracer.FarPlaneDistance = 50.0f;

            rayTracer.CameraPosition = new Vector3(3f, 4f, 15f);
            rayTracer.CameraTarget = new Vector3(3f, 0f, -70f);

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
            floor = new Square(new Vector3(8, 0, 8), new Vector3(-8, 0, -16), new Vector3(8, 0, -16), new Vector3(-8, 0, -16));
            //Material floorMat = new MaterialCircleGradient(.5f, Color.White.ToVector4(), Color.Green.ToVector4());
            Material floorMat = new MaterialBitmap((System.Drawing.Bitmap)System.Drawing.Bitmap.FromFile(@"mtgcard.jpg"));
            floorMat.AmbientStrength = 1f;
            floorMat.DiffuseStrength = 1f;
            floor.Material1 = floorMat;
            floor.MaxU = 3;
            floor.MaxV = 3;
            rayTracer.Primitives.Add(floor);

            sphere1 = new Sphere(new Vector3(3f, 4f, 11f), 1f);
            Material s1Mat = new Material();
            s1Mat.AmbientStrength = 1f;
            s1Mat.DiffuseStrength = 1f;
            s1Mat.SpecularStrength = 1f;
            s1Mat.Exponent = 16;
            s1Mat.setAmbientColor(new Vector4(1f, 0f, 0f, 1f));
            s1Mat.setDiffuseColor(new Vector4(1f, 0f, 0f, 1f));
            s1Mat.setSpecularColor(Vector4.One);
            sphere1.Material1 = s1Mat;
            rayTracer.Primitives.Add(sphere1);

            sphere2 = new Sphere(new Vector3(1.5f, 3f, 9f), 1f);
            Material s2Mat = new Material();
            s2Mat.AmbientStrength = 1f;
            s2Mat.DiffuseStrength = 1f;
            s2Mat.SpecularStrength = 1f;
            s2Mat.Exponent = 16;
            s2Mat.setAmbientColor(new Vector4(0f, 0f, 1f, 1f));
            s2Mat.setDiffuseColor(new Vector4(0f, 0f, 1f, 1f));
            s2Mat.setSpecularColor(Vector4.One);
            sphere2.Material1 = s2Mat;
            rayTracer.Primitives.Add(sphere2);
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

        readonly float degree = MathHelper.ToRadians(1);
        readonly Vector3 rotVec = Vector3.Normalize(Vector3.One);
        float theta = 0;

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
            //sphere1.Rotation = Quaternion.CreateFromAxisAngle(rotVec, theta);
            theta += degree;
            if (theta >= MathHelper.TwoPi) 
                theta -= MathHelper.TwoPi;

            KeyboardState curKeyState = Keyboard.GetState();

#if DEBUG
            sampleTime += gameTime.ElapsedGameTime.TotalSeconds;
            if (sampleTime >= SAMPLE_TIME_FRAME)
            {
                fps = sampleTime / frameCount;
                sampleTime = 0;
                frameCount = 0;
            }
#endif

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            rayTracer.Draw(gameTime);

            spriteBatch.Begin();
            spriteBatch.DrawString(font, "FPS: " + fps, Vector2.Zero, Color.White);
            spriteBatch.End();

            ++frameCount;

            base.Draw(gameTime);
        }
    }
}
