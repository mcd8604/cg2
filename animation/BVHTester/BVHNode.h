
#pragma once

#include <vector>
#include <string>
#include <d3dx10.h>

using namespace std;

enum Channel
{
	None = 0,
	Xposition = 1,
	Yposition = 2,
	Zposition = 4,
	Zrotation = 8,
	Xrotation = 16,
	Yrotation = 32
};

Channel parseChannel( string channelName );

struct KeyFrame
{
	D3DXMATRIX translation;
	D3DXMATRIX rotation;
};

class BVHNode
{
protected:
	BVHNode * parent;
	bool isJoint;
	string name;	
	D3DXVECTOR3 offset;
	vector<BVHNode*> children;
	vector<Channel> channels;
	vector<KeyFrame> keyFrames;
public:
	BVHNode( string name );
	BVHNode( string name, BVHNode * parent );
	~BVHNode( void );
	void Cleanup();
	BVHNode * GetParent();
	void SetName( string name );
	string GetName();
	void SetOffset( D3DXVECTOR3 offset );
	D3DXVECTOR3 GetOffset();
	void AddChild( BVHNode * childBVHNode );
	vector<BVHNode*> GetChildren();
	void AddChannel( Channel channel );
	vector<Channel> GetChannels();
	void AddKeyFrame( D3DXMATRIX translation, D3DXMATRIX rotation );
	KeyFrame GetKeyFrame( int frameIndex );
	int GetNumKeyFrames();
};