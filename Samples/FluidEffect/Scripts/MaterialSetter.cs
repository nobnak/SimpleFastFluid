using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialSetter : MonoBehaviour {

    public Material material;
    public string default_textureName = "_MainTex";

    #region propeties
    public Texture DefaultTexture {
        set => material.SetTexture(default_textureName, value);
    }
    public Texture MainTexture {
        set => material.mainTexture = value;
    }
    #endregion

    #region interfaces
    public void SetTexture(string name, Texture tex) {
        material.SetTexture(name, tex);
    }
    #endregion 
}