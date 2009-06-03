using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Assignment4
{
    enum Channel
    {
        None = 1,
        Xposition = 2,
        Yposition = 4,
        Zposition = 8,
        Zrotation = 16,
        Xrotation = 32,
        Yrotation = 64
    }

    class BVHNode
    {
        protected List<BVHNode> nodes = new List<BVHNode>();
        public List<BVHNode> Nodes
        {
            get { return nodes; }
            set { nodes = value; }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        protected Vector3 offset;
        public Vector3 Offset
        {
            get { return offset; }
            set { offset = value; }
        }

        protected Channel channels;
        public Channel Channels
        {
            get { return channels; }
            set { channels = value; }
        }

        public BVHNode(string name)
        {
            this.name = name;
        }
    }
}
