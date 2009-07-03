//--------------------------------------------------------------------------------------
// File: BVHTester.cpp
//
// This application demonstrates animation using matrix transformations
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------

#include <iostream>
#include <fstream>
#include <string>

#include <vector>
#include <windows.h>

#include <d3d10.h>
#include <d3dx10.h>

#include "Node.h"

#include "resource.h"

using namespace std;
using namespace BVH;

//--------------------------------------------------------------------------------------
// structures
//--------------------------------------------------------------------------------------
struct SimpleVertex
{
    D3DXVECTOR3 Pos;
    D3DXVECTOR4 Color;
};

//--------------------------------------------------------------------------------------
// Global Variables
//--------------------------------------------------------------------------------------
HINSTANCE                   g_hInst = NULL;
HWND                        g_hWnd = NULL;
D3D10_DRIVER_TYPE           g_driverType = D3D10_DRIVER_TYPE_NULL;
ID3D10Device*               g_pd3dDevice = NULL;
IDXGISwapChain*             g_pSwapChain = NULL;
ID3D10RenderTargetView*     g_pRenderTargetView = NULL;
ID3D10Texture2D*            g_pDepthStencil = NULL;
ID3D10DepthStencilView*     g_pDepthStencilView = NULL;
ID3D10Effect*               g_pEffect = NULL;
ID3D10EffectTechnique*      g_pTechnique = NULL;
ID3D10InputLayout*          g_pVertexLayout = NULL;
ID3D10Buffer*               g_pVertexBuffer = NULL;
ID3D10Buffer*               g_pIndexBuffer = NULL;
ID3D10EffectMatrixVariable* g_pWorldVariable = NULL;
ID3D10EffectMatrixVariable* g_pViewVariable = NULL;
ID3D10EffectMatrixVariable* g_pProjectionVariable = NULL;
D3DXMATRIX                  g_World;
D3DXMATRIX                  g_View;
D3DXMATRIX                  g_Projection;

vector<Node*>				g_nodes;
vector<SimpleVertex>		g_EdgeVertices;
int							g_numEdges = 0;
int							g_curFrame = 0;
int							g_numFrames = 0;
float						g_frameTime = 0;

//--------------------------------------------------------------------------------------
// Forward declarations
//--------------------------------------------------------------------------------------
HRESULT InitWindow( HINSTANCE hInstance, int nCmdShow );
HRESULT InitDevice();
HRESULT CreateVertexBuffer( int numVertices );
HRESULT CreateIndexBuffer( int numIndicies );
HRESULT ReadBVH();
HRESULT ProcessHierarchy( vector<string> lines, int * curLine, int * numEdges );
HRESULT ProcessMotionData( vector<string> lines, int * curLine );
HRESULT InitNodeFrames( Node * node, int * dataIndex, vector<float> data );
D3DXMATRIX getNodeRotation( Node * Node, int * dataIndex, vector<float> data );
D3DXMATRIX getNodeTranslation( Node * Node, int * dataIndex, vector<float> data );
void CleanupDevice();
void CleanupNodes();
void CleanupNode( Node * node );
LRESULT CALLBACK    WndProc( HWND, UINT, WPARAM, LPARAM );
void Render();
void RenderNode( Node *node, D3DXMATRIX world );


//--------------------------------------------------------------------------------------
// Entry point to the program. Initializes everything and goes into a message processing 
// loop. Idle time is used to render the scene.
//--------------------------------------------------------------------------------------
int WINAPI wWinMain( HINSTANCE hInstance, HINSTANCE hPrevInstance, LPWSTR lpCmdLine, int nCmdShow )
{
    if( FAILED( InitWindow( hInstance, nCmdShow ) ) )
        return E_FAIL;

    if( FAILED( InitDevice() ) )
    {
        CleanupDevice();
        return E_FAIL;
    }
	
    if( FAILED( ReadBVH() ) )
		return E_FAIL;

	if( FAILED( CreateVertexBuffer( g_numEdges * 2 ) ) )
		return E_FAIL;

	if( FAILED( CreateIndexBuffer( g_numEdges * 2 ) ) )
		return E_FAIL;

    // Main message loop
    MSG msg = {0};
    while( WM_QUIT != msg.message )
    {
        if( PeekMessage( &msg, NULL, 0, 0, PM_REMOVE ) )
        {
            TranslateMessage( &msg );
            DispatchMessage( &msg );
        }
        else
        {
            Render();
        }
    }
	
	CleanupNodes();
    CleanupDevice();

    return ( int )msg.wParam;
}


//--------------------------------------------------------------------------------------
// Register class and create window
//--------------------------------------------------------------------------------------
HRESULT InitWindow( HINSTANCE hInstance, int nCmdShow )
{
    // Register class
    WNDCLASSEX wcex;
    wcex.cbSize = sizeof( WNDCLASSEX );
    wcex.style = CS_HREDRAW | CS_VREDRAW;
    wcex.lpfnWndProc = WndProc;
    wcex.cbClsExtra = 0;
    wcex.cbWndExtra = 0;
    wcex.hInstance = hInstance;
    wcex.hIcon = LoadIcon( hInstance, ( LPCTSTR )IDI_TUTORIAL1 );
    wcex.hCursor = LoadCursor( NULL, IDC_ARROW );
    wcex.hbrBackground = ( HBRUSH )( COLOR_WINDOW + 1 );
    wcex.lpszMenuName = NULL;
    wcex.lpszClassName = L"BVHTester";
    wcex.hIconSm = LoadIcon( wcex.hInstance, ( LPCTSTR )IDI_TUTORIAL1 );
    if( !RegisterClassEx( &wcex ) )
        return E_FAIL;

    // Create window
    g_hInst = hInstance;
    RECT rc = { 0, 0, 640, 480 };
    AdjustWindowRect( &rc, WS_OVERLAPPEDWINDOW, FALSE );
    g_hWnd = CreateWindow( L"BVHTester", L"BVHTester", WS_OVERLAPPEDWINDOW,
                           CW_USEDEFAULT, CW_USEDEFAULT, rc.right - rc.left, rc.bottom - rc.top, NULL, NULL, hInstance,
                           NULL );
    if( !g_hWnd )
        return E_FAIL;

    ShowWindow( g_hWnd, nCmdShow );

    return S_OK;
}


//--------------------------------------------------------------------------------------
// Create Direct3D device and swap chain
//--------------------------------------------------------------------------------------
HRESULT InitDevice()
{
    HRESULT hr = S_OK;

    RECT rc;
    GetClientRect( g_hWnd, &rc );
    UINT width = rc.right - rc.left;
    UINT height = rc.bottom - rc.top;

    UINT createDeviceFlags = 0;
#ifdef _DEBUG
    createDeviceFlags |= D3D10_CREATE_DEVICE_DEBUG;
#endif

    D3D10_DRIVER_TYPE driverTypes[] =
    {
        D3D10_DRIVER_TYPE_HARDWARE,
        D3D10_DRIVER_TYPE_REFERENCE,
    };
    UINT numDriverTypes = sizeof( driverTypes ) / sizeof( driverTypes[0] );

    DXGI_SWAP_CHAIN_DESC sd;
    ZeroMemory( &sd, sizeof( sd ) );
    sd.BufferCount = 1;
    sd.BufferDesc.Width = width;
    sd.BufferDesc.Height = height;
    sd.BufferDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
    sd.BufferDesc.RefreshRate.Numerator = 60;
    sd.BufferDesc.RefreshRate.Denominator = 1;
    sd.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
    sd.OutputWindow = g_hWnd;
    sd.SampleDesc.Count = 1;
    sd.SampleDesc.Quality = 0;
    sd.Windowed = TRUE;

    for( UINT driverTypeIndex = 0; driverTypeIndex < numDriverTypes; driverTypeIndex++ )
    {
        g_driverType = driverTypes[driverTypeIndex];
        hr = D3D10CreateDeviceAndSwapChain( NULL, g_driverType, NULL, createDeviceFlags,
                                            D3D10_SDK_VERSION, &sd, &g_pSwapChain, &g_pd3dDevice );
        if( SUCCEEDED( hr ) )
            break;
    }
    if( FAILED( hr ) )
        return hr;

    // Create a render target view
    ID3D10Texture2D* pBuffer;
    hr = g_pSwapChain->GetBuffer( 0, __uuidof( ID3D10Texture2D ), ( LPVOID* )&pBuffer );
    if( FAILED( hr ) )
        return hr;

    hr = g_pd3dDevice->CreateRenderTargetView( pBuffer, NULL, &g_pRenderTargetView );
    pBuffer->Release();
    if( FAILED( hr ) )
        return hr;

    // Create depth stencil texture
    D3D10_TEXTURE2D_DESC descDepth;
    descDepth.Width = width;
    descDepth.Height = height;
    descDepth.MipLevels = 1;
    descDepth.ArraySize = 1;
    descDepth.Format = DXGI_FORMAT_D32_FLOAT;
    descDepth.SampleDesc.Count = 1;
    descDepth.SampleDesc.Quality = 0;
    descDepth.Usage = D3D10_USAGE_DEFAULT;
    descDepth.BindFlags = D3D10_BIND_DEPTH_STENCIL;
    descDepth.CPUAccessFlags = 0;
    descDepth.MiscFlags = 0;
    hr = g_pd3dDevice->CreateTexture2D( &descDepth, NULL, &g_pDepthStencil );
    if( FAILED( hr ) )
        return hr;

    // Create the depth stencil view
    D3D10_DEPTH_STENCIL_VIEW_DESC descDSV;
    descDSV.Format = descDepth.Format;
    descDSV.ViewDimension = D3D10_DSV_DIMENSION_TEXTURE2D;
    descDSV.Texture2D.MipSlice = 0;
    hr = g_pd3dDevice->CreateDepthStencilView( g_pDepthStencil, &descDSV, &g_pDepthStencilView );
    if( FAILED( hr ) )
        return hr;

    g_pd3dDevice->OMSetRenderTargets( 1, &g_pRenderTargetView, g_pDepthStencilView );

    // Setup the viewport
    D3D10_VIEWPORT vp;
    vp.Width = width;
    vp.Height = height;
    vp.MinDepth = 0.0f;
    vp.MaxDepth = 1.0f;
    vp.TopLeftX = 0;
    vp.TopLeftY = 0;
    g_pd3dDevice->RSSetViewports( 1, &vp );

    // Create the effect
    DWORD dwShaderFlags = D3D10_SHADER_ENABLE_STRICTNESS;
#if defined( DEBUG ) || defined( _DEBUG )
    // Set the D3D10_SHADER_DEBUG flag to embed debug information in the shaders.
    // Setting this flag improves the shader debugging experience, but still allows 
    // the shaders to be optimized and to run exactly the way they will run in 
    // the release configuration of this program.
    dwShaderFlags |= D3D10_SHADER_DEBUG;
    #endif
    hr = D3DX10CreateEffectFromFile( L"BVHTester.fx", NULL, NULL, "fx_4_0", dwShaderFlags, 0, g_pd3dDevice, NULL,
                                         NULL, &g_pEffect, NULL, NULL );
    if( FAILED( hr ) )
    {
        MessageBox( NULL,
                    L"The FX file cannot be located.  Please run this executable from the directory that contains the FX file.", L"Error", MB_OK );
        return hr;
    }

    // Obtain the technique
    g_pTechnique = g_pEffect->GetTechniqueByName( "Render" );

    // Obtain the variables
    g_pWorldVariable = g_pEffect->GetVariableByName( "World" )->AsMatrix();
    g_pViewVariable = g_pEffect->GetVariableByName( "View" )->AsMatrix();
    g_pProjectionVariable = g_pEffect->GetVariableByName( "Projection" )->AsMatrix();

    // Define the input layout
    D3D10_INPUT_ELEMENT_DESC layout[] =
    {
        { "POSITION", 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 0, D3D10_INPUT_PER_VERTEX_DATA, 0 },
        { "COLOR", 0, DXGI_FORMAT_R32G32B32A32_FLOAT, 0, 12, D3D10_INPUT_PER_VERTEX_DATA, 0 },
    };
    UINT numElements = sizeof( layout ) / sizeof( layout[0] );

    // Create the input layout
    D3D10_PASS_DESC PassDesc;
    g_pTechnique->GetPassByIndex( 0 )->GetDesc( &PassDesc );
    hr = g_pd3dDevice->CreateInputLayout( layout, numElements, PassDesc.pIAInputSignature,
                                          PassDesc.IAInputSignatureSize, &g_pVertexLayout );
    if( FAILED( hr ) )
        return hr;

    // Set the input layout
    g_pd3dDevice->IASetInputLayout( g_pVertexLayout );

    // Create vertex buffer (cube)
    /*SimpleVertex vertices[] =
    {
        { D3DXVECTOR3( -1.0f, 1.0f, -1.0f ), D3DXVECTOR4( 0.0f, 0.0f, 1.0f, 1.0f ) },
        { D3DXVECTOR3( 1.0f, 1.0f, -1.0f ), D3DXVECTOR4( 0.0f, 1.0f, 0.0f, 1.0f ) },
        { D3DXVECTOR3( 1.0f, 1.0f, 1.0f ), D3DXVECTOR4( 0.0f, 1.0f, 1.0f, 1.0f ) },
        { D3DXVECTOR3( -1.0f, 1.0f, 1.0f ), D3DXVECTOR4( 1.0f, 0.0f, 0.0f, 1.0f ) },
        { D3DXVECTOR3( -1.0f, -1.0f, -1.0f ), D3DXVECTOR4( 1.0f, 0.0f, 1.0f, 1.0f ) },
        { D3DXVECTOR3( 1.0f, -1.0f, -1.0f ), D3DXVECTOR4( 1.0f, 1.0f, 0.0f, 1.0f ) },
        { D3DXVECTOR3( 1.0f, -1.0f, 1.0f ), D3DXVECTOR4( 1.0f, 1.0f, 1.0f, 1.0f ) },
        { D3DXVECTOR3( -1.0f, -1.0f, 1.0f ), D3DXVECTOR4( 0.0f, 0.0f, 0.0f, 1.0f ) },
    };*/

    /*D3D10_BUFFER_DESC bd;
    bd.Usage = D3D10_USAGE_DEFAULT;
    bd.ByteWidth = sizeof( SimpleVertex ) * 8;
    bd.BindFlags = D3D10_BIND_VERTEX_BUFFER;
    bd.CPUAccessFlags = 0;
    bd.MiscFlags = 0;
    D3D10_SUBRESOURCE_DATA InitData;
    InitData.pSysMem = &g_EdgeVertices;
    hr = g_pd3dDevice->CreateBuffer( &bd, &InitData, &g_pVertexBuffer );
    if( FAILED( hr ) )
        return hr;*/
	
    // Create vertex buffer (edges)
	/*bd.Usage = D3D10_USAGE_DYNAMIC;
	bd.CPUAccessFlags = D3D10_CPU_ACCESS_WRITE;
    D3D10_SUBRESOURCE_DATA InitData2;
    InitData2.pSysMem = &g_EdgeVertices;
    hr = g_pd3dDevice->CreateBuffer( &bd, &InitData2, &g_pVertexBuffer[1] );
    if( FAILED( hr ) )
        return hr;*/

    // Create index buffer
    /*DWORD indices[] =
    {
        3,1,0,
        2,1,3,

        0,5,4,
        1,5,0,

        3,4,7,
        0,4,3,

        1,6,5,
        2,6,1,

        2,7,6,
        3,7,2,

        6,4,5,
        7,4,6,
    };
    bd.Usage = D3D10_USAGE_DEFAULT;
    bd.ByteWidth = sizeof( DWORD ) * 36;
    bd.BindFlags = D3D10_BIND_INDEX_BUFFER;
    bd.CPUAccessFlags = 0;
    bd.MiscFlags = 0;
    InitData.pSysMem = indices;
    hr = g_pd3dDevice->CreateBuffer( &bd, &InitData, &g_pIndexBuffer );
    if( FAILED( hr ) )
        return hr;*/

    // Set index buffer
    //g_pd3dDevice->IASetIndexBuffer( g_pIndexBuffer, DXGI_FORMAT_R32_UINT, 0 );
	
    // Set line primitive topology
    g_pd3dDevice->IASetPrimitiveTopology( D3D10_PRIMITIVE_TOPOLOGY_LINELIST );

    // Initialize the world matrix
    D3DXMatrixIdentity( &g_World );

    // Initialize the view matrix
    D3DXVECTOR3 Eye( 500.0f, 10.0f, 500.0f );
    D3DXVECTOR3 At( 0.0f, 0.0f, 0.0f );
    D3DXVECTOR3 Up( 0.0f, 1.0f, 0.0f );
    D3DXMatrixLookAtLH( &g_View, &Eye, &At, &Up );

    // Initialize the projection matrix
    D3DXMatrixPerspectiveFovLH( &g_Projection, ( float )D3DX_PI * 0.25f, width / ( FLOAT )height, 0.1f, 1000.0f );
    g_pProjectionVariable->SetMatrix( ( float* )&g_Projection );

    return TRUE;
}

HRESULT CreateVertexBuffer(int numVertices)
{
    D3D10_BUFFER_DESC bd;
    bd.Usage = D3D10_USAGE_DYNAMIC;
	bd.ByteWidth = sizeof( SimpleVertex ) * numVertices;
    bd.BindFlags = D3D10_BIND_VERTEX_BUFFER;
    bd.CPUAccessFlags = D3D10_CPU_ACCESS_WRITE;
    bd.MiscFlags = 0;

	//bd.Usage = D3D10_USAGE_DYNAMIC;
	//bd.CPUAccessFlags = D3D10_CPU_ACCESS_WRITE;
    //D3D10_SUBRESOURCE_DATA InitData;
    //InitData.pSysMem = &g_EdgeVertices[0];
	HRESULT hr = g_pd3dDevice->CreateBuffer( &bd, NULL, &g_pVertexBuffer );
    if( FAILED( hr ) )
        return hr;

	UINT stride = sizeof( SimpleVertex );
	UINT offset = 0;
    g_pd3dDevice->IASetVertexBuffers( 0, 1, &g_pVertexBuffer, &stride, &offset );

	return hr;
}

HRESULT CreateIndexBuffer(int numIndicies)
{
	vector<int> indices;
	for(int i = 0; i < numIndicies; ++i)
	{
		indices.push_back(i);
	}
	
    D3D10_BUFFER_DESC bd;
	bd.Usage = D3D10_USAGE_DEFAULT;
    bd.ByteWidth = sizeof( DWORD ) * numIndicies;
    bd.BindFlags = D3D10_BIND_INDEX_BUFFER;
    bd.CPUAccessFlags = 0;
    bd.MiscFlags = 0;
    D3D10_SUBRESOURCE_DATA InitData;
    InitData.pSysMem = &indices[0];
	HRESULT hr = g_pd3dDevice->CreateBuffer( &bd, &InitData, &g_pIndexBuffer ); 

    if( FAILED( hr ) )
        return hr;

    g_pd3dDevice->IASetIndexBuffer( g_pIndexBuffer, DXGI_FORMAT_R32_UINT, 0 );
	
	return hr;
}

//--------------------------------------------------------------------------------------
// Read and process the BVH file
//--------------------------------------------------------------------------------------
HRESULT ReadBVH()
{
	ifstream * bvhFile = new ifstream();
	bvhFile->open("wave.bvh");
	
	if(!bvhFile->is_open())
	{
		cerr << "BVH File not opened.";
		return E_FAIL;
	}

	string line;

	vector<string> lines;

	while(!bvhFile->eof()) 
	{
		getline(*bvhFile, line);
		lines.push_back(line);
	}

	bvhFile->close();
	delete bvhFile;

	if (lines[0] != "HIERARCHY" && lines[0] != "﻿HIERARCHY")
		return E_FAIL;

	int lineNum = 1;

	if( FAILED(ProcessHierarchy(lines, &lineNum, &g_numEdges)))
		return E_FAIL;

	if (lines[lineNum++] != "MOTION")
		return E_FAIL;
	
	if( FAILED(ProcessMotionData(lines, &lineNum)))
		return E_FAIL;

	return S_OK;
}


//--------------------------------------------------------------------------------------
// Read and process the Hierarchy section of the BVH file
//
// Parameters:
// lines - Vector of strings, each is a line from the bvh file
// lineNum - Current line number being processed
// numEdges - Number of edges - parent/child node relations
//--------------------------------------------------------------------------------------
HRESULT ProcessHierarchy(vector<string> lines, int * lineNum, int * numEdges)
{
    int depth = 0;
	
    Node * curNode;

    do
    {
		// Process each line, the first substring of each line determines the process

		string line = lines[(*lineNum)++];

		// position of the index to start string comparison
		// (skip leading white space)
		int pos = line.find_first_not_of(' \t', 0);

		if(line.length() > 0) {

			
			// split each line into a vector of strings
			/*vector<string> line;
			int i, s = 0;
			string::size_type loc = stringLine.find(' ', s);

			while(loc != string::npos)
			{
				line.push_back(stringLine.substr(s, loc));
				s = loc;
				loc = stringLine.find(' ', s);
			}*/

			//char * segment = strtok((char*)line.c_str(), " ");

			if (line.compare(0, 1, "{") == 0) {} // do nothing
			else if (line.compare(pos, 1, "}") == 0)
			{
				Node * parent = curNode->GetParent();
				if (parent != NULL)
				{
					++*numEdges;
					curNode = parent;
				}
				--depth;
			} 
			else if (line.compare(pos, 4, "ROOT") == 0)
			{
				curNode = new Node(line.substr(pos, 4));

				g_nodes.push_back(curNode);

				++depth;
			}
			else if (line.compare(pos, 5, "JOINT") == 0)
			{
				Node * joint = new Node(line.substr(pos, 5), curNode);
				curNode->AddChild(joint);
				curNode = joint;

				++depth;
			}
			else if (line.compare(pos, 3, "End") == 0)
			{
				Node * end = new Node("", curNode);
				curNode->AddChild(end);
				curNode = end;

				++depth;
			}
			else if (line.compare(pos, 6, "OFFSET") == 0)
			{
				D3DXVECTOR3 offset;

				pos += 6;
				int endPos = line.find(' \t', pos + 1);
				offset.x = (float)strtod(line.substr(pos, endPos - pos).c_str(), NULL);

				pos = endPos + 1;
				endPos = line.find(' \t', pos);
				offset.y = (float)strtod(line.substr(pos, endPos - pos).c_str(), NULL);

				pos = endPos + 1;
				endPos = line.find(' \t', pos);
				offset.z = (float)strtod(line.substr(pos).c_str(), NULL);

				curNode->SetOffset(offset);
			}
			else if (line.compare(pos, 8, "CHANNELS") == 0)
			{
				pos += 8;
				int endPos = line.find(' \t', pos + 1);
				int numChannels = (int)strtol(line.substr(pos, endPos - pos).c_str(), NULL, 10);

				for (int c = 2; c < numChannels + 2; ++c)
				{
					Channel channel = Channel::None;

					pos = endPos + 1;
					endPos = line.find(' \t', pos);
					channel = parseChannel(line.substr(pos, endPos - pos));
					
					curNode->AddChannel(channel);
				}
			}
		}
    } while (depth > 0);

	return S_OK;
}

HRESULT ProcessMotionData(vector<string> lines, int * lineNum)
{
    while(*lineNum < lines.size() - 1) 
    {        
		string line = lines[(*lineNum)++];

		if (line.compare(0, 7, "Frames:") == 0)
        {
			int begPos = line.find_first_not_of(' ', 8);
			g_numFrames = (int)strtol(line.substr(begPos).c_str(), NULL, 10);
        }
		else if (line.compare(0, 11, "Frame Time:") == 0)
        {
			int begPos = line.find_first_not_of(' ', 12);
			g_frameTime = (float)strtod(line.substr(begPos).c_str(), NULL);
        }
        else
        {
			// Parse the frame data.
			//
			// Each line is one sample of motion data. 
            // The numbers appear in the order of the channel specifications 
			// as the skeleton hierarchy was parsed.
			
			vector<float> data;

			char * lineEnd = (char *)line.c_str();
			while(errno != ERANGE && *lineEnd)
			{
				data.push_back((float)strtod(lineEnd, &lineEnd));
			} 

			// recursively iterate through nodes, adding a KeyFrame to each
			int dataIndex = 0;
			for(int i = 0; i < g_nodes.size(); i++)
			{
				if(FAILED(InitNodeFrames(g_nodes[i], &dataIndex, data)))
					return E_FAIL;
			}

			//g_frames.push_back(data);
        }
    }

	return S_OK;
}

HRESULT InitNodeFrames(Node * node, int * dataIndex, vector<float> data) 
{
	//if(*dataIndex >= data.size())
		//return E_FAIL;
	vector<Node*> children = node->GetChildren();
	if(children.size() > 0)
	{
		KeyFrame keyFrame;
		keyFrame.translation = getNodeTranslation(node, dataIndex, data);
		keyFrame.rotation = getNodeRotation(node, dataIndex, data);
		node->AddKeyFrame(keyFrame);
			
		for(int i = 0; i < children.size(); i++)
		{
			InitNodeFrames(children[i], dataIndex, data);
		}
	}

	return S_OK;
}

D3DXMATRIX getNodeRotation(Node * Node, int * dataIndex, vector<float> data)
{
	D3DXMATRIX x, y, z;
	D3DXMatrixIdentity(&x);
	D3DXMatrixIdentity(&y);
	D3DXMatrixIdentity(&z);

	vector<Channel> channels = Node->GetChannels();

	for(int i = 0; i < channels.size(); ++i)
	{
		if ((channels[i] & Channel::Xrotation) == Channel::Xrotation)
		{
			D3DXMatrixRotationX(&x, data[(*dataIndex)++] * (D3DX_PI / 180.0f));
		}
		else if ((channels[i] & Channel::Yrotation) == Channel::Yrotation)
		{
			D3DXMatrixRotationY(&y, data[(*dataIndex)++] * (D3DX_PI / 180.0f));
		}
		else if ((channels[i] & Channel::Zrotation) == Channel::Zrotation)
		{
			D3DXMatrixRotationZ(&z, data[(*dataIndex)++] * (D3DX_PI / 180.0f));			
		}
	}
	
    return y * x * z;
}

D3DXMATRIX getNodeTranslation(Node * Node, int * dataIndex, vector<float> data)
{
	float x = 0, y = 0, z = 0;

	vector<Channel> channels = Node->GetChannels();

	for(int i = 0; i < channels.size(); ++i)
	{
		if ((channels[i] & Channel::Xposition) == Channel::Xposition)
		{
			x = data[(*dataIndex)++];
		}
		else if ((channels[i] & Channel::Yposition) == Channel::Yposition)
		{
			y = data[(*dataIndex)++];
		}
		else if ((channels[i] & Channel::Zposition) == Channel::Zposition)
		{
			z = data[(*dataIndex)++];
		}
	}

	D3DXMATRIX translation;
	D3DXMatrixTranslation(&translation, x, y, z);

    return translation;
}

//--------------------------------------------------------------------------------------
// Clean up the objects we've created
//--------------------------------------------------------------------------------------
void CleanupDevice()
{
    if( g_pd3dDevice ) g_pd3dDevice->ClearState();

    if( g_pVertexBuffer ) g_pVertexBuffer->Release();
    if( g_pIndexBuffer ) g_pIndexBuffer->Release();
    if( g_pVertexLayout ) g_pVertexLayout->Release();
    if( g_pEffect ) g_pEffect->Release();
    if( g_pRenderTargetView ) g_pRenderTargetView->Release();
    if( g_pDepthStencil ) g_pDepthStencil->Release();
    if( g_pDepthStencilView ) g_pDepthStencilView->Release();
    if( g_pSwapChain ) g_pSwapChain->Release();
    if( g_pd3dDevice ) g_pd3dDevice->Release();
}

void CleanupNodes()
{
	for(int i = 0; i < g_nodes.size(); ++i)
	{
		CleanupNode(g_nodes[i]);
	}
}

void CleanupNode(Node * node)
{
	vector<Node*> children = node->GetChildren();
	for(int i = 0; i < children.size(); ++i) 
	{
		CleanupNode(children[i]);
	}
}

//--------------------------------------------------------------------------------------
// Called every time the application receives a message
//--------------------------------------------------------------------------------------
LRESULT CALLBACK WndProc( HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam )
{
    PAINTSTRUCT ps;
    HDC hdc;

    switch( message )
    {
        case WM_PAINT:
            hdc = BeginPaint( hWnd, &ps );
            EndPaint( hWnd, &ps );
            break;

        case WM_DESTROY:
            PostQuitMessage( 0 );
            break;

        default:
            return DefWindowProc( hWnd, message, wParam, lParam );
    }

    return 0;
}


//--------------------------------------------------------------------------------------
// Render a frame
//--------------------------------------------------------------------------------------
void Render()
{
	// Update time and frame, looping last frame to first frame
    static float t = 0.0f;
    static DWORD dwTimeStart = 0;
    DWORD dwTimeCur = GetTickCount();
    if( dwTimeStart == 0 )
        dwTimeStart = dwTimeCur;

    t = ( dwTimeCur - dwTimeStart ) / 1000.0f;
	g_curFrame = (int)(t / g_frameTime) % g_numFrames;

    //
    // Clear the back buffer
    //
    float ClearColor[4] = { 0.0f, 0.125f, 0.3f, 1.0f }; //red, green, blue, alpha
    g_pd3dDevice->ClearRenderTargetView( g_pRenderTargetView, ClearColor );

    //
    // Clear the depth buffer to 1.0 (max depth)
    //
    g_pd3dDevice->ClearDepthStencilView( g_pDepthStencilView, D3D10_CLEAR_DEPTH, 1.0f, 0 );

	//
	// Update view matrix - point at first root node
	//
	D3DXVECTOR3 Eye( 500.0f, 10.0f, 500.0f );
	KeyFrame keyFrame = g_nodes[0]->GetKeyFrame(g_curFrame);	
	D3DXVECTOR3 At, scale;
	D3DXQUATERNION rot;
	if(FAILED(D3DXMatrixDecompose(&scale, &rot, &At, &keyFrame.translation)))
		return;
	D3DXVECTOR3 Up( 0.0f, 1.0f, 0.0f );
	D3DXMatrixLookAtLH( &g_View, &Eye, &At, &Up );
    g_pViewVariable->SetMatrix( ( float* )&g_View );
    
    // Set cube vertex buffer
	/*UINT stride = sizeof( SimpleVertex );
	UINT offset = 0;
    g_pd3dDevice->IASetVertexBuffers( 0, 1, &g_pVertexBuffer, &stride, &offset );*/

	// Set triangle primitive topology
    //g_pd3dDevice->IASetPrimitiveTopology( D3D10_PRIMITIVE_TOPOLOGY_TRIANGLELIST );

	//
	// Render the nodes
	//
	g_EdgeVertices.clear();
	for(int i = 0; i < g_nodes.size(); ++i)
	{
		RenderNode(g_nodes[i], g_World);
	}

	// Set edge vertex data
	SimpleVertex *pData = NULL;
	if( SUCCEEDED( g_pVertexBuffer->Map( D3D10_MAP_WRITE_DISCARD, 0, reinterpret_cast< void** >( &pData ) ) ) )
	{
		  memcpy( pData, &g_EdgeVertices[0], sizeof( SimpleVertex ) * g_numEdges * 2 );
		  g_pVertexBuffer->Unmap( );
	}

    //
    // Update variables for the edges
    //
    g_pWorldVariable->SetMatrix( ( float* )&g_World );

    //
    // Render the edges
    //
    D3D10_TECHNIQUE_DESC techDesc;
    g_pTechnique->GetDesc( &techDesc );
    for( UINT p = 0; p < techDesc.Passes; ++p )
    {
        g_pTechnique->GetPassByIndex( p )->Apply( 0 );
		g_pd3dDevice->DrawIndexed(g_numEdges * 2, 0, 0);
    }	

    //
    // Present our back buffer to our front buffer
    //
    g_pSwapChain->Present( 0, 0 );
}

void RenderNode(Node * node, D3DXMATRIX world)
{
	D3DXVECTOR3 offset = node->GetOffset();
    D3DXMATRIX mOffset;
	D3DXMatrixTranslation(&mOffset, offset.x, offset.y, offset.z);

	Node * parent = node->GetParent();
	if( parent != NULL ) 
	{
		SimpleVertex vertex;
		vertex.Pos = D3DXVECTOR3( 0, 0, 0 );
		vertex.Color = D3DXVECTOR4( 1, 1, 1, 1 );
		D3DXVec3TransformCoord( &vertex.Pos, &vertex.Pos, &world );
		g_EdgeVertices.push_back( vertex );
	}

	if( g_curFrame < node->GetNumKeyFrames() )
	{
		KeyFrame keyFrame = node->GetKeyFrame( g_curFrame );
		world = keyFrame.rotation * keyFrame.translation * mOffset * world;
	} else {
		world = mOffset * world;	
	}	

	if( parent != NULL ) 
	{
		SimpleVertex vertex;
		vertex.Pos = D3DXVECTOR3( 0, 0, 0 );
		vertex.Color = D3DXVECTOR4( 1, 1, 1, 1 );
		D3DXVec3TransformCoord( &vertex.Pos, &vertex.Pos, &world );
		g_EdgeVertices.push_back(vertex);
	}

    //
    // Update variables for the cube
    //
    //g_pWorldVariable->SetMatrix( ( float* )&world );

    //
    // Render the cube
    //
   /* D3D10_TECHNIQUE_DESC techDesc;
    g_pTechnique->GetDesc( &techDesc );
    for( UINT p = 0; p < techDesc.Passes; ++p )
    {
        g_pTechnique->GetPassByIndex( p )->Apply( 0 );
        g_pd3dDevice->DrawIndexed( 36, 0, 0 );
    }*/
	
	//
	// Render the children
	//
	vector<Node*> children = node->GetChildren();
	for(int i = 0; i < children.size(); ++i)
	{
		RenderNode(children[i], world);
	}
}