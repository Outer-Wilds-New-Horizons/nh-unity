Shader "SphereTextureWrapper" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		[HDR] _EmissionColor("Emission Color", Color) = (0,0,0)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0

		sampler2D _MainTex;
		fixed4 _EmissionColor;

		struct Input {
			float2 uv_MainTex;
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

			o.Emission = c * _EmissionColor;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
