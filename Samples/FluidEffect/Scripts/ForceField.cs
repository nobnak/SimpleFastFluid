using Gist2.Extensions.ComponentExt;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ForceField : System.IDisposable {

    public const string PATH = "ForceField";
    public static readonly int P_DirAndCenter = Shader.PropertyToID("_DirAndCenter");
    public static readonly int P_InvRadius = Shader.PropertyToID("_InvRadius");

    public Material Mat { get; protected set; }

    public ForceField() {
        Mat = new Material(Resources.Load<Shader>(PATH));
    }

    public void Render(RenderTexture tex, float2 uv, float2 vec, float radius) {
        Mat.SetVector(P_DirAndCenter, new float4(vec, uv));
        Mat.SetFloat(P_InvRadius, math.rcp(radius));
        Graphics.Blit(null, tex, Mat);
    }

    public void Dispose() {
        if (Mat != null) {
            Mat.Destroy();
            Mat = null;
        }
    }
}
