using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace PhantomStars.UI
{
    public class VolumeSlider : MonoBehaviour
    {
        [SerializeField] private string sliderName;
        [SerializeField] private string audioMixerParameter;
        [SerializeField] private AudioMixer audioMixer;

        [Header("Icons")]
        [SerializeField] private Sprite highVolumeIcon;
        [SerializeField] private Sprite mediumVolumeIcon;
        [SerializeField] private Sprite lowVolumeIcon;
        [SerializeField] private Sprite noVolumeIcon;

        [Header("Intern Object")]
        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI nameTextMesh;
        [SerializeField] private TextMeshProUGUI valueTextMesh;
        [SerializeField] private Image volumeIcon;

        private bool volumeActive = true;

        // Start is called before the first frame update
        void Start()
        {
            if (!audioMixer.GetFloat(audioMixerParameter, out float mixerValue))
            {
                Debug.LogError(audioMixerParameter + " is not a public parameter of the audioMixer " + audioMixer.ToString());
            }

            nameTextMesh.text = sliderName;
            InitSlider();
        }

        private void InitSlider()
        {
            audioMixer.GetFloat(audioMixerParameter, out float mixerValue);

            Debug.Log(mixerValue);

            mixerValue = Mathf.Pow(10, (mixerValue / 20f));

            slider.SetValueWithoutNotify(mixerValue);
            valueTextMesh.text = ((int)(100 * slider.value)).ToString();

            UpdateVolumeIcon();
        }

        public void SliderUpdate()
        {
            volumeActive = true;

            UpdateVolumeValue();
        }

        private void UpdateVolumeValue()
        {
            float sliderValue = slider.value;
            if (!volumeActive)
            {
                sliderValue = slider.minValue;
            }

            audioMixer.SetFloat(audioMixerParameter, 20f * Mathf.Log10(sliderValue));
            valueTextMesh.text = ((int)(100 * sliderValue)).ToString();

            UpdateVolumeIcon();
        }

        private void UpdateVolumeIcon()
        {
            if (volumeActive && slider.value > slider.minValue)
            {
                if (slider.value < 0.33)
                {
                    volumeIcon.sprite = lowVolumeIcon;
                }
                else if (slider.value > 0.66)
                {
                    volumeIcon.sprite = highVolumeIcon;
                }
                else
                {
                    volumeIcon.sprite = mediumVolumeIcon;
                }
            }
            else
            {
                volumeIcon.sprite = noVolumeIcon;
            }
        }

        public void SwitchVolume()
        {
            volumeActive = !volumeActive;
            UpdateVolumeValue();
        }

        private void OnValidate()
        {
            if(Selection.activeGameObject != this.gameObject) { return; }

            nameTextMesh.text = sliderName;

            bool test = audioMixer.GetFloat(audioMixerParameter, out float mixerValue);
            if (test)
            {
                Debug.Log("Parameter value : " + mixerValue);
            }

            UpdateVolumeIcon();
        }
    }
}