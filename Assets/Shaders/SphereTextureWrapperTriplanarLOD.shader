Shader "SphereTextureWrapperTriplanarLOD" {
	Properties {
		[Header(Main Maps)]
		[NoScaleOffset] _MainTex ("Albedo Map", 2D) = "white" {}

		_Smoothness ("Smoothness", Range(0.0, 1.0)) = 0.0
		[NoScaleOffset] _SmoothnessMap ("Smoothness Map", 2D) = "white" {}

		_BumpStrength("Normal Strength", Float) = 1.0
		[NoScaleOffset] _BumpMap ("Normal Map", 2D) = "bump" {}

		[HDR] _EmissionColor("Emission Color", Color) = (1,1,1)
		[NoScaleOffset] _EmissionMap("Emission Map", 2D) = "black" {}
		
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0

		//#include "Triplanar.cginc"

		sampler2D _MainTex;
		float _Smoothness;
		sampler2D _SmoothnessMap;
		float _BumpStrength;
		sampler2D _BumpMap;
		sampler2D _EmissionMap;
		fixed4 _EmissionColor;
		
		struct Input {
			float2 uv_MainTex;
			float2 uv_EmissionMap;
			float2 uv_BumpMap;
			float3 worldPos;
		};

		

		void surf (Input IN, inout SurfaceOutputStandard o) {
			float3 v = mul(unity_WorldToObject, float4(IN.worldPos,1)).xyz;
			float PI = 3;

			float latitude = acos(v.z / sqrt(v.x * v.x + v.y * v.y + v.z * v.z));
			float longitude = atan2(v.y, v.x);

			float i = fmod(longitude, 2 * UNITY_PI) / (2.0 * UNITY_PI);
			float j = fmod(latitude, UNITY_PI) / UNITY_PI;
			
			float frac_i = frac(i);
			if(fwidth(i) > fwidth(frac_i) - 0.001) i = frac_i;

			fixed4 c = tex2D(_MainTex, float2(i, j));

			//o.Albedo = c.rgb;
			o.Albedo.rgb = c;

			o.Alpha = c.a;

			o.Emission = tex2D(_EmissionMap, float2(i, j)) * _EmissionColor;

			//o.Normal = UnpackScaleNormal (tex2D (_BumpMap, float2(i, j)), _BumpScale);

			// Normals
			//float3 normalMapFlat = triplanarNormalTangentSpace(IN.vertPos, IN.normal, _DetailBumpScale, IN.tangent, _DetailBumpMap);
			//float3 normal = lerp(float3(0,0,1), normalMapFlat, _DetailBumpStrength);
			//o.Normal = normal; // + UnpackScaleNormal(tex2D (_BumpMap, float2(i, j)), _BumpStrength);; broken due to sideways mapping

			fixed4 smoothnessMap = tex2D(_SmoothnessMap, float2(i, j));
			//float4 smoothnessTile = triplanar(IN.vertPos, IN.normal, _DetailSmoothnessScale, _DetailSmoothnessMap);
			o.Smoothness = smoothnessMap.a * _Smoothness;

			o.Metallic = smoothnessMap.r * _Smoothness;
		}
		ENDCG
	}
	FallBack "SphereTextureWrapper"
}
