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
        private const string ContextMenuPath = "GameObject/ModularAvatar/Light Limit Changer";
        private const int ContextMenuPriority = 130;

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