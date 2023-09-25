using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.Core;
using nadena.dev.modular_avatar.core;
using UnityEngine.SceneManagement;
using System.Linq;

namespace io.github.azukimochi
{
    public sealed class LightLimitChanger : EditorWindow
    {
        public const string Title = "Light Limit Changer For MA";

        public VRCAvatarDescriptor TargetAvatar;
        private VRCAvatarDescriptor _prevTargetAvatar;
        private GameObject _temp;
        private LightLimitChangerSettingsEditor _editor;

        [MenuItem("Tools/Modular Avatar/LightLimitChanger")]
        public static void CreateWindow()
        {
            var window = GetWindow<LightLimitChanger>(Title);
        }

        private void OnEnable()
        {
            _temp = new GameObject()
            {
                hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave
            };
        }

        private void OnDestroy()
        {
            if (_temp != null )
            {
                DestroyImmediate(_temp);
            }
            if (_editor != null )
            {
                DestroyImmediate(_editor);
            }
        }

        private void OnGUI()
        {
            EditorGUIUtility.labelWidth = 280;
            Utils.ShowVersionInfo(); 
            EditorGUILayout.Separator();

            var style = new GUIStyle(EditorStyles.helpBox);
            style.richText = true;
            EditorGUILayout.LabelField($"<size=12>{Localization.S("window.info.deprecated")}\nhttps://azukimochi.github.io/LLC-Docs/docs/プレハブ置くときのやり方.htm</size>", style);
            EditorGUILayout.Space(8);

            TargetAvatar = EditorGUILayout.ObjectField(Localization.G("label.avatar"), TargetAvatar, typeof(VRCAvatarDescriptor), true) as VRCAvatarDescriptor;
            EditorGUILayout.Separator();

            UpdateInnerEditor();

            if (_editor != null)
            {
                try
                {
                    // InspectorのGUI処理を使いまわす
                    _editor.IsWindowMode = true;
                    _editor.OnInspectorGUI();
                    EditorGUILayout.Separator();

                    // 操作対象が一時オブジェクトなら生成ボタンを表示する
                    if ((_editor.target as Component).gameObject.hideFlags.HasFlag(HideFlags.HideInHierarchy))
                    {
                        if (GUILayout.Button(Localization.G("info.generate")))
                        {
                            GenerateAssets();
                        }
                    }
                }
                catch { }
            }

            EditorGUILayout.Separator();
            Localization.ShowLocalizationUI();
        }

        private void UpdateInnerEditor()
        {
            if (_editor == null)
            {
                if (TargetAvatar != null)
                {
                    var settings = TargetAvatar.GetComponentInChildren<LightLimitChangerSettings>();
                    if (settings != null)
                    {
                        _editor = Editor.CreateEditor(settings, typeof(LightLimitChangerSettingsEditor)) as LightLimitChangerSettingsEditor;
                    }
                    else
                    {
                        _temp.GetComponent<LightLimitChangerSettings>()?.Destroy();
                        _editor = Editor.CreateEditor(_temp.AddComponent<LightLimitChangerSettings>(), typeof(LightLimitChangerSettingsEditor)) as LightLimitChangerSettingsEditor;
                    }
                }
                else
                {
                    // nanimo sinai...
                }
            }
            else
            {
                if (TargetAvatar != _prevTargetAvatar)
                {
                    _editor.Destroy();
                    _editor = null;
                }
                else if (TargetAvatar != null)
                {
                    if (_editor.target == null)
                    {
                        _editor.Destroy();
                        _editor = null;
                        _temp.GetComponent<LightLimitChangerSettings>()?.Destroy();
                        _editor = Editor.CreateEditor(_temp.AddComponent<LightLimitChangerSettings>(), typeof(LightLimitChangerSettingsEditor)) as LightLimitChangerSettingsEditor;
                    }
                    else
                    {

                        var settings = TargetAvatar.GetComponentInChildren<LightLimitChangerSettings>();
                        if (settings != null && (_editor.target as Component).gameObject.hideFlags.HasFlag(HideFlags.HideInHierarchy))
                        {
                            _editor.Destroy();
                            _editor = null;
                            _editor = Editor.CreateEditor(settings, typeof(LightLimitChangerSettingsEditor)) as LightLimitChangerSettingsEditor;
                        }
                    }
                }
            }
            _prevTargetAvatar = TargetAvatar;
        }

        private void GenerateAssets()
        {
            const string PrefabGUID = "b3d7759e248364e4dadf8e4fbc37fde1";

            var prefab = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(PrefabGUID)), TargetAvatar.transform);
            EditorUtility.CopySerialized(_editor.target, (prefab as GameObject).GetComponent<LightLimitChangerSettings>());
        }
    }
}