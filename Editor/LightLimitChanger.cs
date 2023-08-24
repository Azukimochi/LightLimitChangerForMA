#if UNITY_EDITOR

using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.Core;
using nadena.dev.modular_avatar.core;

namespace io.github.azukimochi
{
    public sealed class LightLimitChanger : EditorWindow
    {
        public const string Title = "Light Limit Changer For MA";
        public LightLimitChangerParameters Parameters = LightLimitChangerParameters.Default;
        public VRCAvatarDescriptor TargetAvatar;
        private Vector2 _scrollPosition;
        private static bool _isOptionFoldoutOpen = false;

        private const string GenerateObjectName = "Light Limit Changer";

        private static readonly string[] _targetShaderLabels = Enum.GetNames(typeof(Shaders));

        private string infoLabel = "";

        [MenuItem("Tools/Modular Avatar/LightLimitChanger")]
        public static void CreateWindow()
        {
            var window = GetWindow<LightLimitChanger>(Title);
            var pos = window.position;
            pos.size = new Vector2(380, 530);
            window.position = pos;
            //window.maxSize = new Vector2(1000, 450);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            Utils.ShowVersionInfo();
            EditorGUILayout.Separator();
            ShowGeneratorMenu();
            ShowSettingsMenu();
        }

        private void ShowGeneratorMenu()
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            using (new Utils.DisabledScope(EditorApplication.isPlaying))
            {
                using (new Utils.GroupScope(Localization.S("category.select_avatar"), 180))
                {
                    EditorGUI.BeginChangeCheck();
                    TargetAvatar = EditorGUILayout.ObjectField(Localization.G("label.avatar", "tip.select_avatar"), TargetAvatar, typeof(VRCAvatarDescriptor), true) as VRCAvatarDescriptor;
                    if (EditorGUI.EndChangeCheck())
                    {
                        Parameters = TargetAvatar != null && TargetAvatar.TryGetComponentInChildren<LightLimitChangerSettings>(out var settings)
                            ? settings.Parameters
                            : LightLimitChangerParameters.Default;
                    }
                }

                var param = Parameters;

                using (new Utils.GroupScope(Localization.S("category.select_parameter"), 250))
                {
                    param.IsDefaultUse = EditorGUILayout.Toggle(Localization.G("label.use_default", "tip.use_default"), param.IsDefaultUse);
                    param.IsValueSave = EditorGUILayout.Toggle(Localization.G("label.save_value", "tip.save_value"), param.IsValueSave);
                    param.OverwriteDefaultLightMinMax = EditorGUILayout.Toggle(Localization.G("label.override_min_max", "tip.override_min_max"), param.OverwriteDefaultLightMinMax);
                    param.MaxLightValue = EditorGUILayout.FloatField(Localization.G("label.light_max", "tip.light_max"), param.MaxLightValue);
                    param.MinLightValue = EditorGUILayout.FloatField(Localization.G("label.light_min", "tip.light_min"), param.MinLightValue);
                    param.DefaultLightValue = EditorGUILayout.FloatField(Localization.G("label.light_default", "tip.light_default"), param.DefaultLightValue);

                }
                using (new Utils.GroupScope(Localization.S("category.select_option"), 250))
                {
                    param.AllowColorTempControl = EditorGUILayout.Toggle(Localization.G("label.allow_color_tmp", "tip.allow_color_tmp"), param.AllowColorTempControl);
                    param.AllowSaturationControl = EditorGUILayout.Toggle(Localization.G("label.allow_saturation", "tip.allow_saturation"), param.AllowSaturationControl);
                    param.AllowUnlitControl = EditorGUILayout.Toggle(Localization.G("label.allow_unlit", "tip.allow_unlit"), param.AllowUnlitControl);
                    param.AddResetButton = EditorGUILayout.Toggle(Localization.G("label.allow_reset", "tip.allow_reset"), param.AddResetButton);
                    

                    using (var group = new Utils.FoldoutHeaderGroupScope(ref _isOptionFoldoutOpen, Localization.G("category.select_advanced")))
                    {
                        if (group.IsOpen)
                        {
                            EditorGUI.BeginChangeCheck();
                            param.TargetShader = (Shaders)EditorGUILayout.MaskField(Localization.G("label.target_shader", "tip.target_shader"), (int)param.TargetShader, _targetShaderLabels);
                            if (EditorGUI.EndChangeCheck())
                            {
                                infoLabel = param.TargetShader == 0 ? Localization.S("info.shader_must_select") : string.Empty;
                            }
                            param.ExcludeEditorOnly = EditorGUILayout.Toggle(Localization.G("label.allow_editor_only", "tip.allow_editor_only"), param.ExcludeEditorOnly);
                            param.GenerateAtBuild = EditorGUILayout.Toggle(Localization.G("label.allow_gen_playmode", "tip.allow_gen_playmode"), param.GenerateAtBuild);
                            param.AllowOverridePoiyomiAnimTag = EditorGUILayout.Toggle(
                                Localization.G("label.allow_override_poiyomi", "tip.allow_override_poiyomi"), param.AllowOverridePoiyomiAnimTag);
                        }
                    }
                }
                Parameters = param;

                using (new Utils.DisabledScope(TargetAvatar == null || Parameters.TargetShader == 0))
                {
                    string buttonLabel;
                    {
                        buttonLabel = TargetAvatar != null && TargetAvatar.TryGetComponentInChildren<LightLimitChangerSettings>(out var settings) && settings.IsValid()
                        ? "info.re_generate"
                        : "info.generate";
                    }

                    if (GUILayout.Button(Localization.S(buttonLabel)))
                    {
                        infoLabel = Localization.S( "info.process");
                        try
                        {
                            GenerateAssets();
                            infoLabel = Localization.S("info.complete");
                        }
                        catch (Exception e)
                        {
                            infoLabel = $"{Localization.S("info.error")}: {e.Message}";
                        }
                    }
                }

            }
            GUILayout.EndScrollView();
            GUILayout.Label(Utils.Label(infoLabel));
        }

        private void ShowSettingsMenu()
        {
            Localization.ShowLocalizationUI();
        }

        private void GenerateAssets()
        {
            var avatar = TargetAvatar;

            var settings = avatar.GetComponentInChildren<LightLimitChangerSettings>(true);

            if (settings == null || !settings.IsValid())
            {
                var fileName = $"{TargetAvatar.name}_{DateTime.Now:yyyyMMddHHmmss}_{GUID.Generate()}.controller";
                var savePath = EditorUtility.SaveFilePanelInProject(Localization.S("info.save"), System.IO.Path.GetFileNameWithoutExtension(fileName), System.IO.Path.GetExtension(fileName).Trim('.'), Localization.S("info.save_location"));
                if (string.IsNullOrEmpty(savePath))
                {
                    throw new Exception(Localization.S("info.cancelled"));
                }

                var fx = new AnimatorController() { name = System.IO.Path.GetFileName(fileName) };
                AssetDatabase.CreateAsset(fx, savePath);

                var obj = avatar.gameObject.GetOrAddChild(GenerateObjectName);
                settings = obj.GetOrAddComponent<LightLimitChangerSettings>();
                settings.FX = fx;
            }

            settings.Parameters = Parameters;
            LightLimitGenerator.Generate(avatar, settings);
            if (Parameters.AllowColorTempControl || Parameters.AllowSaturationControl)
            {
                string PreferenceKey = "io.github.azukimochi.light-limit-changer.lang";

                if (EditorPrefs.GetInt(PreferenceKey, 1) == 1)
                {
                    EditorUtility.DisplayDialog(
                        "上級者向け設定使用中",
                        "焼き込みなどの前処理を行わないと不具合が起きる可能性があります。", "OK");
                }
                else 
                    EditorUtility.DisplayDialog(
                        "Now using Advanced setting",
                        "If you do not perform preprocessing such as burning, problems may occur.", "OK");
            }
        }
    }
}

#endif
