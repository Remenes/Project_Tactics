
Shader "Holistic/23 SimpleOutlining" {
	Properties {
		_Color ("Outline Color", Color) = (1,0,0,1)
		_Width ("Outline Width", Range(0, .5)) = 0
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "Queue" = "Transparent" }

		Pass {
			ZWrite Off
			Cull Front

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			struct appdata {
				float3 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
			};

			half _Width;

			v2f vert(appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex + v.normal * _Width);
				return o;
			}

			fixed4 _Color;

			fixed4 frag(v2f i) : SV_TARGET {
				return _Color;
			}

			ENDCG
		}
		
	}
	FallBack "Diffuse"
}
