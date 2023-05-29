using Gist2.Deferred;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour {
    public const float PI2_DEG = 360f;

    public float speed = 1f;
    public Vector3 axis = Vector3.up;

    protected Validator changed = new Validator();

    #region unity
    private void OnEnable() {
        changed.Reset();
        changed.OnValidate += () => {
            axis.Normalize();
        };
    }
    private void OnValidate() {
        changed.Invalidate();
    }
    void Update() {
        changed.Validate();
        transform.Rotate(axis, speed * PI2_DEG * Time.deltaTime);
    }
    #endregion
}