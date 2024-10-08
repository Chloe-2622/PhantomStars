using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.U2D.Animation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PhantomStars.UI
{
    public class Menu : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject optionsPanel;
        [SerializeField] private GameObject controlsPanel;

        // Start is called before the first frame update
        void Start()
        {
            mainPanel.SetActive(true);
            optionsPanel.SetActive(false);
            controlsPanel.SetActive(false);
        }

        #region Panal Manager
        public void SwitchOptionsPanel()
        {
            mainPanel.SetActive(!mainPanel.activeSelf);
            optionsPanel.SetActive(!optionsPanel.activeSelf);
        }
        public void SwitchControlsPanel()
        {
            mainPanel.SetActive(!mainPanel.activeSelf);
            controlsPanel.SetActive(!controlsPanel.activeSelf);
        }
        public void ReturnToMain(InputAction.CallbackContext context)
        {
            Debug.Log("Coucou");

            mainPanel.SetActive(true);
            optionsPanel.SetActive(false);
            controlsPanel.SetActive(false);
        }
        #endregion Panal Manager
    }
}