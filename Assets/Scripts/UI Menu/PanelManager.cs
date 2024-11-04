using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class UISelectablePanel
{
    public GameObject panel;
    public GameObject firstSelect;
}

public class PanelManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private List<UISelectablePanel> panels = new();

    private readonly Stack<UISelectablePanel> currentPanels = new();

    private void Start()
    {
        TogglePanel(panels[0].panel);
    }

    public void TogglePanel(GameObject panelToToggle)
    {
        if (currentPanels.Count > 0)
        {
            currentPanels.Peek().firstSelect = EventSystem.current.currentSelectedGameObject;
        }
        EventSystem.current.SetSelectedGameObject(null);

        foreach (UISelectablePanel panel in panels)
        {
            if (panel.panel == panelToToggle)
            {
                currentPanels.Push(panel);
                panel.panel.SetActive(true);
                EventSystem.current.SetSelectedGameObject(panel.firstSelect);
            }
            else
            {
                panel.panel.SetActive(false);
            }
        }
    }

    public void ReturnToPreviousPanel()
    {
        if (currentPanels.Count <= 1) { return; }

        currentPanels.Pop();

        EventSystem.current.SetSelectedGameObject(null);

        foreach (UISelectablePanel panel in panels)
        {
            if (panel == currentPanels.Peek())
            {
                panel.panel.SetActive(true);
                EventSystem.current.SetSelectedGameObject(panel.firstSelect);
            }
            else
            {
                panel.panel.SetActive(false);
            }
        }
    }

    public void ReturnToPreviousPanel(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            ReturnToPreviousPanel();
        }
    }

    public static void LoadGame()
    {
        SceneManager.LoadScene("Chloe");
    }

    public static void Quit()
    {
        Application.Quit();
    }

}
