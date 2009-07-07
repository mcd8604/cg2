#include "Node.h"

namespace BVH
{
	Node::Node(string name)
	{
		this->name = name;
		parent = NULL;
	}

	Node::Node(string name, Node * parent)
	{
		this->name = name;
		this->parent = parent;
	}

	Node::~Node(void)
	{
	}

	void Node::Cleanup()
	{
		for( int i = 0; i < children.size(); ++i ) 
		{
			children[i]->Cleanup();
			delete children[i];
		}
	}

	Node * Node::GetParent()
	{
		return parent;
	}

	void Node::SetName(string name)
	{
		this->name = name;
	}

	string Node::GetName()
	{
		return name;
	}

	void Node::SetOffset(D3DXVECTOR3 offset)
	{
		this->offset = offset;
	}

	D3DXVECTOR3 Node::GetOffset()
	{
		return offset;
	}

	void Node::AddChild(Node * childNode)
	{
		children.push_back(childNode);
	}

	vector<Node*> Node::GetChildren()
	{
		return children;
	}

	void Node::AddChannel(Channel channel)
	{
		channels.push_back(channel);
	}

	vector<Channel> Node::GetChannels()
	{
		return channels;
	}

	void Node::AddKeyFrame(KeyFrame keyFrame)
	{
		keyFrames.push_back(keyFrame);
	}

	KeyFrame Node::GetKeyFrame(int frameIndex)
	{
		return keyFrames[frameIndex];
	}

	int Node::GetNumKeyFrames()
	{
		return keyFrames.size();
	}

	Channel parseChannel(string channelName)
	{
		if(channelName == "Xposition") {
			return Channel::Xposition;
		} else if(channelName == "Yposition") {
			return Channel::Yposition;
		} else if(channelName == "Zposition") {
			return Channel::Zposition;
		} else if(channelName == "Zrotation") {
			return Channel::Zrotation;
		} else if(channelName == "Xrotation") {
			return Channel::Xrotation;
		} else if(channelName == "Yrotation") {
			return Channel::Yrotation;
		}
		return Channel::None;
	}
}