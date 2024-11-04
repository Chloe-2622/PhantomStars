using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool hasHookArrived;
    public bool isFishHooked;

    public PushFishingrod pushFishingRod;

    public int caughtFish = 0;

    public enum GameState
    {
        CATCHING,
        REELING
    }

    public PullFishingRod pullFishingRod;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}