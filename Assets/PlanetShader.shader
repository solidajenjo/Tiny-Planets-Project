Shader "Custom/PlanetShader"
{
	Properties{
	  _MainTex("Texture", 2D) = "white" {}
	  _BumpMap("Bumpmap", 2D) = "bump" {}

	  _MainTex2("Texture Secondary", 2D) = "white" {}
	  _BumpMap2("Bumpmap Secondary", 2D) = "bump" {}

	  _Distance("Grass Height", Range(0,20)) = 0.5

	}
		SubShader{
		  Tags { "RenderType" = "Opaque" }
		  CGPROGRAM
		  #pragma surface surf Lambert addshadow

		  struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float4 vertex : SV_POSITION;
		  };

		  sampler2D _MainTex;
		  sampler2D _BumpMap;
		  sampler2D _MainTex2;
		  sampler2D _BumpMap2;
		  float _Distance;

		  void surf(Input IN, inout SurfaceOutput o) {
			  float d = distance(IN.vertex, float4(0,0,0,0));
			  /*if (d > _Distance)
			  {
				  o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
				  o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
			  }
			  else
			  {
				  o.Albedo = tex2D(_MainTex2, IN.uv_MainTex).rgb;
				  o.Normal = UnpackNormal(tex2D(_BumpMap2, IN.uv_BumpMap));
			  }*/

			  o.Albedo = tex2D(_MainTex2, IN.uv_MainTex).rgb;
			  o.Normal = UnpackNormal(tex2D(_BumpMap2, IN.uv_BumpMap));
		  }

		  
		  ENDCG
	}
		Fallback "Diffuse"
}
