using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Fish : MonoBehaviour
{
    private Camera mainCamera;
    private SpriteRenderer fishSprite;

    public enum FishState
    {
        IDLE,
        REELED
    }

    [Header("Sprites")]
    [SerializeField] private Sprite shadowSprite;
    [SerializeField] private Sprite reeledSprite;

    [SerializeField] private FishState fishState = FishState.IDLE;

    [Header("Movement")]
    [SerializeField] private float iddleSpeed = 3.0f;
    [SerializeField] private float reeledSpeed = 6.0f;

    // Minimum and maximum times the fish will wait before moving again
    [SerializeField] private float minMovingDelay = 1.0f;
    [SerializeField] private float maxMovingDelay = 3.0f;
    private float movingDelay = 0.0f;
    private float movingDelayTimer = 0.0f;

    [SerializeField] private LayerMask hookLayer;

    // Boolean indicating if the fish is moving towards a destination
    private bool isMoving = false;

    // Boolean indicating if the fish can move (used to delay the movement if the fish is not moving)
    private bool canMove = false;

    private bool swapX = false;
    private bool isReeledLeft = false;

    private bool isReeled;

    private void OnEnable()
    {
        ResistanceManager.Instance.EscapeEvent.AddListener(Escaped);
    }
    private void OnDisable() {
        ResistanceManager.Instance.EscapeEvent.RemoveListener(Escaped);
        Gamepad.current.SetMotorSpeeds(0f, 0f);
    }

    private void Awake()
    {
        mainCamera = Camera.main;

        fishSprite = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        switch (fishState)
        {
            case FishState.IDLE:
                RandomMovement();
                break;
            case FishState.REELED:
                ReeledMovement();
                break;
        }
    }

    /// <summary>
    /// Random fish movement when the fish is iddle. 
    /// The destination is a random position within the camera's view.
    /// </summary>
    private void RandomMovement()
    {
        if (isMoving) return;
        if (canMove)
        {
            float minX = mainCamera.ViewportToWorldPoint(Vector2.zero).x + fishSprite.bounds.extents.x;
            float maxX = mainCamera.ViewportToWorldPoint(Vector2.one).x - fishSprite.bounds.extents.x;
            float minY = mainCamera.ViewportToWorldPoint(Vector2.zero).y + fishSprite.bounds.extents.y;
            float maxY = mainCamera.ViewportToWorldPoint(Vector2.one).y - fishSprite.bounds.extents.y;
            minY = (maxY + minY) / 2.0f;

            Vector2 randomPosition = new (Random.Range(minX, maxX), Random.Range(minY, maxY));
            StartCoroutine(MoveRandom(randomPosition, iddleSpeed));
        }
        else
        {
            movingDelayTimer += Time.deltaTime;
            if (movingDelayTimer >= movingDelay)
            {
                canMove = true;
                movingDelayTimer = 0.0f;
            }
        }
    }

    /// <summary>
    /// Movement when the fish is reeled. Its y position is fixed 
    /// and it moves horizontally (faster) towards a random position.
    /// </summary>
    private void ReeledMovement()
    {
        if (isMoving) return;
        float minX;
        float maxX;
        if (swapX)
        {
            if (isReeledLeft) {
                minX = transform.position.x;
                maxX = mainCamera.ViewportToWorldPoint(Vector2.one).x - fishSprite.bounds.extents.x;
            } else {
                minX = mainCamera.ViewportToWorldPoint(Vector2.zero).x + fishSprite.bounds.extents.x;
                maxX = transform.position.x;
            }
        } else {
            minX = mainCamera.ViewportToWorldPoint(Vector2.zero).x + fishSprite.bounds.extents.x;
            maxX = mainCamera.ViewportToWorldPoint(Vector2.one).x - fishSprite.bounds.extents.x;
        }

        Vector2 randomPosition = new (Random.Range(minX, maxX), transform.position.y);
        isReeledLeft = randomPosition.x < transform.position.x;
        StartCoroutine(MoveReeled(randomPosition, reeledSpeed));
    }

    /// <summary>
    /// Generic movement coroutine. The fish will move towards 
    /// the destination at the specified speed.
    /// </summary>
    /// <returns></returns>
    private IEnumerator MoveRandom(Vector2 destination, float speed)
    {
        isMoving = true;
        fishSprite.flipX = destination.x < transform.position.x;

        while (!isReeled && transform.position != (Vector3)destination && isMoving)
        {
            transform.position = Vector2.MoveTowards(transform.position, destination, speed * Time.deltaTime);

            yield return null;
        }
        
        if (!isReeled) isMoving = false;
        canMove = false;
        movingDelay = Random.Range(minMovingDelay, maxMovingDelay);
    }

    private IEnumerator MoveReeled(Vector2 destination, float speed)
    {
        isMoving = true;
        swapX = false;
        fishSprite.flipX = destination.x < transform.position.x;

        float elapsedTime = 0.0f;
        float maxElaspedTime = Random.Range(1.0f, 3.0f);

        while (Vector3.Distance(transform.position, destination) > 0.1f)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= maxElaspedTime)
            {
                swapX = true;
                break;
            }
            destination = new Vector2(destination.x, transform.position.y);
            transform.position = Vector2.MoveTowards(transform.position, destination, speed * Time.deltaTime);

            yield return null;
        }
        isMoving = false;
        canMove = false;
        movingDelay = Random.Range(minMovingDelay, maxMovingDelay);
    }

    private void OnTriggerStay2D(Collider2D other) {
        if (isReeled && other.gameObject == ResistanceManager.Instance.current)
        {
            ResistanceManager.Instance.ChangeResistanceState(ResistanceManager.ResistanceState.REGENERATING);
            ResistanceManager.Instance.SetIsInCurrent(true);
        }
        if (!GameManager.Instance.isFishHooked && other.gameObject.layer == LayerMask.NameToLayer("Hook") && !isReeled && GameManager.Instance.hasHookArrived)
        {
            GameManager.Instance.pullFishingRod.gameObject.SetActive(true);
            GameManager.Instance.pullFishingRod.StartReelingFish(this);
        }
        if (isReeled && other.gameObject.name == "FishingRodHitbox")
        {
            Caught();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (isReeled && other.gameObject == ResistanceManager.Instance.current)
        {

            ResistanceManager.Instance.ChangeResistanceState(ResistanceManager.ResistanceState.RESISTING);
            ResistanceManager.Instance.SetIsInCurrent(false);
        }
    }

    /// <summary>
    /// Pulls the fish toward the player
    /// The x position is fixed and the fish moves vertically
    /// 
    /// Not used ATM
    /// </summary>
    /// <param name="pullDistance"></param>
    public void PullFish(float pullDistance)
    {
        transform.position = new Vector3(transform.position.x, transform.position.y - pullDistance, 0);
    }

    public float GetSpeed()
    {
        return fishState == FishState.IDLE ? iddleSpeed : reeledSpeed;
    }

    public void StartReeling() {
        fishState = FishState.REELED;
        isMoving = false;
        isReeled = true;
        GameManager.Instance.isFishHooked = true;
        GameManager.Instance.pushFishingRod.gameObject.SetActive(false);
        GameManager.Instance.pushFishingRod.SetHookActive(false);
        GameManager.Instance.pushFishingRod.SetIsThrowing(false);
        Camera.main.GetComponent<CameraMovement>().SetIsFollowingTarget(true);
        Camera.main.GetComponent<CameraMovement>().SetTarget(gameObject);
        Camera.main.GetComponent<CameraMovement>().SetTargetPosition(gameObject.transform.position);


        fishSprite.sprite = reeledSprite;
    }

    public void Escaped() {
        fishState = FishState.IDLE;
        fishSprite.sprite = shadowSprite;
        GameManager.Instance.pullFishingRod.StopReelingFish();
        isReeled = false;
        GameManager.Instance.pushFishingRod.gameObject.SetActive(true);
        GameManager.Instance.isFishHooked = false;
        GameManager.Instance.pullFishingRod.gameObject.SetActive(false);
        Camera.main.GetComponent<CameraMovement>().SetIsFollowingTarget(false);
        Camera.main.GetComponent<CameraMovement>().SetTarget(null);

        Gamepad.current.SetMotorSpeeds(0f, 0f);
        GameManager.Instance.pushFishingRod.RetrieveHook();
    }

    public void Caught() {
        if (GameManager.Instance.isTutorialActivated)
        {
            SceneManager.LoadScene("Title Screen");
        }
        else
        {
            GameManager.Instance.pullFishingRod.StopReelingFish();
            GameManager.Instance.caughtFish++;
            GameManager.Instance.isFishHooked = false;
            GameManager.Instance.pushFishingRod.gameObject.SetActive(true);
            GameManager.Instance.pullFishingRod.gameObject.SetActive(false);
            Camera.main.GetComponent<CameraMovement>().SetIsFollowingTarget(false);
            Camera.main.GetComponent<CameraMovement>().SetTarget(null);

            Destroy(gameObject);
            Gamepad.current.SetMotorSpeeds(0f, 0f);
            GameManager.Instance.pushFishingRod.RetrieveHook();

            GameManager.Instance.pullFishingRod.IncreaseFishCounter();
        }
    }
}