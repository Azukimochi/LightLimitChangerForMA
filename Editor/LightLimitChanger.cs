using System.Collections.Generic;
using System.Linq;
using nadena.dev.modular_avatar.core;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace io.github.azukimochi
{
    public sealed class LightLimitChanger : EditorWindow
    {
        public const string Title = "Light Limit Changer For MA";
        private const string ContextMenuPath = "GameObject/ModularAvatar/Light Limit Changer";
        private const int ContextMenuPriority = 130;

        public VRCAvatarDescriptor TargetAvatar;
        private VRCAvatarDescriptor _prevTargetAvatar;
        private GameObject _temp;
        private LightLimitChangerSettingsEditor _editor;

        [MenuItem("Tools/Modular Avatar/LightLimitChanger")]
        public static void CreateWindow() => GetWindow<LightLimitChanger>(Title);

        [MenuItem(ContextMenuPath, true, ContextMenuPriority)]
        public static bool ValidateApplytoAvatar() => Selection.gameObjects.Any(ValidateCore);

        [MenuItem(ContextMenuPath, false, ContextMenuPriority)]
        public static void ApplytoAvatar()
        {
            List<GameObject> objectToCreated = new List<GameObject>();
            foreach (var x in Selection.gameObjects)
            {
                if (!ValidateCore(x))
                    continue;

                var prefab = GeneratePrefab(x.transform);

                objectToCreated.Add(prefab);
            }
            if (objectToCreated.Count == 0)
                return;

            EditorGUIUtility.PingObject(objectToCreated[0]);
            Selection.objects = objectToCreated.ToArray();
        }

        // 選択されているものがアバター本体かつ、LLCが含まれていないときに実行可能
        private static bool ValidateCore(GameObject obj) => obj != null && obj.GetComponent<VRCAvatarDescriptor>() != null && obj.GetComponentInChildren<LightLimitChangerSettings>() == null;

        [MenuItem("CONTEXT/LightLimitChangerSettings/Manual Bake (For Advanced User)")]
        public static void ManualBake(MenuCommand command)
        {
            if (!(EditorUtility.SaveFilePanelInProject("Save", $"LightLimitChanger_{GUID.Generate()}", "asset", "") is string path))
                return;

            var session = new Passes.Session();
            var container = new nadena.dev.ndmf.runtime.GeneratedAssets();
            AssetDatabase.CreateAsset(container, path);

            var cache = new LightLimitChangerObjectCache() { Container = container };
            var settings = command.context as LightLimitChangerSettings;
            session.InitializeSession(settings, cache);
            Passes.GenerateAnimationsPass.Run(session, cache);
            Passes.FinalizePass.Run(settings.GetComponentInParent<VRCAvatarDescriptor>().gameObject, session, cache);
            AssetDatabase.SaveAssets();
        }

        private void OnDestroy()
        {
            if (_temp != null)
            {
                DestroyImmediate(_temp);
            }
            if (_editor != null)
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
            EditorGUILayout.LabelField($"<size=12>{Localization.S("window.info.deprecated")}\n<a href=\"https://azukimochi.github.io/LLC-Docs/docs/howtouse/howtouse-basic/\">https://azukimochi.github.io/LLC-Docs/docs/howtouse/howtouse-basic</a></size>", style);
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
                            var prefab = GeneratePrefab(TargetAvatar.transform);
                            EditorUtility.CopySerialized(_editor.target, prefab.GetComponent<LightLimitChangerSettings>());
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
                        _editor = Editor.CreateEditor(GetTempSettings(), typeof(LightLimitChangerSettingsEditor)) as LightLimitChangerSettingsEditor;
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
                        _editor = Editor.CreateEditor(GetTempSettings(), typeof(LightLimitChangerSettingsEditor)) as LightLimitChangerSettingsEditor;
                    }
                    else
                    {
                        var settings = TargetAvatar.GetComponentInChildren<LightLimitChangerSettings>();
                        if (settings != null && (_editor.target as Component).gameObject.hideFlags.HasFlag(HideFlags.HideInHierarchy))
                        {
                            _editor.Destroy();
                            _editor = null;
                            _editor = Editor.CreateEditor(settings, typeof(LightLimitChangerSettingsEditor)) as LightLimitChangerSettingsEditor;
                            _temp?.Destroy();
                            _temp = null;
                        }
                    }
                }
            }
            _prevTargetAvatar = TargetAvatar;
            if (_temp != null)
            {
                // ApplyとかRevertすると何故かHideFlagsが剥がれるので
                _temp.hideFlags = HideFlags.HideAndDontSave & ~HideFlags.NotEditable;
            }

            LightLimitChangerSettings GetTempSettings()
            {
                _temp?.Destroy();
                _temp = GeneratePrefab();
                _temp.name = "_INTERNAL_LLC_PREFAB_";

                return _temp.GetComponent<LightLimitChangerSettings>();
            }
        }

        private static GameObject GeneratePrefab(Transform parent = null)
        {
            var prefabObj = LightLimitChangerPrefab.Object;
            var prefab = prefabObj != null ? PrefabUtility.InstantiatePrefab(prefabObj, parent) as GameObject : CreateNonPrefabLLC();
            Undo.RegisterCreatedObjectUndo(prefab, "Apply LightLimitChanger");

            // おおもとのプレハブからLLCを消す不届き者がいるかもしれない、、、
            if (prefab.GetComponent<LightLimitChangerSettings>() == null)
                prefab.AddComponent<LightLimitChangerSettings>();

            return prefab;

            GameObject CreateNonPrefabLLC()
            {
                var obj = new GameObject("Light Limit Changer", typeof(LightLimitChangerSettings), typeof(ModularAvatarMenuInstaller));
                obj.transform.parent = parent;
                return obj;
            }
        }
    }
}