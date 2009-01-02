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

#if DEBUG
        double fps;
        int frameCount;
        const double SAMPLE_TIME_FRAME = 1f;
        double sampleTime;
        SpriteFont font;
#endif

#if EFFECT
        private readonly VertexPositionNormalTexture[] floorVertices = {
            new VertexPositionNormalTexture(new Vector3(8, 0, 8), Vector3.Down, Vector2.Zero),
            new VertexPositionNormalTexture(new Vector3(-8, 0, 8), Vector3.Down, new Vector2(1f, 0f)),
            new VertexPositionNormalTexture(new Vector3(8, 0, -16), Vector3.Down, new Vector2(0f, 1f)),
            new VertexPositionNormalTexture(new Vector3(8, 0, -16), Vector3.Down, new Vector2(0f, 1f)),
            new VertexPositionNormalTexture(new Vector3(-8, 0, 8), Vector3.Down, new Vector2(1f, 0f)),
            new VertexPositionNormalTexture(new Vector3(-8, 0, -16), Vector3.Down, Vector2.One)};
        
        GeodesicIcosahedron sphere1;
        GeodesicIcosahedron sphere2;
#else
        List<Primitive> primitives;
        Primitive floor;
        Primitive sphere1;
        Primitive sphere2;
#endif
        Matrix worldMatrix;
        Matrix viewMatrix;
        Matrix projectionMatrix;

        float nearDist = 0.1f;
        float farDist = 50.0f;

#if EFFECT
        BasicEffect effect;
#else 
        Ray[,] rayTable = new Ray[(int)WIDTH, (int)HEIGHT];
        Texture2D projection;
#endif

        readonly Vector3 cameraPos = new Vector3(3f, 4f, 15f);
        readonly Vector3 cameraTarget = new Vector3(3f, 0f, -70f);

        #region Illumination Parameters

        readonly Vector4 ambientLight = new Vector4(0.25f, 0.25f, 0.25f, 1.0f);
        PhongModel phongModel;

        #endregion

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
            InitializeWorld();
            InitializeLighting();

            base.Initialize();
        }

        private void InitializeWorld()
        {
            worldMatrix = Matrix.Identity;
#if Effect
            sphere1 = new GeodesicIcosahedron(this, 1);
            sphere2 = new GeodesicIcosahedron(this, 1);
            sphere1.Position = new Vector3(3f, 4f, 11f);
            sphere2.Position = new Vector3(1.5f, 3f, 9f);
            sphere1.Scale = 0.75f;
            sphere2.Scale = 0.75f;

            Components.Add(sphere1);
            Components.Add(sphere2);
#else
            primitives = new List<Primitive>();
            floor = new Square(new Vector3(8, 0, 8), new Vector3(-8, 0, -16), new Vector3(8, 0, -16), new Vector3(-8, 0, -16));
            floor.AmbientStrength = 1f;
            floor.DiffuseStrength = 1f;
            floor.MaterialColor = new Vector4(0f, 1f, 0f, 1f);
            primitives.Add(floor);

            sphere1 = new Sphere(new Vector3(3f, 4f, 11f), 1f);
            sphere1.AmbientStrength = 1f;
            sphere1.DiffuseStrength = 1f;
            sphere1.MaterialColor = new Vector4(1f, 0f, 0f, 1f);
            primitives.Add(sphere1);

            sphere2 = new Sphere(new Vector3(1.5f, 3f, 9f), 1f);
            sphere2.AmbientStrength = 1f;
            sphere2.DiffuseStrength = 1f;
            sphere2.MaterialColor = new Vector4(0f, 0f, 1f, 1f);
            primitives.Add(sphere2);
#endif
        }

        private void InitializeLighting()
        {
            phongModel = new PhongModel();
            phongModel.AmbientLight = ambientLight;
            
            Light l = new Light();
            l.LightColor = Vector4.One;
            l.Position = new Vector3(5f, 8f, 15f);

            phongModel.Lights.Add(l);
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
      
            GraphicsDevice.VertexDeclaration = new VertexDeclaration(GraphicsDevice, VertexPositionNormalTexture.VertexElements);            
            
            InitializeViewProjection();
        }

        private void InitializeViewProjection()
        {
            viewMatrix = Matrix.CreateLookAt(cameraPos, cameraTarget, Vector3.Up);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, nearDist, farDist);

#if EFFECT
            InitializeEffect();
#else
            populateRayTable();
            projection = new Texture2D(GraphicsDevice, (int)WIDTH, (int)HEIGHT);
#endif
        }

#if EFFECT
        public void InitializeEffect()
        {
            effect = new BasicEffect(GraphicsDevice, new EffectPool());

            effect.LightingEnabled = true;

            effect.DirectionalLight0.Enabled = true;
            effect.DirectionalLight0.Direction = Vector3.Normalize(lightPos);
            effect.DirectionalLight0.DiffuseColor = new Vector3(.5f, .5f, .5f);
            effect.DirectionalLight0.SpecularColor = new Vector3(1f, 1f, 1f);
            effect.SpecularPower = 5f;

            effect.World = worldMatrix;
            effect.View = viewMatrix;
            effect.Projection = projectionMatrix;
        }
#else
        private void populateRayTable()
        {
            Ray ray = new Ray(cameraPos, Vector3.Zero);
            Vector3 viewPlaneVec = Vector3.Zero;

            for (int x = 0; x < WIDTH; ++x)
            {
                for (int y = 0; y < HEIGHT; ++y)
                {
                    viewPlaneVec = GraphicsDevice.Viewport.Unproject(new Vector3(x, y, 0), projectionMatrix, viewMatrix, worldMatrix);
                    ray.Direction = Vector3.Normalize(viewPlaneVec - cameraPos);
                    rayTable[x, y] = ray;
                }
            }
        }
#endif

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
            //GraphicsDevice.RenderState.FillMode = FillMode.WireFrame;

            // TODO: Add your drawing code here
#if EFFECT
            effectDraw();
#else
            rayTracerDraw();
#endif
            ++frameCount;

            base.Draw(gameTime);
        }

#if EFFECT
        private void effectDraw()
        {
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
        }
#else
        private void rayTracerDraw()
        {
            Color[] colorData = new Color[(int)WIDTH * (int)HEIGHT];

            for (int y = 0; y < HEIGHT; ++y)
            {
                for (int x = 0; x < WIDTH; ++x)
                {
                    Ray ray = rayTable[x, y];
                    int index = (y * (int)WIDTH) + x;

                    // get closest intersection
                    float? dist = float.PositiveInfinity;
                    float? curDist = null;
                    Primitive intersected = null;

                    foreach (Primitive primitive in primitives)
                    {
                        curDist = primitive.Intersects(ray);
                        if (curDist < dist)
                        {
                            dist = curDist;
                            intersected = primitive;
                        }
                    }

                    if (intersected != null)
                    {
                        // find polygon intersection
                        /*VertexPositionNormalTexture[] vertexData = intersected.VertexData;
                        float? closestDist = float.PositiveInfinity;
                        Vector3 polyNormal = Vector3.Zero;
                        for (int i = 0; i < vertexData.Length; i += 3)
                        {
                            Plane p = new Plane(vertexData[i].Position, vertexData[i + 1].Position, vertexData[i + 2].Position);
                            float? polyDist = ray.Intersects(p);
                            if (polyDist < closestDist)
                            {
                                polyNormal = p.Normal;
                                closestDist = polyDist; 
                            }
                        }*/

                        // calculate lighting

                        Vector3 intersectPoint = ray.Position + Vector3.Multiply(ray.Direction, (float)dist);
                        colorData[index] = phongModel.CalculateLighting(intersected, intersectPoint);
                    }
                    else
                    {
                        // use background color
                        colorData[index] = Color.CornflowerBlue;
                    }
                }
            }
            projection = new Texture2D(GraphicsDevice, (int)WIDTH, (int)HEIGHT);
            projection.SetData<Color>(colorData);

            spriteBatch.Begin();
            spriteBatch.Draw(projection, Vector2.Zero, Color.White);
            spriteBatch.DrawString(font, "FPS: " + fps, Vector2.Zero, Color.White);
            spriteBatch.End();

        }
#endif
    }
}
