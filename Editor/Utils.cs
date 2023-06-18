#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using UnityEditor.Animations;

namespace io.github.azukimochi
{
    internal static class Utils
    {
        public static void ClearSubAssets(this UnityEngine.Object obj)
        {
            var path = AssetDatabase.GetAssetPath(obj);
            if (path != null)
            {
                foreach(var asset in AssetDatabase.LoadAllAssetsAtPath(path))
                {
                    if (AssetDatabase.IsSubAsset(asset))
                    {
                        GameObject.DestroyImmediate(asset, true);
                    }
                }
            }
        }

        public static void ClearLayers(this AnimatorController controller) => controller.layers = Array.Empty<AnimatorControllerLayer>();

        public static bool TryGetComponentInChildren<T>(this Component component, out T result) where T : Component
        {
            result = component.GetComponentInChildren<T>();
            return result != null;
        }

        public static GameObject GetOrAddChild(this GameObject obj, string name)
        {
            var c = obj.transform.EnumerateChildren().FirstOrDefault(x => x.name == name)?.gameObject;
            if (c == null)
            {
                c = new GameObject(name);
                c.transform.parent = obj.transform;
            }
            return c;
        }

        public static IEnumerable<Transform> EnumerateChildren(this Transform tr)
        {
            int count = tr.childCount;
            for(int i = 0 ; i < count; i++)
            {
                yield return tr.GetChild(i);
            }
        }

        public static T AddTo<T>(this T obj, UnityEngine.Object asset) where T : UnityEngine.Object
        {
            AssetDatabase.AddObjectToAsset(obj, asset);
            return obj;
        }

        public static T HideInHierarchy<T>(this T obj) where T : UnityEngine.Object
        {
            obj.hideFlags |= HideFlags.HideInHierarchy;
            return obj;
        }

        public static string GetRelativePath(this Transform transform, Transform root, bool includeRelativeTo = false)
        {
            var buffer = _relativePathBuffer;
            if (buffer is null)
            {
                buffer = _relativePathBuffer = new string[128];
            }

            var t = transform;
            int idx = buffer.Length;
            while (t != null && t != root)
            {
                buffer[--idx] = t.name;
                t = t.parent;
            }
            if (includeRelativeTo && t != null && t == root)
            {
                buffer[--idx] = t.name;
            }

            return string.Join("/", buffer, idx, buffer.Length - idx);
        }

        private static string[] _relativePathBuffer;

        public struct FoldoutHeaderGroupScope : IDisposable
        {
            public bool IsOpen;
            public FoldoutHeaderGroupScope(ref bool isOpen, string content)
            {
                IsOpen = isOpen = EditorGUILayout.BeginFoldoutHeaderGroup(isOpen, content);
            }

            public void Dispose()
            {
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
        }
    }
}

#endif
