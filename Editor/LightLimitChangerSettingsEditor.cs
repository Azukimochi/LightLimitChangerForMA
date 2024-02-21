using UnityEditor;
using UnityEngine;

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
        private SerializedProperty InitialTempControlValue;
        private SerializedProperty InitialSaturationControlValue;
        private SerializedProperty InitialMonochromeControlValue;
        private SerializedProperty InitialUnlitControlValue;
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
            InitialTempControlValue = parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.InitialTempControlValue));
            InitialSaturationControlValue = parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.InitialSaturationControlValue));
            InitialMonochromeControlValue = parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.InitialMonochromeControlValue));
            InitialUnlitControlValue = parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.InitialUnlitControlValue));
            AddResetButton = parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.AddResetButton));
            IsSeparateLightControl = parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.IsSeparateLightControl));
            IsGroupingAdditionalControls = parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.IsGroupingAdditionalControls));
            Excludes = serializedObject.FindProperty(nameof(LightLimitChangerSettings.Excludes));
            WriteDefaults = serializedObject.FindProperty(nameof(LightLimitChangerSettings.WriteDefaults));
        }

        public override void OnInspectorGUI()
        {
            //new custom Editor styles include bold and large labels
            GUIStyle boldLabel = new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Bold, fontSize = 15};
            
            if (!IsWindowMode)
            {
                Utils.ShowVersionInfo();
                EditorGUILayout.Separator();
            }

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField(Localization.S("label.category.general_settings"), boldLabel);
            
            EditorGUILayout.PropertyField(IsDefaultUse, Localization.G("label.use_default", "tip.use_default"));
            EditorGUILayout.PropertyField(IsValueSave, Localization.G("label.save_value", "tip.save_value"));
            EditorGUILayout.Space(10);
            EditorGUILayout.PropertyField(IsSeparateLightControl, Localization.G("label.separate_light_control"));
            EditorGUILayout.PropertyField(MaxLightValue, Localization.G("label.light_max", "tip.light_max"));
            EditorGUILayout.PropertyField(MinLightValue, Localization.G("label.light_min", "tip.light_min"));
            EditorGUI.BeginDisabledGroup(IsSeparateLightControl.boolValue == true);
            EditorGUILayout.PropertyField(DefaultLightValue, Localization.G("label.light_default", "tip.light_default"));
            EditorGUI.EndDisabledGroup();
            
            EditorGUI.BeginDisabledGroup(IsSeparateLightControl.boolValue == false);
            _isCepareteInitValFoldoutOpen = EditorGUILayout.Foldout(_isCepareteInitValFoldoutOpen,
                Localization.G("label.separate_light_control_init_val"));
            if (_isCepareteInitValFoldoutOpen)
            {
                EditorGUILayout.PropertyField(DefaultMinLightValue, Localization.G("label.light_min_default"));
                EditorGUILayout.PropertyField(DefaultMaxLightValue, Localization.G("label.light_max_default"));
            }
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(Localization.S("label.category.additional_settings"), boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(AllowColorTempControl, Localization.G("label.allow_color_tmp", "tip.allow_color_tmp"));
            EditorGUI.BeginDisabledGroup(AllowColorTempControl.boolValue == false);
            EditorGUILayout.PropertyField(InitialTempControlValue, Localization.G(""));
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(AllowSaturationControl, Localization.G("label.allow_saturation", "tip.allow_saturation"));
            EditorGUI.BeginDisabledGroup(AllowSaturationControl.boolValue == false);
            EditorGUILayout.PropertyField(InitialSaturationControlValue, Localization.G(""));
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(AllowMonochromeControl, Localization.G("label.allow_monochrome", "tip.allow_monochrome"));
            EditorGUI.BeginDisabledGroup(AllowMonochromeControl.boolValue == false);
            EditorGUILayout.PropertyField(InitialMonochromeControlValue, Localization.G(""));
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(AllowUnlitControl, Localization.G("label.allow_unlit", "tip.allow_unlit"));
            EditorGUI.BeginDisabledGroup(AllowUnlitControl.boolValue == false);
            EditorGUILayout.PropertyField(InitialUnlitControlValue, Localization.G(""));
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.PropertyField(AddResetButton, Localization.G("label.allow_reset", "tip.allow_reset"));
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(IsGroupingAdditionalControls, Localization.G("label.grouping_additional_controls"));


            EditorGUILayout.Space(10);
            using (var group = new Utils.FoldoutHeaderGroupScope(ref _isOptionFoldoutOpen, Localization.G("category.select_option")))
            {
                if (group.IsOpen)
                {
                    EditorGUILayout.PropertyField(OverwriteDefaultLightMinMax, Localization.G("label.override_min_max", "tip.override_min_max"));
                    EditorGUILayout.PropertyField(TargetShaders, Localization.G("label.target_shader", "tip.target_shader"));
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
