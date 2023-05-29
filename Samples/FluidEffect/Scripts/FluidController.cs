using Gist2.Extensions.SizeExt;
using LLGraphicsUnity;
using SimpleAndFastFluids;
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
        var size = preset.sourceImage.Size();
        if (math.any(size < 4)) return;

        events.Output_Image?.Invoke(preset.sourceImage);

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
    public class Events {
        public UnityEvent<Texture> Output_Image = new UnityEvent<Texture>();
    }
    [System.Serializable]
    public class Preset {
        public MonoBehaviour[] effects = new MonoBehaviour[0];
        public Texture sourceImage;
    }
    [System.Serializable]
    public class Tuner {
    }
    #endregion
}