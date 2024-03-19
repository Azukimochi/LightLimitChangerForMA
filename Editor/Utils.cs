using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace io.github.azukimochi
{
    internal static class Utils
    {
        private static GUIStyle Bluestyle = new GUIStyle(EditorStyles.foldout)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 14,
            
        };
        static Utils()
        {
            Bluestyle.focused.textColor = EditorStyles.linkLabel.focused.textColor;
            Bluestyle.active.textColor = EditorStyles.linkLabel.focused.textColor;
            Bluestyle.normal.textColor = EditorStyles.linkLabel.focused.textColor;
            Bluestyle.onFocused.textColor = EditorStyles.linkLabel.focused.textColor;
            Bluestyle.onActive.textColor = EditorStyles.linkLabel.focused.textColor;
            Bluestyle.onNormal.textColor = EditorStyles.linkLabel.focused.textColor;
        }
        

        // ContainsとDeconstructは2019との互換性の為に必要なので消さないこと！

        public static bool Contains(this string str, string value, StringComparison comparison)
        {
            return str.IndexOf(value, comparison) != -1;
        }

        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue value)
        {
            key = kvp.Key;
            value = kvp.Value;
        }

        public static void Destroy(this Object obj)
        {
            Object.DestroyImmediate(obj);
        }

        public static bool IsNeedToEnterChildren(this SerializedPropertyType type)
        {
            switch (type)
            {
                case SerializedPropertyType.String:
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Boolean:
                case SerializedPropertyType.Float:
                case SerializedPropertyType.Color:
                case SerializedPropertyType.ObjectReference:
                case SerializedPropertyType.LayerMask:
                case SerializedPropertyType.Enum:
                case SerializedPropertyType.Vector2:
                case SerializedPropertyType.Vector3:
                case SerializedPropertyType.Vector4:
                case SerializedPropertyType.Rect:
                case SerializedPropertyType.ArraySize:
                case SerializedPropertyType.Character:
                case SerializedPropertyType.AnimationCurve:
                case SerializedPropertyType.Bounds:
                case SerializedPropertyType.Gradient:
                case SerializedPropertyType.Quaternion:
                case SerializedPropertyType.FixedBufferSize:
                case SerializedPropertyType.Vector2Int:
                case SerializedPropertyType.Vector3Int:
                case SerializedPropertyType.RectInt:
                case SerializedPropertyType.BoundsInt:
                    return false;
                default:
                    return true;
            }
        }

        public static Color With(this Color color, float? r = null, float? g = null, float? b = null, float? a = null)
        {
            color.r = r ?? color.r;
            color.g = g ?? color.g;
            color.b = b ?? color.b;
            color.a = a ?? color.a;
            return color;
        }

        public static bool Equals(this Color @this, Color other, ShaderInfoUtility.IncludeField field = ShaderInfoUtility.IncludeField.RGBA)
        {
            if (field.HasFlag(ShaderInfoUtility.IncludeField.R) && @this.r != other.r)
                return false;

            if (field.HasFlag(ShaderInfoUtility.IncludeField.G) && @this.g != other.g)
                return false;

            if (field.HasFlag(ShaderInfoUtility.IncludeField.B) && @this.b != other.b)
                return false;

            if (field.HasFlag(ShaderInfoUtility.IncludeField.A) && @this.a != other.a)
                return false;

            return true;
        }

        public static bool TryGetComponentInChildren<T>(this GameObject obj, out T result) where T : Component
        {
            result = obj.GetComponentInChildren<T>();
            return result != null;
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

        public static string GetVersion()
        {
            var packageInfo = JsonUtility.FromJson<PackageInfo>(System.IO.File.ReadAllText(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.dataPath), AssetDatabase.GUIDToAssetPath("a82bfa088b3f7634aaadfdea98eb87e0"))));
            return packageInfo.version ?? ":: Failed get current version";
        }

        private static bool _isVersionInfoFoldoutOpen = true;
        private static bool _isDocumentLinkFoldoutOpen = true;
        private static GUIContent _titleCache = null;

        public static void ShowVersionInfo()
        {
            if (_titleCache == null)
            {
                _titleCache = new GUIContent($"{LightLimitChanger.Title} {GetVersion()}");
            }
            EditorGUILayout.LabelField(_titleCache, new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Bold, fontSize = 15});
            var changeLog = new GUIContent(Localization.G("label.changelog"));
            using (var foldout = new FoldoutHeaderGroupScope(ref _isVersionInfoFoldoutOpen, changeLog, Bluestyle))
            {
                if (foldout.IsOpen)
                {
                    DrawWebButton("Light Limit Changer OfficialSite | 更新履歴 Changelog", "https://azukimochi.github.io/LLC-Docs/docs/changelog");
                    DrawWebButton("X|Twitter", "https://twitter.com/search?q=from%3Aazukimochi25%20%23LightLimitChanger&src=typed_query&f=live");
                }
            }
        }

        public static void ShowDocumentLink()
        {
            var document = new GUIContent(Localization.G("label.document"));
            using (var foldout = new FoldoutHeaderGroupScope(ref _isDocumentLinkFoldoutOpen, document, Bluestyle))
            {
                if (foldout.IsOpen)
                {
                    DrawWebButton("Light Limit Changer OfficialSite | おすすめ設定 RecomentSetting", "https://azukimochi.github.io/LLC-Docs/docs/tutorial/howtouse-recommend");
                    DrawWebButton("Light Limit Changer OfficialSite | 設定概要 Discription", "https://azukimochi.github.io/LLC-Docs/docs/discription/disc_param");
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

        public struct FoldoutHeaderGroupScope : IDisposable
        {
            public bool IsOpen;
            public FoldoutHeaderGroupScope(ref bool isOpen, string content) : this(ref isOpen, Label(content))
            { }

            public FoldoutHeaderGroupScope(ref bool isOpen, GUIContent content, GUIStyle style = null)
            {
                IsOpen = isOpen = EditorGUILayout.BeginFoldoutHeaderGroup(isOpen, content, style);
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