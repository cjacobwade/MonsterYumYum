Shader "Custom/MonsterSkin-Sprite" 
{

	Properties
	{
		[PerRendererData] _MainTex ( "Sprite Texture", 2D ) = "white" {}
		_SkinColor ("Skin Color", Color) = ( 1, 1, 1, 1 )
		_FurColor ( "Fur Color", Color ) = ( 1, 1, 1, 1 )
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
			};
			
			fixed4 _SkinColor;
			fixed4 _FurColor;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

			sampler2D _MainTex;

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 col = tex2D( _MainTex, IN.texcoord );
				
				col.rgb = lerp( fixed3( 0, 0, 0 ), col.rgb, step( 0.3, col.a ) );
				
				col.rgb = lerp( lerp( fixed3( 0, 0, 0 ), _SkinColor.rgb, step( 0.5, col.g ) ), _FurColor.rgb, step( 0.5, col.r ) );

				return col;
			}
		ENDCG
		}
	}
}
