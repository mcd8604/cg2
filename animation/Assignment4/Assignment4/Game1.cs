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

namespace Assignment4
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;

        BVHReader bvhReader;

        Vector3 camPos = new Vector3(50f, 50f, 200f);
        Vector3 camTarget = Vector3.Zero;
        float nearPlane = 0.1f;
        float farPlane = 1000f;

        Stack<Matrix> matrixStack;
        Matrix view;
        Matrix projection;

        Model sphere;

        VertexDeclaration vpcDeclaration;
        List<Vector3> pointList;
        VertexPositionColor[] floorVertices;

        BoundingBox boundingBox;

        BasicEffect effect;

        bool paused = false;
        int curFrame = 0;
        float curFrameTime = 0f;

        KeyboardState lastState = Keyboard.GetState();

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
            bvhReader = new BVHReader();
            bvhReader.ReadFile("Jog.bvh");

            Reset();

            base.Initialize();
        }

        private void Reset()
        {
            curTime = 0;
            curFrame = 0;
            curFrameTime = bvhReader.FrameTime;

            InitializePoints();
            InitializeFloor();
            InitializeCamera();

            matrixStack = new Stack<Matrix>();
            matrixStack.Push(Matrix.Identity);
        }

        private void InitializeCamera()
        {
            camTarget = (boundingBox.Max - boundingBox.Min) / 2;
            float dist = Vector3.Distance(boundingBox.Min, boundingBox.Max);
            camPos = new Vector3(camTarget.X + dist, camTarget.Y + dist, camTarget.Z + dist);
            UpdateView();
        }

        private void UpdateView()
        {
            view = Matrix.CreateLookAt(camPos, camTarget, Vector3.Up);
            if (effect != null) effect.View = view;
        }

        private void InitializeFloor()
        {
            floorVertices = new VertexPositionColor[6];

            //floorVertices[0] = new VertexPositionColor(new Vector3(boundingBox.Max.X, boundingBox.Min.Y, boundingBox.Max.Z), Color.White);
            //floorVertices[1] = new VertexPositionColor(new Vector3(boundingBox.Min.X, boundingBox.Min.Y, boundingBox.Max.Z), Color.White);
            //floorVertices[2] = new VertexPositionColor(new Vector3(boundingBox.Min.X, boundingBox.Min.Y, boundingBox.Min.Z), Color.White);

            //floorVertices[3] = new VertexPositionColor(new Vector3(boundingBox.Max.X, boundingBox.Min.Y, boundingBox.Max.Z), Color.White);
            //floorVertices[4] = new VertexPositionColor(new Vector3(boundingBox.Min.X, boundingBox.Min.Y, boundingBox.Min.Z), Color.White);
            //floorVertices[5] = new VertexPositionColor(new Vector3(boundingBox.Max.X, boundingBox.Min.Y, boundingBox.Min.Z), Color.White);

            floorVertices[0] = new VertexPositionColor(new Vector3(1000, boundingBox.Min.Y, 1000), Color.White);
            floorVertices[1] = new VertexPositionColor(new Vector3(-1000, boundingBox.Min.Y, 1000), Color.White);
            floorVertices[2] = new VertexPositionColor(new Vector3(-1000, boundingBox.Min.Y, -1000), Color.White);

            floorVertices[3] = new VertexPositionColor(new Vector3(1000, boundingBox.Min.Y, 1000), Color.White);
            floorVertices[4] = new VertexPositionColor(new Vector3(-1000, boundingBox.Min.Y, -1000), Color.White);
            floorVertices[5] = new VertexPositionColor(new Vector3(1000, boundingBox.Min.Y, -1000), Color.White);
        }

        private void InitializePoints()
        {
            pointList = new List<Vector3>();
            foreach (BVHNode curNode in bvhReader.Nodes)
            {
                InitializeNode(curNode, Vector3.Zero);
            }
            boundingBox = BoundingBox.CreateFromPoints(pointList);
        }

        private void InitializeNode(BVHNode curNode, Vector3 curOffset)
        {
            curOffset += curNode.Offset;
            pointList.Add(curOffset);

            foreach (BVHNode childNode in curNode.Nodes)
            {
                InitializeNode(childNode, curOffset);
            }
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("font");

            // TODO: use this.Content to load your game content here
            //GraphicsDevice.RenderState.PointSize = 5f;

            vpcDeclaration = new VertexDeclaration(GraphicsDevice, VertexPositionColor.VertexElements);

            view = Matrix.CreateLookAt(camPos, camTarget, Vector3.Up);
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, nearPlane, farPlane);

            sphere = Content.Load<Model>("sphere");

            initEffect();

            sphere.Meshes[0].MeshParts[0].Effect = effect;
        }

        private void initEffect()
        {
            effect = new BasicEffect(GraphicsDevice, new EffectPool());

            effect.World = matrixStack.Peek();
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

        double curTime = 0;

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
            UpdateInput(gameTime);
            UpdateFrame(gameTime);

            base.Update(gameTime);
        }

        private void UpdateFrame(GameTime gameTime)
        {
            if (!paused)
            {
                curTime += gameTime.ElapsedGameTime.TotalSeconds;

                if (curTime > curFrameTime)
                {
                    //advance frame
                    curFrame += (int)(curTime / curFrameTime);

                    if (curFrame < 0)
                        curFrame = bvhReader.Frames.Length - 1;

                    if (curFrame > bvhReader.Frames.Length - 1)
                        curFrame = 0;

                    curTime %= curFrameTime;
                }
            }
        }

        private void UpdateInput(GameTime gameTime)
        {
            KeyboardState curState = Keyboard.GetState();

            if (curState.IsKeyDown(Keys.P) && lastState.IsKeyUp(Keys.P))
            {
                paused = !paused;
            }

            if (curState.IsKeyDown(Keys.Space) && lastState.IsKeyUp(Keys.Space))
            {
                Reset();
            }

            if (curState.IsKeyDown(Keys.Up) /*&& lastState.IsKeyUp(Keys.Up)*/)
            {
                curFrameTime /= 1.01f;
            }

            if (curState.IsKeyDown(Keys.Down) /*&& lastState.IsKeyUp(Keys.Down)*/)
            {
                curFrameTime *= 1.01f;
            }

            if (paused)
            {
                if (curState.IsKeyDown(Keys.Left) /*&& lastState.IsKeyUp(Keys.Left)*/)
                {
                    --curFrame;
                    if (curFrame < 0)
                        curFrame = bvhReader.NumFrames - 1;
                }

                if (curState.IsKeyDown(Keys.Right) /*&& lastState.IsKeyUp(Keys.Right)*/)
                {
                    ++curFrame;
                    if (curFrame >= bvhReader.NumFrames)
                        curFrame = 0;
                }
            }

            lastState = curState;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            GraphicsDevice.VertexDeclaration = vpcDeclaration;

            DrawFloor();
            DrawNodes();

            updateCamera();

            DrawText();

            base.Draw(gameTime);
        }

        readonly Vector2 strPos_FrameNum = new Vector2(0, 24);
        readonly Vector2 strPos_SamplingRate = new Vector2(0, 48);

        private void DrawText()
        {
            spriteBatch.Begin();
            spriteBatch.DrawString(font, paused ? "Paused" : "Running", Vector2.Zero, Color.White);
            spriteBatch.DrawString(font, "Current Frame: " + curFrame, strPos_FrameNum, Color.White);
            spriteBatch.DrawString(font, "Sampling Rate (fps): " + 1f / curFrameTime, strPos_SamplingRate, Color.White);
            spriteBatch.End();
        }

        private void DrawFloor()
        {
            effect.World = Matrix.Identity;
            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, floorVertices, 0, 2);
                pass.End();
            }
            effect.End();
        }

        private void DrawNodes()
        {
            pointList = new List<Vector3>();

            int dataIndex = -1;
            for (int i = 0; i < bvhReader.Nodes.Count; ++i)
            {
                dataIndex = DrawNode(bvhReader.Nodes[i], dataIndex);
            }
        }

        private int DrawNode(BVHNode bvhNode, int dataIndex)
        {
            Matrix localWorld = Matrix.CreateTranslation(getNodeTranslation(bvhNode, ref dataIndex) + bvhNode.Offset);
            //Matrix translation = getNodeTranslation(bvhNode, ref dataIndex);
            Matrix rotation = getNodeRotation(bvhNode, ref dataIndex);
            Matrix top = matrixStack.Peek();

            Matrix current = top * rotation * localWorld;
            pointList.Add(current.Translation);
            
            effect.World = current;
            sphere.Meshes[0].Draw();
            effect.World = Matrix.Identity;

            // If the node is a child, draw a line from the parent to the node.

            if(bvhNode is BVHChildNode) 
            {
                BVHChildNode child = (BVHChildNode)bvhNode;
                VertexPositionColor[] line = { 
                                                 new VertexPositionColor(top.Translation, Color.Yellow), 
                                                 new VertexPositionColor(current.Translation, Color.Yellow) 
                                             };
                effect.Begin();
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Begin();
                    GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, line, 0, 1);
                    pass.End();
                }
                effect.End();
            }

            matrixStack.Push(current);

            foreach (BVHNode child in bvhNode.Nodes)
                DrawNode(child, dataIndex);

            matrixStack.Pop();

            return dataIndex;
        }

        private Matrix getNodeRotation(BVHNode bvhNode, ref int dataIndex)
        {
            //Quaternion zRotation = Quaternion.Identity;
            //Quaternion xRotation = Quaternion.Identity;
            //Quaternion yRotation = Quaternion.Identity;
            Matrix zRotation = Matrix.Identity;
            Matrix xRotation = Matrix.Identity;
            Matrix yRotation = Matrix.Identity;

            if ((bvhNode.Channels & Channel.Zrotation) == Channel.Zrotation)
            {
                //zRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, bvhReader.Frames[curFrame][++dataIndex]);
                zRotation = Matrix.CreateRotationZ(MathHelper.ToRadians(bvhReader.Frames[curFrame][++dataIndex]));
            }
            if ((bvhNode.Channels & Channel.Xrotation) == Channel.Xrotation)
            {
                //xRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, bvhReader.Frames[curFrame][++dataIndex]);
                xRotation = Matrix.CreateRotationX(MathHelper.ToRadians(bvhReader.Frames[curFrame][++dataIndex]));
            }
            if ((bvhNode.Channels & Channel.Yrotation) == Channel.Yrotation)
            {
                //yRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, bvhReader.Frames[curFrame][++dataIndex]);
                yRotation = Matrix.CreateRotationY(MathHelper.ToRadians(bvhReader.Frames[curFrame][++dataIndex]));
            }
            
            return zRotation * xRotation * yRotation;
        }

        private Vector3 getNodeTranslation(BVHNode bvhNode, ref int dataIndex)
        {
            Vector3 translation = Vector3.Zero;

            if ((bvhNode.Channels & Channel.Xposition) == Channel.Xposition)
            {
                translation.X = bvhReader.Frames[curFrame][++dataIndex];
            }
            if ((bvhNode.Channels & Channel.Yposition) == Channel.Yposition)
            {
                translation.Y = bvhReader.Frames[curFrame][++dataIndex];
            }
            if ((bvhNode.Channels & Channel.Zposition) == Channel.Zposition)
            {
                translation.Z = bvhReader.Frames[curFrame][++dataIndex];
            }

            //return Matrix.CreateTranslation(translation);
            return translation;
        }

        // rotation angle
        //private float theta = .01f;

        private void updateCamera()
        {
            boundingBox = BoundingBox.CreateFromPoints(pointList);
            camTarget = boundingBox.Min + (boundingBox.Max - boundingBox.Min) / 2;

            // rotate cam
            //camPos = Vector3.Transform(camPos, Quaternion.CreateFromAxisAngle(Vector3.UnitY, theta));
            UpdateView();
        }
    }
}
