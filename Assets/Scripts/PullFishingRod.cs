using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PhantomStars
{
    public class PullFishingRod : MonoBehaviour
    {
        [SerializeField] private InputActionReference pullAction;
        [SerializeField] private InputActionReference horizontalPullAction;
        [SerializeField] private InputActionReference startReelingAction;

        [SerializeField] private Fish pulledFish;
        [SerializeField] private float pullSpeed = 0.5f;
        private bool isPulling = false;
        private bool isPullingHorizontally = false;

        private float initialDistance;

        public void VerticalPulling(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                isPulling = true;
                ResistanceManager.Instance.SetIsPullingFish(true);
            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                isPulling = false;
                ResistanceManager.Instance.SetIsPullingFish(false);
            }
        }
        public void HorizontalPulling(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                isPullingHorizontally = true;
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
                Debug.Log("Start reeling fish");
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

            if (isPulling)
            {
                pulledFish.transform.position = Vector2.MoveTowards(pulledFish.transform.position, transform.position, pullSpeed * Time.deltaTime);
            }
            if (isPullingHorizontally)
            {
                Vector2 horizontalPull = horizontalPullAction.action.ReadValue<Vector2>();

                pulledFish.transform.Translate(new Vector3(horizontalPull.x, 0, 0) * pulledFish.GetSpeed() * 1.33f * Time.deltaTime);
            }
        }

        public void SetPulledFish(Fish fish)
        {
            pulledFish = fish;
        }

        public bool IsPullingFish() {
            return isPulling;
        }

        public void StartReelingFish(Fish fish) {
            pulledFish = fish;
            initialDistance = pulledFish.transform.position.y - transform.position.y;

            ResistanceManager.Instance.SetIsFishReeled(true);
            fish.StartReeling();
        }

        public void StopReelingFish() {
            pulledFish = null;
            ResistanceManager.Instance.SetIsFishReeled(false);
            Camera.main.GetComponent<CameraShake>().SetIsShaking(false);
        }
    }
}