using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Assignment1
{
    class ModelContainer
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

        public ModelContainer(Model m)
        {
            this.model = m;
        }

        public void Draw()
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                mesh.Draw();
            }
        }

        public void SetEffect(BasicEffect effect)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = effect;
                }
            }
        }
    }
}
