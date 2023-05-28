using Gist2.Extensions.ComponentExt;
using Unity.Mathematics;
using UnityEngine;

namespace SimpleAndFastFluids {

	public class Solver : System.IDisposable {
		public enum Pass {
			Init = 0,
			Fluid,
			Advect,
			Lerp,
		};

		public const string PATH = "Solver";

        public static readonly int P_Tex0 = Shader.PropertyToID("_Tex0");

        public static readonly int P_Dt = Shader.PropertyToID("_Dt");
        public static readonly int P_KineticVis = Shader.PropertyToID("_KineticVis");
		public static readonly int P_Density0 = Shader.PropertyToID("_Density0");
        public static readonly int P_S = Shader.PropertyToID("_S");
        public static readonly int P_Force = Shader.PropertyToID("_Force");

		public static readonly int P_Emission = Shader.PropertyToID("_Emission");
		public static readonly int P_Dissipation = Shader.PropertyToID("_Dissipation");

        protected Material mat;

		public Solver() {
			mat = new Material(Resources.Load<Shader>(PATH));
		}

		#region IDisposable
		public void Dispose() {
			if (mat != null) {
				mat.Destroy();
				mat = null;
			}
		}
		#endregion

		public static void Swap<T>(ref T t0, ref T t1) { var tmp = t0; t0 = t1; t1 = tmp; }

		public void Clear(RenderTexture fluid0) {
			Graphics.Blit(null, fluid0, mat, (int)Pass.Init);
		}
        public void Solve(Texture fluid0_tex, RenderTexture fluid1_tex, Texture force_tex, 
			float dt, float viscosity = 0f, float k = 0f, float force = 0f, float density0 = 1f) {
			var kinetic_vis = viscosity / density0;
			var s = k / (dt * density0);

			mat.SetTexture(P_Tex0, force_tex);
			mat.SetFloat(P_Force, force);
			mat.SetFloat(P_Dt, dt);
			mat.SetFloat(P_KineticVis, kinetic_vis);
			mat.SetFloat(P_Density0, density0);
			mat.SetFloat(P_S, s);
			Graphics.Blit(fluid0_tex, fluid1_tex, mat, (int)Pass.Fluid);
        }
		public void Advect(Texture main0, RenderTexture main1, Texture fluid, float dt) {
			mat.SetTexture(P_Tex0, fluid);
			mat.SetFloat(P_Dt, dt);
			Graphics.Blit(main0, main1, mat, (int)Pass.Advect);
		}
		public void Lerp(Texture src, RenderTexture dst, Texture fallback_tex, 
			float dt, float fallback = 0f, float dissipation = 0f) {

			mat.SetTexture(P_Tex0, fallback_tex);

            mat.SetFloat(P_Dt, dt);
            mat.SetFloat(P_Dissipation, dissipation);
			mat.SetFloat(P_Emission, fallback);

			Graphics.Blit(src, dst, mat, (int)Pass.Lerp);
		}
	}
}
