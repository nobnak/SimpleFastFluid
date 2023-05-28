using Gist2.Extensions.SizeExt;
using Gist2.Wrappers;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

namespace SimpleAndFastFluids {

    public class MouseForceTex : MonoBehaviour, IEffect {

        public Links links = new Links();
		public Events events = new Events();
        public Tuner tuner = new Tuner();

        ForceField forceField;
        float3 _mousePos;
        RenderTextureWrapper forceTex;

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

			UpdateMousePos(Input.mousePosition);
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
        #endregion

        #region IEffect
        void IEffect.Next(float dt) {
            float3 mousePos_pxc = Input.mousePosition;
			var v_pxc = UpdateMousePos(mousePos_pxc) / dt;

            if (!Input.GetMouseButton (0)) {
                v_pxc *= 0f;
            }

            forceField.Render(forceTex, mousePos_pxc.xy, v_pxc.xy, tuner.forceRadius);
        }
        void IEffect.Prepare(int2 size) {
            forceTex.Size = size;
        }
        #endregion

        #region methods
        float3 UpdateMousePos (float3 mousePos) {
            var dx = mousePos - _mousePos;
            _mousePos = mousePos;
            return dx;
        }
        #endregion

        #region declarations

        [System.Serializable]
        public class Links {
            public FluidEffect fluidEffect;
        }
        [System.Serializable]
        public class Events {
            public TextureEvent OnCreate;

            [System.Serializable]
            public class TextureEvent : UnityEvent<Texture> { }
        }
        [System.Serializable]
        public class Tuner {
            public float forceRadius = 0.05f;
        }
        #endregion
    }

}