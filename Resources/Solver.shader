Shader "SimpleAndFastFluids/Solver" {
	Properties {
		[MainTexture] _MainTex ("Texture", 2D) = "black" {}
	}
	SubShader {
		Cull Off ZWrite Off ZTest Always

		CGINCLUDE
			#define DX 1.0
			#define DIFF (1.0 / (2.0 * DX))
			#define DAMPING 0.9999
			#define DENSITY0 1.0
			#pragma target 5.0
			
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float4 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			// (u, v, w, rho)
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
			
			sampler2D _Tex0;
			float4 _Tex0_TexelSize;

			v2f vert(appdata v) {
                float2 uvb = v.uv;
                if (_MainTex_TexelSize.y < 0)
                    uvb.y = 1 - uvb.y;

				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = float4(v.uv, uvb);
				return o;
			}
		ENDCG

		// Init
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			float4 frag (v2f i) : SV_Target {
				return float4(0, 0, 0, DENSITY0);
			}
			ENDCG
		}

		// Solve
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			float _Dt;
			float _KineticVis;
			float _Density0;
			float _S;
			float _Force;

			float4 frag (v2f i) : SV_Target {
				float2 duv = _MainTex_TexelSize.xy;
				float4 u = tex2D(_MainTex, i.uv.zw);
				float4 ul = tex2D(_MainTex, i.uv.zw - float2(duv.x, 0));
				float4 ur = tex2D(_MainTex, i.uv.zw + float2(duv.x, 0));
				float4 ub = tex2D(_MainTex, i.uv.zw - float2(0, duv.y));
				float4 ut = tex2D(_MainTex, i.uv.zw + float2(0, duv.y));

				float4 dudx = DIFF * (ur - ul);
				float4 dudy = DIFF * (ut - ub);

				// 1. Mass equation (Solve for Density)
				float2 rho_grad = float2(dudx.w, dudy.w);
				float u_div = dudx.x + dudy.y;
				u.w -= _Dt * dot(u.xyw, float3(rho_grad, u_div));
				u.w = clamp(u.w, 0.5, 3);

				// 2. Momentum equation (Solve for Velocity)
				// 2a. Semi-Lagrange for Transport equation
				u.xy = tex2D(_MainTex, i.uv.zw - _Dt * duv * u.xy).xy;
				// 2b. Remains of Mementum equation
				float2 f = _Force * tex2D(_Tex0, i.uv.zw).xy;
				float2 u_lap = ul.xy + ur.xy + ub.xy + ut.xy - 4.0 * u.xy;
				u.xy += _Dt * (-_S * rho_grad + f + _KineticVis * u_lap);

				// Fallback
				float dt_inv = 1 / _Dt;
				u.xy = clamp (u.xy, -dt_inv, dt_inv);
				u.xy *= DAMPING;
				u.w = lerp(u.w, DENSITY0, saturate(1 - DAMPING));

				// Boundary
				float2 px = i.uv.xy * _MainTex_TexelSize.zw;
				if (any(px < 1) || any((_MainTex_TexelSize.zw - px) < 1))
					u = float4(0, 0, 0, DENSITY0);

				u.z = saturate(dot(1, max(0, -u.xy)));
				return u;
			}
			ENDCG
		}

		// Advect
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			float _Dt;

			float4 frag (v2f i) : SV_Target {
				float2 duv = _Tex0_TexelSize.xy;
				float4 u = tex2D(_Tex0, i.uv.zw);
				float4 c = tex2D(_MainTex, i.uv.xy - _Dt * duv * u.xy);

				return clamp(c, 0.0, 2.0);
			}
			ENDCG
		}

		// Lerp
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			float _Dt;
            float _Emission;
            float _Dissipation;
			
			float4 frag (v2f i) : SV_Target {
				float4 csrc = tex2D(_MainTex, i.uv.xy);
				float4 cemit = tex2D(_Tex0, i.uv.zw);

                csrc = float4(csrc.rgb, (1.0 - _Dissipation * _Dt) * csrc.a);
                return lerp(csrc, float4(cemit.rgb, 1) * cemit.a, _Dt * _Emission);
			}
			ENDCG
		}
	}
}
