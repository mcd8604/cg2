using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Assignment4
{
    abstract class BVHChildNode : BVHNode
    {
        protected BVHNode parent;
        public BVHNode Parent
        {
            get { return parent; }
        }

        public BVHChildNode(string name) : base(name) { }
    }
}
