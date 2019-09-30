Shader "Hidden/V/ST_PBR/BW_2_MO_S"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Factor("_Factor", FLOAT) = 1
		_MetaMin("Metallic Min",Range(0.0,1.0)) = 0.1
		_MetaMax("Metallic Max",Range(0.0,1.0)) = 0.1
		_SmoothMin("Smooth Min",Range(0.0,1.0)) = 0.7
		_SmoothMax("Smooth Max",Range(0.0,1.0)) = 0.9
		_SmoothScale("SmoothScale",Range(0.0,1.0)) = 1.0
		
	}
		SubShader
		{
			// No culling or depth
			Cull Off ZWrite Off ZTest Always

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
				};

				struct v2f
				{
					float2 uv : TEXCOORD0;
					float4 vertex : SV_POSITION;
				};

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = v.uv;
					return o;
				}

				sampler2D _MainTex;
				float _Factor;
				float _MetaMin;
				float _MetaMax;
				float _SmoothMin;
				float _SmoothMax;
				float _SmoothScale;

				fixed4 frag(v2f i) : SV_Target
				{
					float4 col = tex2D(_MainTex, i.uv);
					col.g =  max(0.6,col.r); //TODO: just do a megical number for Occlution 				
					col.a = smoothstep(_SmoothMin, _SmoothMax, col.r)* 0.5 * _SmoothScale + 0.5;
					if (col.r > _MetaMax) { col.r = 0.0; }
					col.r = smoothstep(_MetaMin, _MetaMax, col.r);
					return col;
				}
				ENDCG
			}
		}
}
