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
        private SerializedProperty DefaultLightValue;
        private SerializedProperty MaxLightValue;
        private SerializedProperty MinLightValue;
        private SerializedProperty TargetShader;
        private SerializedProperty AllowSaturationControl;
        private SerializedProperty AddResetButton;
        private SerializedProperty GenerateAtBuild;
        private SerializedProperty ExcludeEditorOnly;

        private static bool _isOptionFoldoutOpen = true;
        private static bool _isVersionInfoFoldoutOpen = false;
        private const string Title = "Light Limit Changer For MA";
        private static string Version = string.Empty;

        private void OnEnable()
        {
            FX =                        serializedObject.FindProperty  (nameof(LightLimitChangerSettings.FX));
            var parameters =            serializedObject.FindProperty  (nameof(LightLimitChangerSettings.Parameters));
            IsDefaultUse =              parameters.FindPropertyRelative(nameof(LightLimitChangeParameters.IsDefaultUse));
            IsValueSave =               parameters.FindPropertyRelative(nameof(LightLimitChangeParameters.IsValueSave));
            DefaultLightValue =         parameters.FindPropertyRelative(nameof(LightLimitChangeParameters.DefaultLightValue));
            MaxLightValue =             parameters.FindPropertyRelative(nameof(LightLimitChangeParameters.MaxLightValue));
            MinLightValue =             parameters.FindPropertyRelative(nameof(LightLimitChangeParameters.MinLightValue));
            TargetShader =              parameters.FindPropertyRelative(nameof(LightLimitChangeParameters.TargetShader));
            AllowSaturationControl =    parameters.FindPropertyRelative(nameof(LightLimitChangeParameters.AllowSaturationControl));
            AddResetButton =            parameters.FindPropertyRelative(nameof(LightLimitChangeParameters.AddResetButton));
            GenerateAtBuild =           parameters.FindPropertyRelative(nameof(LightLimitChangeParameters.GenerateAtBuild));
            ExcludeEditorOnly =         parameters.FindPropertyRelative(nameof(LightLimitChangeParameters.ExcludeEditorOnly));

            Version = Utils.GetVersion();
        }

        public override void OnInspectorGUI()
        {
            ShowVersionInfo();

            EditorGUILayout.Separator();

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(IsDefaultUse, Localization.G("DefaultUse"));
            EditorGUILayout.PropertyField(IsValueSave, Localization.G("SaveValue"));
            EditorGUILayout.PropertyField(MaxLightValue, Localization.G("MaxLight"));
            EditorGUILayout.PropertyField(MinLightValue, Localization.G("MinLight"));
            EditorGUILayout.PropertyField(DefaultLightValue, Localization.G("DefaultLight"));

            using (var group = new Utils.FoldoutHeaderGroupScope(ref _isOptionFoldoutOpen, Localization.S("Options")))
            {
                if (group.IsOpen)
                {
                    EditorGUILayout.PropertyField(FX, Localization.G("FX"));
                    EditorGUILayout.Separator();

                    TargetShader.intValue = EditorGUILayout.MaskField(Localization.G("Target Shader"), TargetShader.intValue, TargetShader.enumNames);

                    EditorGUILayout.PropertyField(AllowSaturationControl, Localization.G("Allow Saturation Control"));
                    EditorGUILayout.PropertyField(AddResetButton, Localization.G("Add Reset Button"));
                    EditorGUILayout.PropertyField(ExcludeEditorOnly, Localization.G("Exclude EditorOnly"));
                    EditorGUILayout.Separator();
                    EditorGUILayout.PropertyField(GenerateAtBuild, Localization.G("Generate At Build/PlayMode"));

                    EditorGUILayout.Separator();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(Localization.G("Pregeneration"), GUILayout.Width(EditorGUIUtility.labelWidth));
                    if (GUILayout.Button("Process"))
                    {
                        GenerateAssets();
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUILayout.Separator();


            Localization.ShowLocalizationUI();
        }

        private void GenerateAssets()
        {
            var settings = target as LightLimitChangerSettings;
            var avatar = settings.gameObject.FindAvatarFromParent();
            if (avatar == null)
            {
                return;
            }
            Undo.SetCurrentGroupName("Light Limit Changer");
            int undoGroup = Undo.GetCurrentGroup();
            Undo.RegisterCompleteObjectUndo(settings, "");
            if (FX.objectReferenceValue == null)
            {
                var fileName = $"{avatar.name}_{DateTime.Now:yyyyMMddHHmmss}_{GUID.Generate()}.controller";
                var savePath = EditorUtility.SaveFilePanelInProject(Localization.S("Save"), System.IO.Path.GetFileNameWithoutExtension(fileName), System.IO.Path.GetExtension(fileName).Trim('.'), Localization.S("Save Location"));
                if (string.IsNullOrEmpty(savePath))
                {
                    Undo.CollapseUndoOperations(undoGroup);
                    return;
                }

                var fx = new AnimatorController() { name = System.IO.Path.GetFileName(fileName) };
                AssetDatabase.CreateAsset(fx, savePath);
                settings.FX = fx;
            }
            //PrefabUtility.UnpackPrefabInstance(settings.gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            LightLimitGenerator.Generate(avatar, settings);
            settings.enabled = false;
            EditorUtility.SetDirty(settings);
            Undo.CollapseUndoOperations(undoGroup);
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
