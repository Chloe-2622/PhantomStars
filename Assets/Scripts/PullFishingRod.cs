using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PullFishingRod : MonoBehaviour
{
    [SerializeField] private InputActionReference pullAction;
    [SerializeField] private InputActionReference horizontalPullAction;
    [SerializeField] private InputActionReference startReelingAction;

    [SerializeField] private Fish pulledFish;
    [SerializeField] private float defaultPullSpeed = 0.5f;
    private float pullSpeed;
    private bool isPulling = false;
    private bool isPullingHorizontally = false;

    private float initialDistance;

    private float lastClosestAngle;
    private float lastClosestAngleTime;

    private float rollsThisSecond;
    private float rollPerSeconds;
    private float rollTimer;

    
    Vector2 horizontalPull;

    private Dictionary<float, (float, float)> closestAnglesOrder = new Dictionary<float, (float, float)> {
        {0, (90, -90)},
        {90, (180, 0)},
        {180, (90, -90)},
        {-90, (180, 0)}
    };

    private void Awake() {
        pullSpeed = defaultPullSpeed;
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

    /// <summary>
    /// This function and input action are for debug purposes
    /// The function to be called when the fish has been hooked
    /// during the hook phase needs to be StartReelingFish(Fish fish)
    /// </summary>
    /// <param name="context"></param>
    public void StartReeling(InputAction.CallbackContext context)
    {
        if (pulledFish != null) return;
        if (context.phase == InputActionPhase.Started)
        {
            pulledFish = GameObject.Find("Fish").GetComponent<Fish>();
            StartReelingFish(pulledFish);
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
            SetIsPullingFish(false);
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
        ResistanceManager.Instance.SetIsFishReeled(true);
        ResistanceManager.Instance.showDangerBar(true);
        fish.StartReeling();
    }

    public void StopReelingFish() {
        pulledFish = null;
        ResistanceManager.Instance.SetIsFishReeled(false);
        GameManager.Instance.pushFishingRod.enabled = true;
        Camera.main.GetComponent<CameraMovement>().SetIsShaking(false);
        ResistanceManager.Instance.showDangerBar(false);
    }
}