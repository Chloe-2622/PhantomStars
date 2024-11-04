using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PhantomStars.UI
{
    public class VolumeSlider : Selectable, ISubmitHandler
    {
        [Header("Audio")]
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
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI nameTextMesh;
        [SerializeField] private TextMeshProUGUI valueTextMesh;
        [SerializeField] private Image volumeIcon;

        private bool volumeActive = true;

        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();

            if (!audioMixer.GetFloat(audioMixerParameter, out float mixerValue))
            {
                Debug.LogError(audioMixerParameter + " is not a public parameter of the audioMixer " + audioMixer.ToString());
            }

            slider.colors = this.colors;
            button.colors = this.colors;

            nameTextMesh.text = sliderName;

            slider.SetValueWithoutNotify(PlayerPrefs.GetFloat(audioMixerParameter, 1f));
            volumeActive = PlayerPrefs.GetInt(audioMixerParameter + "_active", 1) == 1;

            UpdateMixerValue();
        }

        public void SliderUpdate()
        {
            volumeActive = true;
            PlayerPrefs.SetFloat(audioMixerParameter, slider.value);
            PlayerPrefs.SetInt(audioMixerParameter + "_active", 1);
            PlayerPrefs.Save();

            UpdateMixerValue();
        }

        private void UpdateMixerValue()
        {
            if (volumeActive)
            {
                audioMixer.SetFloat(audioMixerParameter, 20f * Mathf.Log10(slider.value));
            }
            else
            {
                audioMixer.SetFloat(audioMixerParameter, 20f * Mathf.Log10(slider.minValue));
            }

            valueTextMesh.text = ((int)(100 * slider.value)).ToString();

            UpdateSliderIcon();
        }

        private void UpdateSliderValue()
        {
            audioMixer.GetFloat(audioMixerParameter, out float mixerValue);

            mixerValue = Mathf.Pow(10, (mixerValue / 20f));

            slider.SetValueWithoutNotify(mixerValue);
            valueTextMesh.text = ((int)(100 * slider.value)).ToString();

            UpdateSliderIcon();
        }

        private void UpdateSliderIcon()
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
            if (volumeActive)
            {
                audioMixer.SetFloat(audioMixerParameter, 20f * Mathf.Log10(slider.value));
            }
            else
            {
                audioMixer.SetFloat(audioMixerParameter, 20f * Mathf.Log10(slider.minValue));
            }
            PlayerPrefs.SetInt(audioMixerParameter + "_active", volumeActive ? 1 : 0);
            PlayerPrefs.Save();

            UpdateSliderIcon();
        }

        #region Selectable
        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);

            button.OnSelect(eventData);
            slider.OnSelect(eventData);
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);

            button.OnDeselect(eventData);
            slider.OnDeselect(eventData);
        }

        public override void OnMove(AxisEventData eventData)
        {
            base.OnMove(eventData);
            
            slider.OnMove(eventData);
        }

        public void OnSubmit(BaseEventData eventData)
        {
            button.OnPointerClick(new PointerEventData(EventSystem.current));
        }
        #endregion Selectable

        protected override void OnValidate()
        {
            if (Selection.activeGameObject != this.gameObject) { return; }

            nameTextMesh.text = sliderName;

            bool test = audioMixer.GetFloat(audioMixerParameter, out float mixerValue);
            if (test)
            {
                Debug.Log("Parameter value : " + mixerValue);
            }
            UpdateSliderIcon();
        }
    }
}