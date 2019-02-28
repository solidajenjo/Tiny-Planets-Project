Shader "TOZ/Object/TriProj/World/StandardBumped" {
	Properties {
		_Color("Main Color", Color) = (1, 1, 1, 1)
		_MainTex("Base (RGB)", 2D) = "white" {}
		_BumpMap("Normalmap", 2D) = "bump" {}

		_MainTex2("High Base (RGB)", 2D) = "white" {}
		_BumpMap2("High Normalmap", 2D) = "bump" {}
		
		_Noise("Noise", 2D) = "bump" {}

		_Blend("Blending", Range (0.01, 0.4)) = 0.2
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_Height("Height", Range(0,200)) = 0.0
	}

	SubShader {
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Standard vertex:vert fullforwardshadows
		#pragma target 3.0

		fixed4 _Color;
		sampler2D _MainTex, _BumpMap, _Noise;
		sampler2D _MainTex2, _BumpMap2;
		float4 _MainTex_ST, _BumpMap_ST;
		fixed _Blend;
		half _Glossiness;
		half _Metallic;
		half _Height;

		struct Input {
			float3 weight : TEXCOORD0;
			float3 worldPos;
		};

		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			fixed3 n = max(abs(v.normal) - _Blend, 0);
			o.weight = n / (n.x + n.y + n.z).xxx;
		}

		void surf(Input IN, inout SurfaceOutputStandard o) {
			//Unity 5 texture interpolators already fill in limits, and no room for packing
			//So we do the uvs per pixel :(
			fixed2 uvx = (IN.worldPos.yz - _MainTex_ST.zw) * _MainTex_ST.xy;
			fixed2 uvy = (IN.worldPos.xz - _MainTex_ST.zw) * _MainTex_ST.xy;
			fixed2 uvz = (IN.worldPos.xy - _MainTex_ST.zw) * _MainTex_ST.xy;
			fixed4 cz = tex2D(_MainTex, uvx) * IN.weight.xxxx;
			fixed4 cy = tex2D(_MainTex, uvy) * IN.weight.yyyy;
			fixed4 cx = tex2D(_MainTex, uvz) * IN.weight.zzzz;
			fixed4 col = (cz + cy + cx) * _Color;
			float3 albedo1 = col.rgb;

			fixed2 h_uvx = (IN.worldPos.yz - _MainTex_ST.zw) * _MainTex_ST.xy;
			fixed2 h_uvy = (IN.worldPos.xz - _MainTex_ST.zw) * _MainTex_ST.xy;
			fixed2 h_uvz = (IN.worldPos.xy - _MainTex_ST.zw) * _MainTex_ST.xy;
			fixed4 h_cz = tex2D(_MainTex2, uvx) * IN.weight.xxxx;
			fixed4 h_cy = tex2D(_MainTex2, uvy) * IN.weight.yyyy;
			fixed4 h_cx = tex2D(_MainTex2, uvz) * IN.weight.zzzz;
			fixed4 h_col = (h_cz + h_cy + h_cx) * _Color;
			float3 albedo2 = h_col.rgb;

			//float d = length(IN.worldPos) * 10 / _Height;
			//o.Albedo = lerp(albedo1, albedo2, d);
			float l = length(IN.worldPos);
			if (l > _Height)
			{
				float h = _Height * 1.01f;
				if (l < h)
				{
					float a = h - _Height;
					float b = l - _Height;
					float d = b / a;
					o.Albedo = lerp(albedo2, albedo1, d);
				}
				else
					o.Albedo = albedo1;
			}
			else
				o.Albedo = albedo2;

			uvx = (IN.worldPos.yz - _BumpMap_ST.zw) * _BumpMap_ST.xy;
			uvy = (IN.worldPos.xz - _BumpMap_ST.zw) * _BumpMap_ST.xy;
			uvz = (IN.worldPos.xy - _BumpMap_ST.zw) * _BumpMap_ST.xy;
			fixed3 bz = UnpackNormal(tex2D(_BumpMap, uvx)) * IN.weight.xxx;
			fixed3 by = UnpackNormal(tex2D(_BumpMap, uvy)) * IN.weight.yyy;
			fixed3 bx = UnpackNormal(tex2D(_BumpMap, uvz)) * IN.weight.zzz;
			o.Normal = bz + by + bx;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = col.a;
		}
		ENDCG
	}

	FallBack "Legacy Shaders/Diffuse"
}