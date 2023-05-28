using Gist2.Extensions.ComponentExt;
using Gist2.Extensions.SizeExt;
using Unity.Mathematics;
using UnityEngine;

namespace SimpleAndFastFluids {

    public class ForceField : System.IDisposable {

        public const string PATH = "ForceField";
        public static readonly int P_Velocity_pxc = Shader.PropertyToID("_Velocity_pxc");
        public static readonly int P_Radius_pxc = Shader.PropertyToID("_Radius_pxc");

        public static readonly int P_Dest_TexelSize = Shader.PropertyToID("_Dest_TexelSize");

        public Material Mat { get; protected set; }

        public ForceField() {
            Mat = new Material(Resources.Load<Shader>(PATH));
        }

        public void Render(RenderTexture tex, float2 center_pxc, float2 velocity_pxc, float radius_pxc) {
            var dest_size = tex.Size();
            var dest_texel_size = new float4(math.rcp(dest_size), dest_size);

            Mat.SetVector(P_Velocity_pxc, new float4(velocity_pxc, center_pxc));
            Mat.SetVector(P_Radius_pxc, new float4(math.rcp(radius_pxc), 0, radius_pxc, 0));
            Mat.SetVector(P_Dest_TexelSize, dest_texel_size);
            Graphics.Blit(null, tex, Mat);
        }

        public void Dispose() {
            if (Mat != null) {
                Mat.Destroy();
                Mat = null;
            }
        }
    }
}