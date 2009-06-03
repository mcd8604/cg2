using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assignment4
{
    class BVHJoint : BVHChildNode
    {
        public BVHJoint(string name, BVHNode parent)
            : base(name)
        {
            this.parent = parent;
        }
    }
}
