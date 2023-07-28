using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.Core;
using VRC.SDK3.Avatars.Components;

namespace io.github.azukimochi
{
    [CustomEditor(typeof(LightLimitChangerSettings))]
    internal sealed class LightLimitChangerSettingsEditor : Editor
    {
        private SerializedProperty FX;
        private SerializedProperty IsDefaultUse;
        private SerializedProperty IsValueSave;
        private SerializedProperty OverwriteDefaultLightMinMax;
        private SerializedProperty DefaultLightValue;
        private SerializedProperty MaxLightValue;
        private SerializedProperty MinLightValue;
        private SerializedProperty TargetShader;
        private SerializedProperty AllowColorTempControl;
        private SerializedProperty AllowSaturationControl;
        private SerializedProperty AllowUnlitControl;
        private SerializedProperty AddResetButton;
        private SerializedProperty GenerateAtBuild;
        private SerializedProperty ExcludeEditorOnly;

        private static bool _isOptionFoldoutOpen = true;

        private void OnEnable()
        {
            FX =                            serializedObject.FindProperty  (nameof(LightLimitChangerSettings.FX));
            var parameters =                serializedObject.FindProperty  (nameof(LightLimitChangerSettings.Parameters));
            IsDefaultUse =                  parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.IsDefaultUse));
            IsValueSave =                   parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.IsValueSave));
            OverwriteDefaultLightMinMax =   parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.OverwriteDefaultLightMinMax));
            DefaultLightValue =             parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.DefaultLightValue));
            MaxLightValue =                 parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.MaxLightValue));
            MinLightValue =                 parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.MinLightValue));
            TargetShader =                  parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.TargetShader));
            AllowColorTempControl =        parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.AllowColorTempControl));
            AllowSaturationControl =        parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.AllowSaturationControl));
            AllowUnlitControl =             parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.AllowUnlitControl));
            AddResetButton =                parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.AddResetButton));
            GenerateAtBuild =               parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.GenerateAtBuild));
            ExcludeEditorOnly =             parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.ExcludeEditorOnly));
        }

        public override void OnInspectorGUI()
        {
            Utils.ShowVersionInfo();

            EditorGUILayout.Separator();

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(IsDefaultUse, Localization.G("DefaultUse"));
            EditorGUILayout.PropertyField(IsValueSave, Localization.G("SaveValue"));
            EditorGUILayout.PropertyField(OverwriteDefaultLightMinMax, Localization.G("Overwrite Default Min/Max"));
            EditorGUILayout.PropertyField(MaxLightValue, Localization.G("MaxLight[0-10]"));
            EditorGUILayout.PropertyField(MinLightValue, Localization.G("MinLight[0-10]"));
            EditorGUILayout.PropertyField(DefaultLightValue, Localization.G("DefaultLight[0-1]"));

            EditorGUILayout.PropertyField(AllowColorTempControl, Localization.G("Allow Color Temperature Ctrl"));
            EditorGUILayout.PropertyField(AllowSaturationControl, Localization.G("Allow Saturation Control"));
            EditorGUILayout.PropertyField(AllowUnlitControl, Localization.G("Allow Unlit Control"));
            EditorGUILayout.PropertyField(AddResetButton, Localization.G("Add Reset Button"));

            using (var group = new Utils.FoldoutHeaderGroupScope(ref _isOptionFoldoutOpen, Localization.G("Options")))
            {
                if (group.IsOpen)
                {
                    TargetShader.intValue = EditorGUILayout.MaskField(Localization.G("Target Shader"), TargetShader.intValue, TargetShader.enumNames);

                    EditorGUILayout.PropertyField(ExcludeEditorOnly, Localization.G("Exclude EditorOnly"));
                    EditorGUILayout.Separator();
                    EditorGUILayout.PropertyField(GenerateAtBuild, Localization.G("Generate At Build/PlayMode"));

                    EditorGUILayout.Separator();
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.PropertyField(FX);
                    EditorGUI.EndDisabledGroup();
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUILayout.Separator();


            Localization.ShowLocalizationUI();
        }
    }
}
