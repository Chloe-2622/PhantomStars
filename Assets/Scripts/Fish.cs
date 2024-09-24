using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fish : MonoBehaviour
{
    private Camera mainCamera;
    private SpriteRenderer fishSprite;

    public enum FishState {
        IDLE,
        REELED
    }

    [SerializeField] private FishState fishState = FishState.IDLE;

    [Header("Movement")]
    [SerializeField] private float iddleSpeed = 3.0f;
    [SerializeField] private float reeledSpeed = 6.0f;
    [SerializeField] private float minMovingDelay = 1.0f;
    [SerializeField] private float maxMovingDelay = 3.0f;

    private bool isMoving = false;
    private bool canMove = false;
    private float movingDelay = 0.0f;
    private float movingDelayTimer = 0.0f;

    private void Awake() {
        mainCamera = Camera.main;

        fishSprite = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update() {
        switch (fishState) {
            case FishState.IDLE:
                RandomMovement();
                break;
            case FishState.REELED:
                ReeledMovement();
                break;
        }
        RandomMovement();
    }

    private void RandomMovement() {
        if (isMoving) return;
        if (canMove) {
            float minX = mainCamera.ViewportToWorldPoint(Vector2.zero).x + fishSprite.bounds.extents.x;
            float maxX = mainCamera.ViewportToWorldPoint(Vector2.one).x - fishSprite.bounds.extents.x;
            float minY = mainCamera.ViewportToWorldPoint(Vector2.zero).y + fishSprite.bounds.extents.y;
            float maxY = mainCamera.ViewportToWorldPoint(Vector2.one).y - fishSprite.bounds.extents.y;

            Vector2 randomPosition = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
            StartCoroutine(Move(randomPosition, iddleSpeed));
        } else {
            movingDelayTimer += Time.deltaTime;
            if (movingDelayTimer >= movingDelay) {
                canMove = true;
                movingDelayTimer = 0.0f;
            }
        }
    }

    private IEnumerator Move(Vector2 destination, float speed) {
        isMoving = true;
        fishSprite.flipX = destination.x < transform.position.x;

        while (transform.position != (Vector3)destination) {
            transform.position = Vector2.MoveTowards(transform.position, destination, speed * Time.deltaTime);
            Debug.DrawLine(transform.position, destination, Color.red);
            yield return null;
        }
        isMoving = false;
        canMove = false;
        movingDelay = Random.Range(minMovingDelay, maxMovingDelay);
    }

    private void ReeledMovement() {
        if (isMoving) return;
        float minX = mainCamera.ViewportToWorldPoint(Vector2.zero).x + fishSprite.bounds.extents.x;
        float maxX = mainCamera.ViewportToWorldPoint(Vector2.one).x - fishSprite.bounds.extents.x;

        Vector2 randomPosition = new Vector2(Random.Range(minX, maxX), transform.position.y);
        StartCoroutine(Move(randomPosition, reeledSpeed));
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject == ResistanceManager.Instance.courant) {
            ResistanceManager.Instance.ChangeResistanceState(ResistanceManager.ResistanceState.REGENERATING);
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject == ResistanceManager.Instance.courant) {

            ResistanceManager.Instance.ChangeResistanceState(ResistanceManager.ResistanceState.RESISTING);
        }
    }

    public void PullFish(float pullDistance) {
        transform.position = new Vector3(transform.position.x, transform.position.y - pullDistance, 0);
    }
}
