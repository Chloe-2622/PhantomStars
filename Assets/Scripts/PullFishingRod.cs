using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class PullFishingRod : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private GameObject controllerUIHelp;
    [SerializeField] private GameObject tutorialUI;

    [Header("Parameters")]
    [SerializeField] private float defaultPullSpeed = 0.5f;

    [Header("Fish Counter")]
    [SerializeField] private TextMeshProUGUI fishCounter;
    private int fishCount = 0;

    private Fish pulledFish;
    private float pullSpeed;
    private bool isPulling = false;
    private bool isPullingHorizontally = false;

    private float initialDistance;

    private float lastClosestAngle;
    private float lastClosestAngleTime;

    private float rollsThisSecond;
    private float rollPerSeconds;
    private float rollTimer;

    bool tutorial = true;
    bool continueTutorialPressed = false;
    private bool fishPulling = false;


    Vector2 horizontalPull;

    private Dictionary<float, (float, float)> closestAnglesOrder = new Dictionary<float, (float, float)> {
        {0, (90, -90)},
        {90, (180, 0)},
        {180, (90, -90)},
        {-90, (180, 0)}
    };

    private void Awake() {
        pullSpeed = defaultPullSpeed;
        tutorial = GameManager.Instance.isTutorialActivated;
        GameManager.Instance.pullFishingRod = this;
    }

    public void KeyboardVerticalPulling(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            fishPulling = true;
            StartCoroutine(KeyboardVerticalPullingCoroutine());
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            fishPulling = false;
        }
    }

    public IEnumerator KeyboardVerticalPullingCoroutine()
    {
        while (fishPulling)
        {
            Debug.Log("Coucou");

            SetIsPullingFish(true);
            yield return null;
        }
    }

    public void VerticalPulling(InputAction.CallbackContext context)
    {
        if (pulledFish == null) return;
        float angle = Mathf.Atan2(context.ReadValue<Vector2>().y, context.ReadValue<Vector2>().x) * Mathf.Rad2Deg;

        float closestAngle;
        if (angle > 45 && angle < 135)
        {
            closestAngle = 90;
        }
        else if (angle > 135 || angle < -135)
        {
            closestAngle = 180;
        }
        else if (angle < -45 && angle > -135)
        {
            closestAngle = -90;
        }
        else
        {
            closestAngle = 0;
        }

        if (closestAngle != lastClosestAngle && (closestAnglesOrder[lastClosestAngle].Item1 == closestAngle || closestAnglesOrder[lastClosestAngle].Item2 == closestAngle)) 
        {
            lastClosestAngle = closestAngle;
            lastClosestAngleTime = Time.time;
            rollsThisSecond += 0.25f;
            pullSpeed = defaultPullSpeed * rollPerSeconds;
            SetIsPullingFish(true);
        }
    }
    public void HorizontalPulling(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            isPullingHorizontally = true;
        }
        else if (context.phase == InputActionPhase.Performed)
        {
            horizontalPull = context.ReadValue<Vector2>();
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            isPullingHorizontally = false;
        }
    }
    public void ContinueTutorial(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            continueTutorialPressed = true;
        }
    }

    private void Update()
    {
        PullingBehaviour();
    }

    private void PullingBehaviour() {
        if (pulledFish == null) return;

        float progression = (pulledFish.transform.position.y - transform.position.y - initialDistance) / initialDistance;
        ResistanceManager.Instance.SetFishProgression(progression);

        rollTimer += Time.deltaTime;
        if (rollTimer > 0.25f) {
            rollPerSeconds = 4 * rollsThisSecond;
            rollsThisSecond = 0;
            rollTimer = 0;
        }
        if (Time.time - lastClosestAngleTime > 0.2f) {
            if (!fishPulling)
            {
                SetIsPullingFish(false);
            }
        }
        if (isPulling)
        {
            pulledFish.transform.position = Vector2.MoveTowards(pulledFish.transform.position, transform.position, pullSpeed * Time.deltaTime);
        }
        if (isPullingHorizontally)
        {
            pulledFish.transform.Translate(new Vector3(horizontalPull.x, 0, 0) * pulledFish.GetSpeed() * 1.33f * Time.deltaTime);
        }
        
    }

    public void SetPulledFish(Fish fish)
    {
        pulledFish = fish;
    }

    public void SetIsPullingFish(bool value)
    {
        isPulling = value;
        ResistanceManager.Instance.SetIsPullingFish(value);
    }

    public bool IsPullingFish() {
        return isPulling;
    }

    public void StartReelingFish(Fish fish) {
        pulledFish = fish;
        initialDistance = pulledFish.transform.position.y - transform.position.y;

        // GameManager.Instance.pushFishingRod.enabled = false;
        // ResistanceManager.Instance.SetIsFishReeled(true);
        // ResistanceManager.Instance.showDangerBar(true);
        if (!tutorial) StartCoroutine(ShowControllerUIHelp());
        fish.StartReeling();

        if (tutorial)
        {
            tutorial = false;
            StartCoroutine(TutorialCoroutine());
        } else 
        {
            ResistanceManager.Instance.SetIsFishReeled(true);
            ResistanceManager.Instance.showDangerBar(true);
        }
    }

    public void StopReelingFish() {
        pulledFish = null;
        ResistanceManager.Instance.SetIsFishReeled(false);
        GameManager.Instance.pushFishingRod.enabled = true;
        Camera.main.GetComponent<CameraMovement>().SetIsShaking(false);
        ResistanceManager.Instance.showDangerBar(false);
        if (!tutorial) controllerUIHelp.SetActive(false);
    }

    private IEnumerator ShowControllerUIHelp() {
        controllerUIHelp.SetActive(true);
        GameObject arrows = controllerUIHelp.transform.Find("Arrows").gameObject;
        float rotationSpeed = -500.0f;

        while (pulledFish != null) {
            arrows.transform.Rotate(new Vector3(0, 0, rotationSpeed * Time.deltaTime));
            yield return null;
        }
    }

    private IEnumerator TutorialCoroutine()
    {
        tutorialUI.SetActive(true);

        // Show horizontal pull tutorial
        GameObject tutorialHorizontal = tutorialUI.transform.Find("Tutorial_Horizontal").gameObject;
        tutorialHorizontal.SetActive(true);
        continueTutorialPressed = false;
        Time.timeScale = 0;
        while (!continueTutorialPressed)
        {
            yield return null;
        }
        tutorialHorizontal.SetActive(false);
        Time.timeScale = 1;

        yield return new WaitForSeconds(5);

        // show vertical pull tutorial
        GameObject tutorialVertical = tutorialUI.transform.Find("Tutorial_Vertical").gameObject;
        tutorialVertical.SetActive(true);
        continueTutorialPressed = false;
        Time.timeScale = 0;
        while (!continueTutorialPressed)
        {
            yield return null;
        }
        tutorialVertical.SetActive(false);
        Time.timeScale = 1;
        StartCoroutine(ShowControllerUIHelp());

        yield return new WaitForSeconds(3);

        // Explain the resistance
        GameObject tutorialResistance = tutorialUI.transform.Find("Tutorial_Resistance").gameObject;
        tutorialResistance.SetActive(true);
        continueTutorialPressed = false;
        ResistanceManager.Instance.SetIsFishReeled(true);
        ResistanceManager.Instance.showDangerBar(true);
        Time.timeScale = 0;
        while (!continueTutorialPressed)
        {
            yield return null;
        }
        tutorialResistance.SetActive(false);
        Time.timeScale = 1;
        
        tutorialUI.SetActive(false);
    }

    public void IncreaseFishCounter()
    {
        fishCount++;
        fishCounter.text = fishCount.ToString();

        if (fishCount == 8)
        {
            SceneManager.LoadScene("Title Screen");
        }
    }
}