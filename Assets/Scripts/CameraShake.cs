using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private float shakeIntensity = 0.0f;
    private float shakeReductionFactor = 10.0f;
    private bool isShaking;

    private Vector3 defaultPosition;

    private void Awake() {
        defaultPosition = transform.localPosition;
    }

    private void Update() {
        if (isShaking) Shake(shakeIntensity);
        else transform.localPosition = defaultPosition;
    }
 
    public void Shake(float intensity)
    {
        Vector3 randomPosition = defaultPosition + Random.insideUnitSphere * intensity / shakeReductionFactor;
        transform.localPosition = randomPosition;
    }

    public void SetIsShaking(bool isShaking) {
        this.isShaking = isShaking;
    }

    public void SetShakeIntensity(float shakeIntensity) {
        this.shakeIntensity = shakeIntensity;
    }
}
