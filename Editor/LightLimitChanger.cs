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
        public LightLimitChangeParameters Parameters = LightLimitChangeParameters.Default;
        public VRCAvatarDescriptor TargetAvatar;
        private bool _isOptionFoldoutOpen = false;
        private bool _isVersionInfoFoldoutOpen = false;

        private const string GenerateObjectName = "Light Limit Changer";

        public const string Title = "Light Limit Changer For MA";
        public static string Version = string.Empty;

        private static readonly string[] _targetShaderLabels = Enum.GetNames(typeof(Shaders));

        private string infoLabel = "";

        [MenuItem("Tools/Modular Avatar/LightLimitChanger")]
        public static void CreateWindow()
        {
            var window = GetWindow<LightLimitChanger>(Title);
            window.minSize = new Vector2(380, 400);
            window.maxSize = new Vector2(1000, 400);
        }

        private void OnEnable()
        {
            Version = Utils.GetVersion();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            ShowVersionInfo();
            EditorGUILayout.Separator();
            ShowGeneratorMenu();
            EditorGUILayout.Separator();
            ShowSettingsMenu();
        }

        private void ShowGeneratorMenu()
        {
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
                            : LightLimitChangeParameters.Default;
                    }
                }

                using (new Utils.GroupScope(Localization.S("Parameter"), 180))
                {
                    var param = Parameters;

                    param.IsDefaultUse = EditorGUILayout.Toggle(Localization.S("DefaultUse"), param.IsDefaultUse);
                    param.IsValueSave = EditorGUILayout.Toggle(Localization.S("SaveValue"), param.IsValueSave);
                    param.MaxLightValue = EditorGUILayout.FloatField(Localization.S("MaxLight"), param.MaxLightValue);
                    param.MinLightValue = EditorGUILayout.FloatField(Localization.S("MinLight"), param.MinLightValue);
                    param.DefaultLightValue = EditorGUILayout.FloatField(Localization.S("DefaultLight"), param.DefaultLightValue);

                    using (var group = new Utils.FoldoutHeaderGroupScope(ref _isOptionFoldoutOpen, Localization.S("Options")))
                    {
                        if (group.IsOpen)
                        {
                            EditorGUI.BeginChangeCheck();
                            param.TargetShader = (Shaders)EditorGUILayout.MaskField(Localization.S("Target Shader"), (int)param.TargetShader, _targetShaderLabels);
                            if (EditorGUI.EndChangeCheck())
                            {
                                infoLabel = param.TargetShader == 0 ? Localization.S("Target Shader must be selected") : string.Empty;
                            }

                            param.AllowSaturationControl = EditorGUILayout.Toggle(Localization.S("Allow Saturation Control"), param.AllowSaturationControl);
                            param.AddResetButton = EditorGUILayout.Toggle(Localization.S("Add Reset Button"), param.AddResetButton);
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

                GUILayout.Label(infoLabel);
            }
        }

        private void ShowSettingsMenu()
        {
            Localization.ShowLocalizationUI();
        }

        private void ShowVersionInfo()
        {
            using (var foldout = new Utils.FoldoutHeaderGroupScope(ref _isVersionInfoFoldoutOpen, $"{Title} {Version}"))
            {
                if (foldout.IsOpen)
                {
                    DrawWebButton("BOOTH", "https://mochis-factory.booth.pm/items/4864776");
                    DrawWebButton("GitHub", "https://github.com/Azukimochi/LightLimitChangerForMA");
                }
            }
            
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

        /*
         * Quouted from https://github.com/lilxyzw/lilToon/blob/2ef370dc444172787c075ec3a822438c2bee26cb/Assets/lilToon/Editor/lilEditorGUI.cs#L65
         *
         * Copyright (c) 2020-2023 lilxyzw
         * 
         * Full Licence: https://github.com/lilxyzw/lilToon/blob/master/LICENSE
        */
        private static void DrawWebButton(string text, string URL)
        {
            var position = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect());
            var icon = EditorGUIUtility.IconContent("BuildSettings.Web.Small");
            icon.text = text;
            var style = new GUIStyle(EditorStyles.label) { padding = new RectOffset() };
            if (GUI.Button(position, icon, style))
            {
                Application.OpenURL(URL);
            }
        }
    }
}

#endif
