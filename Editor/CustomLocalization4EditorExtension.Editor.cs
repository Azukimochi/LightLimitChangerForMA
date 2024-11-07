// https://github.com/anatawa12/CustomLocalization4EditorExtension/blob/4bbc0a6e04f71d645c4baabe0727a492b97a5999/Editor/CustomLocalization4EditorExtension.Editor.cs
// MIT License
// Copyright (c) 2022 anatawa12

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace CustomLocalization4EditorExtension
{
    // as a package, this is public, as a embed library, public to embedder library so mark as internal
#if COM_ANATAWA12_CUSTOM_LOCALIZATION_FOR_EDITOR_EXTENSION_AS_PACKAGE
    public
#else
    internal
#endif
        sealed class Localization
    {
        // constructor configurations
        [NotNull] private readonly string _inputAssetPath;
        [NotNull] private readonly string _defaultLocaleName;
        [NotNull] private readonly string _localeSettingEditorPrefsKey;

        // the if this is not null, we found at lease one localization asset(s).
        [CanBeNull] private LocaleAssetConfig _config;
        private bool _initialized;

        public string CurrentLocaleCode
        {
            [CanBeNull] get => _config?.CurrentLocaleCode;
            [NotNull]
            set
            {
                if (_config?.TrySetLocale(value) == false)
                {
                    throw new ArgumentException($"locale {value} not found", nameof(value));
                }
            }
        }

        public IReadOnlyDictionary<string, LocaleLocalization> LocalizationByIsoCode
        {
            get
            {
                if (!_initialized)
                    Setup();

                return _config?.ParsedByIsoCode ?? new Dictionary<string, LocaleLocalization>();
            }
        }

        private static readonly float LanguagePickerHeight = EditorGUIUtility.singleLineHeight;

        /// <summary>
        /// Instantiate Localization
        /// </summary>
        /// <param name="localeAssetPath">
        /// The asset path or GUID of <code cref="LocalizationAsset">LocalizationAsset</code> or
        /// the asset path to directory contains <code cref="LocalizationAsset">LocalizationAsset</code>s.
        /// </param>
        /// <param name="defaultLocale">The fallback locale of localization.</param>
        /// <exception cref="ArgumentException">Both keyLocale and defaultLocale are null</exception>
        public Localization(
            [NotNull] string localeAssetPath,
            [NotNull] string defaultLocale)
        {
            _inputAssetPath = localeAssetPath;
            _defaultLocaleName = defaultLocale;
            _localeSettingEditorPrefsKey =
                $"com.anatawa12.custom-localization-for-editor-extension.locale.{localeAssetPath}";
        }

        /// <summary>
        /// Instantiate Localization
        /// </summary>
        /// <param name="localeAssetPath">
        /// The asset path or GUID of <code cref="LocalizationAsset">LocalizationAsset</code> or
        /// the asset path to directory contains <code cref="LocalizationAsset">LocalizationAsset</code>s.
        /// </param>
        /// <param name="defaultLocale">The fallback locale of localization.</param>
        /// <param name="localeSettingEditorPrefsKey">The <see cref="EditorPrefs"/> key to store current locale</param>
        /// <exception cref="ArgumentException">Both keyLocale and defaultLocale are null</exception>
        public Localization(
            [NotNull] string localeAssetPath,
            [NotNull] string defaultLocale,
            [NotNull] string localeSettingEditorPrefsKey)
        {
            _inputAssetPath = localeAssetPath;
            _defaultLocaleName = defaultLocale;
            _localeSettingEditorPrefsKey = localeSettingEditorPrefsKey;
        }

        /// <summary>
        /// Initializes the localization.
        /// This function will access assets so you should not call this in your constructor.
        /// Calling this function is not required.　If you don't call this manually,
        /// The first call of <c cref="Tr">Tr</c> will call this function.
        /// </summary>
        public void Setup()
        {
            var currentLocaleCode = _config?.CurrentLocaleCode;
            _initialized = true;

            // reset
            _config = null;

            // find directories
            string directoryPath = null;

            if (_inputAssetPath.EndsWith("/"))
            {
                // it's asset path to folder
                if (Directory.Exists(_inputAssetPath))
                    directoryPath = _inputAssetPath;
            }
            else if (IsGuid(_inputAssetPath))
            {
                // it's GUID to folder or LocalizationAsset
                var path = AssetDatabase.GUIDToAssetPath(_inputAssetPath);
                if (Directory.Exists(path))
                    directoryPath = path;
                else if (File.Exists(path))
                    directoryPath = Path.GetDirectoryName(path);
            }
            else
            {
                // it's asset path to LocalizationAsset
                if (File.Exists(_inputAssetPath))
                    directoryPath = Path.GetDirectoryName(_inputAssetPath);
            }

            if (directoryPath == null)
            {
                Debug.LogError($"inputAssetPath not found: {_inputAssetPath}");
                return;
            }

            try
            {
                // It's weird but Directory.GetFiles works fine even with fake `Packages/<package-id>` paths.
                var locales = Directory.GetFiles($"{directoryPath}")
                    .Select(AssetDatabase.LoadAssetAtPath<LocalizationAsset>)
                    .Where(asset => asset != null)
                    .OrderBy(asset => asset.localeIsoCode)
                    .Select(NewParsedLocalization)
                    .ToArray();

                if (locales.Length == 0)
                {
                    Debug.LogError($"no locales found at {directoryPath}");
                    return;
                }

                _config = new LocaleAssetConfig(locales, _defaultLocaleName, currentLocaleCode, _localeSettingEditorPrefsKey);
            }
            catch (IOException e)
            {
                Debug.LogError(e);
            }
            catch (AggregateException e)
            {
                Debug.LogError($"locale duplicated: {e}");
            }
        }
        
#pragma warning disable CS0618 // Type or member is obsolete
        [NotNull]
        private static LocaleLocalization NewParsedLocalization(LocalizationAsset asset) =>
            NewParsedLocalization0(asset);

        [Obsolete("Error Remover")]
        [NotNull]
        private static LocaleLocalization NewParsedLocalization0(LocalizationAsset asset) =>
            new LocaleLocalization(asset);
#pragma warning restore CS0618 // Type or member is obsolete

        /// <summary>
        /// Translate the specified text via this Localization setting
        /// </summary>
        /// <param name="key">
        /// The untranslated text. This should be english but can be changed via keyLocale of constructor.
        /// </param>
        /// <returns>The translated text or translation key</returns>
        [NotNull]
        public string Tr([NotNull] string key) => TryTr(key) ?? key;

        /// <summary>
        /// Translate the specified text via this Localization setting
        /// </summary>
        /// <param name="key">
        /// The untranslated text. This should be english but can be changed via keyLocale of constructor.
        /// </param>
        /// <returns>The translated text or null</returns>
        [CanBeNull]
        public string TryTr([NotNull] string key)
        {
            if (!_initialized)
                Setup();

            return _config?.TryGetLocalizedString(key);
        }

        public void DrawLanguagePicker() =>
            DrawLanguagePicker(EditorGUILayout.GetControlRect(false, GetDrawLanguagePickerHeight()));

        public void DrawLanguagePicker(Rect rect)
        {
            if (_config == null)
            {
                EditorGUI.Popup(rect, 0, new[] { "No Locale Available" });
                return;
            }

            _config.DrawLanguagePicker(rect);
        }

        public float GetDrawLanguagePickerHeight() => LanguagePickerHeight;

        #region utilities

        private static bool IsGuid([NotNull] string mayGuid)
        {
            int hexCnt = 0;
            foreach (var c in mayGuid)
            {
#pragma warning disable CS0642
                if ('0' <= c && c <= '9') hexCnt++;
                else if ('a' <= c && c <= 'f') hexCnt++;
                else if ('A' <= c && c <= 'F') hexCnt++;
                else if (c == '-') ;
                else return false;
#pragma warning restore CS0642

                if (hexCnt > 32) return false;
            }

            return hexCnt == 32;
        }

        #endregion

        class LocaleAssetConfig
        {
            [NotNull] private readonly LocaleLocalization[] _locales;
            [NotNull] private readonly string _localeSettingEditorPrefsKey;
            [NotNull] private readonly string[] _localeNameList;
            [CanBeNull] private readonly LocaleLocalization _defaultLocale;
            
            [NotNull] private LocaleLocalization CurrentLocale {
                get => _currentLocale;
                set
                {
                    var index = Array.IndexOf(_locales, value);
                    if (index == -1) throw new ArgumentException("invalid locale for this localization");
                    _currentLocaleIndex = index;
                    _currentLocale = value;
                }
            }
            [NotNull] private LocaleLocalization _currentLocale;
            private int _currentLocaleIndex;

            public readonly IReadOnlyDictionary<string, LocaleLocalization> ParsedByIsoCode;

            public LocaleAssetConfig([NotNull] LocaleLocalization[] locales,
                [NotNull] string defaultLocaleName,
                [CanBeNull] string currentLocaleCode, 
                [NotNull] string localeSettingEditorPrefsKey)
            {
                if (locales.Length == 0)
                    throw new ArgumentException("empty", nameof(locales));

                if (currentLocaleCode == null)
                {
                    var foundLocale = EditorPrefs.GetString(localeSettingEditorPrefsKey);
                    if (!string.IsNullOrEmpty(foundLocale)) currentLocaleCode = foundLocale;
                }

                _locales = locales;
                _localeSettingEditorPrefsKey = localeSettingEditorPrefsKey;
                _localeNameList = _locales.Select(id => id.Name).ToArray();

                ParsedByIsoCode = _locales.ToDictionary(x => x.LocaleIsoCode);

                ParsedByIsoCode.TryGetValue(defaultLocaleName, out _defaultLocale);
                if (_defaultLocale == null)
                    Debug.LogError($"locale asset for default locale({defaultLocaleName}) not found.");

                if (currentLocaleCode == null)
                {
                    // previous locale is not exists. use default
                    CurrentLocale = _defaultLocale ?? _locales[0];
                }
                else if (!TrySetLocale(currentLocaleCode))
                {
                    Debug.LogWarning($"locale not found: {currentLocaleCode}");
                    CurrentLocale = _defaultLocale ?? _locales[0];
                }
            }

            public string CurrentLocaleCode => CurrentLocale.LocaleIsoCode;

            public bool TrySetLocale(string localeCode)
            {
                if (!ParsedByIsoCode.TryGetValue(localeCode, out var locale)) return false;

                EditorPrefs.SetString(_localeSettingEditorPrefsKey, localeCode);

                CurrentLocale = locale;
                return true;
            }

            public void DrawLanguagePicker(Rect rect)
            {
                int newIndex = EditorGUI.Popup(rect, _currentLocaleIndex, _localeNameList);
                if (newIndex == _currentLocaleIndex) return;

                CurrentLocale = _locales[newIndex];
                EditorPrefs.SetString(_localeSettingEditorPrefsKey, CurrentLocale.LocaleIsoCode);
            }

            public string TryGetLocalizedString(string key)
            {
                return CurrentLocale.TryGetLocalizedString(key) ?? _defaultLocale?.TryGetLocalizedString(key);
            }
        }

        #region PropertyDrawers

        [CustomPropertyDrawer(typeof(CL4EELocalePickerAttribute))]
        class LocalePickerAttribute : DecoratorDrawer
        {
            private bool _initialized;
            [CanBeNull] private Localization _localization;

            public override float GetHeight()
            {
                Initialize();
                return (_localization?.GetDrawLanguagePickerHeight() ?? EditorGUIUtility.singleLineHeight)
                       + EditorGUIUtility.standardVerticalSpacing;
            }

            public override void OnGUI(Rect position)
            {
                position.height -= EditorGUIUtility.standardVerticalSpacing;

                if (_localization == null)
                {
                    EditorGUI.LabelField(position, "ERROR", "CL4EE Localization Instance Not Found");
                }
                else
                {
                    _localization.DrawLanguagePicker(position);
                }
            }

            protected void Initialize()
            {
                if (_initialized) return;
                var attr = (CL4EELocalePickerAttribute)attribute;
                _localization = CL4EE.GetLocalization(attr.TargetAssembly);
                _initialized = true;
            }
        }

        [CustomPropertyDrawer(typeof(CL4EELocalizedAttribute))]
        class LocalizedAttributeDrawer : InheritingDrawer<CL4EELocalizedAttribute>
        {
            private CL4EELocalizedAttribute _attribute;
            [CanBeNull] private Localization _localization;
            private string _localeCode;
            private GUIContent _label = new GUIContent();

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                InitializeUpstream(property);
                return base.GetPropertyHeight(property, _label);
            }

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                InitializeUpstream(property);
                Update();
                base.OnGUI(position, property, _label);
            }

            protected override void Initialize()
            {
                _attribute = (CL4EELocalizedAttribute)attribute;
                _localization = CL4EE.GetLocalization(fieldInfo.Module.Assembly);
                Update(true);
            }

            private void Update(bool force = false)
            {
                if (force || _localeCode != _localization?.CurrentLocaleCode)
                {
                    _label = new GUIContent(_localization?.Tr(_attribute.LocalizationKey) ?? _attribute.LocalizationKey);
                    if (_attribute.TooltipKey != null)
                        _label.tooltip = _localization?.Tr(_attribute.TooltipKey) ?? _attribute.TooltipKey;
                    _localeCode = _localization?.CurrentLocaleCode;
                }
            }
        }

        abstract class InheritingDrawer<TAttr> : PropertyDrawer where TAttr : PropertyAttribute
        {
            private PropertyDrawer _upstreamDrawer;
            private bool _initialized;

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                InitializeUpstream(property);
                return _upstreamDrawer?.GetPropertyHeight(property, label) ?? GetDefaultPropertyHeight(property, label);
            }

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                InitializeUpstream(property);
                if (_upstreamDrawer != null)
                    _upstreamDrawer.OnGUI(position, property, label);
                else
                    OnGUIDefault(position, property, label);
            }

            protected abstract void Initialize();

            protected void InitializeUpstream(SerializedProperty property)
            {
                if (_initialized) return;
                Initialize();

                foreach (var propAttr in fieldInfo.GetCustomAttributes<PropertyAttribute>()
                             .Reverse()
                             .TakeWhile(x => x.GetType() != typeof(TAttr)))
                    HandleDrawnType(property, propAttr.GetType(), propAttr);

                // if we cannot find upstream PropertyDrawer, we find it using 
                if (_upstreamDrawer == null)
                {
                    var type = fieldInfo.FieldType;
                    // the path ends with array element
                    if (Regex.IsMatch(property.propertyPath, "\\.Array\\.data\\[[0-9]+\\]$"))
                    {
                        if (type.IsArray)
                        {
                            type = type.GetElementType() ?? throw new InvalidOperationException();
                        }
                        else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                        {
                            type = type.GetGenericArguments()[0];
                        }
                    }

                    HandleDrawnType(property, type, null);
                }

                _initialized = true;
            }

            private void HandleDrawnType(SerializedProperty property, Type drawnType, PropertyAttribute attr)
            {
                Type forPropertyAndType = Reflections.GetDrawerTypeForPropertyAndType(property, drawnType);
                if (forPropertyAndType == null)
                    return;
                if (typeof (PropertyDrawer).IsAssignableFrom(forPropertyAndType))
                {
                    _upstreamDrawer = (PropertyDrawer) Activator.CreateInstance(forPropertyAndType);
                    Reflections.SetFieldAndAttribute(_upstreamDrawer, fieldInfo, attr);
                }
            }

            private float GetDefaultPropertyHeight(SerializedProperty property, GUIContent label)
            {
                property = property.Copy();
                var height = EditorGUI.GetPropertyHeight(property.propertyType, label);
                var enterChildren = property.isExpanded && HasVisibleChildFields(property);
                if (!enterChildren) return height;

                var label1 = new GUIContent(label.text);
                var endProperty = property.GetEndProperty();
                while (property.NextVisible(enterChildren) && !SerializedProperty.EqualContents(property, endProperty))
                {
                    height += EditorGUI.GetPropertyHeight(property, label1, true);
                    height += EditorGUIUtility.standardVerticalSpacing;
                    enterChildren = false;
                }

                return height;
            }

            private void OnGUIDefault(Rect position, SerializedProperty property, GUIContent label)
            {
                // cache value
                var iconSize = EditorGUIUtility.GetIconSize();
                var enabled = GUI.enabled;
                var indentLevel = EditorGUI.indentLevel;

                var indentLevelOffset = indentLevel - property.depth;

                var serializedProperty = property.Copy();
                position.height = EditorGUI.GetPropertyHeight(serializedProperty.propertyType, label);
                EditorGUI.indentLevel = serializedProperty.depth + indentLevelOffset;
                var enterChildren = Reflections.DefaultPropertyField(position, serializedProperty, label) &&
                                    HasVisibleChildFields(serializedProperty);
                position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                if (enterChildren)
                {
                    var endProperty = serializedProperty.GetEndProperty();
                    while (serializedProperty.NextVisible(enterChildren) && !SerializedProperty.EqualContents(serializedProperty, endProperty))
                    {
                        EditorGUI.indentLevel = serializedProperty.depth + indentLevelOffset;
                        position.height = EditorGUI.GetPropertyHeight(serializedProperty, null, false);
                        EditorGUI.BeginChangeCheck();
                        enterChildren = EditorGUI.PropertyField(position, serializedProperty, null, false) &&
                                        HasVisibleChildFields(serializedProperty);
                        if (EditorGUI.EndChangeCheck())
                            break;
                        position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                    }
                }

                // restore value
                GUI.enabled = enabled;
                EditorGUIUtility.SetIconSize(iconSize);
                EditorGUI.indentLevel = indentLevel;
            }
            
            private static bool HasVisibleChildFields(SerializedProperty property)
            {
                switch (property.propertyType)
                {
                    case SerializedPropertyType.Vector2:
                    case SerializedPropertyType.Vector3:
                    case SerializedPropertyType.Rect:
                    case SerializedPropertyType.Bounds:
                    case SerializedPropertyType.Vector2Int:
                    case SerializedPropertyType.Vector3Int:
                    case SerializedPropertyType.RectInt:
                    case SerializedPropertyType.BoundsInt:
                        return false;
                    default:
                        return property.hasVisibleChildren;
                }
            }
        }

        static class Reflections
        {
            [NotNull] private static readonly MethodInfo GetDrawerTypeForPropertyAndTypeInfo;
            [NotNull] private static readonly FieldInfo FieldInfoInfo;
            [NotNull] private static readonly FieldInfo AttributeInfo;
            [NotNull] private static readonly MethodInfo DefaultPropertyFieldInfo;

            static Reflections()
            {
                var type = typeof(Editor).Assembly.GetType("UnityEditor.ScriptAttributeUtility");
                var methodInfo = type.GetMethod("GetDrawerTypeForPropertyAndType",
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static,
                    null,
                    new[] { typeof(SerializedProperty), typeof(Type) },
                    null);
                GetDrawerTypeForPropertyAndTypeInfo = methodInfo ?? throw new InvalidOperationException();
                FieldInfoInfo = typeof(PropertyDrawer).GetField("m_FieldInfo", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public) ?? throw new InvalidOperationException();
                AttributeInfo = typeof(PropertyDrawer).GetField("m_Attribute", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public) ?? throw new InvalidOperationException();
                DefaultPropertyFieldInfo = typeof(EditorGUI)
                    .GetMethod("DefaultPropertyField",
                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static,
                        null,
                        new[]
                        {
                            typeof(Rect),
                            typeof(SerializedProperty),
                            typeof(GUIContent),
                        },
                        null) ?? throw new InvalidOperationException();
            }

            public static Type GetDrawerTypeForPropertyAndType(SerializedProperty property, Type type) => 
                (Type)GetDrawerTypeForPropertyAndTypeInfo.Invoke(null, new object[] { property, type });

            public static void SetFieldAndAttribute(PropertyDrawer drawer, FieldInfo fieldInfo, PropertyAttribute attribute)
            {
                FieldInfoInfo.SetValue(drawer, fieldInfo);
                AttributeInfo.SetValue(drawer, attribute);
            }

            internal static bool DefaultPropertyField(Rect position, SerializedProperty property, GUIContent label) =>
                (bool)DefaultPropertyFieldInfo.Invoke(null, new object[] { position, property, label });
        }

        #endregion
    }

#if COM_ANATAWA12_CUSTOM_LOCALIZATION_FOR_EDITOR_EXTENSION_AS_PACKAGE
    public
#else
    internal
#endif
        class LocaleLocalization
    {
        private LocalizationAsset Asset { get; }
        public string Name { get; }
        public string LocaleIsoCode { get; }

        [Obsolete("Internal CL4EE API. Don't use this.", error: true)]
        internal LocaleLocalization(LocalizationAsset asset)
        {
            Asset = asset;
            LocaleIsoCode = asset.localeIsoCode;
            Name = TryGetLocalizedString($"locale:{asset.localeIsoCode}") ?? DefaultLocaleName(asset.localeIsoCode);
        }

        [CanBeNull]
        public string TryGetLocalizedString(string key)
        {
            string localized;
            return Asset != null && (localized = Asset.GetLocalizedString(key)) != key ? localized : null;
        }

        private static readonly Dictionary<string, string> FallbackLocaleNames = new Dictionary<string, string>();

        private static string DefaultLocaleName(string code)
        {
            if (FallbackLocaleNames.TryGetValue(code, out var name)) return name;

            name = new CultureInfo(code).EnglishName;
            FallbackLocaleNames[code] = name;

            return name;
        }
    }

#if COM_ANATAWA12_CUSTOM_LOCALIZATION_FOR_EDITOR_EXTENSION_AS_PACKAGE
    public
#else
    internal
#endif
        // ReSharper disable once InconsistentNaming
        static class CL4EE
    {
        private static readonly Dictionary<Assembly, Func<Localization>> LocalizationGetter =
            new Dictionary<Assembly, Func<Localization>>();

        /// <returns>Localization instance for caller assembly</returns>
        [CanBeNull]
        public static Localization GetLocalization() => GetLocalization(Assembly.GetCallingAssembly());

        [CanBeNull]
        public static Localization GetLocalization([NotNull] Assembly assembly) => GetLocalizationGetter(assembly)();

        [NotNull]
        public static string Tr([NotNull] string localizationKey) =>
            GetLocalization(Assembly.GetCallingAssembly())?.Tr(localizationKey) ?? localizationKey;

        public static void DrawLanguagePicker()
        {
            var localization = GetLocalization(Assembly.GetCallingAssembly());
            if (localization == null)
                EditorGUILayout.LabelField("ERROR", "CL4EE Localization Instance Not Found");
            else
                localization.DrawLanguagePicker();
        }

        [NotNull]
        private static Func<Localization> GetLocalizationGetter([NotNull] Assembly assembly)
        {
            if (LocalizationGetter.TryGetValue(assembly, out var getter))
                return getter;

            if (IsDisallowedAssemblyName(assembly.GetName().Name))
            {
                Debug.LogError("Getting Assembly for unity default assemblies are not allowed.");
                return () => null;
            }

            if (assembly.GetCustomAttribute<RedirectCL4EEInstanceAttribute>() is RedirectCL4EEInstanceAttribute attr)
            {
                Assembly redirectTo = null;
                try
                {
                    redirectTo = Assembly.Load(attr.RedirectToName);
                }
                catch (Exception e)
                {
                    Debug.LogError(
                        $"Unable to load redirected assembly for {assembly.GetName().Name} ({attr.RedirectToName}).");
                    Debug.LogException(e);
                }

                if (redirectTo != null)
                    return GetLocalizationGetter(redirectTo);
            }

            const BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            var types = assembly.GetTypes();

            var properties = types.SelectMany(x => x.GetProperties(bindingFlags))
                .Where(x => x.PropertyType == typeof(Localization))
                .Where(x => x.GetMethod != null)
                .Where(x => x.GetCustomAttribute<AssemblyCL4EELocalizationAttribute>() != null)
                .ToArray();

            if (properties.Length == 0)
            {
                Debug.LogError($"Static property with AssemblyLocalizationInstanceAttribute not found for {assembly}");
                getter = () => null;
            }
            else
            {
                if (properties.Length != 1)
                {
                    Debug.LogError(
                        $"Multiple static property with AssemblyLocalizationInstanceAttribute not found for {assembly}! First one will be used");
                }

                var propertyInfo = properties[0];
                var getterMethod = propertyInfo.GetMethod;

                getter = (Func<Localization>)Delegate.CreateDelegate(typeof(Func<Localization>), getterMethod);
            }

            LocalizationGetter[assembly] = getter;

            return getter;
        }

        // default assembly names are not allowed.
        // https://docs.unity3d.com/ja/2019.4/Manual/ScriptCompileOrderFolders.html
        private static bool IsDisallowedAssemblyName(string name)
        {
            switch (name)
            {
                case "Assembly-CSharp-firstpass":
                case "Assembly-CSharp-Editor-firstpass":
                case "Assembly-CSharp":
                case "Assembly-CSharp-Editor":
                    return true;
                default:
                    return false;
            }
        }
    }

    /// <summary>
    /// Specifies the Localization instance for that assembly.
    /// You can use specified instance using <see cref="CL4EE"/> class or Cl4EeLocalizedAttribute (to be implemented)
    /// </summary>
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Property)]
#if COM_ANATAWA12_CUSTOM_LOCALIZATION_FOR_EDITOR_EXTENSION_AS_PACKAGE
    public
#else
    internal
#endif
        // ReSharper disable once InconsistentNaming
        sealed class AssemblyCL4EELocalizationAttribute : Attribute
    {
        
    }
}
#endif 
