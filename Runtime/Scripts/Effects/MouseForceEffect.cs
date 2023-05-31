using Gist2.Deferred;
using Gist2.Extensions.ComponentExt;
using Gist2.Extensions.SizeExt;
using Gist2.Wrappers;
using System.Collections.Generic;
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
            float2 velocity_pxc = default;
            if (Input.GetMouseButton(0)) {
                if (TryGetMousePos_Pxc(out var mousePos_pxc)) {
                    var nextVelocity_pxc = UpdateMousePos_Pxc(mousePos_pxc) / dt;
                    if (!Input.GetMouseButtonDown(0)) {
                        velocity_pxc = nextVelocity_pxc;
                    }
                }
            }
            forceField.Render(forceTex, mousePos_pxc, velocity_pxc, tuner.forceRadius);
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
                    if (math.any(panelSize_pxc < 4)) {
                        Debug.LogWarning($"Display texture too small: size={panelSize_pxc}");
                        break;
                    }
                    if (links.touchpanels == null) {
                        Debug.LogWarning($"Touch panel is not set");
                        break;
                    }

                    var ray = Camera.main.ScreenPointToRay(screenPos_pxc);
                    foreach (var tp in links.touchpanels) {
                        if (tp.Raycast(ray, out var hit, float.MaxValue)) {
                            var uv = (float2)hit.textureCoord;
                            nextMousePos_pxc = uv * panelSize_pxc;
                            //Debug.Log($"Touch pos: uv={uv} px={nextMousePos_pxc}");
                            return true;
                        }
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
            public List<Collider> touchpanels = new List<Collider>();
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