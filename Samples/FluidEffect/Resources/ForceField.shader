Shader "SimpleFluid/ForceField" {
	Properties {
		_DirAndCenter ("Direction & Center", Vector) = (0, 1, 0.5, 0.5)
		_InvRadius ("Inv Radius", Float) = 10
	}
	SubShader {
		Cull Off ZWrite Off ZTest Always

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float4 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			float4 _Velocity_pxc;
			float4 _Radius_pxc;
			float4 _Dest_TexelSize;

			v2f vert (appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = float4(v.uv, v.uv * _Dest_TexelSize.zw);
				return o;
			}
			
			float4 frag (v2f i) : SV_Target {
				float2 dpx =  (i.uv.zw - _Velocity_pxc.zw) * _Radius_pxc.x;
				return float4(_Velocity_pxc.xy * saturate(1.0 - dot(dpx, dpx)), 0, 0);
			}
			ENDCG
		}
	}
}
