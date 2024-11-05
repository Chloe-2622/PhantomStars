using UnityEngine;
using UnityEditor;
using UnityEditor.UI;
using PhantomStars.UI;

namespace PhantomStars
{
    [CustomEditor(typeof(VolumeSlider))]
    public class VolumeSliderEditor : SelectableEditor
    {
        [Header("Audio")]
        SerializedProperty sliderName;
        SerializedProperty audioMixerParameter;
        SerializedProperty audioMixer;

        [Header("Icons")]
        SerializedProperty highVolumeIcon;
        SerializedProperty mediumVolumeIcon;
        SerializedProperty lowVolumeIcon;
        SerializedProperty noVolumeIcon;

        [Header("Intern Object")]
        SerializedProperty slider;
        SerializedProperty button;
        SerializedProperty nameTextMesh;
        SerializedProperty valueTextMesh;
        SerializedProperty volumeIcon;

        protected override void OnEnable()
        {
            base.OnEnable();

            sliderName = serializedObject.FindProperty("sliderName");
            audioMixerParameter = serializedObject.FindProperty("audioMixerParameter");
            audioMixer = serializedObject.FindProperty("audioMixer");

            highVolumeIcon = serializedObject.FindProperty("highVolumeIcon");
            mediumVolumeIcon = serializedObject.FindProperty("mediumVolumeIcon");
            lowVolumeIcon = serializedObject.FindProperty("lowVolumeIcon");
            noVolumeIcon = serializedObject.FindProperty("noVolumeIcon");

            slider = serializedObject.FindProperty("slider");
            button = serializedObject.FindProperty("button");
            nameTextMesh = serializedObject.FindProperty("nameTextMesh");
            valueTextMesh = serializedObject.FindProperty("valueTextMesh");
            volumeIcon = serializedObject.FindProperty("volumeIcon");
        }

        public override void OnInspectorGUI()
        {
            // Affiche les propriétés de base du Button
            base.OnInspectorGUI();

            // Rafraîchir l'objet sérialisé
            serializedObject.Update();

            // Afficher les propriétés personnalisées de VolumeSlider
            EditorGUILayout.PropertyField(sliderName);
            EditorGUILayout.PropertyField(audioMixerParameter);
            EditorGUILayout.PropertyField(audioMixer);

            EditorGUILayout.PropertyField(highVolumeIcon);
            EditorGUILayout.PropertyField(mediumVolumeIcon);
            EditorGUILayout.PropertyField(lowVolumeIcon);
            EditorGUILayout.PropertyField(noVolumeIcon);

            EditorGUILayout.PropertyField(slider);
            EditorGUILayout.PropertyField(button);
            EditorGUILayout.PropertyField(nameTextMesh);
            EditorGUILayout.PropertyField(valueTextMesh);
            EditorGUILayout.PropertyField(volumeIcon);

            // Appliquer les changements
            serializedObject.ApplyModifiedProperties();
        }
    }
}