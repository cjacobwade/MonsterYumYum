// Unlit alpha-blended shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "Custom/MonsterSkin" 
{
	Properties 
	{
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_SkinColor ( "Skin Color", Color ) = ( 1, 1, 1, 1 )
		_FurColor ( "Fur Color", Color ) = ( 1, 1, 1, 1 )
	}

	SubShader 
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 100
		
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha 
		
		Pass 
		{  
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fog
				
				#include "UnityCG.cginc"

				struct appdata_t 
				{
					float4 vertex : POSITION;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f {
					float4 vertex : SV_POSITION;
					half2 texcoord : TEXCOORD0;
					UNITY_FOG_COORDS(1)
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				
				fixed4 _FurColor;
				fixed4 _SkinColor;
				
				v2f vert (appdata_t v)
				{
					v2f o;
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}
				
				fixed4 frag (v2f i) : SV_Target
				{
					fixed4 col = tex2D( _MainTex, i.texcoord );
					col.rgb = lerp( lerp( fixed3( 0, 0, 0 ), _SkinColor.rgb, col.g ), _FurColor.rgb, step( 0.1, col.r ) );
					
					UNITY_APPLY_FOG(i.fogCoord, col);
					return col;
				}
			ENDCG
		}
	}
}
