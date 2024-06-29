using System;
using nadena.dev.ndmf.util;
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
        private SerializedProperty AllowEmissionControl;
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

        private bool ApplySettingToAvatar = false;
        private bool ApplySettingToProject = false;

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
            AllowEmissionControl = parameters.FindPropertyRelative(nameof(LightLimitChangerParameters.AllowEmissionControl));
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
                Utils.ShowDocumentLink();
                EditorGUILayout.Separator();
            }

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.LabelField(Localization.S("label.category.general_settings"), boldLabel);
                    EditorGUILayout.Space(5);
            
                    EditorGUILayout.PropertyField(IsDefaultUse, Localization.G("label.use_default", "tip.use_default"));
                    EditorGUILayout.PropertyField(IsValueSave, Localization.G("label.save_value", "tip.save_value"));
                }

                using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                {
                    EditorGUILayout.LabelField(Localization.S("category.save_settings"), boldLabel);
                    EditorGUILayout.Space(5);
                    
                    var settings = target as LightLimitChangerSettings;
                    var parameter = settings?.Parameters;
                    EditorGUI.BeginDisabledGroup(parameter == null || PrefabUtility.IsPartOfAnyPrefab(target) == false);

                    ApplySettingToAvatar = GUILayout.Button(Localization.S("label.apply_settings_avatar"), EditorStyles.miniButton);
                    if (ApplySettingToAvatar)
                    {
                        LightLimitChangerPrefab.SavePrefabSetting(serializedObject);
                    }
                    ApplySettingToProject = GUILayout.Button(Localization.S("label.apply_settings_project"), EditorStyles.miniButton);
                    if (ApplySettingToProject)
                    {
                        if (EditorUtility.DisplayDialog(Localization.S("Window.info.gloabl_settings.save"), Localization.S("Window.info.global_settings.save_message"), Localization.S("Window.info.choice.apply_save"), Localization.S("Window.info.cancel")))
                        {
                            LightLimitChangerPrefab.SavePrefabSettingAsGlobal(serializedObject, parameter);
                        }
                    }
                    
                    EditorGUI.EndDisabledGroup();

                    if (PrefabUtility.IsPartOfAnyPrefab(target) == false)
                    {
                        EditorGUILayout.HelpBox("UnpackPrefabされているので保存できません", MessageType.Info);
                    }
                }
            }
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
                EditorGUILayout.PropertyField(DefaultMaxLightValue, Localization.G("label.light_max_default"));
                EditorGUILayout.PropertyField(DefaultMinLightValue, Localization.G("label.light_min_default"));
            }
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(Localization.S("label.category.additional_settings"), boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.Space(4);
                    EditorGUILayout.LabelField(Localization.G("label.support_shaders"), EditorStyles.boldLabel);

                    PropertyFieldWithShaderLabel(AllowColorTempControl,
                        Localization.G("label.allow_color_tmp", "tip.allow_color_tmp"),
                        ShaderType.LilToon | ShaderType.Poiyomi | ShaderType.Sunao);

                    PropertyFieldWithShaderLabel(AllowSaturationControl,
                        Localization.G("label.allow_saturation", "tip.allow_saturation"),
                        ShaderType.LilToon | ShaderType.Poiyomi | ShaderType.Sunao);

                    PropertyFieldWithShaderLabel(AllowMonochromeControl,
                        Localization.G("label.allow_monochrome", "tip.allow_monochrome"),
                        ShaderType.LilToon | ShaderType.Poiyomi | ShaderType.Sunao);

                    PropertyFieldWithShaderLabel(AllowUnlitControl,
                        Localization.G("label.allow_unlit", "tip.allow_unlit"),
                        ShaderType.LilToon | ShaderType.Sunao);

                    PropertyFieldWithShaderLabel(AllowEmissionControl,
                        Localization.G("label.allow_emission", "tip.allow_emission"),
                        ShaderType.LilToon);

                    EditorGUILayout.Space(5);
                    EditorGUILayout.PropertyField(AddResetButton, Localization.G("label.allow_reset", "tip.allow_reset"));
                    EditorGUILayout.PropertyField(IsGroupingAdditionalControls, Localization.G("label.grouping_additional_controls"));

                }

                using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                {
                    EditorGUILayout.LabelField(Localization.G("info.initial_val"));
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.Space(10);
                        EditorGUILayout.LabelField(Localization.G("label.color_temp"), GUILayout.MaxWidth(70.0f), GUILayout.ExpandWidth(false));
                        EditorGUI.BeginDisabledGroup(AllowColorTempControl.boolValue == false);
                        EditorGUILayout.PropertyField(InitialTempControlValue, Localization.G(""));
                        EditorGUI.EndDisabledGroup();
                    }
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.Space(10);
                        EditorGUILayout.LabelField(Localization.G("label.saturation"), GUILayout.MaxWidth(70.0f), GUILayout.ExpandWidth(false));
                        EditorGUI.BeginDisabledGroup(AllowSaturationControl.boolValue == false);
                        EditorGUILayout.PropertyField(InitialSaturationControlValue, Localization.G(""));
                        EditorGUI.EndDisabledGroup();

                    }
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.Space(10);
                        EditorGUILayout.LabelField(Localization.G("label.monochrome"), GUILayout.MaxWidth(70.0f), GUILayout.ExpandWidth(false));
                        EditorGUI.BeginDisabledGroup(AllowMonochromeControl.boolValue == false);
                        EditorGUILayout.PropertyField(InitialMonochromeControlValue, Localization.G(""));
                        EditorGUI.EndDisabledGroup();
                    }
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.Space(10);
                        EditorGUILayout.LabelField(Localization.G("label.unlit"), GUILayout.MaxWidth(70.0f), GUILayout.ExpandWidth(false));
                        EditorGUI.BeginDisabledGroup(AllowUnlitControl.boolValue == false);
                        EditorGUILayout.PropertyField(InitialUnlitControlValue, Localization.G(""));
                        EditorGUI.EndDisabledGroup();
                    }
                }
            }

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

        private static readonly Lazy<GUIStyle> supportedLabelStyle_LilToon = new Lazy<GUIStyle>(() => ColoredStyleFactory(EditorStyles.miniLabel, new Color(0, 128, 128)));
        private static readonly Lazy<GUIStyle> supportedLabelStyle_Poiyomi = new Lazy<GUIStyle>(() => ColoredStyleFactory(EditorStyles.miniLabel, new Color(128, 128, 0)));
        private static readonly Lazy<GUIStyle> supportedLabelStyle_Sunao = new Lazy<GUIStyle>(() => ColoredStyleFactory(EditorStyles.miniLabel,  new Color(0, 128, 0)));

        private static GUIStyle ColoredStyleFactory(GUIStyle original, Color color)
        {
            var style = new GUIStyle(original);
            style.normal.textColor = color;
            return style;
        }

        private void PropertyFieldWithShaderLabel(SerializedProperty property, GUIContent label, ShaderType shaderType)
        {
            var mask = (target as LightLimitChangerSettings)?.Parameters?.TargetShaders.ToBitMask();
            bool disabled = ((uint)shaderType & mask) == 0;

            EditorGUI.BeginDisabledGroup(disabled);

            var rect = EditorGUILayout.GetControlRect();
            EditorGUI.BeginProperty(rect, label, property);

            var shaderLabelRect = rect;
            var propertyRect = rect;

            shaderLabelRect.width = 16f;

            propertyRect.width -= 32;
            propertyRect.x += 32;

            if (shaderType.HasFlag(ShaderType.LilToon))
            {
                EditorGUI.LabelField(shaderLabelRect, new GUIContent("L", string.Format(Localization.S("label.shader_supported"), "lilToon")), supportedLabelStyle_LilToon.Value);
            }
            shaderLabelRect.x += 8;
            if (shaderType.HasFlag(ShaderType.Sunao))
            {
                EditorGUI.LabelField(shaderLabelRect, new GUIContent("S", string.Format(Localization.S("label.shader_supported"), "Sunao Shader")), supportedLabelStyle_Sunao.Value);
            }
            shaderLabelRect.x += 8;
            if (shaderType.HasFlag(ShaderType.Poiyomi))
            {
                EditorGUI.LabelField(shaderLabelRect, new GUIContent("P", string.Format(Localization.S("label.shader_supported"), "Poiyomi Toon")), supportedLabelStyle_Poiyomi.Value);
            }

            var labelRect = propertyRect;
            labelRect.width = EditorGUIUtility.labelWidth - 32;
            EditorGUI.LabelField(labelRect, label);

            propertyRect.width -= labelRect.width;
            propertyRect.x += labelRect.width + 2; // ← チェックボックスずれるので気合で合わせてる

            EditorGUI.PropertyField(propertyRect, property, GUIContent.none);
            EditorGUI.EndProperty();

            EditorGUI.EndDisabledGroup();
        }

        [Flags]
        private enum ShaderType
        {
            None = 0,
            LilToon = 1,
            Sunao = 2,
            Poiyomi = 4,
        }
    }
}
