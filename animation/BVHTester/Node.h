
#pragma once

#include <vector>
#include <string>
#include <d3dx10.h>

using namespace std;

namespace BVH
{
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

	class Node
	{
	protected:
		Node * parent;
		bool isJoint;
		string name;	
		D3DXVECTOR3 offset;
		vector<Node*> children;
		vector<Channel> channels;
		vector<KeyFrame> keyFrames;
	public:
		Node( string name );
		Node( string name, Node * parent );
		~Node( void );
		void Cleanup();
		Node * GetParent();
		void SetName( string name );
		string GetName();
		void SetOffset( D3DXVECTOR3 offset );
		D3DXVECTOR3 GetOffset();
		void AddChild( Node * childNode );
		vector<Node*> GetChildren();
		void AddChannel( Channel channel );
		vector<Channel> GetChannels();
		void AddKeyFrame( KeyFrame keyFrame );
		KeyFrame GetKeyFrame( int frameIndex );
		int GetNumKeyFrames();
	};
}