using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Assignment2
{
    class ModelContainer : DrawableGameComponent
    {
        private Model model;
        public Model Model
        {
            get { return model; }
        }

        private Vector3 position = Vector3.Zero;
        private Matrix positionMatrix = Matrix.Identity;
        public Vector3 Position
        {
            get { return position; }
            set { position = value;  }
        }

        private Quaternion rotation = Quaternion.Identity;
        private Matrix rotationMatrix = Matrix.Identity;
        public Quaternion Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        private float scale = 1f;
        private Matrix scaleMatrix = Matrix.Identity;
        public float Scale
        {
            get { return scale; }
            set { scale = value;  }
        }

        private BoundingSphere boundingSphere;
        public BoundingSphere getBoundingSphere()
        {
            return boundingSphere.Transform(Transform);
        }

        public Matrix Transform
        {
            get
            {
                positionMatrix = Matrix.CreateTranslation(position);
                rotationMatrix = Matrix.CreateFromQuaternion(rotation);
                scaleMatrix = Matrix.CreateScale(scale);
                return Matrix.Multiply(Matrix.Multiply(rotationMatrix, scaleMatrix), positionMatrix);
            }
        }

        private BasicEffect effect;

        private List<KeyFrame> keyFrames;
        public List<KeyFrame> KeyFrames
        {
            get { return keyFrames; }
            set 
            { 
                keyFrames = value;
                Reset();
            }
        }

        private int currentKeyFrameIndex;
        private int nextKeyFrameIndex;
        private double time;

        public ModelContainer(Model m, Game game) : base(game)
        {
            this.model = m;
        }

        public override void Update(GameTime gameTime)
        {
            time += gameTime.ElapsedGameTime.TotalSeconds;

            if (time > keyFrames[nextKeyFrameIndex].time)
            {
                advanceKeyFrame();
            }

            float amount = (float)((time - keyFrames[currentKeyFrameIndex].time) / (keyFrames[nextKeyFrameIndex].time - keyFrames[currentKeyFrameIndex].time));

            position = Vector3.Lerp(keyFrames[currentKeyFrameIndex].position, keyFrames[nextKeyFrameIndex].position, amount);
            rotation = Quaternion.Lerp(keyFrames[currentKeyFrameIndex].rotation, keyFrames[nextKeyFrameIndex].rotation, amount);
            
            base.Update(gameTime);
        }

        /// <summary>
        /// Advances indices to the next KeyFrame. 
        /// Calls Reset() when nextKeyFrameIndex is at the end of the list. 
        /// </summary>
        private void advanceKeyFrame()
        {
            ++currentKeyFrameIndex;

            if (nextKeyFrameIndex < keyFrames.Count - 1)
            {
                ++nextKeyFrameIndex;
            }
            else
            {
                Reset();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            effect.World = Transform;
            foreach (ModelMesh mesh in model.Meshes)
            {
                mesh.Draw();
            }
 	        base.Draw(gameTime);
        }

        public void SetEffect(BasicEffect effect)
        {
            this.effect = effect;
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = effect;
                }
            }
        }

        public void Reset()
        {
            time = 0;
            currentKeyFrameIndex = 0;
            nextKeyFrameIndex = 1;
        }
    }
}
