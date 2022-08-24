Shader "NewHorizons/UnlitRing1Pixel" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_InnerRadius("InnerRadius", Range(0,1)) = 0.0
		_Alpha ("Alpha", range(0.0, 1.0)) = 1.0
	}
	SubShader {
		Tags
		{
			"RenderType"="Opaque"
			"Queue"="Transparent"
			"PreviewType"="Plane"
		}
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha
		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				#include "UnityCG.cginc"
				struct appdata_t {
					float4 vertex : POSITION;
					float2 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};
				struct v2f {
					float4 vertex : SV_POSITION;
					float2 texcoord : TEXCOORD0;
					UNITY_VERTEX_OUTPUT_STEREO
				};
				sampler2D _MainTex;
				float4 _MainTex_ST;
				float _InnerRadius;
				float _Alpha;
				v2f vert (appdata_t v)
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}
				fixed4 frag (v2f i) : SV_Target
				{
					float x = (i.texcoord.x - 0.5) * 2.0;
					float y = (i.texcoord.y - 0.5) * 2.0;
					float r = sqrt(x * x + y * y);

					float r_sample = (r - _InnerRadius) / (1 - _InnerRadius);			

					fixed4 col = float4(0,0,0,0);

					if(r < 1.0 && r > _InnerRadius) 
					{
						col = tex2D(_MainTex, float2(0, r_sample));
					}

					col.a = col.a * _Alpha;

					UNITY_APPLY_FOG(i.fogCoord, col);

					return col;
				}
			ENDCG
		}
	}
}