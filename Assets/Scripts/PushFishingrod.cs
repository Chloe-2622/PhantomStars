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
    private bool hasHookArrived = true;
    private bool isThrowing;
    private bool isHookAvailable;

    private float maxRadius;

    [Header("Tutorial")]
    [SerializeField] private GameObject tutorialUI;
    [SerializeField] private GameObject loadingBar;
    [SerializeField] private GameObject retreiveTutorialUI;
    private bool continueTutorialPressed = false;
    private bool tutorialOnce = false;
    private readonly float holdingTime = 1.0f;
    private float holdingTimer = 0.0f;

    private void OnDisable()
    {
        StopAllCoroutines();
        if (target != null) {target.SetActive(false);}
    }

    void Start()
    {
        isHookAvailable = true;
        GameManager.Instance.pushFishingRod = this;

        SetUIActive(false);
        Vector2 windowSize = -Camera.main.ScreenToWorldPoint(Vector3.zero);

        maxRadius = Mathf.Min(windowSize.x, 2 * windowSize.y);

        directionArrow.transform.SetPositionAndRotation(Camera.main.WorldToScreenPoint(transform.position), Quaternion.Euler(0, 0, 90));

        target = GameObject.Instantiate(targetPrefab);
        target.SetActive(false);

        hook = GameObject.Instantiate(hookPrefab);
        hook.SetActive(false);

        if (GameManager.Instance.isTutorialActivated)
        {
            StartCoroutine(TutorialCoroutine());
        }
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

    #region Tutorial

    private IEnumerator TutorialCoroutine()
    {
        tutorialUI.SetActive(true);

        yield return new WaitForSeconds(2);

        // Show horizontal pull tutorial
        GameObject tutorialCharge = tutorialUI.transform.Find("Tutorial_Charge").gameObject;
        tutorialCharge.SetActive(true);
        Time.timeScale = 0;
        float t = 0.0f;
        float readingTime = 5.0f;
        loadingBar.SetActive(true);
        while (t < readingTime) {
            t += Time.unscaledDeltaTime;
            loadingBar.transform.localScale = new Vector3(Mathf.Lerp(12.0f, 0.0f, t / readingTime), 0.34f, 1);
            yield return null;
        }
        tutorialCharge.SetActive(false);
        loadingBar.SetActive(false);
        Time.timeScale = 1;

        while (holdingTimer < holdingTime)
        {
            Debug.Log("Holding time: " + holdingTimer);
            yield return null;
        }

        GameObject tutorialRelease = tutorialUI.transform.Find("Tutorial_Release").gameObject;
        tutorialRelease.SetActive(true);
        loadingBar.SetActive(true);
        Time.timeScale = 0;
        t = 0.0f;
        readingTime = 7.0f;
        while (t < readingTime) {
            t += Time.unscaledDeltaTime;
            loadingBar.transform.localScale = new Vector3(Mathf.Lerp(12.0f, 0.0f, t / readingTime), 0.34f, 1);
            yield return null;
        }
        loadingBar.SetActive(false);
        tutorialRelease.SetActive(false);
        Time.timeScale = 1;

        tutorialUI.SetActive(false);
    }

    private IEnumerator RetrieveTutorialCoroutine()
    {
        continueTutorialPressed = false;
        retreiveTutorialUI.SetActive(true);

        while (!continueTutorialPressed)
        {
            yield return null;
        }

        retreiveTutorialUI.SetActive(false);
    }

    public void ContinueTutorial(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            continueTutorialPressed = true;
        }
    }

    #endregion Tutorial

    #region Charge
    public void ChargeInput(InputAction.CallbackContext context)
    {
        if (!isHookAvailable) return;

        if (!gameObject.activeSelf) { return; }
        if (Time.timeScale <= 0) return;
        if (context.phase == InputActionPhase.Started && !isThrowing)
        {
            holdingTimer = 0.0f;
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
            holdingTimer = 0.0f;

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
            holdingTimer += Time.deltaTime;
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

        if (Time.timeScale == 0) return;

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
        if (Time.timeScale == 0) return;
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
        isHookAvailable = false;

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

        if (GameManager.Instance.isTutorialActivated && !tutorialOnce)
        {
            tutorialOnce = true;
            yield return StartCoroutine(RetrieveTutorialCoroutine());
        }
        

        SetHasHookArrived(true);
        isThrowing = false;

        target.SetActive(false);
    }
    #endregion Throw

    public void RetrieveHook()
    {
        hook.SetActive(false);

        isHookAvailable = true;
    }

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