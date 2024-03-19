﻿using System.IO;
using nadena.dev.modular_avatar.core;
using UnityEditor;
using UnityEngine;

namespace io.github.azukimochi
{
    [InitializeOnLoad]
    internal static class LightLimitChangerPrefab
    {
        private const string GeneratedPrefabGUIDKey = "io.github.azukimochi.LightLimitChanger.Prefab";
        private const string PrefabPath = "Assets/LightLimitChanger/Light Limit Changer.prefab";

        private const string GlobalSettingsIDKey = "io.github.azukimochi.LightLimitChanger.GlobalSettings.ID.Global";
        private const string GlobalSettingsLocalIDKey = "io.github.azukimochi.LightLimitChanger.GlobalSettings.ID.Local";
        private const string GlobalSettingsValueKey = "io.github.azukimochi.LightLimitChanger.GlobalSettings.Value";

        static LightLimitChangerPrefab()
        {
            SceneView.duringSceneGui += scene =>
            {
                if (EditorPrefs.GetString(GlobalSettingsIDKey) == PlayerPrefs.GetString(GlobalSettingsLocalIDKey))
                    return;


                Handles.BeginGUI();

                // TODO: ここでGUIをぐいっと
                if (GUILayout.Button("更新"))
                {
                    CheckChangeGlobalSettings();
                }
                Handles.EndGUI();
            };
        }

        public static GameObject Object
        {
            get
            {
                var guid = PlayerPrefs.GetString(GeneratedPrefabGUIDKey, null);
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(guid) || string.IsNullOrEmpty(path) || AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) == null)
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

        public static void SavePrefabSetting(LightLimitChangerParameters parameters)
        {
            using (var scope = new PrefabEditScope(AssetDatabase.GetAssetPath(Object)))
            {
                if (scope.Prefab.TryGetComponent<LightLimitChangerSettings>(out var llc))
                {
                    llc.Parameters = parameters;
                    EditorUtility.SetDirty(llc);
                }
            }
        }

        public static void SavePrefabSettingAsGlobal(LightLimitChangerParameters parameters)
        {
            SavePrefabSetting(parameters);
            var json = JsonUtility.ToJson(parameters);
            var key = GUID.Generate().ToString();
            EditorPrefs.SetString(GlobalSettingsIDKey, key);
            PlayerPrefs.SetString(GlobalSettingsLocalIDKey, key);
            EditorPrefs.SetString(GlobalSettingsValueKey, json);
        }

        private static void CheckChangeGlobalSettings()
        {
            var globalID = EditorPrefs.GetString(GlobalSettingsIDKey);
            if (EditorPrefs.GetString(GlobalSettingsIDKey) == PlayerPrefs.GetString(GlobalSettingsLocalIDKey))
                return;

            var value = JsonUtility.FromJson<LightLimitChangerParameters>(EditorPrefs.GetString(GlobalSettingsValueKey));
            SavePrefabSetting(value);
            PlayerPrefs.SetString(GlobalSettingsLocalIDKey, globalID);
        }

        private readonly ref struct PrefabEditScope
        {
            private readonly GameObject instance;
            private readonly string path;

            public GameObject Prefab => instance;

            public PrefabEditScope(string path)
            {
                this.path = path;
                instance = PrefabUtility.LoadPrefabContents(path);
            }

            public void Dispose()
            {
                PrefabUtility.SaveAsPrefabAsset(instance, path);
                PrefabUtility.UnloadPrefabContents(instance);
            }
        }
    }
}
