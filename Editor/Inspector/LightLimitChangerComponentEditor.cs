using Target = io.github.azukimochi.LightLimitChangerComponent;

namespace io.github.azukimochi;

[CustomEditor(typeof(Target))]
internal sealed partial class LightLimitChangerComponentEditor : Editor
{
    private static Texture2D linearGrayTexture;
    internal enum Tab
    {
        BasicSettings,
        AdvancedSettings,
        DiescriptionMode,
    }
    
    public static Tab SelectedTab = Tab.BasicSettings;

    public static bool IsAdvancedMode { get => Preferences.Local.AdvancedMode; set => Preferences.Local.AdvancedMode = value; }

    public static bool ShowDescriptions { get => Preferences.Local.ShowDescription; set => Preferences.Local.ShowDescription = value; }

    public bool PresetMode { get; set; }

    public void OnEnable()
    {
        SelectedTab = IsAdvancedMode ? ShowDescriptions ? Tab.DiescriptionMode : Tab.AdvancedSettings : Tab.BasicSettings;
        var target = (Target)base.target;
        PresetMode = target.transform.parent?.GetComponent<Target>() != null;
    }

    public override void OnInspectorGUI()
    {
        var target = (Target)base.target;
        CategoryLabel($"{LightLimitChanger.Title} {LightLimitChanger.Version}");

        using (new EditorGUILayout.HorizontalScope()) {
            GUILayout.FlexibleSpace();
            // タブを描画する
            EditorGUI.BeginChangeCheck();
            SelectedTab = (Tab)GUILayout.Toolbar((int)SelectedTab, Styles.TabToggles.Value, Styles.TabButtonStyle, Styles.TabButtonSize);
            if (EditorGUI.EndChangeCheck())
            {
                IsAdvancedMode = SelectedTab >= Tab.AdvancedSettings;
                ShowDescriptions = SelectedTab > Tab.AdvancedSettings;
            }
            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.Space();

        CategoryLabel(L10n.TrStr("category:preset"));
        EditorGUILayout.Space();
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.DelayedTextField(serializedObject.FindProperty(nameof(Target.PresetName)), GUIContent.none);
            if (GUILayout.Button("S", GUILayout.Width(24)))
            {
                PresetManager.Global.Update(target.PresetName, target);
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.Space();
        CategoryLabel(L10n.TrStr("category:general-settings"));
        EditorGUILayout.Space();

        static void DoPropertyGUI<TSettings>(SerializedProperty property, string title) where TSettings: ISettings
        {
            using var scope = new ShurikenHeaderGroupScope(property, title);
            try
            {
                if (!property.hasChildren)
                    return;
                var settings = (TSettings)property.boxedValue;
                if (scope.IsOpened)
                {
                    OnGUI(settings, property);
                }
            }
            catch(Exception e) { Debug.LogException(e); }
        }

        DoPropertyGUI<LightingSettings>(serializedObject.FindProperty("General.LightingControl"), L10n.TrStr("category:lighting-settings"));
        DoPropertyGUI<ColorControlSettings>(serializedObject.FindProperty("General.ColorControl"), L10n.TrStr("category:color-settings"));

        CategoryLabel(L10n.TrStr("category:material-settings"));
        EditorGUILayout.Space();

        DoPropertyGUI<LilToonSettings>(serializedObject.FindProperty("LilToon"), L10n.TrStr("category:liltoon-settings"));
        DoPropertyGUI<LilDistanceFadeSettings>(serializedObject.FindProperty("LilToon.DistanceFade"), L10n.TrStr("category:liltoon-distancefade-settings"));
        DoPropertyGUI<PoiyomiSettings>(serializedObject.FindProperty("Poiyomi"), L10n.TrStr("category:poiyomi-settings"));
        DoPropertyGUI<UnlitWFSettings>(serializedObject.FindProperty("UnlitWF"), L10n.TrStr("category:unlitwf-settings"));

        if (!PresetMode)
        {
            CategoryLabel(L10n.TrStr("category:other-settings"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(L10n.TrStr("settings:other/excludes/label"), EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Excludes"), true);
            if(ShowDescriptions)
            {
                EditorGUILayout.HelpBox(L10n.TrStr("settings:other/excludes/description"), MessageType.Info);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(L10n.TrStr("settings:other/writedefault/label"), EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("WriteDefaults"));
            if(ShowDescriptions)
            {
                EditorGUILayout.HelpBox(L10n.TrStr("settings:other/writedefault/description"), MessageType.Info);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(L10n.TrStr("settings:other/target_shader/label"), EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("TargetShader"));
            if(ShowDescriptions)
            {
                EditorGUILayout.HelpBox(L10n.TrStr("settings:other/target_shader/description"), MessageType.Info);
            }
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(L10n.TrStr("settings:other/language/label"), EditorStyles.boldLabel);
        {
            var position = EditorGUILayout.GetControlRect(true, L10n.Localization.GetDrawLanguagePickerHeight());
            var p = position;
            p.width = EditorGUIUtility.labelWidth;
            EditorGUI.LabelField(p, L10n.Tr("Language"));
            position.x += p.width + 4;
            position.width -= p.width + 4;
            L10n.Localization.DrawLanguagePicker(position);
        }

        serializedObject.ApplyModifiedProperties();
    }


    private static void CategoryLabel(string title) => EditorGUILayout.LabelField(title, Styles.CategoryLabel.Value);

    private static void DrawSeparator()
    {
        var position = EditorGUILayout.GetControlRect(false, 12);
        position.y += 4;
        position.height = 1;
        position.width -= 4;
        position.x += 2;
        EditorGUI.DrawRect(position, Color.gray with { a = 0.25f });
    }
}
