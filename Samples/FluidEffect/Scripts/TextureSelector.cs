using SimpleAndFastFluids;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public class TextureSelector : MonoBehaviour, IEffect {

    public Events events = new Events();
    public Tuner tuner = new Tuner();

    #region properties
    public Texture Input_Tex0 { protected get; set; }
    public Texture Input_Tex1 { protected get; set; }
    public Texture Input_Tex2 { protected get; set; }
    public Texture Input_Tex3 { protected get; set; }

    public Texture Output_Tex { get; protected set; }
    #endregion

    #region IEffect
    public void Next(float dt) {
    }

    public void Prepare(int2 size) {
        Texture tex = SelectTexture();
        events.Output_Texture?.Invoke(tex);
    }
    #endregion

    #region methods
    private Texture SelectTexture() {
        var tex = Input_Tex0;
        switch (tuner.output) {
            case TextureType.Tex1:
            tex = Input_Tex1;
            break;
            case TextureType.Tex2:
            tex = Input_Tex2;
            break;
            case TextureType.Tex3:
            tex = Input_Tex3;
            break;
        }

        return tex;
    }
    #endregion

    #region declarations
    public enum TextureType {
        Tex0 = default,
        Tex1,
        Tex2,
        Tex3
    }

    [System.Serializable]
    public class Events {
        public UnityEvent<Texture> Output_Texture = new UnityEvent<Texture>();
    }
    [System.Serializable]
    public class Tuner {
        public TextureType output = default;
    }
    #endregion
}