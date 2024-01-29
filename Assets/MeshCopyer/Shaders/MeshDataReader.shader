// 创建者:   Harling
// 创建时间: 2023-10-10 10:50:48
// 备注:     由PIToolKit工具生成

Shader "Hidden/MeshDataReader" 
{ 
	CGINCLUDE
	#pragma target 5.0

	#include "UnityCG.cginc"
	#include "UnityGBuffer.cginc"

	#pragma multi_compile _ _Position
	uniform	int _PositionDimension;

	#pragma multi_compile _ _Normal
	uniform	int _NormalDimension;

	#pragma multi_compile _ _Tangent
	uniform	int _TangentDimension;

	#pragma multi_compile _ _Color
	uniform	int _ColorDimension;

	#pragma multi_compile _ _TexCoord0
	uniform	int _TexCoord0Dimension;

	#pragma multi_compile _ _TexCoord1
	uniform	int _TexCoord1Dimension;

	#pragma multi_compile _ _TexCoord2
	uniform	int _TexCoord2Dimension;

	#pragma multi_compile _ _TexCoord3
	uniform	int _TexCoord3Dimension;


	uniform int Stride;
	uniform RWStructuredBuffer<float> Vertics : register(u1);


	uniform RWStructuredBuffer<uint> Triangles : register(u2);
	struct v2g
	{ 
		float4 Pos:SV_POSITION;
		int Idx : NORMAL0;
		UNITY_VERTEX_OUTPUT_STEREO
		UNITY_VERTEX_INPUT_INSTANCE_ID 
    }; 
 
    struct g2f 
    { 
		float4 Pos:SV_POSITION;
		UNITY_VERTEX_OUTPUT_STEREO
		UNITY_VERTEX_INPUT_INSTANCE_ID 
    }; 

	void SetVertexBuffer(in appdata_full adf,in uint vid)
	{
		//数据布局
		uint idx=Stride*vid;
		uint i = 0;
		#if _Position
		for(i=0;i< _PositionDimension;i++)
		{
			Vertics[idx++]=adf.vertex[i];
		}
		#endif

		#if _Normal
		for(i=0;i<_NormalDimension;i++)
		{
			Vertics[idx++]=adf.normal[i];
		}
		#endif

		#if _Tangent
		for(i=0;i<_TangentDimension;i++)
		{
			Vertics[idx++]=adf.tangent[i];
		}
		#endif

		#if _Color
		for(i=0;i<_ColorDimension;i++)
		{
			Vertics[idx++]=adf.color[i];
		}
		#endif

		#if _TexCoord0
		for(i=0;i<_TexCoord0Dimension;i++)
		{
			Vertics[idx++]=adf.texcoord[i];
		}
		#endif

		#if _TexCoord1
		for(i=0;i<_TexCoord1Dimension;i++)
		{
			Vertics[idx++]=adf.texcoord1[i];
		}
		#endif

		#if _TexCoord2
		for(i=0;i<_TexCoord2Dimension;i++)
		{
			Vertics[idx++]=adf.texcoord2[i];
		}
		#endif

		#if _TexCoord3
		for(i=0;i<_TexCoord3Dimension;i++)
		{
			Vertics[idx++]=adf.texcoord3[i];
		}
		#endif
	}
             
    void Vert (appdata_full adf,uint id:SV_INSTANCEID,uint vid : SV_VertexID,out v2g o) 
    { 
		UNITY_SETUP_INSTANCE_ID(adf);
		UNITY_TRANSFER_INSTANCE_ID(adf, o);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

		SetVertexBuffer(adf,vid);

		o.Pos=UnityObjectToClipPos(adf.vertex);
		o.Idx=vid;
    } 
	//定义每次调用图元着色器所允许输出的最大顶点数目
    [maxvertexcount(3)] 
    void Geom(triangle v2g input[3], uint triangleID: SV_PrimitiveID,inout TriangleStream<g2f> outStream)
	{ 
		UNITY_SETUP_INSTANCE_ID(input[0]);
		int idx = triangleID * 3;
		[loop]
        for(int i=0;i<3;i++)
		{ 
            g2f o=(g2f)0; 
			o.Pos=input[i].Pos;

			Triangles[idx+i]= input[i].Idx;
			
			UNITY_TRANSFER_INSTANCE_ID(input[i], o);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					
            outStream.Append(o); 
        }
		//表示当前图元构建完毕，下一次调用开始绘制新图元
        outStream.RestartStrip(); 
    }
    void Frag(g2f data,out fixed4 col:SV_TARGET)
	{
		UNITY_SETUP_INSTANCE_ID(data);
		clip(-1);
		col=0;
	}
	ENDCG

    SubShader 
    { 
		Tags {"RenderType" = "Opaque" "Queue"="Geometry" "DisableBatching"="False"}
        Pass 
        { 
			Blend Off
			Cull Back
			ZWrite Off
			ZTest LEqual
			Offset 0, 0
			ColorMask RGBA
            CGPROGRAM 
            #pragma vertex Vert 
			#pragma geometry Geom 
			#pragma fragment Frag
            ENDCG 
        } 
    } 
} 