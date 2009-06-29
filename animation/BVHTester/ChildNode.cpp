#include "ChildNode.h"

namespace BVH
{
	ChildNode::ChildNode(string name, Node * parent, bool isJoint)
	: Node(name)
	{
		this->parent = parent;
		this->isJoint = isJoint;
	}

	ChildNode::~ChildNode(void)
	{
	}


	Node * ChildNode::GetParent()
	{
		return parent;
	}
}
