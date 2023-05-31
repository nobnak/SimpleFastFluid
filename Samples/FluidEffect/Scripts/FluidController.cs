using Gist2.Extensions.SizeExt;
using LLGraphicsUnity;
using SimpleAndFastFluids;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public class FluidController : MonoBehaviour {

    public Preset preset = new Preset();
    public Tuner tuner = new Tuner();
    public Events events = new Events();

    #region unity
    private void OnEnable() {
    }
    private void OnDisable() {
    }
    private void Update() {
        var size = preset.texturesize.GetSize();
        if (math.any(size < 4)) return;

        preset.textureSetters.ForEach(v => v?.Set());

        foreach (IEffect eff in preset.effects)
            if (eff != null && eff.isActiveAndEnabled)
                eff.Prepare(size);

        var dt = Time.deltaTime;
        foreach (IEffect eff in preset.effects)
            if (eff != null && eff.isActiveAndEnabled)
                eff.Next(dt);
    }
    #endregion

    #region declarations
    [System.Serializable]
    public class TextureSetter {
        public Texture image;
        public UnityEvent<Texture> target = new UnityEvent<Texture>();

        public void Set() => target?.Invoke(image);
    }
    [System.Serializable]
    public class TextureSize {
        public int2 default_size = new int2(4, 4);
        public Texture texture;

        public int2 GetSize() => texture != null ? texture.Size() : default_size;
    }
    [System.Serializable]
    public class Events {
    }
    [System.Serializable]
    public class Preset {
        public List<MonoBehaviour> effects = new List<MonoBehaviour>();
        public List<TextureSetter> textureSetters = new List<TextureSetter>();
        public TextureSize texturesize = new TextureSize();
    }
    [System.Serializable]
    public class Tuner {
    }
    #endregion
}