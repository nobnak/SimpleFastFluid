using Gist2.Extensions.LODExt;
using Gist2.Wrappers;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

namespace SimpleAndFastFluids {

    public class ImageAdvectionEffect : MonoBehaviour, IEffect {

		public Preset preset = new Preset();
		public Tuner tuner = new Tuner();
		public Events events = new Events();

		protected float time_residue = 0f;
        protected Solver solver;
		protected RenderTextureWrapper image0, image1;

        #region properties
		public Texture Input_Fluid { get; set; }
		public Texture Input_Image { get; set; }

        public Texture Output_Image { get; protected set; }
        #endregion

        #region Unity
        void OnEnable() {
            RenderTexture GenImageTex(int2 size) {
                var tex = new RenderTexture(size.x, size.y, 0, preset.format);
                tex.hideFlags = HideFlags.DontSave;
                tex.filterMode = FilterMode.Bilinear;
                tex.wrapMode = TextureWrapMode.Clamp;
                return tex;
            }
            void Clear(RenderTexture rt) {
                var active = RenderTexture.active;
                RenderTexture.active = rt;
                GL.Clear(true, true, Color.clear);
                RenderTexture.active = active;
            }

            solver = new Solver();

            image0 = new RenderTextureWrapper(GenImageTex);
			image1 = new RenderTextureWrapper(GenImageTex);

			image0.Changed += v => {
				if (v.Value != null) Clear(v);
			};
			image1.Changed += v => {
				if (v.Value != null) Clear(v);
			};
		}
		void OnDisable() {
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
        #endregion

        #region IEffect
        void IEffect.Next(float dt) {
            AdvectImage(dt);
            FallbackToSource(dt);
            Notify();
        }
        void IEffect.Prepare(int2 size) {
            var size_image = size.LOD(tuner.lod);
            var prev_image = image0.Size;
            image0.Size = image1.Size = size_image;
            if (math.any(size_image != prev_image))
                Debug.Log($"{GetType().Name} size changed: image={size_image}");
        }
        #endregion

		#region methods
		protected void AdvectImage (float dt) {
			solver.Advect(image0, image1, Input_Fluid, dt);
            Solver.Swap(ref image0, ref image1);
		}
		protected void FallbackToSource (float dt) {
			if (Input_Image != null) {
				solver.Lerp(image0, image1, Input_Image, dt, tuner.fallbackToImage);
				Solver.Swap(ref image0, ref image1);
			}
		}
        protected void Notify() {
            Output_Image = image0;
            events.Output_Image?.Invoke(Output_Image);
        }
        #endregion

        #region declarations
        [System.Serializable]
		public class Preset {
            [Header("Texture Format")]
            public RenderTextureFormat format = RenderTextureFormat.ARGBHalf;
        }
		[System.Serializable]
		public class Events {
			public TextureEvent Output_Image = new TextureEvent();

			[System.Serializable]
			public class TextureEvent : UnityEvent<Texture> { }
		}

        [System.Serializable]
		public class Tuner {
            [Header("Image")]
            public float fallbackToImage = 0.1f;
            [Range(0, 4)]
            public int lod = 0;
        }
		#endregion
	}
}
