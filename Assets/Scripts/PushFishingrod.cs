using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace PhantomStars
{
    public class PushFishingrod : MonoBehaviour
    {
        [Header("Actions")]
        [SerializeField] private InputActionReference chargeAction;
        [SerializeField] private InputActionReference stickChooseDirectionAction;
        [SerializeField] private InputActionReference crossChooseDirectionAction;

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

        private Vector2 windowSize;

        #region Initialize
        private void OnEnable()
        {
            chargeAction.action.Enable();
            stickChooseDirectionAction.action.Enable();
            crossChooseDirectionAction.action.Enable();

            chargeAction.action.started += StartCharge;
            chargeAction.action.canceled += StopCharge;

            stickChooseDirectionAction.action.performed += StickDirection;
            crossChooseDirectionAction.action.started += StartCrossDirection;
            crossChooseDirectionAction.action.canceled += StopCrossDirection;
        }

        private void OnDisable()
        {
            chargeAction.action.Disable();
            stickChooseDirectionAction.action.Disable();
            crossChooseDirectionAction.action.Disable();

            chargeAction.action.started -= StartCharge;
            chargeAction.action.canceled -= StopCharge;

            stickChooseDirectionAction.action.performed -= StickDirection;
            crossChooseDirectionAction.action.started -= StartCrossDirection;
            crossChooseDirectionAction.action.canceled -= StopCrossDirection;

            StopAllCoroutines();
        }

        void Start()
        {
            SetUIActive(false);
            windowSize = -Camera.main.ScreenToWorldPoint(Vector3.zero);

            Vector3 pos = Camera.main.WorldToScreenPoint(transform.position);
            directionArrow.transform.SetLocalPositionAndRotation(pos, Quaternion.Euler(0, 0, 90));
        }
        #endregion Initialize


        private void Update()
        {
            if (!crossChooseDirection || !isCharging) { return; }

            float direction = crossChooseDirectionAction.action.ReadValue<Vector2>()[0];

            float newAngle = Mathf.Max(0, Mathf.Min(180, directionArrow.transform.rotation.eulerAngles.z - direction * angleStep * Time.deltaTime));

            directionArrow.transform.rotation = Quaternion.Euler(0, 0, newAngle);
        }

        private void SetUIActive(bool active)
        {
            directionArrow.gameObject.SetActive(active);
            chargeImage.gameObject.SetActive(active);
        }

        #region Charge
        private void StartCharge(InputAction.CallbackContext context)
        {
            SetUIActive(true);
            isCharging = true;
            StartCoroutine(Charge(1));
        }

        private void StopCharge(InputAction.CallbackContext context)
        {
            SetUIActive(false);
            isCharging = false;
            StopAllCoroutines();

            Debug.Log(time / fillTime);
        }

        private IEnumerator Charge(int sens)
        {
            time = fillTime * ((1 - sens) / 2);

            while (time >= 0 && time <= fillTime)
            {
                chargeJauge.fillAmount = Mathf.Lerp(0, 1, time / fillTime);
                time += sens * Time.deltaTime;
                yield return null;
            }

            chargeJauge.fillAmount = (float)((1 + sens) / 2);
            StartCoroutine(Charge(-1 * sens));
        }
        #endregion Charge

        #region Direction
        private void StickDirection(InputAction.CallbackContext context)
        {
            if (!isCharging) { return; }

            Vector2 direction = context.action.ReadValue<Vector2>();

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

        private void StartCrossDirection(InputAction.CallbackContext context)
        {
            crossChooseDirection = true;
        }

        private void StopCrossDirection(InputAction.CallbackContext context)
        {
            crossChooseDirection = false;
        }
        #endregion Direction

        private void Throw()
        {

        }
    }
}