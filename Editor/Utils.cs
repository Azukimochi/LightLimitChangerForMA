#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using UnityEditor.Animations;
using VRC.SDK3.Avatars.Components;
using System.Reflection;

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

        public static bool TryGetComponentInChildren<T>(this Component component, out T result) where T : Component => component.gameObject.TryGetComponentInChildren(out result);

        public static bool TryGetComponentInChildren<T>(this GameObject obj, out T result) where T : Component
        {
            result = obj.GetComponentInChildren<T>();
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

        public static VRCAvatarDescriptor FindAvatarFromParent(this GameObject obj)
        {
            var tr = obj.transform;
            VRCAvatarDescriptor avatar = null;
            while(tr != null && (avatar = tr.GetComponent<VRCAvatarDescriptor>()) == null)
            {
                tr = tr.parent;
            }
            return avatar;
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

        public static IEnumerable<string> EnumeratePropertyNames(this Shader shader) => Enumerable.Range(0, shader.GetPropertyCount()).Select(shader.GetPropertyName);

        private static MethodInfo _GetGeneratedAssetsFolder = typeof(nadena.dev.modular_avatar.core.editor.AvatarProcessor).Assembly.GetTypes().FirstOrDefault(x => x.Name == "Util")?.GetMethod(nameof(GetGeneratedAssetsFolder), BindingFlags.Static | BindingFlags.NonPublic);

        public static string GetGeneratedAssetsFolder()
        {
            var method = _GetGeneratedAssetsFolder;
            if (method != null)
                return method.Invoke(null, null) as string;

            return AssetDatabase.GUIDToAssetPath(AssetDatabase.CreateFolder("Assets/", "_LightLimitChangerTemporary"));
        }

        public static string GetVersion()
        {
            var packageInfo = JsonUtility.FromJson<PackageInfo>(System.IO.File.ReadAllText(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.dataPath), AssetDatabase.GUIDToAssetPath("a82bfa088b3f7634aaadfdea98eb87e0"))));
            return packageInfo.version ?? ":: Failed get current version";
        }

        [Serializable]
        private struct PackageInfo
        {
            public string version;
        }

        public struct DisabledScope : IDisposable
        {
            public DisabledScope(bool disabled)
            {
                EditorGUI.BeginDisabledGroup(disabled);
            }

            public void Dispose()
            {
                EditorGUI.EndDisabledGroup();
            }
        }

        public struct GroupScope : IDisposable
        {
            private float _originalLabelWidth;
            public GroupScope(string header, float labelWidth)
            {
                GUILayout.Label($"---- {header}", EditorStyles.boldLabel);
                _originalLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = labelWidth;

                EditorGUI.indentLevel++;
            }

            public void Dispose()
            {
                EditorGUIUtility.labelWidth = _originalLabelWidth;
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
        }

        public struct FoldoutHeaderGroupScope : IDisposable
        {
            public bool IsOpen;
            public FoldoutHeaderGroupScope(ref bool isOpen, string content)
            {
                IsOpen = isOpen = EditorGUILayout.BeginFoldoutHeaderGroup(isOpen, content);
                if (isOpen)
                {
                    EditorGUI.indentLevel++;
                }
            }

            public void Dispose()
            {
                if (IsOpen)
                {
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
        }

        public static class Animation
        {
            private static AnimationCurve _singleton;
            private static readonly Keyframe[] _buffer1 = new Keyframe[1];
            private static readonly Keyframe[] _buffer2 = new Keyframe[2];

            public static AnimationCurve Constant(float value)
            {
                var curve = _singleton;
                if (curve == null)
                {
                    return _singleton = AnimationCurve.Constant(0, 0, value);
                }
                _buffer1[0] = new Keyframe(0, value);
                curve.keys = _buffer1;
                return curve;
            }

            public static AnimationCurve Linear(float start, float end)
            {
                var curve = _singleton;
                if (curve == null)
                {
                    return _singleton = AnimationCurve.Linear(0, start, 1 / 60f, end);
                }
                _buffer2[0] = new Keyframe(0, start);
                _buffer2[1] = new Keyframe(1 / 60f, end);
                curve.keys = _buffer2;
                return curve;
            }
        }
    }
}

#endif
