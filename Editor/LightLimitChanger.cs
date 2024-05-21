using System.Collections.Generic;
using System.Linq;
using nadena.dev.modular_avatar.core;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace io.github.azukimochi
{
    public static class LightLimitChanger
    {
        public const string Title = "Light Limit Changer For MA";
        private const string ContextMenuPath = "GameObject/Light Limit Changer/Setup";
        private const int ContextMenuPriority = 130;

        [MenuItem(ContextMenuPath, true, ContextMenuPriority)]
        public static bool ValidateApplytoAvatar(MenuCommand command) => Selection.gameObjects.Any(x => x.TryGetComponent<VRCAvatarDescriptor>(out _));

        [MenuItem(ContextMenuPath, false, ContextMenuPriority)]
        public static void ApplytoAvatar(MenuCommand command)
        {
            var target = command.context as GameObject;
            if (target == null || !target.TryGetComponent<VRCAvatarDescriptor>(out _))
            {
                return;
            }

            if (target.TryGetComponentInChildren<LightLimitChangerSettings>(out _))
            {
                EditorUtility.DisplayDialog("Light Limit Changer for MA Setup", string.Format(Localization.S("Window.info.error.already_setup"), target.name), "OK");
                return;
            }

            var prefab = GeneratePrefab(target.transform);
            EditorGUIUtility.PingObject(prefab);
            Selection.objects = Selection.gameObjects.Where(x => x.TryGetComponent<LightLimitChangerSettings>(out _)).Append(prefab).ToArray();
        }

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