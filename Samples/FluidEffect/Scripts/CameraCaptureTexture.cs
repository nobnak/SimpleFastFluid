using Gist2.Extensions.SizeExt;
using Gist2.Wrappers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

public class CameraCaptureTexture : MonoBehaviour {

    public Events events = new Events();
    public Links links = new Links();
    public Preset preset = new Preset();

    protected CameraWrapper captureCam;
    protected RenderTextureWrapper captureTex;

    #region properties
    public Texture CurrentOutput { get => captureTex; }
    #endregion

    #region unity
    private void OnEnable() {
        captureCam = new CameraWrapper(c => {
            if (c == null) {
                var go = new GameObject(name);
                go.hideFlags = HideFlags.DontSave;
                go.transform.SetParent(transform);
                c = go.AddComponent<Camera>();
                CameraCaptureBridge.AddCaptureAction(c, OnCameraCaptureAction);
            }
            return c;
        });
        captureTex = new RenderTextureWrapper(size => {
            RenderTexture result = null;
            if (size.x >= 4 && size.y >= 4) {
                result = new RenderTexture(size.x, size.y, 32, preset.format);
                result.hideFlags = HideFlags.DontSave;
            }
            events.OnCreate?.Invoke(result);
            return result;
        });
    }
    private void OnDisable() {
        if (captureCam != null) {
            CameraCaptureBridge.RemoveCaptureAction(captureCam, OnCameraCaptureAction);
            captureCam.Dispose();
            captureCam = null;
        }
        if (captureTex != null) {
            captureTex.Dispose();
            captureTex = null;
        }
    }
    private void Update() {
        if (captureCam != null) {
            var source = links.source != null ? links.source : Camera.main;
            captureTex.Size = source.Size();
            captureCam.Value.CopyFrom(source);
            captureCam.Value.depth += 1;
            captureCam.Value.cullingMask = preset.cullingMask;
            captureCam.Value.targetTexture = captureTex;
        }
    }
    #endregion

    #region methods
    protected void OnCameraCaptureAction(RenderTargetIdentifier rti, CommandBuffer cb) {
        if (captureTex != null)
            cb.Blit(rti, captureTex.Value);
    }
    #endregion

    #region declarations
    [System.Serializable]
    public class Events {

        public TextureEvent OnCreate = new TextureEvent();

        [System.Serializable]
        public class TextureEvent : UnityEvent<Texture> { }
    }
    [System.Serializable]
    public class Links {
        public Camera source;
    }
    [System.Serializable]
    public class Preset {
        public RenderTextureFormat format = RenderTextureFormat.ARGB32;
        public LayerMask cullingMask;
    }
    #endregion
}
