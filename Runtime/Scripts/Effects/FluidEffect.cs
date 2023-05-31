using Gist2.Extensions.LODExt;
using Gist2.Wrappers;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

namespace SimpleAndFastFluids {

    public class FluidEffect : MonoBehaviour, IEffect {

		public Preset preset = new Preset();
		public Tuner tuner = new Tuner();
		public Events events = new Events();

		protected float time_residue = 0f;
        protected Solver solver;
		protected RenderTextureWrapper fluid0, fluid1;

		#region properties
		public Texture Input_Force { get; set; }
        public Texture Input_Boundary { get; set; }

		public Texture Output_Fluid { get; protected set; }
		#endregion

		#region Unity
		void OnEnable() {
            RenderTexture GenFluidTex(int2 size) {
                var tex = new RenderTexture(size.x, size.y, 0, preset.format);
                tex.hideFlags = HideFlags.DontSave;
                tex.wrapMode = TextureWrapMode.Clamp;
                return tex;
            }

            solver = new Solver();

            fluid0 = new RenderTextureWrapper(GenFluidTex);
			fluid1 = new RenderTextureWrapper(GenFluidTex);

			fluid0.Changed += v => {
				if (solver != null) {
					time_residue = 0f;
					if (v.Value != null) solver.Clear(v);
				}
			};
			fluid1.Changed += v => {
                if (solver != null) {
                    time_residue = 0f;
                    if (v.Value != null) solver.Clear(v);
                }
            };
		}
		void OnDisable() {
            if (fluid0 != null) {
                fluid0.Dispose();
                fluid0 = null;
            }
            if (fluid1 != null) {
                fluid1.Dispose();
                fluid1 = null;
            }
            if (solver != null) {
                solver.Dispose();
                solver = null;
            }
        }

        void Notify() {
            Output_Fluid = fluid0;
			events.Output_Fluid?.Invoke(Output_Fluid);
		}
        #endregion

        #region IEffect
        void IEffect.Next(float dt) {
            Solve(dt);
            Notify();
        }
        void IEffect.Prepare(int2 size) {
            var size_solver = size.LOD(tuner.eff.lod_solver);
            var prev_solver = fluid0.Size;
            fluid0.Size = fluid1.Size = size_solver;
            if (math.any(size_solver != prev_solver))
                Debug.Log($"{GetType().Name} size changed: fluid={size_solver}");
        }
        #endregion

        #region interfaces
        public void Reset() {
			fluid0?.Release();
			fluid1?.Release();
		}
		#endregion

		#region methods
		private void Solve(float dt) {
			var time_step = tuner.solver.timeStep;
			time_residue += dt * tuner.eff.simulationSpeed;
			while (time_residue >= time_step) {
                time_residue -= time_step;
                solver.Solve(fluid0, fluid1, Input_Force, time_step,
                    viscosity: tuner.solver.vis,
					k: tuner.solver.k,
					force: tuner.solver.force,
                    boundary_tex: Input_Boundary);
                Solver.Swap(ref fluid0, ref fluid1);
            }
        }
        #endregion

        #region declarations
        public const string FLUIDABLE_KW_SOURCE = "FLUIDABLE_OUTPUT_SOURCE";

        [System.Serializable]
		public class Preset {
			public Camera source_cam;
            [Header("Texture Format")]
            public RenderTextureFormat format = RenderTextureFormat.ARGBFloat;
        }
		[System.Serializable]
		public class Events {
			public TextureEvent Output_Fluid = new TextureEvent();

			[System.Serializable]
			public class TextureEvent : UnityEvent<Texture> { }
		}
		[System.Serializable]
		public class EffectTuner {
			[Header("Simulation")]
			public float simulationSpeed = 1f;
			[Range(0, 4)]
			public int lod_solver = 0;
		}

        #region declarations
        [System.Serializable]
        public class SolverTuner {
            public float force = 1f;
            public float k = 0.12f;
            public float vis = 0.1f;
            public float timeStep = 0.1f;
        }
        #endregion
        [System.Serializable]
		public class Tuner {
			public EffectTuner eff = new EffectTuner();
			public SolverTuner solver = new SolverTuner();
		}
		#endregion
	}
}
