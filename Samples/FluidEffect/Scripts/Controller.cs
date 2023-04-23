using Gist2.Extensions.SizeExt;
using LLGraphicsUnity;
using SimpleAndFastFluids.Examples;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour {

    public Preset preset = new Preset();
    public Tuner tuner = new Tuner();

    CurrentCamera currentCamera;

    #region unity
    private void OnEnable() {
        currentCamera = new CurrentCamera();
    }
    private void OnDisable() {
        if (currentCamera != null) {
            currentCamera.Dispose();
            currentCamera = null;
        }
    }
    private void OnRenderObject() {
        var c = currentCamera.Value;
        if (c == null || (c.cullingMask & (1 << gameObject.layer)) == 0
            || !isActiveAndEnabled
            || preset.fluidEffect == null
            || !tuner.show) 
            return;

        var size = c.Size();
        using (new GLMatrixScope()) {
            GL.LoadPixelMatrix(0, size.x, size.y, 0);

            var rect = new Rect(0f, 0f, size.x, size.y);
            var tex = preset.debugTex != null ? preset.debugTex : preset.fluidEffect.CurrentOutput;
            if (tex != null)
                Graphics.DrawTexture(rect, tex);
        }
    }
    private void Update() {
        if (preset.cameraCapture.CurrentOutput != null) {
            preset.fluidEffect.SetSize(preset.cameraCapture.CurrentOutput.Size());
        }
    }
    #endregion

    #region declarations
    [System.Serializable]
    public class Preset {
        public Texture debugTex;
        public FluidEffect fluidEffect;
        public MouseForceTex mouseForce;
        public CameraCaptureTexture cameraCapture;
    }
    [System.Serializable]
    public class Tuner {
        public bool show;
    }
    #endregion
}