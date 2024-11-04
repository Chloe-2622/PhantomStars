using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEditor;

[System.Serializable]
public struct QualityLevel
{
    public string qualityName;
    public float value;
}

public class URPQualityField : MonoBehaviour
{
    /* Install Universal RP Package
     * Create a URP Asset (Create > Rendering)
     * Set URP Asset as Render Pipeline (Edit > Project Settings > Graphics)
     */

    [SerializeField] private UniversalRenderPipelineAsset urpAsset;

    [SerializeField] private int qualityLevelIndex;
    [SerializeField] private List<QualityLevel> qualityLevels;
    [SerializeField] private TMP_Dropdown dropdown;

    // Start is called before the first frame update
    void Start()
    {
        qualityLevelIndex = PlayerPrefs.GetInt("Graphic Quality", 2);
        Debug.Log(qualityLevelIndex);

        dropdown.SetValueWithoutNotify(qualityLevelIndex);
    }

    public void UpdateQuality()
    {
        qualityLevelIndex = dropdown.value;

        PlayerPrefs.SetInt("Graphic Quality", qualityLevelIndex);
        PlayerPrefs.Save();

        Debug.Log(PlayerPrefs.GetInt("Graphic Quality", 2));


        urpAsset.renderScale = qualityLevels[qualityLevelIndex].value;
    }

    private void OnValidate()
    {
        if (Selection.activeGameObject != this.gameObject) { return; }

        dropdown.ClearOptions();
        List<string> options = new();

        foreach (QualityLevel level in qualityLevels) 
        {
            options.Add(level.qualityName);
        }
        dropdown.AddOptions(options);

        dropdown.SetValueWithoutNotify(qualityLevelIndex);
    }
}
