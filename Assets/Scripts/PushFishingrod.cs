using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Diagnostics.CodeAnalysis;
using TMPro;

public class PushFishingrod : MonoBehaviour
{
    [Header("Boat")]
    [SerializeField] private Transform boat;

    [Header("UI")]
    [SerializeField] private Image chargeImage;
    [SerializeField] private Image directionArrow;

    [Header("Charge")]
    [SerializeField] private Image chargeJauge;
    [SerializeField] private float fillTime;
    private float time;
    private bool isCharging;

    [Header("Direction")]
    [SerializeField] private float angleStep;
    private bool crossChooseDirection = false;
    private int crossDirection;

    [Header("Throw")]
    [SerializeField] private float minThrowingRadius;
    [SerializeField] private float maxThrowTime;

    [Header("Target")]
    [SerializeField] private GameObject targetPrefab;
    [SerializeField] private float targetBlinkTime;
    private GameObject target;

    [Header("Hook")]
    [SerializeField] private GameObject hookPrefab;
    private GameObject hook;
    private bool hasHookArrived;
    private bool isThrowing;

    private float maxRadius;

    private void OnDisable()
    {
        StopAllCoroutines();
        SetUIActive(false);
        if (target != null) {target.SetActive(false);}
    }

    void Start()
    {
        SetUIActive(false);
        Vector2 windowSize = -Camera.main.ScreenToWorldPoint(Vector3.zero);

        maxRadius = Mathf.Min(windowSize.x, 2 * windowSize.y);

        directionArrow.transform.SetPositionAndRotation(Camera.main.WorldToScreenPoint(transform.position), Quaternion.Euler(0, 0, 90));

        target = GameObject.Instantiate(targetPrefab);
        target.SetActive(false);

        hook = GameObject.Instantiate(hookPrefab);
        hook.SetActive(false);
    }

    private void Update()
    {
        if (!crossChooseDirection || !isCharging) { return; }

        float newAngle = Fcrop(0, 180, directionArrow.transform.rotation.eulerAngles.z - crossDirection * angleStep * Time.deltaTime);

        directionArrow.transform.rotation = Quaternion.Euler(0, 0, newAngle);
    }

    private void SetUIActive(bool active)
    {
        if (directionArrow != null) directionArrow.gameObject.SetActive(active);
        chargeImage.gameObject.SetActive(active);
    }

    #region Charge
    public void ChargeInput(InputAction.CallbackContext context)
    {
        if (gameObject.activeSelf == false) { return; }
        if (context.phase == InputActionPhase.Started && !isThrowing)
        {
            SetUIActive(true);
            isCharging = true;
            StartCoroutine(Charging(1));
            SetHasHookArrived(false);
            SetHookActive(false);
        }
        else if (context.phase == InputActionPhase.Canceled && !isThrowing && isCharging)
        {
            SetUIActive(false);
            isCharging = false;
            StopAllCoroutines();

            Throw(time / fillTime);
        }
    }

    private IEnumerator Charging(int sens)
    {
        time = fillTime * ((1 - sens) / 2);

        while (time >= 0 && time <= fillTime)
        {
            chargeJauge.fillAmount = Mathf.Lerp(0, 1, time / fillTime);
            time += sens * Time.deltaTime;
            yield return null;
        }

        chargeJauge.fillAmount = (float)((1 + sens) / 2);
        StartCoroutine(Charging(-1 * sens));
    }
    #endregion Charge

    #region Direction
    public void StickDirectionInput(InputAction.CallbackContext context)
    {
        if (!isCharging) { return; }

        Vector2 direction = context.action.ReadValue<Vector2>();

        if (direction == Vector2.zero) { return; }

        if (direction.y < 0)
        {
            if (direction.x >= 0)
            {
                directionArrow.transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            else
            {
                directionArrow.transform.rotation = Quaternion.Euler(0, 0, 180);
            }
            return;
        }

        float angle = Vector2.Angle(Vector2.right, direction);
        directionArrow.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void CrossDirectionInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            crossChooseDirection = true;
            crossDirection = (int)context.action.ReadValue<Vector2>()[0];
        }
        if (context.phase == InputActionPhase.Performed)
        {
            crossDirection = (int)context.action.ReadValue<Vector2>()[0];
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            crossChooseDirection = false;
        }
    }
    #endregion Direction

    #region Throw
    private void Throw(float ratio)
    {
        Vector2 goal = directionArrow.transform.right * (minThrowingRadius + (maxRadius - minThrowingRadius) * ratio) + transform.position;

        // Target
        target.SetActive(true);
        target.transform.position = goal;

        // Hook
        hook.SetActive(true);
        hook.transform.SetPositionAndRotation(transform.position, directionArrow.transform.rotation * Quaternion.Euler(0, 0 , -90));

        float minThrowTime = (maxThrowTime * minThrowingRadius) / maxRadius;
        float throwTime = minThrowTime + (maxThrowTime - minThrowTime) * ratio;

        StartCoroutine(Throwing(goal, throwTime));;
    }

    private IEnumerator Throwing(Vector2 goal, float throwTime)
    {
        float t = 0;
        float targetBlink_t = 0;
        isThrowing = true;

        while (t < throwTime)
        {
            hook.transform.position = Vector3.Lerp(transform.position, goal, t / throwTime);

            if (targetBlink_t > targetBlinkTime)
            {
                target.SetActive(!target.activeSelf);
                targetBlink_t = 0;
            }
            
            t += Time.deltaTime;
            targetBlink_t += Time.deltaTime;
            yield return null;
        }

        SetHasHookArrived(true);
        isThrowing = false;

        target.SetActive(false);
    }
    #endregion Throw

    private static float Fcrop(float min, float max, float value)
    {
        return Mathf.Max(min, Mathf.Min(max, value));
    }

    public bool HasHookArrived()
    {
        return hasHookArrived;
    }

    private void SetHasHookArrived(bool value)
    {
        hasHookArrived = value;
        GameManager.Instance.hasHookArrived = value;
    }

    public void SetHookActive(bool value)
    {
        hook.SetActive(value);
    }

    public void SetIsThrowing(bool value)
    {
        isThrowing = value;
    }
}