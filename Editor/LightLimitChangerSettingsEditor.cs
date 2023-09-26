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
    [CanEditMultipleObjects]
    [CustomEditor(typeof(LightLimitChangerSettings))]
    internal sealed class LightLimitChangerSettingsEditor : Editor
    {
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

        private static bool _isOptionFoldoutOpen = true;

        internal bool IsWindowMode = false;

        private void OnEnable()
        {
            var parameters =                serializedObject.FindProperty  (nameof(LightLimitChangerSettings.Parameters));
            IsDefaultUse =                  parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.IsDefaultUse));
            IsValueSave =                   parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.IsValueSave));
            OverwriteDefaultLightMinMax =   parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.OverwriteDefaultLightMinMax));
            DefaultLightValue =             parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.DefaultLightValue));
            MaxLightValue =                 parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.MaxLightValue));
            MinLightValue =                 parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.MinLightValue));
            TargetShader =                  parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.TargetShader));
            AllowColorTempControl =         parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.AllowColorTempControl));
            AllowSaturationControl =        parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.AllowSaturationControl));
            AllowUnlitControl =             parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.AllowUnlitControl));
            AddResetButton =                parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.AddResetButton));
        }

        public override void OnInspectorGUI()
        {
            if (!IsWindowMode)
            {
                Utils.ShowVersionInfo();
                EditorGUILayout.Separator();
            }

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(IsDefaultUse, Localization.G("label.use_default"));
            EditorGUILayout.PropertyField(IsValueSave, Localization.G("label.save_value"));
            EditorGUILayout.PropertyField(OverwriteDefaultLightMinMax, Localization.G("label.override_min_max"));
            EditorGUILayout.PropertyField(MaxLightValue, Localization.G("label.light_max"));
            EditorGUILayout.PropertyField(MinLightValue, Localization.G("label.light_min"));
            EditorGUILayout.PropertyField(DefaultLightValue, Localization.G("label.light_default"));

            EditorGUILayout.PropertyField(AllowColorTempControl, Localization.G("label.allow_color_tmp"));
            EditorGUILayout.PropertyField(AllowSaturationControl, Localization.G("label.allow_saturation"));
            EditorGUILayout.PropertyField(AllowUnlitControl, Localization.G("label.allow_unlit"));
            EditorGUILayout.PropertyField(AddResetButton, Localization.G("label.allow_reset"));

            using (var group = new Utils.FoldoutHeaderGroupScope(ref _isOptionFoldoutOpen, Localization.G("category.select_option")))
            {
                if (group.IsOpen)
                {
                    TargetShader.intValue = EditorGUILayout.MaskField(Localization.G("label.target_shader"), TargetShader.intValue, ShaderInfo.RegisteredShaderInfoNames);
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            if (!IsWindowMode)
            {
                EditorGUILayout.Separator();
                Localization.ShowLocalizationUI();
            }
        }
    }
}
