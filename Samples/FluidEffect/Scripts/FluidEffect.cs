using Gist2.Extensions.LODExt;
using Gist2.Extensions.SizeExt;
using Gist2.Wrappers;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace SimpleAndFastFluids.Examples {
    public class FluidEffect : MonoBehaviour {

		public Preset preset = new Preset();
		public Tuner tuner = new Tuner();
		public Events events = new Events();

		protected float time_residue = 0f;
        protected Solver solver;
		protected RenderTextureWrapper fluid0, fluid1, image0, image1;

		#region properties
		public Texture Force { get; set; }
		public Texture Source { get; set; }
		public Texture CurrentOutput { get; protected set; }
		#endregion

		#region Unity
		void OnEnable() {
			solver = new Solver();

            fluid0 = new RenderTextureWrapper(GenFluidTex);
			fluid1 = new RenderTextureWrapper(GenFluidTex);
			image0 = new RenderTextureWrapper(GenImageTex);
			image1 = new RenderTextureWrapper(GenImageTex);

			fluid0.Changed += v => {
				if (solver != null) {
					time_residue = 0f;
					if (v.Value != null) solver.Clear(v);
				}
			};
			fluid1.Changed += v => {
				if (v.Value != null) solver.Clear(v);
			};
			image0.Changed += v => {
				if (v.Value != null) Clear(v);
			};
			image1.Changed += v => {
				if (v.Value != null) Clear(v);
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
            if (image0 != null) {
                image0.Dispose();
                image0 = null;
            }
            if (image1 != null) {
                image1.Dispose();
                image1 = null;
            }
            if (solver != null) {
                solver.Dispose();
                solver = null;
            }
        }

        void Update() {
			var dt = Time.deltaTime;
			var size = tuner.perf.simulationSize;

			if (size.x < 4 || size.y < 4
				|| tuner.perf.simulationSpeed <= 0f) return;

			Prepare();
			Solve(dt);
			AdvectImage(dt);

			FallbackToSource();
			Notify();
		}
		void Notify() {
			switch (tuner.debug.outputMode) {
			case OutputModeEnum.Fluid:
                CurrentOutput = fluid0;
				break;
			case OutputModeEnum.Force:
                CurrentOutput = Force;
				break;
			case OutputModeEnum.AdvectionSource:
                CurrentOutput = Source;
				break;
			case OutputModeEnum.AdvectedImage:
			default:
				CurrentOutput = image0;
				break;
			}
			events.OnUpdateAdvectedImageTexture?.Invoke(CurrentOutput);
		}
		#endregion

		#region interfaces
		public void Reset() {
			fluid0?.Release();
			fluid1?.Release();
			image0?.Release();
			image1?.Release();
		}
		public void SetSize(int2 simulationSize) {
			if (math.any(tuner.perf.simulationSize != simulationSize)) {
				tuner.perf.simulationSize = simulationSize;
			}
		}
		#endregion

		#region methods
		protected void Prepare () {
			var size = tuner.perf.simulationSize;
			var size_solver = size.LOD(tuner.perf.lod_solver + tuner.perf.lod_image);
			var size_image = size.LOD(tuner.perf.lod_image);
			var prev_solver = fluid0.Size;
			var prev_image = image0.Size;
			fluid0.Size = fluid1.Size = size_solver;
			image0.Size = image1.Size = size_image;
			if (math.any(size_solver != prev_solver) || math.any(size_image != prev_image))
				Debug.Log($"Fluid size changed: fluid={size_solver} image={size_image}");
		}

		private void Solve(float dt) {
			time_residue += dt * tuner.perf.simulationSpeed;
			while (solver.Solve(fluid0, fluid1, Force, tuner.solver, ref time_residue)) {
                Solver.Swap(ref fluid0, ref fluid1);
            }
        }
		protected void AdvectImage (float dt) {
			solver.Advect(image0, image1, fluid0, dt);
            Solver.Swap(ref image0, ref image1);
		}
		protected void FallbackToSource () {
			if (Source != null) {
				solver.Lerp(image0, image1, Source, tuner.perf.fallbackToImage, tuner.perf.imageDissipation);
				Solver.Swap(ref image0, ref image1);
			}
		}
		protected RenderTexture GenFluidTex(int2 size) {
			var tex = new RenderTexture(size.x, size.y, 0, preset.textureFormat_advect);
			tex.hideFlags = HideFlags.DontSave;
			tex.wrapMode = TextureWrapMode.Clamp;
			return tex;
		}
		protected RenderTexture GenImageTex(int2 size) {
			var tex = new RenderTexture(size.x, size.y, 0, preset.textureFormat_advect);
			tex.hideFlags = HideFlags.DontSave;
			tex.filterMode = FilterMode.Bilinear;
			tex.wrapMode = TextureWrapMode.Clamp;
			return tex;
		}
		protected RenderTexture GenSourceTex(int2 size) {
			var tex = new RenderTexture(size.x, size.y, 0, preset.textureFormat_source, RenderTextureReadWrite.Linear); ;
			tex.hideFlags = HideFlags.DontSave;
			tex.antiAliasing = math.max(1, QualitySettings.antiAliasing);
			return tex;
		}
		protected void Clear(RenderTexture rt) {
			var active = RenderTexture.active;
			RenderTexture.active = rt;
			GL.Clear(true, true, Color.clear);
			RenderTexture.active = active;
		}
        #endregion

        #region declarations
        public enum OutputModeEnum { Normal = 0, Force, Fluid, AdvectionSource, AdvectedImage }

        public const string FLUIDABLE_KW_SOURCE = "FLUIDABLE_OUTPUT_SOURCE";

        [System.Serializable]
		public class Preset {
			public Camera source_cam;

            [Header("Texture Format")]
            public RenderTextureFormat textureFormat_advect = RenderTextureFormat.ARGBFloat;
            public RenderTextureFormat textureFormat_source = RenderTextureFormat.ARGB32;
        }
		[System.Serializable]
		public class Events {
			public TextureEvent OnUpdateAdvectedImageTexture = new TextureEvent();

			[System.Serializable]
			public class TextureEvent : UnityEvent<Texture> { }
		}
		[System.Serializable]
		public class PerformanceTuner {
			[Header("Image")]
			public float fallbackToImage = 0.1f;
			public float imageDissipation = 0.1f;

			[Header("Simulation")]
			public float simulationSpeed = 1f;
			public int2 simulationSize = new int2(4, 4);
			[Range(0, 4)]
			public int lod_solver = 0;
            [Range(0, 4)]
            public int lod_image = 0;
		}
		[System.Serializable]
		public class DebugTuner {
			public OutputModeEnum outputMode;
		}
		[System.Serializable]
		public class Tuner {
			public PerformanceTuner perf = new PerformanceTuner();
			public Solver.Tuner solver = new Solver.Tuner();
			public DebugTuner debug = new DebugTuner();
		}
		#endregion
	}
}
