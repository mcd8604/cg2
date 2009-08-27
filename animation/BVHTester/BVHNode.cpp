#include "BVHNode.h"

BVHNode::BVHNode(string name)
{
	this->name = name;
	parent = NULL;
}

BVHNode::BVHNode(string name, BVHNode * parent)
{
	this->name = name;
	this->parent = parent;
}

BVHNode::~BVHNode(void)
{
}

void BVHNode::Cleanup()
{
	for( int i = 0; i < children.size(); ++i ) 
	{
		children[i]->Cleanup();
		delete children[i];
	}
}

BVHNode * BVHNode::GetParent()
{
	return parent;
}

void BVHNode::SetName(string name)
{
	this->name = name;
}

string BVHNode::GetName()
{
	return name;
}

void BVHNode::SetOffset(D3DXVECTOR3 offset)
{
	this->offset = offset;
}

D3DXVECTOR3 BVHNode::GetOffset()
{
	return offset;
}

void BVHNode::AddChild(BVHNode * childBVHNode)
{
	children.push_back(childBVHNode);
}

vector<BVHNode*> BVHNode::GetChildren()
{
	return children;
}

void BVHNode::AddChannel(Channel channel)
{
	channels.push_back(channel);
}

vector<Channel> BVHNode::GetChannels()
{
	return channels;
}

void BVHNode::AddKeyFrame(D3DXMATRIX translation, D3DXMATRIX rotation)
{
	KeyFrame keyFrame;
	keyFrame.translation = translation;
	keyFrame.rotation = rotation;
	keyFrames.push_back(keyFrame);
}

KeyFrame BVHNode::GetKeyFrame(int frameIndex)
{
	return keyFrames[frameIndex];
}

int BVHNode::GetNumKeyFrames()
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