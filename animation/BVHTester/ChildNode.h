#pragma once

#include "Node.h"


namespace BVH
{
	class ChildNode : public Node
	{
	protected:
		Node * parent;
		bool isJoint;
	public:
		ChildNode(string name, Node * parent, bool isJoint);
		~ChildNode(void);
		Node * GetParent();
	};
}