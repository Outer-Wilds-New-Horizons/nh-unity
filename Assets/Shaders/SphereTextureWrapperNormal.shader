Shader "SphereTextureWrapperNormal" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_EmissionMap("Emission Map", 2D) = "black" {}
		[HDR] _EmissionColor("Emission Color", Color) = (1,1,1)

		[Normal] _BumpMap ("Normal Map", 2D) = "bump" {}
		_BumpScale("Normal Map Scale", Float) = 1.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _EmissionMap;
		fixed4 _EmissionColor;
		sampler2D _BumpMap;
		half _BumpScale;

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

			o.Normal = UnpackScaleNormal (tex2D (_BumpMap, float2(i, j)), _BumpScale);
		}
		ENDCG
	}
	FallBack "SphereTextureWrapper"
}
