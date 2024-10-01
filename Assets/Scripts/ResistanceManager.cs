using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PhantomStars
{
    public class ResistanceManager : MonoBehaviour
    {
        public static ResistanceManager Instance { get; private set; }

        public enum ResistanceState
        {
            REGENERATING,
            RESISTING,
            HARDRESISTING
        }

        [Header("Parameters")]
        // When the resistance reaches maxResistance, the fish escapes
        // The resistance is increased by resistanceRate every second
        [SerializeField] private float resistance = 50f;
        [SerializeField] private float maxResistance = 100f;
        [SerializeField] private float resistanceRate = 10f;

        // Rate at which the resistance decreases while in current and not pulling
        [SerializeField] private float resistanceRegenerationRate = 5f;

        // Rate at which the resistance increases while in current and pulling or while not in current and not pulling
        [SerializeField] private float resistanceResistingRate = 10f;

        // Rate at which the resistance increases while not in current and pulling
        [SerializeField] private float resistanceHardResistingRate = 20f;

        [Header("Current")]
        public GameObject current;
        private bool isInCurrent = false;

        [Header("Fishing Rod")]
        private bool isPullingFish = false;
        [SerializeField] private bool isFishReeled = false;
        // Progression of the fish between 0 (farthest) and 1 (closest)
        private float fishProgression = 0.0f;

        [Header("Events")]
        public UnityEvent EscapeEvent = new UnityEvent();

        private ResistanceState resistanceState = ResistanceState.REGENERATING;

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (isFishReeled) UpdateResistance();
        }

        public void ChangeResistanceState(ResistanceState newState)
        {
            resistanceState = newState;

            switch (resistanceState)
            {
                case ResistanceState.REGENERATING:
                    resistanceRate = -resistanceRegenerationRate;
                    break;
                case ResistanceState.RESISTING:
                    resistanceRate = resistanceResistingRate;
                    break;
                case ResistanceState.HARDRESISTING:
                    resistanceRate = resistanceHardResistingRate;
                    break;
            }
        }

        private void UpdateResistance()
        {
            if (isInCurrent) {
                if (isPullingFish && resistanceState != ResistanceState.RESISTING)
                {
                    ChangeResistanceState(ResistanceState.RESISTING);
                }
                else if (!isPullingFish && resistanceState != ResistanceState.REGENERATING)
                {
                    ChangeResistanceState(ResistanceState.REGENERATING);
                }
            } else {
                if (isPullingFish && resistanceState != ResistanceState.HARDRESISTING)
                {
                    ChangeResistanceState(ResistanceState.HARDRESISTING);
                }
                else if (!isPullingFish && resistanceState != ResistanceState.RESISTING)
                {
                    ChangeResistanceState(ResistanceState.RESISTING);
                }
            }
            resistance += resistanceRate * (resistanceRate > 0 ? (1 + fishProgression) : 1) * Time.deltaTime;

            Camera.main.GetComponent<CameraShake>().SetIsShaking(resistanceRate > 0);
            if (resistanceRate > 0) Camera.main.GetComponent<CameraShake>().SetShakeIntensity(Mathf.Exp(4 * resistance / maxResistance - 4));

            resistance = Mathf.Clamp(resistance, 0, maxResistance);
            if (resistance >= maxResistance)
            {
                EscapeEvent.Invoke();
            }
        }

        public void SetIsInCurrent(bool value)
        {
            isInCurrent = value;
        }

        public bool IsInCurrent()
        {
            return isInCurrent;
        }

        public void SetIsPullingFish(bool value)
        {
            isPullingFish = value;
        }

        public void SetIsFishReeled(bool value)
        {
            isFishReeled = value;
            if (!value)
            {
                resistance = 50f;
            }
        }   

        public void SetFishProgression(float progression)
        {
            fishProgression = progression;
        }
    }
}