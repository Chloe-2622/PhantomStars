using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using UnityEditor;

namespace PhantomStars {
    public class QualityField : MonoBehaviour
    {
        [SerializeField] private UniversalRenderPipelineAsset urpAsset;

        [Header("Values")]
        [SerializeField] private int qualityLevel;
        [SerializeField] private List<float> qualityValues;

        [Header("Intern Object")]
        [SerializeField] private TMP_Dropdown dropdown;

        // Start is called before the first frame update
        void Start()
        {
            if (qualityValues.Count < dropdown.options.Count) 
            { 
                Debug.LogError("Not enought quality values given. Need at least " + dropdown.options.Count); 
            }

            dropdown.SetValueWithoutNotify(qualityLevel);
        }

        public void UpdateQuality() 
        {
            qualityLevel = dropdown.value;

            urpAsset.renderScale = qualityValues[dropdown.value];
        }

        private void OnValidate()
        {
            if (Selection.activeGameObject != this.gameObject) { return; }

            dropdown.SetValueWithoutNotify(qualityLevel);
        }

    }
}