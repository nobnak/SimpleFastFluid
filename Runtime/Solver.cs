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
        public static readonly int P_KVis = Shader.PropertyToID("_KVis");
        public static readonly int P_S = Shader.PropertyToID("_S");
        public static readonly int P_ForcePower = Shader.PropertyToID("_ForcePower");

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
        public float Solve(RenderTexture fluid0, RenderTexture fluid1, Texture force, Tuner tuner, float dt) {
			var kvis = tuner.vis;
			var s = tuner.k / dt;

			if (dt >= tuner.timeStep) {
				dt -= tuner.timeStep;

				mat.SetTexture(P_Tex0, force);
				mat.SetFloat(P_ForcePower, tuner.forcePower);
				mat.SetFloat(P_Dt, tuner.timeStep);
				mat.SetFloat(P_KVis, kvis);
				mat.SetFloat(P_S, s);
				Graphics.Blit(fluid0, fluid1, mat, (int)Pass.Fluid);
			}
			return dt;
        }
		public void Advect(RenderTexture main0, RenderTexture main1, Texture fluid, float dt) {
			mat.SetTexture(P_Tex0, fluid);
			mat.SetFloat(P_Dt, dt);
			Graphics.Blit(main0, main1, mat, (int)Pass.Advect);
		}
		public void Lerp(RenderTexture src, RenderTexture dst, RenderTexture emit_tex, 
			float emission = 0f, float dissipation = 0f) {
			mat.SetTexture(P_Tex0, emit_tex);
			mat.SetFloat(P_Dissipation, dissipation);
			mat.SetFloat(P_Emission, emission);
			Graphics.Blit(src, dst, mat, (int)Pass.Lerp);
		}

		#region declarations
		[System.Serializable]
		public class Tuner {
			public float forcePower = 1f;
			public float k = 0.12f;
			public float vis = 0.1f;
			public float timeStep = 0.01f;
		}
		#endregion
	}
}
