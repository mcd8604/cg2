// BVHFigure.cpp
//
// Authors: 
//	Mike DeMauro
//
// Summary: 
//	This class is used to:
//	- Process node and frame data from a Biovision Heirarchy (BVH) file.
//  - Render the animated figure using DirectX 10.

#include "BVHFigure.h"

/// <summary>
/// Creates a BVHFigure with no nodes. Use BVHFigure::ReadBVH to 
/// </summary>
BVHFigure::BVHFigure(void)
{
	edgeVertexBuffer = NULL;
	edgeIndexBuffer = NULL;
	cubeVertexBuffer = NULL;
	cubeIndexBuffer = NULL;
	techniqueRender = NULL;
	d3dDevice = NULL;
	worldVariable = NULL;
	numEdges = 0;
	curFrame = 0;
	numFrames = 0;
	frameTime = 0;
}

/// <summary>
/// Deconstructor for BVHFigure
/// </summary>
BVHFigure::~BVHFigure(void)
{
}

/// <summary>
/// Releases buffers and deletes all nodes.
/// </summary>
void BVHFigure::Cleanup()
{
    if( edgeVertexBuffer ) edgeVertexBuffer->Release();
    if( edgeIndexBuffer ) edgeIndexBuffer->Release();
    if( cubeVertexBuffer ) cubeVertexBuffer->Release();
    if( cubeIndexBuffer ) cubeIndexBuffer->Release();
	for( int i = 0; i < nodes.size(); ++i )
	{
		nodes[i]->Cleanup();
		delete nodes[i];
	}
}

/// <summary>
/// Read and process the BVH file.
/// </summary>
/// <param name='fileName'>Name of BVH file to process.</param>
HRESULT BVHFigure::ReadBVH( string fileName )
{
	// Open the file 

	ifstream * bvhFile = new ifstream();
	bvhFile->open( fileName.c_str() );
	
	if( !bvhFile->is_open() )
	{
#if DEBUG
		cerr << "BvVHFigure::ReadBVH: BVH File not opened.";
#endif
		return E_FAIL;
	}

	// Read lines of the file into a vector of strings

	string line;
	vector<string> lines;

	while( !bvhFile->eof() ) 
	{
		getline( *bvhFile, line );
		lines.push_back( line );
	}

	// Close the file

	bvhFile->close();
	delete bvhFile;

	// Process the lines

	if ( lines[0] != "HIERARCHY" && lines[0] != "﻿HIERARCHY" )
		return E_FAIL;

	int lineNum = 1;

	if ( FAILED(ProcessHierarchy(lines, &lineNum, &numEdges ) ) )
		return E_FAIL;

	if ( lines[lineNum++] != "MOTION" )
		return E_FAIL;
	
	if( FAILED( ProcessMotionData( lines, &lineNum ) ) )
		return E_FAIL;

	return S_OK;
}

/// <summary>Read and process the Hierarchy section of the BVH file.</summary>
/// <param name='lines'>Vector of strings, each is a line from the bvh file.</param>
/// <param name='lineNum'>Current line number being processed.</param>
/// <param name='numEdges'>Pointer to a value to set number of edges ( parent/child node relations ).</param>
HRESULT BVHFigure::ProcessHierarchy( vector<string> lines, int * lineNum, int * numEdges )
{
    int depth = 0;
	
    BVHNode * curBVHNode;

    do
    {
		// Process each line, the first substring of each line determines which process

		string line = lines[( *lineNum )++];

		// position of the index to start string comparison

		int pos = line.find_first_not_of( ' \t', 0 );

		if( line.length() > 0 ) {

			if ( line.compare( 0, 1, "{" ) == 0 ) {} // do nothing
			else if ( line.compare( pos, 1, "}" ) == 0 )
			{
				BVHNode * parent = curBVHNode->GetParent();
				if ( parent != NULL )
				{
					++*numEdges;
					curBVHNode = parent;
				}
				--depth;
			} 
			else if ( line.compare( pos, 4, "ROOT" ) == 0)
			{
				curBVHNode = new BVHNode( line.substr( pos, 4 ) );
				nodes.push_back( curBVHNode );

				++depth;
			}
			else if ( line.compare( pos, 5, "JOINT" ) == 0 )
			{
				BVHNode * joint = new BVHNode( line.substr( pos, 5 ), curBVHNode );
				curBVHNode->AddChild( joint );
				curBVHNode = joint;

				++depth;
			}
			else if ( line.compare( pos, 3, "End" ) == 0 )
			{
				BVHNode * end = new BVHNode( "", curBVHNode );
				curBVHNode->AddChild( end );
				curBVHNode = end;

				++depth;
			}
			else if ( line.compare( pos, 6, "OFFSET" ) == 0 )
			{
				D3DXVECTOR3 offset;

				pos += 6;
				int endPos = line.find( ' \t', pos + 1 );
				offset.x = ( float )strtod( line.substr( pos, endPos - pos ).c_str(), NULL );

				pos = endPos + 1;
				endPos = line.find( ' \t', pos );
				offset.y = ( float )strtod( line.substr( pos, endPos - pos ).c_str(), NULL );

				pos = endPos + 1;
				endPos = line.find( ' \t', pos );
				offset.z = ( float )strtod( line.substr( pos ).c_str(), NULL );

				curBVHNode->SetOffset( offset );
			}
			else if ( line.compare( pos, 8, "CHANNELS" ) == 0 )
			{
				pos += 8;
				int endPos = line.find( ' \t', pos + 1 );
				int numChannels = ( int )strtol( line.substr( pos, endPos - pos ).c_str(), NULL, 10 );

				for ( int c = 2; c < numChannels + 2; ++c )
				{
					Channel channel = Channel::None;

					pos = endPos + 1;
					endPos = line.find( ' \t', pos );
					channel = parseChannel( line.substr( pos, endPos - pos ) );
					
					curBVHNode->AddChannel( channel );
				}
			}
		}
    } while ( depth > 0 );

	return S_OK;
}

/// <summary>Read and process the Motion data section of the BVH file.</summary>
/// <param name='lines'>Vector of strings, each is a line from the bvh file.</param>
/// <param name='lineNum'>Pointer to the current line number being processed.</param>
HRESULT BVHFigure::ProcessMotionData( vector<string> lines, int * lineNum )
{
    while( *lineNum < lines.size() - 1 ) 
    {        
		string line = lines[( *lineNum )++];

		if ( line.compare( 0, 7, "Frames:" ) == 0 )
        {
			int begPos = line.find_first_not_of( ' ', 8 );
			numFrames = ( int )strtol( line.substr( begPos ).c_str(), NULL, 10 );
        }
		else if ( line.compare( 0, 11, "Frame Time:" ) == 0 )
        {
			int begPos = line.find_first_not_of( ' ', 12 );
			frameTime = ( float )strtod( line.substr( begPos ).c_str(), NULL );
        }
        else
        {
			// Parse the frame data.
			//
			// Each line is one sample of motion data. 
            // The numbers appear in the order of the channel specifications 
			// as the skeleton hierarchy was parsed.
			
			vector<float> data;

			char * lineEnd = ( char * )line.c_str();
			while( errno != ERANGE && *lineEnd )
			{
				data.push_back( ( float )strtod( lineEnd, &lineEnd ) );
			} 

			// recursively iterate through nodes, adding a KeyFrame to each
			int dataIndex = 0;
			for( int i = 0; i < nodes.size(); i++ )
			{
				if( FAILED( InitBVHNodeFrames( nodes[i], data, &dataIndex ) ) )
					return E_FAIL;
			}
        }
    }

	return S_OK;
}

/// <summary>Creates a KeyFrame for a node and for it's children, recursively.</summary>
/// <param name='node'>The node to initialize.</param>
/// <param name='data'>The current vector of motion data.</param>
/// <param name='dataIndex'>Pointer to the current data index.</param>
HRESULT BVHFigure::InitBVHNodeFrames( BVHNode * node, vector<float> data, int * dataIndex ) 
{
	vector<BVHNode*> children = node->GetChildren();
	if( children.size() > 0 )
	{
		vector<Channel> channels = node->GetChannels();

		node->AddKeyFrame( 
			GetBVHNodeTranslation( channels, data, dataIndex ), 
			GetBVHNodeRotation( channels, data, dataIndex ) );
			
		for( int i = 0; i < children.size(); i++ )
		{
			InitBVHNodeFrames( children[i], data, dataIndex );
		}
	}

	return S_OK;
}

/// <summary>
///	Creates a rotation matrix given a node's channels.
/// </summary>
/// <param name='channels'>Channels of a node.</param>
/// <param name='data'>The current vector of motion data.</param>
/// <param name='dataIndex'>Pointer to the current data index.</param>
/// <returns>A rotation matrix.<returns>
D3DXMATRIX BVHFigure::GetBVHNodeRotation( vector<Channel> channels, vector<float> data, int * dataIndex )
{
	D3DXMATRIX rX, rY, rZ;
	D3DXMatrixIdentity( &rX );
	D3DXMatrixIdentity( &rY );
	D3DXMatrixIdentity( &rZ );

	for( int i = 0; i < channels.size(); ++i )
	{		
		if ( ( channels[i] & Channel::Xrotation ) == Channel::Xrotation )
		{
			D3DXMatrixRotationX( &rX, data[( *dataIndex )++] * ( D3DX_PI / 180.0f ) );
		}
		else if ( ( channels[i] & Channel::Yrotation ) == Channel::Yrotation )
		{
			D3DXMatrixRotationY( &rY, data[( *dataIndex )++] * ( D3DX_PI / 180.0f ) );
		}
		else if ( ( channels[i] & Channel::Zrotation ) == Channel::Zrotation )
		{
			D3DXMatrixRotationZ( &rZ, data[( *dataIndex )++] * ( D3DX_PI / 180.0f ) );			
		}
	}

	return rY * rX * rZ;
}

/// <summary>
///	Creates a translation matrix given a node's channels.
/// </summary>
/// <param name='channels'>Channels of a node.</param>
/// <param name='data'>The current vector of motion data.</param>
/// <param name='dataIndex'>Pointer to the current data index.</param>
/// <returns>A translation matrix.<returns>
D3DXMATRIX BVHFigure::GetBVHNodeTranslation( vector<Channel> channels, vector<float> data, int * dataIndex )
{
	float tX = 0, tY = 0, tZ = 0;

	for( int i = 0; i < channels.size(); ++i )
	{			
		if ( ( channels[i] & Channel::Xposition ) == Channel::Xposition )
		{
			tX = data[( *dataIndex )++];
		}
		else if ( ( channels[i] & Channel::Yposition ) == Channel::Yposition )
		{
			tY = data[( *dataIndex )++];
		}
		else if ( ( channels[i] & Channel::Zposition ) == Channel::Zposition )
		{
			tZ = data[( *dataIndex )++];
		}
	}

	D3DXMATRIX translation;
	D3DXMatrixTranslation(&translation, tX, tY, tZ);
	
	return translation;
}


/// <summary>
///	Initialize the graphics properties of the BVHFigure.
/// </summary>
/// <param name='d3dDevice'></param>
/// <param name='techniqueRender'></param>
/// <param name='worldVariable'></param>
HRESULT BVHFigure::Initialize( ID3D10Device * d3dDevice, 
							  ID3D10EffectTechnique * techniqueRender, 
							  ID3D10EffectMatrixVariable * worldVariable )
{
	this->d3dDevice = d3dDevice;
	this->techniqueRender = techniqueRender;
	this->worldVariable = worldVariable;

    // Initialize the world matrix
    D3DXMatrixIdentity( &world );

	return CreateVertexBuffer();
}

/// <summary>
///	Creates vertex and index buffers for the BVHFigure.
/// </summary>
HRESULT BVHFigure::CreateVertexBuffer()
{
	int numVertices = numEdges * 2;

    D3D10_BUFFER_DESC bd;
    bd.Usage = D3D10_USAGE_DYNAMIC;
	bd.ByteWidth = sizeof( SimpleVertex ) * numVertices;
    bd.BindFlags = D3D10_BIND_VERTEX_BUFFER;
    bd.CPUAccessFlags = D3D10_CPU_ACCESS_WRITE;
    bd.MiscFlags = 0;

	HRESULT hr = d3dDevice->CreateBuffer( &bd, NULL, &edgeVertexBuffer );

    if( FAILED( hr ) )
        return hr;

    // Create cube vertex buffer
    SimpleVertex vertices[] =
    {
        { D3DXVECTOR3( -1.0f, 1.0f, -1.0f ), D3DXVECTOR4( 0.0f, 0.0f, 1.0f, 1.0f ) },
        { D3DXVECTOR3( 1.0f, 1.0f, -1.0f ), D3DXVECTOR4( 0.0f, 1.0f, 0.0f, 1.0f ) },
        { D3DXVECTOR3( 1.0f, 1.0f, 1.0f ), D3DXVECTOR4( 0.0f, 1.0f, 1.0f, 1.0f ) },
        { D3DXVECTOR3( -1.0f, 1.0f, 1.0f ), D3DXVECTOR4( 1.0f, 0.0f, 0.0f, 1.0f ) },
        { D3DXVECTOR3( -1.0f, -1.0f, -1.0f ), D3DXVECTOR4( 1.0f, 0.0f, 1.0f, 1.0f ) },
        { D3DXVECTOR3( 1.0f, -1.0f, -1.0f ), D3DXVECTOR4( 1.0f, 1.0f, 0.0f, 1.0f ) },
        { D3DXVECTOR3( 1.0f, -1.0f, 1.0f ), D3DXVECTOR4( 1.0f, 1.0f, 1.0f, 1.0f ) },
        { D3DXVECTOR3( -1.0f, -1.0f, 1.0f ), D3DXVECTOR4( 0.0f, 0.0f, 0.0f, 1.0f ) },
    };

	bd.ByteWidth = sizeof( SimpleVertex ) * 8;
	bd.Usage = D3D10_USAGE_IMMUTABLE;
	bd.BindFlags = D3D10_BIND_VERTEX_BUFFER;
	bd.CPUAccessFlags = 0;
	bd.MiscFlags = 0;

	D3D10_SUBRESOURCE_DATA initData;
	initData.pSysMem = &vertices;

	hr = d3dDevice->CreateBuffer( &bd, &initData, &cubeVertexBuffer );

    if( FAILED( hr ) )
        return hr;

    // Create cube index buffer
    DWORD indices[] =
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

	bd.ByteWidth = sizeof( DWORD ) * 36;
	bd.Usage = D3D10_USAGE_IMMUTABLE;
	bd.BindFlags = D3D10_BIND_INDEX_BUFFER;
	bd.CPUAccessFlags = 0;
	bd.MiscFlags = 0;

	initData.pSysMem = &indices;

	hr = d3dDevice->CreateBuffer( &bd, &initData, &cubeIndexBuffer );
   
	if( FAILED( hr ) )
        return hr;	
	
	return hr;
}

/// <summary>
/// Update time and frame, loops last frame to first frame
/// </summary>
void BVHFigure::Update(float time)
{
	curFrame = ( int )( time / frameTime ) % numFrames;
}

/// <summary>
/// Update view matrix to look at the first root node of this figure.
/// </summary>
/// <param name='Eye'>Pointer to the view's Eye vector.</param>
/// <param name='Up'>Pointer to the view's Up vector.</param>
/// <param name='viewVariable'>Pointer to the view's matrix variable.</param>
void BVHFigure::LookAt( D3DXVECTOR3 * Eye, D3DXVECTOR3 * Up, ID3D10EffectMatrixVariable * viewVariable )
{
	D3DXMATRIX viewMatrix;
	KeyFrame keyFrame = nodes[0]->GetKeyFrame( curFrame );	
	D3DXVECTOR3 At, scale;
	D3DXQUATERNION rot;
	if( FAILED( D3DXMatrixDecompose( &scale, &rot, &At, &keyFrame.translation ) ) )
		return;
	D3DXMatrixLookAtLH( &viewMatrix, Eye, &At, Up );
	viewVariable->SetMatrix( ( float* )&viewMatrix );
}

/// <summary>
/// Renders the BVHFigure.
/// </summary>
void BVHFigure::Render()
{
	// Set topology
	d3dDevice->IASetPrimitiveTopology( D3D10_PRIMITIVE_TOPOLOGY_TRIANGLELIST );

	// Set cube buffers
	UINT stride = sizeof( SimpleVertex );
	UINT offset = 0;
    d3dDevice->IASetVertexBuffers( 0, 1, &cubeVertexBuffer, &stride, &offset );
    d3dDevice->IASetIndexBuffer( cubeIndexBuffer, DXGI_FORMAT_R32_UINT, 0 );

	// Render the nodes
	edgeVertices.clear();
	for( int i = 0; i < nodes.size(); ++i )
	{
		RenderBVHNode( nodes[i], world );
	}

	// Render the edges
	RenderEdges();
}

/// <summary>
/// Renders a node of the BVHFigure and its children, recursively.
/// </summary>
void BVHFigure::RenderBVHNode( BVHNode * node, D3DXMATRIX parentWorld )
{
	D3DXVECTOR3 offset = node->GetOffset();
    D3DXMATRIX mOffset;
	D3DXMatrixTranslation( &mOffset, offset.x, offset.y, offset.z );

	BVHNode * parent = node->GetParent();
	if( parent != NULL ) 
	{
		SimpleVertex vertex;
		vertex.Pos = D3DXVECTOR3( 0, 0, 0 );
		vertex.Color = D3DXVECTOR4( 1, 1, 1, 1 );
		D3DXVec3TransformCoord( &vertex.Pos, &vertex.Pos, &parentWorld );
		edgeVertices.push_back( vertex );
	}

	if( curFrame < node->GetNumKeyFrames() )
	{
		KeyFrame keyFrame = node->GetKeyFrame( curFrame );
		parentWorld = keyFrame.rotation * keyFrame.translation * mOffset * parentWorld;
	} else {
		parentWorld = mOffset * parentWorld;	
	}	

	if( parent != NULL ) 
	{
		SimpleVertex vertex;
		vertex.Pos = D3DXVECTOR3( 0, 0, 0 );
		vertex.Color = D3DXVECTOR4( 1, 1, 1, 1 );
		D3DXVec3TransformCoord( &vertex.Pos, &vertex.Pos, &parentWorld );
		edgeVertices.push_back( vertex );
	}

    //
    // Update variables for the cube
    //
    worldVariable->SetMatrix( ( float* )&parentWorld );

    //
    // Render the cube
    //
    D3D10_TECHNIQUE_DESC techDesc;
    techniqueRender->GetDesc( &techDesc );
    for( UINT p = 0; p < techDesc.Passes; ++p )
    {
        techniqueRender->GetPassByIndex( p )->Apply( 0 );
		d3dDevice->DrawIndexed( 36, 0, 0 );
    }
	
	//
	// Render the children
	//
	vector<BVHNode*> children = node->GetChildren();
	for( int i = 0; i < children.size(); ++i )
	{
		RenderBVHNode( children[i], parentWorld );
	}
}

/// <summary>
/// Renders the edges of the BVHFigure.
/// </summary>
void BVHFigure::RenderEdges() 
{
	// Set edge vertex data
	SimpleVertex *pData = NULL;
	if( SUCCEEDED( edgeVertexBuffer->Map( D3D10_MAP_WRITE_DISCARD, 0, reinterpret_cast< void** >( &pData ) ) ) )
	{
		  memcpy( pData, &edgeVertices[0], sizeof( SimpleVertex ) * numEdges * 2 );
		  edgeVertexBuffer->Unmap();
	}

	// Set vertex buffer
	UINT stride = sizeof( SimpleVertex );
	UINT offset = 0;
    d3dDevice->IASetVertexBuffers( 0, 1, &edgeVertexBuffer, &stride, &offset );

    // Update variables for the edges
    worldVariable->SetMatrix( ( float* )&world );
    
	// Set topology
	d3dDevice->IASetPrimitiveTopology( D3D10_PRIMITIVE_TOPOLOGY_LINELIST );

    // Render the edges
    D3D10_TECHNIQUE_DESC techDesc;
    techniqueRender->GetDesc( &techDesc );
    for( UINT p = 0; p < techDesc.Passes; ++p )
    {
        techniqueRender->GetPassByIndex( p )->Apply( 0 );
		d3dDevice->Draw( numEdges * 2, 0 );
    }	
}