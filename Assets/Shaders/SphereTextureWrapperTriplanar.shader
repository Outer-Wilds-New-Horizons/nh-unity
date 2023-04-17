Shader "SphereTextureWrapperTriplanar" {
	Properties {
		[Header(Main Maps)]
		[NoScaleOffset] _MainTex ("Albedo Map", 2D) = "white" {}

		_Smoothness ("Smoothness", Range(0.0, 1.0)) = 0.0
		_Metallic ("Metallic", Range(0.0, 1.0)) = 0.0
		[NoScaleOffset] _SmoothnessMap ("Smoothness Map", 2D) = "white" {}

		_BumpStrength("Normal Strength", Float) = 1.0
		[NoScaleOffset] _BumpMap ("Normal Map", 2D) = "bump" {}

		[HDR] _EmissionColor("Emission Color", Color) = (1,1,1)
		[NoScaleOffset] _EmissionMap("Emission Map", 2D) = "black" {}
		

		[Header(Tiling)]
		_DetailScale ("Tile Albedo Scale", Float) = 1
		[NoScaleOffset] _DetailAlbedo ("Tile Albedo x2", 2D) = "grey" {}

		_DetailSmoothnessScale ("Tile Smoothness Scale", Float) = 1
		_DetailSmoothness ("Tile Smoothness Strength", Range(0.0, 1.0)) = 0.0
		[NoScaleOffset] _DetailSmoothnessMap ("Tile Smoothness Map", 2D) = "grey" {}
		
		_DetailBumpScale ("Tile Normal Scale", Float) = 1
		_DetailBumpStrength("Tile Normal Strength", Float) = 1.0
		[NoScaleOffset] _DetailBumpMap ("Tile Normal Map", 2D) = "bump" {}
		
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows vertex:vert
		#pragma target 3.0

		#include "Triplanar.cginc"

		sampler2D _MainTex;
		float _Smoothness;
		float _Metallic;
		sampler2D _SmoothnessMap;
		float _BumpStrength;
		sampler2D _BumpMap;
		sampler2D _EmissionMap;
		fixed4 _EmissionColor;

		float _DetailScale;
		sampler2D _DetailAlbedo;
		float _DetailSmoothnessScale;
		float _DetailSmoothness;
		sampler2D _DetailSmoothnessMap;
		float _DetailBumpStrength;
		float _DetailBumpScale;
		sampler2D _DetailBumpMap;
		
		struct Input {
			float2 uv_MainTex;
			float2 uv_EmissionMap;
			float2 uv_BumpMap;
			float3 worldPos;
			float3 vertPos;
			float3 normal;
			float4 tangent;
			
		};

		void vert (inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT (Input,o);
			o.vertPos = v.vertex;
			o.normal = v.normal;
			o.tangent = v.tangent;
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			float3 v = mul(unity_WorldToObject, float4(IN.worldPos,1)).xyz;
			float PI = 3;

			float latitude = acos(-v.y / sqrt(v.x * v.x + v.y * v.y + v.z * v.z));
			float longitude = atan2(v.x, -v.z);

			float i = fmod(longitude, 2 * UNITY_PI) / (2.0 * UNITY_PI);
			float j = fmod(latitude, UNITY_PI) / UNITY_PI;
			
			float frac_i = frac(i);
			if(fwidth(i) > fwidth(frac_i) - 0.001) i = frac_i;

			fixed4 c = tex2D(_MainTex, float2(i, j));

			float4 tileAlbedo = triplanar(IN.vertPos, IN.normal, _DetailScale, _DetailAlbedo);

			//o.Albedo = c.rgb;
			o.Albedo.rgb = c * tileAlbedo * 4;

			o.Alpha = c.a;

			o.Emission = tex2D(_EmissionMap, float2(i, j)) * _EmissionColor;


			float3 triplanarNormal = triplanarNormalTangentSpace(IN.vertPos, IN.normal, _DetailBumpScale, IN.tangent, _DetailBumpMap);
			float3 mapNormal = UnpackScaleNormal(tex2D (_BumpMap, float2(i, j)), _BumpStrength);
			float3 normal = lerp(float3(0,0,1), triplanarNormal, _DetailBumpStrength) + mapNormal;
			o.Normal = normal;

			fixed4 smoothnessMap = tex2D(_SmoothnessMap, float2(i, j));
			float4 smoothnessTile = triplanar(IN.vertPos, IN.normal, _DetailSmoothnessScale, _DetailSmoothnessMap);
			o.Smoothness = smoothnessMap.a * smoothnessTile.a * 2 * _Smoothness;
			//o.Smoothness = lerp(smoothnessMap.a, smoothnessTile.a, 0.5) * _Smoothness;
			//o.Smoothness = (smoothnessMap.a * _Smoothness) + (smoothnessTile.a * _DetailSmoothness);
			//o.Smoothness = lerp(smoothnessMap.a, smoothnessTile.a, _DetailSmoothness) * _Smoothness;
			//o.Smoothness = lerp(smoothnessMap.a * _Smoothness, smoothnessTile.a * _DetailSmoothness, _DetailSmoothness);

			o.Metallic = smoothnessMap.r * smoothnessTile.r * 2 * _Metallic;
		}
		ENDCG
	}
	FallBack "SphereTextureWrapper"
}
