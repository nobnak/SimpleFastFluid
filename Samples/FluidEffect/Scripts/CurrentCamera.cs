using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CurrentCamera : System.IDisposable {

    public Camera Value { get; protected set; }

    public CurrentCamera() {
        RenderPipelineManager.beginCameraRendering += ListenOnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering += ListenOnEndCameraRendering;
    }

    public void Dispose() {
        RenderPipelineManager.beginCameraRendering -= ListenOnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering -= ListenOnEndCameraRendering;
    }

    public static implicit operator Camera(CurrentCamera cc) => cc?.Value;

    #region methods
    protected void ListenOnBeginCameraRendering(ScriptableRenderContext context, Camera current)
        => Value = current;
    protected void ListenOnEndCameraRendering(ScriptableRenderContext context, Camera current)
        => Value = null;
    #endregion
}