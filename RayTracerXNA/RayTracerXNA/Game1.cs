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

namespace RayTracerXNA
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        private const float WIDTH = 800f;
        private const float HEIGHT = 600f;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private readonly VertexPositionNormalTexture[] floorVertices = {
            new VertexPositionNormalTexture(new Vector3(8, 0, 8), Vector3.Down, Vector2.Zero),
            new VertexPositionNormalTexture(new Vector3(-8, 0, 8), Vector3.Down, new Vector2(1f, 0f)),
            new VertexPositionNormalTexture(new Vector3(8, 0, -16), Vector3.Down, new Vector2(0f, 1f)),
            new VertexPositionNormalTexture(new Vector3(8, 0, -16), Vector3.Down, new Vector2(0f, 1f)),
            new VertexPositionNormalTexture(new Vector3(-8, 0, 8), Vector3.Down, new Vector2(1f, 0f)),
            new VertexPositionNormalTexture(new Vector3(-8, 0, -16), Vector3.Down, Vector2.One)};

        GeodesicIcosahedron sphere1;
        GeodesicIcosahedron sphere2;

        Matrix worldMatrix;
        Matrix viewMatrix;
        Matrix projectionMatrix;

        BasicEffect effect;

        readonly Vector3 cameraPos = new Vector3(3f, 4f, 15f);
        readonly Vector3 cameraTarget = new Vector3(3f, 0f, -70f);

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
            sphere1 = new GeodesicIcosahedron(this, 5);
            sphere2 = new GeodesicIcosahedron(this, 2);
            sphere1.Position = new Vector3(3f, 4f, 11f);
            sphere2.Position = new Vector3(1.5f, 3f, 9f);
            sphere1.Scale = 0.75f;
            sphere2.Scale = 0.75f;

            Components.Add(sphere1);
            Components.Add(sphere2);

            worldMatrix = Matrix.Identity;

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
            GraphicsDevice.VertexDeclaration = new VertexDeclaration(GraphicsDevice, VertexPositionNormalTexture.VertexElements);

            viewMatrix = Matrix.CreateLookAt(cameraPos, cameraTarget, Vector3.Up);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.1f, 50.0f);

            initEffect();
        }

        public void initEffect()
        {
            effect = new BasicEffect(GraphicsDevice, new EffectPool());

            effect.LightingEnabled = true;

            effect.DirectionalLight0.Enabled = true;
            effect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(5f, 8f, 15f));
            effect.DirectionalLight0.DiffuseColor = new Vector3(.5f, .5f, .5f);
            effect.DirectionalLight0.SpecularColor = new Vector3(1f, 1f, 1f);
            effect.SpecularPower = 5f;

            effect.World = worldMatrix;
            effect.View = viewMatrix;
            effect.Projection = projectionMatrix;
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
            sphere1.Rotation = Quaternion.CreateFromAxisAngle(rotVec, theta);
            theta += degree;
            if (theta >= MathHelper.TwoPi) 
                theta -= MathHelper.TwoPi;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
            //GraphicsDevice.RenderState.FillMode = FillMode.WireFrame;

            // TODO: Add your drawing code here
            effect.World = Matrix.Identity;
            effect.Begin();
            foreach (EffectTechnique tech in effect.Techniques)
            {
                foreach (EffectPass pass in tech.Passes)
                {
                    pass.Begin();
                    GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, floorVertices, 0, floorVertices.Length / 3);
                    pass.End();
                }   
            }
            effect.End();

            effect.World = sphere1.Transform;
            effect.Begin();
            foreach (EffectTechnique tech in effect.Techniques)
            {
                foreach (EffectPass pass in tech.Passes)
                {
                    pass.Begin();
                    GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, sphere1.VertexData, 0, sphere1.VertexData.Length / 3);
                    pass.End();
                }
            }
            effect.End();

            effect.World = sphere2.Transform;
            effect.Begin();
            foreach (EffectTechnique tech in effect.Techniques)
            {
                foreach (EffectPass pass in tech.Passes)
                {
                    pass.Begin();
                    GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, sphere2.VertexData, 0, sphere2.VertexData.Length / 3);
                    pass.End();
                }
            }
            effect.End();

            base.Draw(gameTime);
        }
    }
}
