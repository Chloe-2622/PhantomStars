using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhantomStars
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

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
}