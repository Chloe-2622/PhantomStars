using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float shakeIntensity = 0.0f;
    [SerializeField] private float shakeReductionFactor = 10.0f;

    private GameObject target;
    private Vector3 targetPosition;
    private bool isShaking;
    private bool isFollowingTarget;
    private bool isMovingToTarget;
    private Vector3 defaultPosition;

    private void Awake() {
        defaultPosition = transform.localPosition;
        targetPosition = defaultPosition;
    }

    private void Update() {
        if (isShaking) Shake(shakeIntensity);
        else transform.localPosition = targetPosition;

        if (isFollowingTarget && !isMovingToTarget) {
            isMovingToTarget = true;
            StartCoroutine(MoveToTarget(targetPosition));
        }
    }
 

    public void Shake(float intensity)
    {
        Vector3 randomPosition = targetPosition + ((Time.timeScale != 0) ? Random.insideUnitSphere * intensity / shakeReductionFactor : Vector3.zero);
        transform.localPosition = randomPosition;
    }

    private IEnumerator MoveToTarget(Vector3 targetPosition) {
        Camera.main.orthographicSize = 3;
        while (isFollowingTarget) {
            SetTargetPosition(target.transform.position);

            Vector3 newPosition = Vector3.MoveTowards(transform.localPosition, new Vector3(targetPosition.x, targetPosition.y, -10), 10.0f * Time.deltaTime);
            newPosition.y = Mathf.Clamp(newPosition.y, -2.09f, 2.35f);
            transform.localPosition = newPosition;
            yield return null;
        }
        isMovingToTarget = false;
        this.targetPosition = defaultPosition;
        Camera.main.orthographicSize = 5;
    }

    public void SetIsShaking(bool isShaking) {
        this.isShaking = isShaking;
    }

    public void SetShakeIntensity(float shakeIntensity) {
        this.shakeIntensity = shakeIntensity;
    }

    public void SetIsFollowingTarget(bool isFollowingTarget) {
        this.isFollowingTarget = isFollowingTarget;
    }

    public void SetTargetPosition(Vector3 targetPosition) {
        this.targetPosition = new Vector3(targetPosition.x, targetPosition.y, -10.0f);
    }

    public void SetTarget(GameObject target) {
        this.target = target;
    }
}
