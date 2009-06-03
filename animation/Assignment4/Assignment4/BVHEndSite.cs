using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assignment4
{
    class BVHEndSite : BVHChildNode
    {
        public BVHEndSite(BVHNode parent)
            : base(string.Empty)
        {
            this.parent = parent;
        }
    }
}
