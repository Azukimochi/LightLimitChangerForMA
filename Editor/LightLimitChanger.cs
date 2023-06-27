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
            pos.size = new Vector2(380, 450);
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
                using (new Utils.GroupScope(Localization.S("Select Avatar"), 180))
                {
                    EditorGUI.BeginChangeCheck();
                    TargetAvatar = EditorGUILayout.ObjectField(Localization.S("Avatar"), TargetAvatar, typeof(VRCAvatarDescriptor), true) as VRCAvatarDescriptor;
                    if (EditorGUI.EndChangeCheck())
                    {
                        Parameters = TargetAvatar != null && TargetAvatar.TryGetComponentInChildren<LightLimitChangerSettings>(out var settings)
                            ? settings.Parameters
                            : LightLimitChangerParameters.Default;
                    }
                }

                using (new Utils.GroupScope(Localization.S("Parameter"), 180))
                {
                    var param = Parameters;

                    param.IsDefaultUse = EditorGUILayout.Toggle(Localization.G("DefaultUse"), param.IsDefaultUse);
                    param.IsValueSave = EditorGUILayout.Toggle(Localization.G("SaveValue"), param.IsValueSave);
                    param.OverwriteDefaultLightMinMax = EditorGUILayout.Toggle(Localization.G("Overwrite Default Min/Max"), param.OverwriteDefaultLightMinMax);
                    param.MaxLightValue = EditorGUILayout.FloatField(Localization.G("MaxLight"), param.MaxLightValue);
                    param.MinLightValue = EditorGUILayout.FloatField(Localization.G("MinLight"), param.MinLightValue);
                    param.DefaultLightValue = EditorGUILayout.FloatField(Localization.G("DefaultLight"), param.DefaultLightValue);

                    using (var group = new Utils.FoldoutHeaderGroupScope(ref _isOptionFoldoutOpen, Localization.G("Options")))
                    {
                        if (group.IsOpen)
                        {
                            EditorGUI.BeginChangeCheck();
                            param.TargetShader = (Shaders)EditorGUILayout.MaskField(Localization.G("Target Shader"), (int)param.TargetShader, _targetShaderLabels);
                            if (EditorGUI.EndChangeCheck())
                            {
                                infoLabel = param.TargetShader == 0 ? Localization.S("Target Shader must be selected") : string.Empty;
                            }

                            param.AllowSaturationControl = EditorGUILayout.Toggle(Localization.G("Allow Saturation Control"), param.AllowSaturationControl);
                            param.AddResetButton = EditorGUILayout.Toggle(Localization.G("Add Reset Button"), param.AddResetButton);

                            param.ExcludeEditorOnly = EditorGUILayout.Toggle(Localization.G("Exclude EditorOnly"), param.ExcludeEditorOnly);
                            EditorGUILayout.Separator();
                            param.GenerateAtBuild = EditorGUILayout.Toggle(Localization.G("Generate At Build/PlayMode"), param.GenerateAtBuild);
                        }
                    }

                    Parameters = param;
                }
                
                using (new Utils.DisabledScope(TargetAvatar == null || Parameters.TargetShader == 0))
                {
                    string buttonLabel;
                    {
                        buttonLabel = TargetAvatar != null && TargetAvatar.TryGetComponentInChildren<LightLimitChangerSettings>(out var settings) && settings.IsValid()
                        ? "Regenerate"
                        : "Generate";
                    }

                    if (GUILayout.Button(Localization.S(buttonLabel)))
                    {
                        infoLabel = Localization.S( "Processing");
                        try
                        {
                            GenerateAssets();
                            infoLabel = Localization.S("Complete");
                        }
                        catch (Exception e)
                        {
                            infoLabel = $"{Localization.S("Error")}: {e.Message}";
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
                var savePath = EditorUtility.SaveFilePanelInProject(Localization.S("Save"), System.IO.Path.GetFileNameWithoutExtension(fileName), System.IO.Path.GetExtension(fileName).Trim('.'), Localization.S("Save Location"));
                if (string.IsNullOrEmpty(savePath))
                {
                    throw new Exception(Localization.S("Cancelled"));
                }

                var fx = new AnimatorController() { name = System.IO.Path.GetFileName(fileName) };
                AssetDatabase.CreateAsset(fx, savePath);

                var obj = avatar.gameObject.GetOrAddChild(GenerateObjectName);
                settings = obj.GetOrAddComponent<LightLimitChangerSettings>();
                settings.FX = fx;
            }

            settings.Parameters = Parameters;
            LightLimitGenerator.Generate(avatar, settings);
        }
    }
}

#endif
