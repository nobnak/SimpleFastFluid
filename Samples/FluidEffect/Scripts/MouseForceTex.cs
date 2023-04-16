using UnityEngine;
using Gist2.Extensions.ComponentExt;
using UnityEngine.Events;
using Gist2.Wrappers;
using Gist2.Extensions.SizeExt;

namespace SimpleAndFastFluids.Examples {

    public class MouseForceTex : MonoBehaviour {

        public Links links = new Links();
		public Events events = new Events();
        public Tuner tuner = new Tuner();

        ForceField forceField;
        Vector3 _mousePos;
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
			forceTex.Changed += v => {
				events.OnCreate?.Invoke(v);
                if (links.fluidEffect != null)
                    links.fluidEffect.Force = v;
			};

			UpdateMousePos(Input.mousePosition);
		}
    	void Update () {
            UpdateForceField();
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

		#region methods
		void UpdateForceField() {
            var mousePos = Input.mousePosition;
			var dx = UpdateMousePos(mousePos) / Time.deltaTime;
            var forceVector = Vector2.zero;
            var uv = Vector2.zero;

            if (Input.GetMouseButton (0)) {
                uv = Camera.main.ScreenToViewportPoint (mousePos);
                forceVector = Vector2.ClampMagnitude ((Vector2)dx, 1f);
            }

			var c = Camera.main;
			forceTex.Size = c.Size();

            forceField.Render(forceTex, uv, forceVector, tuner.forceRadius);
        }
        Vector3 UpdateMousePos (Vector3 mousePos) {
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