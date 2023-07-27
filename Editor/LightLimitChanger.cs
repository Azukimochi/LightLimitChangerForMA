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
            pos.size = new Vector2(380, 500);
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
                    TargetAvatar = EditorGUILayout.ObjectField(Localization.G("Avatar", "Set the avatar to generate animation"), TargetAvatar, typeof(VRCAvatarDescriptor), true) as VRCAvatarDescriptor;
                    if (EditorGUI.EndChangeCheck())
                    {
                        Parameters = TargetAvatar != null && TargetAvatar.TryGetComponentInChildren<LightLimitChangerSettings>(out var settings)
                            ? settings.Parameters
                            : LightLimitChangerParameters.Default;
                    }
                }

                var param = Parameters;

                using (new Utils.GroupScope(Localization.S("Parameter"), 180))
                {
                    

                    param.IsDefaultUse = EditorGUILayout.Toggle(Localization.G("DefaultUse", "Use the light animation in the initial state"), param.IsDefaultUse);
                    param.IsValueSave = EditorGUILayout.Toggle(Localization.G("SaveValue", "Keep brightness changes in the avatar"), param.IsValueSave);
                    param.OverwriteDefaultLightMinMax = EditorGUILayout.Toggle(Localization.G("Overwrite Default Min/Max", "Override the default avatar brightness with the lower and upper limit parameters below"), param.OverwriteDefaultLightMinMax);
                    param.MaxLightValue = EditorGUILayout.FloatField(Localization.G("MaxLight[0-10]", "Brightness upper limit setting"), param.MaxLightValue);
                    param.MinLightValue = EditorGUILayout.FloatField(Localization.G("MinLight[0-10]", "Brightness lower limit setting"), param.MinLightValue);
                    param.DefaultLightValue = EditorGUILayout.FloatField(Localization.G("DefaultLight[0-1]", "Initial brightness setting"), param.DefaultLightValue);

                }
                using (new Utils.GroupScope(Localization.S("Options"), 180))
                {
                    param.AllowColorTemp = EditorGUILayout.Toggle(Localization.G("Allow Color Temperature Control", "You can enable the Color Temperature adjustment function"), param.AllowColorTemp);
                    param.AllowSaturationControl = EditorGUILayout.Toggle(Localization.G("Allow Saturation Control", "You can enable the saturation adjustment function"), param.AllowSaturationControl);
                    param.AllowUnlitControl = EditorGUILayout.Toggle(Localization.G("Allow Unlit Control", "You can enable the Unlit adjustment function (Liltoon/Sunao Only)"), param.AllowUnlitControl);
                    param.AddResetButton = EditorGUILayout.Toggle(Localization.G("Add Reset Button", "Add a reset button to return the parameter to the set value"), param.AddResetButton);

                    using (var group = new Utils.FoldoutHeaderGroupScope(ref _isOptionFoldoutOpen, Localization.G("Advanced Setting")))
                    {
                        if (group.IsOpen)
                        {
                            EditorGUI.BeginChangeCheck();
                            param.TargetShader = (Shaders)EditorGUILayout.MaskField(Localization.G("Target Shader", "You can choose which shader to control"), (int)param.TargetShader, _targetShaderLabels);
                            if (EditorGUI.EndChangeCheck())
                            {
                                infoLabel = param.TargetShader == 0 ? Localization.S("Target Shader must be selected") : string.Empty;
                            }
                            param.ExcludeEditorOnly = EditorGUILayout.Toggle(Localization.G("Exclude EditorOnly", "Exclude objects marked with EditorOnly tag from animation"), param.ExcludeEditorOnly);
                            param.GenerateAtBuild = EditorGUILayout.Toggle(Localization.G("Generate At Build/PlayMode", "Automatically generate animations at build/play mode"), param.GenerateAtBuild);
                        }
                    }
                }
                Parameters = param;

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
