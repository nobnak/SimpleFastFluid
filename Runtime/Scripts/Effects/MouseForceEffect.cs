using Gist2.Deferred;
using Gist2.Extensions.ComponentExt;
using Gist2.Extensions.SizeExt;
using Gist2.Wrappers;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

namespace SimpleAndFastFluids {

    public class MouseForceEffect : MonoBehaviour, IEffect {

        public Links links = new Links();
		public Events events = new Events();
        public Tuner tuner = new Tuner();

        protected Validator changed = new Validator();

        ForceField forceField;
        int2 panelSize_pxc;
        float2 mousePos_pxc;
        RenderTextureWrapper forceTex;

        #region properties
        public Tuner CurrTuner {
            get => tuner.DeepCopy();
            set {
                tuner = value.DeepCopy();
            }
        }
        #endregion

        #region unity
        private void OnEnable() {
            forceField = new ForceField();

			var c = Camera.main;
			forceTex = new RenderTextureWrapper(size => {
				var tex = new RenderTexture(size.x, size.y, 0, RenderTextureFormat.RGFloat);
				tex.hideFlags = HideFlags.DontSave;
				tex.wrapMode = TextureWrapMode.Clamp;
				tex.filterMode = FilterMode.Bilinear;
				return tex;
			});
			forceTex.Changed += v => {
				events.OnCreate?.Invoke(v);
                if (links.fluidEffect != null)
                    links.fluidEffect.Input_Force = v;
			};

            changed.Reset();
            changed.OnValidate += () => {
                UpdateMousePos_Pxc(GetMousePos_Pxc());
            };
		}
		private void OnDisable() {
            if (forceField != null) {
                forceField.Dispose();
                forceField = null;
            }
			if (forceTex != null) {
				forceTex.Dispose();
				forceTex = null;
			}          
        }
        private void OnValidate() {
            changed.Invalidate();
        }
        #endregion

        #region IEffect
        void IEffect.Next(float dt) {
            float2 center_pxc = default;
            if (Input.GetMouseButton(0)) {
                if (TryGetMousePos_Pxc(out var mousePos_pxc)) {
                    var nextCenter_pxc = UpdateMousePos_Pxc(mousePos_pxc) / dt;
                    if (!Input.GetMouseButtonDown(0))
                        center_pxc = nextCenter_pxc;
                }
            }
            forceField.Render(forceTex, mousePos_pxc, center_pxc, tuner.forceRadius);
        }
        void IEffect.Prepare(int2 size) {
            changed.Validate();
            panelSize_pxc = size;
            forceTex.Size = size;
        }
        #endregion

        #region methods
        bool TryGetMousePos_Pxc(out float2 nextMousePos_pxc) {
            var screenPos_pxc = Input.mousePosition;
            switch (tuner.collisionMode) {
                default: {
                    nextMousePos_pxc = ((float3)screenPos_pxc).xy;
                    return true;
                }
                case CollisionMode.Collider: {
                    var ray = Camera.main.ScreenPointToRay(screenPos_pxc);
                    if (math.all(panelSize_pxc >= 4) 
                        && links.touchpanel != null 
                        && links.touchpanel.Raycast(ray, out var hit, float.MaxValue)) {
                        nextMousePos_pxc = (float2)hit.textureCoord * panelSize_pxc;
                        return true;
                    }
                    break;
                }
            }

            nextMousePos_pxc = default;
            return false;
        }
        float2 UpdateMousePos_Pxc (float2 mousePos) {
            var dx = mousePos - mousePos_pxc;
            mousePos_pxc = mousePos;
            return dx;
        }
        #endregion

        #region declarations
        public enum CollisionMode {
            Screen = 0,
            Collider,
        }
        [System.Serializable]
        public class Links {
            public FluidEffect fluidEffect;
            public Collider touchpanel;
        }
        [System.Serializable]
        public class Events {
            public TextureEvent OnCreate;

            [System.Serializable]
            public class TextureEvent : UnityEvent<Texture> { }
        }
        [System.Serializable]
        public class Tuner {
            public CollisionMode collisionMode = default;
            public float forceRadius = 0.05f;
        }
        #endregion
    }

}