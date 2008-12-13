using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace RayTracerXNA
{
    sealed class GeodesicIcosahedron : DrawableGameComponent
    {
        private int subDivisions;

        private List<Vector3> vertices;

        private Vector3 position = Vector3.Zero;
        private Matrix positionMatrix = Matrix.Identity;
        public Vector3 Position
        {
            get { return position; }
            set { position = value; positionMatrix = Matrix.CreateTranslation(position); }
        }

        private Quaternion rotation = Quaternion.Identity;
        private Matrix rotationMatrix = Matrix.Identity;
        public Quaternion Rotation
        {
            get { return rotation; }
            set { rotation = value; rotationMatrix = Matrix.CreateFromQuaternion(rotation); }
        }

        private float scale = 1f;
        private Matrix scaleMatrix = Matrix.Identity;
        public float Scale
        {
            get { return scale; }
            set { scale = value; scaleMatrix = Matrix.CreateScale(scale); }
        }

        public Matrix Transform
        {
            get {
                return Matrix.Multiply(Matrix.Multiply(rotationMatrix, scaleMatrix), positionMatrix);
            }
        }

        private VertexPositionNormalTexture[] vertexData;
        public VertexPositionNormalTexture[] VertexData
        {
            get { return vertexData; }
        }

        public GeodesicIcosahedron(Game game, int subDivisions) : base(game)
        {
            this.subDivisions = subDivisions;
        }

        protected override void LoadContent()
        {
            createModel();
            base.LoadContent();
        }

        private static readonly float a = (float)(2 / (1 + Math.Sqrt(5.0)));
        private static readonly float radius = (float)(Math.Sqrt(a * a + 1));

        private static readonly Vector3 v0 = new Vector3(0f, a, -1f);
        private static readonly Vector3 v1 = new Vector3(-a, 1f, 0f);
        private static readonly Vector3 v2 = new Vector3(a, 1f, 0f);
        private static readonly Vector3 v3 = new Vector3(0f, a, 1f);
        private static readonly Vector3 v4 = new Vector3(-1f, 0f, a);
        private static readonly Vector3 v5 = new Vector3(0f, -a, 1f);
        private static readonly Vector3 v6 = new Vector3(1f, 0f, a);
        private static readonly Vector3 v7 = new Vector3(1f, 0f, -a);
        private static readonly Vector3 v8 = new Vector3(0f, -a, -1f);
        private static readonly Vector3 v9 = new Vector3(-1f, 0f, -a);
        private static readonly Vector3 v10 = new Vector3(-a, -1f, 0f);
        private static readonly Vector3 v11 = new Vector3(a, -1f, 0f);


        private void addTriangle(Vector3 a, Vector3 b, Vector3 c)
        {
            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);
        }

        private void createModel()
        {
            vertices = new List<Vector3>();

            // First create an icosahedron as a starting shape, credit via CG1 notes

            //addTriangle( v0 , v1 , v2 );
            //addTriangle( v3 , v2 , v1 );
            //addTriangle( v3 , v4 , v5 );	
            //addTriangle( v3 , v5 , v6 );
            //addTriangle( v0 , v7 , v8 );
            //addTriangle( v0 , v8 , v9 );
            //addTriangle( v5 , v10 , v11 );	
            //addTriangle( v8 , v11 , v10 );
            //addTriangle( v1 , v9 , v4 );		
            //addTriangle( v10 , v4 , v9 );
            //addTriangle( v2 , v6 , v7 );	
            //addTriangle( v11 , v7 , v6 );
            //addTriangle( v3 , v1 , v4 );	
            //addTriangle( v3 , v6 , v2 );
            //addTriangle( v0 , v9 , v1 );
            //addTriangle( v0 , v2 , v7 );
            //addTriangle( v8 , v10 , v9 );
            //addTriangle( v8 , v7 , v11 );
            //addTriangle( v5 , v4 , v10 );
            //addTriangle( v5 , v11 , v6 );

            addTriangle(v2, v1, v0);
            addTriangle(v1, v2, v3);
            addTriangle(v5, v4, v3);
            addTriangle(v6, v5, v3);
            addTriangle(v8, v7, v0);
            addTriangle(v9, v8, v0);
            addTriangle(v11, v10, v5);
            addTriangle(v10, v11, v8);
            addTriangle(v4, v9, v1);
            addTriangle(v9, v4, v10);
            addTriangle(v7, v6, v2);
            addTriangle(v6, v7, v11);
            addTriangle(v4, v1, v3);
            addTriangle(v2, v6, v3);
            addTriangle(v1, v9, v0);
            addTriangle(v7, v2, v0);
            addTriangle(v9, v10, v8);
            addTriangle(v11, v7, v8);
            addTriangle(v10, v4, v5);
            addTriangle(v6, v11, v5);

            // Next, tesselate the icosahedron

            for(int i = 1; i < subDivisions; ++i) {
            	
                // get current triangle vertices, clear old list
                List<Vector3> curVertices = new List<Vector3>(vertices);
                vertices.Clear();

                // subdivide each triangle 
                for(int j = 0; j < curVertices.Count; j+=3) {
            		
                    Vector3 p1, p2, p3, m12, m23, m13;
                    p1 = curVertices[j];
                    p2 = curVertices[j + 1];
                    p3 = curVertices[j + 2];

                    // calculate midpoint vectors
                    m12 = new Vector3((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2, (p1.Z + p2.Z) / 2);
                    m23 = new Vector3((p2.X + p3.X) / 2, (p2.Y + p3.Y) / 2, (p2.Z + p3.Z) / 2);
                    m13 = new Vector3((p1.X + p3.X) / 2, (p1.Y + p3.Y) / 2, (p1.Z + p3.Z) / 2);

                    // normalize midpoint vectors
                    Vector3.Normalize(m12);
                    Vector3.Normalize(m23);
                    Vector3.Normalize(m13);
            		
                    // adjust radii
                    m12 *= radius;
                    m23 *= radius;
                    m13 *= radius;

                    // triangle 1 - point 1, midpoint 1-2, midpoint 1-3
                    addTriangle(p1, m12, m13);

                    // triangle 2 - point 2, midpoint 2-3. midpoint 1-2
                    addTriangle(p2, m23, m12);
            		
                    // triangle 3 - point 3, midpoint 1-3, midpoint 2-3
                    addTriangle(p3, m13, m23);

                    // triangle 4 - midpoint 1-2, midpoint 2-3, midpoint 1-3
                    addTriangle(m12, m23, m13);
                }
            }

            // Set vertexData
            vertexData = new VertexPositionNormalTexture[vertices.Count];
            int k = 0;
            foreach (Vector3 vector in vertices)
            {
                vertexData[k] = new VertexPositionNormalTexture(vector, Vector3.Negate(vector), Vector2.Zero);
                ++k;
            }
        }
    }
}
