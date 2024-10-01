using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PhantomStars
{
    public class PullFishingRod : MonoBehaviour
    {
        [SerializeField] private InputActionReference pullAction;

        [SerializeField] private Fish pulledFish;
        [SerializeField] private float pullSpeed = 10.0f;
        private bool isPulling = false;

        private void OnEnable()
        {
            pullAction.action.Enable();
            pullAction.action.started += StartPulling;
            pullAction.action.canceled += StopPulling;
        }

        private void OnDisable()
        {
            pullAction.action.Disable();
            pullAction.action.started -= StartPulling;
            pullAction.action.canceled -= StopPulling;
        }

        private void StartPulling(InputAction.CallbackContext context)
        {
            Debug.Log("Start pulling");
            isPulling = true;

        }

        private void StopPulling(InputAction.CallbackContext context)
        {
            Debug.Log("Stop pulling");
            isPulling = false;
        }

        private void Update()
        {
            if (isPulling && pulledFish != null)
            {
                pulledFish.transform.position = Vector2.MoveTowards(pulledFish.transform.position, transform.position, pullSpeed * Time.deltaTime);
            }
        }

        public void SetPulledFish(Fish fish)
        {
            pulledFish = fish;
        }
    }
}