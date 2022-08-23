Shader "NewHorizons/Ring1Pixel"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Cutoff("Alpha Cutoff", Range(0,1)) = 0.0
        _InnerRadius("InnerRadius", Range(0,1)) = 0.0
        _Alpha ("Alpha", range(0.0, 1.0)) = 1.0
    }
 
    SubShader
    {
        Tags
        {
            "Queue"="AlphaTest"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }
 
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
 
        CGPROGRAM
        #pragma surface surf Lambert vertex:vert nofog alphatest:_Cutoff addshadow
		
		static const float PI = 3.141926535;
		
        sampler2D _MainTex;
        fixed4 _Color;
		fixed3 _SunPosition;
		float _InnerRadius;
		float _Alpha;
 
        struct Input
        {
            float2 uv_MainTex;
			float3 dotP;
        };
       
        void vert (inout appdata_full v, out Input o)
        {         
            UNITY_INITIALIZE_OUTPUT(Input, o);
			float4 vertWorldPos = mul(unity_ObjectToWorld, v.vertex);
			//float3 lightDir = _WorldSpaceLightPos0.xyz;
			float3 lightDir = _SunPosition - vertWorldPos.xyz;

			float3 worldNormal = normalize(mul(unity_ObjectToWorld, float4(v.normal, 0.0)).xyz);
			
			float x = dot(worldNormal, lightDir);
			float y = sqrt(x * x - 1);
			float angle = atan2(y, x);
			
			if(x <= 0) 
			{
				v.normal = -v.normal;
			}
        }
 
        void surf (Input IN, inout SurfaceOutput o)
        {
			float x = (IN.uv_MainTex.x - 0.5) * 2.0;
			float y = (IN.uv_MainTex.y - 0.5) * 2.0;
			float r = sqrt(x * x + y * y);

			float r_sample = (r - _InnerRadius) / (1 - _InnerRadius);			

            if(r < 1.0 && r > _InnerRadius) 
			{
				fixed4 c = tex2D(_MainTex, float2(0, r_sample)) * _Color;
			    o.Albedo = c.rgb;
				o.Alpha = c.a * _Alpha;
			}
			else 
			{
				o.Alpha = 0.0;
			}
        }
        ENDCG
    }
	Fallback "Diffuse"
}
 