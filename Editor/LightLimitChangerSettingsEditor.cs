using UnityEditor;

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
        private SerializedProperty DefaultMinLightValue;
        private SerializedProperty DefaultMaxLightValue;
        private SerializedProperty MaxLightValue;
        private SerializedProperty MinLightValue;
        private SerializedProperty TargetShaders;
        private SerializedProperty AllowColorTempControl;
        private SerializedProperty AllowSaturationControl;
        private SerializedProperty AllowMonochromeControl;
        private SerializedProperty AllowUnlitControl;
        private SerializedProperty AddResetButton;
        private SerializedProperty IsGroupingAdditionalControls;
        private SerializedProperty IsSeparateLightControl;
        private SerializedProperty Excludes;
        private SerializedProperty WriteDefaults;

        private static bool _isOptionFoldoutOpen = true;
        private static bool _isCepareteInitValFoldoutOpen = false;

        internal bool IsWindowMode = false;

        private void OnEnable()
        {
            var parameters = serializedObject.FindProperty(nameof(LightLimitChangerSettings.Parameters));
            IsDefaultUse = parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.IsDefaultUse));
            IsValueSave = parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.IsValueSave));
            OverwriteDefaultLightMinMax = parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.OverwriteDefaultLightMinMax));
            DefaultLightValue = parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.DefaultLightValue));
            DefaultMinLightValue = parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.DefaultMinLightValue));
            DefaultMaxLightValue = parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.DefaultMaxLightValue));
            MaxLightValue = parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.MaxLightValue));
            MinLightValue = parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.MinLightValue));
            TargetShaders = parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.TargetShaders));
            AllowColorTempControl = parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.AllowColorTempControl));
            AllowSaturationControl = parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.AllowSaturationControl));
            AllowMonochromeControl = parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.AllowMonochromeControl));
            AllowUnlitControl = parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.AllowUnlitControl));
            AddResetButton = parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.AddResetButton));
            IsSeparateLightControl = parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.IsSeparateLightControl));
            IsGroupingAdditionalControls = parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.IsGroupingAdditionalControls));
            Excludes = serializedObject.FindProperty(nameof(LightLimitChangerSettings.Excludes));
            WriteDefaults = serializedObject.FindProperty(nameof(LightLimitChangerSettings.WriteDefaults));
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

            EditorGUILayout.PropertyField(IsDefaultUse, Localization.G("label.use_default", "tip.use_default"));
            EditorGUILayout.PropertyField(IsValueSave, Localization.G("label.save_value", "tip.save_value"));
            EditorGUILayout.PropertyField(OverwriteDefaultLightMinMax, Localization.G("label.override_min_max", "tip.override_min_max"));
            EditorGUILayout.PropertyField(MaxLightValue, Localization.G("label.light_max", "tip.light_max"));
            EditorGUILayout.PropertyField(MinLightValue, Localization.G("label.light_min", "tip.light_min"));
            EditorGUI.BeginDisabledGroup(IsSeparateLightControl.boolValue == true);
            EditorGUILayout.PropertyField(DefaultLightValue, Localization.G("label.light_default", "tip.light_default"));
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.PropertyField(AllowColorTempControl, Localization.G("label.allow_color_tmp", "tip.allow_color_tmp"));
            EditorGUILayout.PropertyField(AllowSaturationControl, Localization.G("label.allow_saturation", "tip.allow_saturation"));
            EditorGUILayout.PropertyField(AllowMonochromeControl, Localization.G("label.allow_monochrome", "tip.allow_monochrome"));
            EditorGUILayout.PropertyField(AllowUnlitControl, Localization.G("label.allow_unlit", "tip.allow_unlit"));
            EditorGUILayout.PropertyField(AddResetButton, Localization.G("label.allow_reset", "tip.allow_reset"));

            using (var group = new Utils.FoldoutHeaderGroupScope(ref _isOptionFoldoutOpen, Localization.G("category.select_option")))
            {
                if (group.IsOpen)
                {
                    EditorGUILayout.PropertyField(TargetShaders, Localization.G("label.target_shader", "tip.target_shader"));
                    EditorGUILayout.PropertyField(IsSeparateLightControl, Localization.G("label.separate_light_control"));

                    EditorGUI.BeginDisabledGroup(IsSeparateLightControl.boolValue == false);
                    _isCepareteInitValFoldoutOpen = EditorGUILayout.Foldout(_isCepareteInitValFoldoutOpen,
                        Localization.G("label.separate_light_control_init_val"));
                    if (_isCepareteInitValFoldoutOpen)
                    {
                        EditorGUILayout.PropertyField(DefaultMinLightValue, Localization.G("label.light_min_default"));
                        EditorGUILayout.PropertyField(DefaultMaxLightValue, Localization.G("label.light_max_default"));
                    }
                    EditorGUI.EndDisabledGroup();

                    EditorGUILayout.PropertyField(IsGroupingAdditionalControls, Localization.G("label.grouping_additional_controls"));
                    WriteDefaults.intValue = EditorGUILayout.Popup(Utils.Label("Write Defaults"), WriteDefaults.intValue, new[] { Localization.S("label.match_avatar"), "OFF", "ON" });
                    EditorGUILayout.PropertyField(Excludes, Localization.G("label.excludes"));
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
