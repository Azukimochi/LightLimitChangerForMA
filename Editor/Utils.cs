#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using UnityEditor.Animations;
using VRC.SDK3.Avatars.Components;
using System.Reflection;
using VRC.Core.Pool;

using Object = UnityEngine.Object;

namespace io.github.azukimochi
{
    internal static class Utils
    {
        public static void ClearSubAssets(this Object obj)
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

        public static T UndoGetOrAddComponent<T>(this GameObject obj) where T : Component
        {
            var component = obj.GetComponent<T>();
            if (component == null)
            {
                component = Undo.AddComponent<T>(obj);
            }
            return component;
        }

        public static VRCAvatarDescriptor FindAvatarFromParent(this GameObject obj) => obj.GetComponentInParent<VRCAvatarDescriptor>();

        public static IEnumerable<Transform> EnumerateChildren(this Transform tr)
        {
            int count = tr.childCount;
            for(int i = 0 ; i < count; i++)
            {
                yield return tr.GetChild(i);
            }
        }

        public static T AddTo<T>(this T obj, Object asset) where T : Object
        {
            AssetDatabase.AddObjectToAsset(obj, asset);
            return obj;
        }

        public static T HideInHierarchy<T>(this T obj) where T : Object
        {
            obj.hideFlags |= HideFlags.HideInHierarchy;
            return obj;
        }

        public static T Clone<T>(this T obj) where T : Object => Object.Instantiate(obj);

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

        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> valueFactory)
        {
            if (!dictionary.TryGetValue(key, out var value))
            {
                value = valueFactory(key);
                dictionary.Add(key, value);
            }
            return value;
        }

        public static T GetValue<T>(this Material material, string name, T defaultValue = default)
        {
            if (material != null && material.HasProperty(name))
            {
                if (typeof(T) == typeof(float))
                    return (T)(object)material.GetFloat(name);

                else if (typeof(T) == typeof(int))
                    return (T)(object)material.GetInt(name);

                else if (typeof(T) == typeof(Color))
                    return (T)(object)material.GetColor(name);

                else if (typeof(T) == typeof(Vector4))
                    return (T)(object)material.GetVector(name);

            }
            return defaultValue;
        }

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

        private static bool _isVersionInfoFoldoutOpen = true;
        private static GUIContent _titleCache = null;

        public static void ShowVersionInfo()
        {
            if (_titleCache == null)
            {
                _titleCache = new GUIContent($"{LightLimitChanger.Title} {GetVersion()}");
            }
            using (var foldout = new FoldoutHeaderGroupScope(ref _isVersionInfoFoldoutOpen, _titleCache))
            {
                if (foldout.IsOpen)
                {
                    DrawWebButton("Light Limit Changer OfficialSite", "https://azukimochi.github.io/LLC-Docs/");
                    DrawWebButton("X|Twitter", "https://twitter.com/search?q=from%3Aazukimochi25%20%23LightLimitChanger&src=typed_query&f=live");
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
            style.normal.textColor = style.focused.textColor;
            style.hover.textColor = style.focused.textColor;
            if (GUI.Button(position, icon, style))
            {
                Application.OpenURL(URL);
            }
        }

        private static GUIContent _labelSingleton = new GUIContent();

        public static GUIContent Label(string text, string textTip = null)
        {
            var label = _labelSingleton;
            label.text = text;
            label.tooltip = textTip;

            return label;
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
            public FoldoutHeaderGroupScope(ref bool isOpen, string content) : this(ref isOpen, Label(content))
            { }

            public FoldoutHeaderGroupScope(ref bool isOpen, GUIContent content)
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
            private static readonly Keyframe[] _buffer3 = new Keyframe[3];

            private const float TimeEnd = 1 / 60f;

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
                float tangent = (end - start) / TimeEnd;
                _buffer2[0] = new Keyframe(0, start, 0, tangent);
                _buffer2[1] = new Keyframe(TimeEnd, end, tangent, 0);
                curve.keys = _buffer2;
                return curve;
            }
            public static AnimationCurve Linear(float start, float mid, float end)
            {
                var curve = _singleton;
                float tangent = (mid - start) / TimeEnd;
                float tangent2 = (end - mid) / TimeEnd;
                _buffer3[0] = new Keyframe(0, start, 0, tangent);
                _buffer3[1] = new Keyframe(TimeEnd, mid, tangent, tangent2);
                _buffer3[2] = new Keyframe(TimeEnd * 2, end, tangent2, 0);
                if (curve == null)
                {
                    curve = _singleton = new AnimationCurve(_buffer3);
                }
                else
                {
                    curve.keys = _buffer3;
                }
                return curve;
            }
        }

        public static class ArrayPool<T>
        {
            public static T[] Rent(int minimumSize)
            {
                var size = 16 << (int)(Math.Log((uint)minimumSize - 1 | 15, 2) - 3);
                return ArrayPool.Get<T>(size).Array;
            }

            public static void Return(T[] array)
            {
                ArrayPool.Release(array);
            }
        }
    }
}

#endif
