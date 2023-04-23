Shader "SphereTextureWrapperTriplanar" {
	Properties {
		[Header(Main Maps)]
		[NoScaleOffset] _MainTex ("Albedo Map", 2D) = "white" {}

		_Smoothness ("Smoothness", Range(0.0, 1.0)) = 0.0
		_Metallic ("Metallic", Range(0.0, 1.0)) = 0.0
		[NoScaleOffset] _SmoothnessMap ("Smoothness Map", 2D) = "white" {}

		_BumpStrength ("Normal Strength", Float) = 1.0
		[NoScaleOffset] _BumpMap ("Normal Map", 2D) = "bump" {}

		[HDR] _EmissionColor ("Emission Color", Color) = (1,1,1)
		[NoScaleOffset] _EmissionMap ("Emission Map", 2D) = "black" {}


		[Space(25)]
		[NoScaleOffset] _BlendMap ("Blend Map", 2D) = "white" {}
		

		[Header(Base Tile)]
		[Space]
		[Toggle(BASE_TILE)] _BaseTile ("Active", Float) = 0
		[Space]

		_BaseTileScale ("Base Tile Scale", Float) = 1
		[NoScaleOffset] _BaseTileAlbedo ("Base Tile Albedo x2", 2D) = "grey" {}
		[NoScaleOffset] _BaseTileSmoothnessMap ("Base Tile Smoothness Map", 2D) = "grey" {}
		_BaseTileBumpStrength ("Base Tile Normal Strength", Float) = 1.0
		[NoScaleOffset] _BaseTileBumpMap ("Base Tile Normal Map", 2D) = "bump" {}

		
		[Header(Red Tile)]
		[Space]
		[Toggle(RED_TILE)] _RedTile ("Active", Float) = 0
		[Space]

		_RedTileScale ("Red Tile Scale", Float) = 1
		[NoScaleOffset] _RedTileAlbedo ("Red Tile Albedo x2", 2D) = "grey" {}
		[NoScaleOffset] _RedTileSmoothnessMap ("Red Tile Smoothness Map", 2D) = "grey" {}
		_RedTileBumpStrength ("Red Tile Normal Strength", Float) = 1.0
		[NoScaleOffset] _RedTileBumpMap ("Red Tile Normal Map", 2D) = "bump" {}


		[Header(Green Tile)]
		[Space]
		[Toggle(GREEN_TILE)] _GreenTile ("Active", Float) = 0
		[Space]

		_GreenTileScale ("Green Tile Scale", Float) = 1
		[NoScaleOffset] _GreenTileAlbedo ("Green Tile Albedo x2", 2D) = "grey" {}
		[NoScaleOffset] _GreenTileSmoothnessMap ("Green Tile Smoothness Map", 2D) = "grey" {}
		_GreenTileBumpStrength ("Green Tile Normal Strength", Float) = 1.0
		[NoScaleOffset] _GreenTileBumpMap ("Green Tile Normal Map", 2D) = "bump" {}

		[Header(Blue Tile)]
		[Space]
		[Toggle(BLUE_TILE)] _BlueTile ("Active", Float) = 0
		[Space]

		_BlueTileScale ("Blue Tile Scale", Float) = 1
		[NoScaleOffset] _BlueTileAlbedo ("Blue Tile Albedo x2", 2D) = "grey" {}
		[NoScaleOffset] _BlueTileSmoothnessMap ("Blue Tile Smoothness Map", 2D) = "grey" {}
		_BlueTileBumpStrength ("Blue Tile Normal Strength", Float) = 1.0
		[NoScaleOffset] _BlueTileBumpMap ("Blue Tile Normal Map", 2D) = "bump" {}


		[Header(Alpha Tile)]
		[Space]
		[Toggle(ALPHA_TILE)] _AlphaTile ("Active", Float) = 0
		[Space]

		_AlphaTileScale ("Alpha Tile Scale", Float) = 1
		[NoScaleOffset] _AlphaTileAlbedo ("Alpha Tile Albedo x2", 2D) = "grey" {}
		[NoScaleOffset] _AlphaTileSmoothnessMap ("Alpha Tile Smoothness Map", 2D) = "grey" {}
		_AlphaTileBumpStrength ("Alpha Tile Normal Strength", Float) = 1.0
		[NoScaleOffset] _AlphaTileBumpMap ("Alpha Tile Normal Map", 2D) = "bump" {}

		
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows vertex:vert
		#pragma target 3.0
		#pragma multi_compile _ BASE_TILE
		#pragma multi_compile _ RED_TILE
		#pragma multi_compile _ GREEN_TILE
		#pragma multi_compile _ BLUE_TILE 
		#pragma multi_compile _ ALPHA_TILE

		#include "Triplanar.cginc"

		UNITY_DECLARE_TEX2D(_MainTex);
		float _Smoothness;
		float _Metallic;
		UNITY_DECLARE_TEX2D_NOSAMPLER(_SmoothnessMap);
		float _BumpStrength;
		UNITY_DECLARE_TEX2D_NOSAMPLER(_BumpMap);
		UNITY_DECLARE_TEX2D_NOSAMPLER(_EmissionMap);
		fixed4 _EmissionColor;

		UNITY_DECLARE_TEX2D_NOSAMPLER(_BlendMap);

		bool _BaseTile;
		float _BaseTileScale;
		sampler2D _BaseTileAlbedo;
		sampler2D _BaseTileSmoothnessMap;
		float _BaseTileBumpStrength;
		sampler2D _BaseTileBumpMap;

		bool _RedTile;
		float _RedTileScale;
		sampler2D _RedTileAlbedo;
		sampler2D _RedTileSmoothnessMap;
		float _RedTileBumpStrength;
		sampler2D _RedTileBumpMap;

		bool _GreenTile;
		float _GreenTileScale;
		sampler2D _GreenTileAlbedo;
		sampler2D _GreenTileSmoothnessMap;
		float _GreenTileBumpStrength;
		sampler2D _GreenTileBumpMap;

		bool _BlueTile;
		float _BlueTileScale;
		sampler2D _BlueTileAlbedo;
		sampler2D _BlueTileSmoothnessMap;
		float _BlueTileBumpStrength;
		sampler2D _BlueTileBumpMap;

		bool _AlphaTile;
		float _AlphaTileScale;
		UNITY_DECLARE_TEX2D_NOSAMPLER(_AlphaTileAlbedo);
		UNITY_DECLARE_TEX2D_NOSAMPLER(_AlphaTileSmoothnessMap);
		float _AlphaTileBumpStrength;
		sampler2D _AlphaTileBumpMap;
		
		struct Input {
			float2 uv_MainTex;
			float2 uv_EmissionMap;
			float2 uv_BumpMap;
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
			float3 v = IN.vertPos;

			// idk why Y is down but it is
			float latitude = acos(-v.y / sqrt(v.x * v.x + v.y * v.y + v.z * v.z));
			float longitude = atan2(v.z, v.x);

			float i = fmod(longitude, 2 * UNITY_PI) / (2.0 * UNITY_PI);
			float j = fmod(latitude, UNITY_PI) / UNITY_PI;
			
			float frac_i = frac(i);
			if(fwidth(i) > fwidth(frac_i) - 0.001) i = frac_i;

			o.Albedo = UNITY_SAMPLE_TEX2D(_MainTex, float2(i, j));

			o.Emission = UNITY_SAMPLE_TEX2D_SAMPLER(_EmissionMap, _MainTex, float2(i, j)) * _EmissionColor;

			float3 mapNormal = UnpackScaleNormal(UNITY_SAMPLE_TEX2D_SAMPLER(_BumpMap, _MainTex, float2(i, j)), _BumpStrength);
			o.Normal = mapNormal;

			fixed4 smoothnessMap = UNITY_SAMPLE_TEX2D_SAMPLER(_SmoothnessMap, _MainTex, float2(i, j));
			//float4 smoothnessTile = triplanar(IN.vertPos, IN.normal, _DetailSmoothnessScale, _BaseTileSmoothnessMap);
			o.Smoothness = smoothnessMap.a * _Smoothness;

			//o.Smoothness = lerp(smoothnessMap.a, smoothnessTile.a, 0.5) * _Smoothness;
			//o.Smoothness = (smoothnessMap.a * _Smoothness) + (smoothnessTile.a * _DetailSmoothness);
			//o.Smoothness = lerp(smoothnessMap.a, smoothnessTile.a, _DetailSmoothness) * _Smoothness;
			//o.Smoothness = lerp(smoothnessMap.a * _Smoothness, smoothnessTile.a * _DetailSmoothness, _DetailSmoothness);

			o.Metallic = smoothnessMap.r * _Metallic;


			float4 tileAlbedo;
			float4 smoothnessTile;
			float3 triplanarNormal;

			float4 blendMap = UNITY_SAMPLE_TEX2D_SAMPLER(_BlendMap, _MainTex, float2(i, j));

			#if defined(RED_TILE)

			tileAlbedo = triplanar(IN.vertPos, IN.normal, _RedTileScale, _RedTileAlbedo);
			o.Albedo.rgb *= lerp(1, tileAlbedo * 4, blendMap.r);
			
			smoothnessTile = triplanar(IN.vertPos, IN.normal, _RedTileScale, _RedTileSmoothnessMap);
			o.Smoothness *= lerp(1, smoothnessTile.a * 2, blendMap.r);
			o.Metallic *= lerp(1, smoothnessTile.r * 2, blendMap.r);

			triplanarNormal = triplanarNormalTangentSpace(IN.vertPos, IN.normal, _RedTileScale, IN.tangent, _RedTileBumpMap);
			triplanarNormal.z = 0;
			o.Normal += triplanarNormal * _RedTileBumpStrength * blendMap.r;

			#endif

			#if defined(GREEN_TILE)

			tileAlbedo = triplanar(IN.vertPos, IN.normal, _GreenTileScale, _GreenTileAlbedo);
			o.Albedo.rgb *= lerp(1, tileAlbedo * 4, blendMap.g);
			
			smoothnessTile = triplanar(IN.vertPos, IN.normal, _GreenTileScale, _GreenTileSmoothnessMap);
			o.Smoothness *= lerp(1, smoothnessTile.a * 2, blendMap.g);
			o.Metallic *= lerp(1, smoothnessTile.r * 2, blendMap.g);

			triplanarNormal = triplanarNormalTangentSpace(IN.vertPos, IN.normal, _GreenTileScale, IN.tangent, _GreenTileBumpMap);
			triplanarNormal.z = 0;
			o.Normal += triplanarNormal * _GreenTileBumpStrength * blendMap.g;

			#endif

			#if defined(BLUE_TILE)

			tileAlbedo = triplanar(IN.vertPos, IN.normal, _BlueTileScale, _BlueTileAlbedo);
			o.Albedo.rgb *= lerp(1, tileAlbedo * 4, blendMap.b);
			
			smoothnessTile = triplanar(IN.vertPos, IN.normal, _BlueTileScale, _BlueTileSmoothnessMap);
			o.Smoothness *= lerp(1, smoothnessTile.a * 2, blendMap.b);
			o.Metallic *= lerp(1, smoothnessTile.r * 2, blendMap.b);

			triplanarNormal = triplanarNormalTangentSpace(IN.vertPos, IN.normal, _BlueTileScale, IN.tangent, _BlueTileBumpMap);
			triplanarNormal.z = 0;
			o.Normal += triplanarNormal * _BlueTileBumpStrength * blendMap.b;

			#endif

			#if defined(ALPHA_TILE)

			// Do triplanar math out here to use macros, in order to save samplers
			float2 uvX = IN.vertPos.zy * _AlphaTileScale;
			float2 uvY = IN.vertPos.xz * _AlphaTileScale;
			float2 uvZ = IN.vertPos.xy * _AlphaTileScale;
			float4 colX = UNITY_SAMPLE_TEX2D_SAMPLER(_AlphaTileAlbedo, _MainTex, uvX);
			float4 colY = UNITY_SAMPLE_TEX2D_SAMPLER(_AlphaTileAlbedo, _MainTex, uvY);
			float4 colZ = UNITY_SAMPLE_TEX2D_SAMPLER(_AlphaTileAlbedo, _MainTex, uvZ);
			float3 blendWeight = IN.normal * IN.normal;
			blendWeight /= dot(blendWeight, 1);

			tileAlbedo = colX * blendWeight.x + colY * blendWeight.y + colZ * blendWeight.z;
			o.Albedo.rgb *= lerp(1, tileAlbedo * 4, blendMap.a);
			
			//uvX = IN.vertPos.zy * _AlphaTileScale;
			//uvY = IN.vertPos.xz * _AlphaTileScale;
			//uvZ = IN.vertPos.xy * _AlphaTileScale;
			colX = UNITY_SAMPLE_TEX2D_SAMPLER(_AlphaTileSmoothnessMap, _MainTex, uvX);
			colY = UNITY_SAMPLE_TEX2D_SAMPLER(_AlphaTileSmoothnessMap, _MainTex, uvY);
			colZ = UNITY_SAMPLE_TEX2D_SAMPLER(_AlphaTileSmoothnessMap, _MainTex, uvZ);
			//blendWeight = IN.normal * IN.normal;
			//blendWeight /= dot(blendWeight, 1);

			smoothnessTile = colX * blendWeight.x + colY * blendWeight.y + colZ * blendWeight.z;
			o.Smoothness *= lerp(1, smoothnessTile.a * 2, blendMap.a);
			o.Metallic *= lerp(1, smoothnessTile.r * 2, blendMap.a);

			triplanarNormal = triplanarNormalTangentSpace(IN.vertPos, IN.normal, _AlphaTileScale, IN.tangent, _AlphaTileBumpMap);
			triplanarNormal.z = 0;
			o.Normal += triplanarNormal * _AlphaTileBumpStrength * blendMap.a;

			#endif

			#if defined(BASE_TILE)

			float baseBlend = 0;
			if (_RedTile && blendMap.r > baseBlend) baseBlend = blendMap.r;
			if (_GreenTile && blendMap.g > baseBlend) baseBlend = blendMap.g;
			if (_BlueTile && blendMap.b > baseBlend) baseBlend = blendMap.b;
			if (_AlphaTile && blendMap.a > baseBlend) baseBlend = blendMap.a;
			baseBlend = 1 - baseBlend;

			tileAlbedo = triplanar(IN.vertPos, IN.normal, _BaseTileScale, _BaseTileAlbedo);
			o.Albedo.rgb *= lerp(1, tileAlbedo * 4, baseBlend);
			
			smoothnessTile = triplanar(IN.vertPos, IN.normal, _BaseTileScale, _BaseTileSmoothnessMap);
			o.Smoothness *= lerp(1, smoothnessTile.a * 2, baseBlend);
			o.Metallic *= lerp(1, smoothnessTile.r * 2, baseBlend);

			triplanarNormal = triplanarNormalTangentSpace(IN.vertPos, IN.normal, _BaseTileScale, IN.tangent, _BaseTileBumpMap);
			triplanarNormal.z = 0;
			o.Normal += triplanarNormal * _BaseTileBumpStrength * baseBlend;

			#endif
		}
		ENDCG
	}
	FallBack "SphereTextureWrapper"
}
