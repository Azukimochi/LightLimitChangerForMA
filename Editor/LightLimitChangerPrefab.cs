using System.IO;
using nadena.dev.modular_avatar.core;
using UnityEditor;
using UnityEngine;

namespace io.github.azukimochi
{
    internal static class LightLimitChangerPrefab
    {
        private const string GeneratedPrefabGUIDKey = "io.github.azukimochi.LightLimitChanger.Prefab";
        private const string PrefabPath = "Assets/LightLimitChanger/Light Limit Changer.prefab";

        public static GameObject Object
        {
            get
            {
                var guid = PlayerPrefs.GetString(GeneratedPrefabGUIDKey, null);
                if (string.IsNullOrEmpty(guid) || string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(guid)))
                {
                    guid = CreatePrefab();
                }
                return AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
            }
        }

        private static string CreatePrefab()
        {
            var directory = Path.GetDirectoryName(PrefabPath);
            if (!AssetDatabase.IsValidFolder(directory))
            {
                AssetDatabase.CreateFolder(Path.GetDirectoryName(directory), Path.GetFileName(directory));
            }
            var obj = new GameObject(Path.GetFileNameWithoutExtension(PrefabPath)) { hideFlags = HideFlags.HideInHierarchy };
            
            obj.AddComponent<LightLimitChangerSettings>();
            obj.AddComponent<ModularAvatarMenuInstaller>();

            PrefabUtility.SaveAsPrefabAsset(obj, PrefabPath);
            GameObject.DestroyImmediate(obj);
            var guid = AssetDatabase.AssetPathToGUID(PrefabPath);
            PlayerPrefs.SetString(GeneratedPrefabGUIDKey, guid);
            return guid;
        }
    }
}
