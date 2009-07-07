#pragma once

#include <iostream>
#include <fstream>
#include <string>

#include "Node.h"

#include <d3dx10.h>

using namespace std;
using namespace BVH;

struct SimpleVertex
{
    D3DXVECTOR3 Pos;
    D3DXVECTOR4 Color;
};

class BVHFigure
{
protected:
	ID3D10Device*				d3dDevice;
	ID3D10EffectTechnique*		techniqueRender;
	ID3D10Buffer*               edgeVertexBuffer;
	ID3D10Buffer*               edgeIndexBuffer;
	ID3D10Buffer*               cubeVertexBuffer;
	ID3D10Buffer*               cubeIndexBuffer;
	ID3D10EffectMatrixVariable* worldVariable;
	D3DXMATRIX                  world;
	vector<Node*>				nodes;
	vector<SimpleVertex>		edgeVertices;
	int							numEdges;
	int							curFrame;
	int							numFrames;
	float						frameTime;
	HRESULT ProcessHierarchy( vector<string> lines, int * lineNum, int * numEdges );
	HRESULT ProcessMotionData( vector<string> lines, int * lineNum );
	HRESULT InitNodeFrames( Node * node, vector<float> data , int * dataIndex);
	D3DXMATRIX GetNodeRotation( Node * node, vector<float> data, int * dataIndex );
	D3DXMATRIX GetNodeTranslation( Node * node, vector<float> data, int * dataIndex );
	HRESULT CreateVertexBuffer();
public:
	BVHFigure(void);
	~BVHFigure(void);
	HRESULT ReadBVH( string fileName );
	HRESULT Initialize( ID3D10Device * d3dDevice, 
		ID3D10EffectTechnique * techniqueRender, 
		ID3D10EffectMatrixVariable * worldVariable );
	void Update( float time );
	void LookAt( D3DXVECTOR3 * Eye, D3DXVECTOR3 * Up, ID3D10EffectMatrixVariable * viewVariable );
	void Render();
	void RenderNode( Node * node, D3DXMATRIX parentWorld );
	void RenderEdges();
	void Cleanup();
};
