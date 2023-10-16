using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using UnityEditor.Animations;
using VRC.SDK3.Avatars.Components;
using VRC.Core.Pool;

using Object = UnityEngine.Object;

namespace io.github.azukimochi
{
    internal static class Utils
    {
        public static LightLimitControlType GetControlTypeFlags(in this LightLimitChangerParameters parameters)
        {
            var flags = LightLimitControlType.Light;

            if (parameters.AllowColorTempControl)
            {
                flags |= LightLimitControlType.ColorTemperature;
            }
            if (parameters.AllowSaturationControl)
            {
                flags |= LightLimitControlType.Saturation;
            }
            if (parameters.AllowUnlitControl)
            {
                flags |= LightLimitControlType.Unlit;
            }

            return flags;
        }

        public static bool HasFlag(this int x, int y) => (x & y) == y;

        public static void Destroy(this Object obj)
        {
            Object.DestroyImmediate(obj);
        }

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

        public static T AddTo<T>(this T obj, LightLimitChangerObjectCache cache) where T : Object
        {
            return cache.Register(obj);
        }

        public static T HideInHierarchy<T>(this T obj) where T : Object
        {
            obj.hideFlags |= HideFlags.HideInHierarchy;
            return obj;
        }

        public static T Clone<T>(this T obj) where T : Object => Object.Instantiate(obj);

        public static IEnumerable<Material> GetAnimatedMaterials(this RuntimeAnimatorController runtimeAnimatorController)
        {
            foreach (var anim in runtimeAnimatorController.animationClips)
            {
                foreach (var bind in AnimationUtility.GetObjectReferenceCurveBindings(anim))
                {
                    if (bind.type == typeof(MeshRenderer) || bind.type == typeof(SkinnedMeshRenderer))
                    {
                        foreach (var curve in AnimationUtility.GetObjectReferenceCurve(anim, bind))
                        {
                            var material = curve.value as Material;
                            if (material != null)
                                yield return material;
                        }
                    }
                }
            }
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

        public static bool Contains(this string str, string value, StringComparison comparison)
        {
            return str.IndexOf(value, comparison) != -1;
        }

        public static IEnumerable<int> EnumeratePropertyNameIDs(this Shader shader)
        {
            var count = shader.GetPropertyCount();
            for (int i = 0; i < count; i++)
            {
                yield return shader.GetPropertyNameId(i);
            }
        }

        public static T GetOrDefault<T>(this Material material, int nameID, T defaultValue = default)
        {
            if (material.HasProperty(nameID))
            {
                // 単純な型引数と型の比較は実行時に消去される

                //if (typeof(Texture).IsAssignableFrom(typeof(T)))
                if (typeof(T) == typeof(Texture) || typeof(T) == typeof(Texture2D) || typeof(T) == typeof(RenderTexture))
                    return (T)(object)material.GetTexture(nameID);
                if (typeof(T) == typeof(Color))
                    return (T)(object)material.GetColor(nameID);
                if (typeof(T) == typeof(Vector4))
                    return (T)(object)material.GetVector(nameID);
                if (typeof(T) == typeof(int))
                    return (T)(object)material.GetInt(nameID);
                if (typeof(T) == typeof(float))
                    return (T)(object)material.GetFloat(nameID);
            }

            return defaultValue;
        }

        public static bool TrySet<T>(this Material material, int nameID, T value)
        {
            if (!material.HasProperty(nameID))
                return false;

            //if (typeof(Texture).IsAssignableFrom(typeof(T)))
            if (typeof(T) == typeof(Texture) || typeof(T) == typeof(Texture2D) || typeof(T) == typeof(RenderTexture))
                material.SetTexture(nameID, (Texture)(object)value);
            else if (typeof(T) == typeof(Color))
                material.SetColor(nameID, (Color)(object)value);
            else if (typeof(T) == typeof(Vector4))
                material.SetVector(nameID, (Vector4)(object)value);
            else if (typeof(T) == typeof(int))
                material.SetInt(nameID, (int)(object)value);
            else if (typeof(T) == typeof(float))
                material.SetFloat(nameID, (float)(object)value);
            else 
                return false;

            return true;
        }

        // https://github.com/bdunderscore/modular-avatar/blob/b15520271455350cf728bc1b95b874dc30682eb2/Packages/nadena.dev.modular-avatar/Editor/Util.cs#L162C9-L178C10
        // Originally under MIT License
        // Copyright (c) 2022 bd_
        public static string GetGeneratedAssetsFolder()
        {
            var path = "Assets/999_Modular_Avatar_Generated";

            var pathParts = path.Split('/');

            for (int i = 1; i < pathParts.Length; i++)
            {
                var subPath = string.Join("/", pathParts, 0, i + 1);
                if (!AssetDatabase.IsValidFolder(subPath))
                {
                    AssetDatabase.CreateFolder(string.Join("/", pathParts, 0, i), pathParts[i]);
                }
            }

            return path;
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
    }
}