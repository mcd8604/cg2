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

namespace BilliardsSimulation
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Matrix world;
        Matrix view;
        Matrix projection;

        Vector3 camPosition;
        Vector3 camTarget;

        Vector3 tableMin = Vector3.Zero;
        Vector3 tableMax = new Vector3(1.298f, 0, 2.438f);

        BasicEffect effect;

        Ball[] balls;
        Texture2D[] ballTextures;

        Model sphere;

        VertexDeclaration vertexDecl;
        VertexPositionNormalTexture[] tableVertices;
        Texture2D tableTexture;

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
            InitializeWorld();

            base.Initialize();
        }

        private void InitializeWorld()
        {
            tableVertices = new VertexPositionNormalTexture[6];
            tableVertices[0] = new VertexPositionNormalTexture(tableMax, Vector3.Up, Vector2.One);
            tableVertices[1] = new VertexPositionNormalTexture(new Vector3(tableMin.X, 0, tableMax.Z), Vector3.Up, Vector2.UnitY);
            tableVertices[2] = new VertexPositionNormalTexture(tableMin, Vector3.Up, Vector2.Zero);

            tableVertices[3] = new VertexPositionNormalTexture(tableMax, Vector3.Up, Vector2.One);
            tableVertices[4] = new VertexPositionNormalTexture(tableMin, Vector3.Up, Vector2.Zero);
            tableVertices[5] = new VertexPositionNormalTexture(new Vector3(tableMax.X, 0, tableMin.Z), Vector3.Up, Vector2.UnitX);
            
            camTarget = (tableMax - tableMin) / 2;
            camPosition = new Vector3(camTarget.X * 4, camTarget.X * 2, camTarget.Z * 2);

            balls = new Ball[3];
            
            Ball ball1 = new Ball();
            ball1.Position = new Vector3(camTarget.X, ball1.Radius, camTarget.Z / 2);
            ball1.ApplyForce(new Vector3(0f, 0f, 0f));
            balls[0] = ball1;

            Ball ball2 = new Ball();
            ball2.Position = new Vector3(camTarget.X + (ball2.Radius), ball2.Radius, camTarget.Z * (3f / 2));
            ball2.ApplyForce(new Vector3(0f, 0f, -40f));
            balls[1] = ball2;

            Ball ball3 = new Ball();
            ball3.Position = new Vector3(camTarget.X / 2, ball3.Radius, camTarget.X / 3);
            ball3.ApplyForce(new Vector3(0f, 0f, 0f));
            balls[2] = ball3;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            InitializeMatrices();
            InitializeEffect();

            sphere = Content.Load<Model>("sphere");
            foreach (ModelMesh mesh in sphere.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = effect;
                }
            }

            vertexDecl = new VertexDeclaration(GraphicsDevice, VertexPositionNormalTexture.VertexElements);

            InitializeTextures();
        }

        private void InitializeTextures()
        {
            Color[] colorData = new Color[1];

            ballTextures = new Texture2D[3];

            ballTextures[0] = new Texture2D(GraphicsDevice, 1, 1);
            colorData[0] = Color.Yellow;
            ballTextures[0].SetData<Color>(colorData);

            ballTextures[1] = new Texture2D(GraphicsDevice, 1, 1);
            colorData[0] = Color.Beige;
            ballTextures[1].SetData<Color>(colorData);

            ballTextures[2] = new Texture2D(GraphicsDevice, 1, 1);
            colorData[0] = Color.Red;
            ballTextures[2].SetData<Color>(colorData);

            tableTexture = new Texture2D(GraphicsDevice, 1, 1);
            colorData[0] = Color.Green;
            tableTexture.SetData<Color>(colorData);
        }

        private void InitializeMatrices()
        {
            world = Matrix.Identity;
            view = Matrix.CreateLookAt(camPosition, camTarget, Vector3.Up);
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.001f, 1000f);
        }

        private void InitializeEffect()
        {
            effect = new BasicEffect(GraphicsDevice, new EffectPool());

            effect.World = world;
            effect.View = view;
            effect.Projection = projection;

            effect.EnableDefaultLighting();
            effect.TextureEnabled = true;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        KeyboardState lastState = Keyboard.GetState();

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
            float dT = (float)gameTime.ElapsedGameTime.TotalSeconds;

            KeyboardState curState = Keyboard.GetState();
            
            if(curState.IsKeyDown(Keys.Space) && lastState.IsKeyUp(Keys.Space))
            {
                InitializeWorld();
            }

            lastState = curState;

            foreach (Ball ball in balls)
            {
                ball.ApplyFriction();
                ball.UpdatePosition(dT);
                ball.UpdateMomentum(dT);
            }

            checkCollisions(dT);

            foreach (Ball ball in balls)
            {
                ball.UpdateVelocity();
            }

            base.Update(gameTime);
        }

        private void checkCollisions(float dT)
        {
            int i1 = 0;
            foreach (Ball ball in balls)
            {
                // ball-ball collision
                if (i1 < balls.Length - 1)
                {
                    for (int i2 = i1 + 1; i2 < balls.Length; ++i2)
                    {
                        ball.CheckCollision(balls[i2], dT);
                    }
                }
                
                checkCushionCollision(ball);
                
                ++i1;
            }
        }

        private void checkCushionCollision(Ball ball)
        {
            //minX
            if (ball.Position.X - ball.Radius <= tableMin.X)
            {
                ball.Collide(Vector3.UnitX);
            }
            else
                //maxX
                if (ball.Position.X + ball.Radius >= tableMax.X)
                {
                    ball.Collide(Vector3.Zero - Vector3.UnitX);
                }

            //minZ
            if (ball.Position.Z - ball.Radius <= tableMin.Z)
            {
                ball.Collide(Vector3.UnitZ);
            }
            else
                //maxZ
                if (ball.Position.Z + ball.Radius >= tableMax.Z)
                {
                    ball.Collide(Vector3.Zero - Vector3.UnitZ);
                }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.VertexDeclaration = vertexDecl;
            effect.Texture = tableTexture;
            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                GraphicsDevice.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, tableVertices, 0, tableVertices.Length / 3);
                pass.End();
            }
            effect.End();

            int i = 0;
            foreach (Ball ball in balls)
            {
                effect.Texture = ballTextures[i];
                effect.World = Matrix.Multiply(Matrix.CreateScale(ball.Radius), Matrix.CreateTranslation(ball.Position));
                foreach (ModelMesh mesh in sphere.Meshes)
                {
                    mesh.Draw();
                }
                ++i;
            }
            effect.World = world;

            base.Draw(gameTime);
        }
    }
}
