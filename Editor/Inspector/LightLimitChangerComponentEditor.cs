using Target = io.github.azukimochi.LightLimitChangerComponent;
using System.Linq;
using UnityEngine.UIElements;

namespace io.github.azukimochi;

[CustomEditor(typeof(Target))]
internal sealed class LightLimitChangerComponentEditor : Editor
{
    private static Texture2D linearGrayTexture;
    internal enum Tab
    {
        BasicSettings,
        AdvancedSettings,
        DiescriptionMode,
    }
    
    public static Tab SelectedTab = Tab.BasicSettings;

    private void OnEnable()
    {
    }

    public override void OnInspectorGUI()
    {
        var target = (Target)base.target;

        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.FloatField("Version", target.Version);
        }
        using (new EditorGUILayout.HorizontalScope()) {
            GUILayout.FlexibleSpace();
            // タブを描画する
            SelectedTab = (Tab)GUILayout.Toolbar((int)SelectedTab, Styles.TabToggles.Value, Styles.TabButtonStyle, Styles.TabButtonSize);
            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.Space();
        CategoryLabel("General Settings");
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("General.AllowParameterController"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("General.OverwriteMaterialParameters"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("General.OverwriteMeshSettings"));
        EditorGUILayout.Space();

        DoFoldoutedGroup(serializedObject.FindProperty("General.LightingControl"), "Lighting Settings", static property =>
        {
            EditorGUILayout.PropertyField(property.FindPropertyRelative("MaxLight"));
            DescriptionHelpBox("明るさの上限値");

            using (DisableScope.If(!property.FindPropertyRelative("MaxLight.Enable").boolValue))
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative("MaxLightRange"));
                DescriptionHelpBox("明るさの上限の範囲");
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(property.FindPropertyRelative("MinLight"));
            DescriptionHelpBox("明るさの下限値");

            using (DisableScope.If(!property.FindPropertyRelative("MinLight.Enable").boolValue))
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative("MinLightRange"));
                DescriptionHelpBox("明るさの下限の範囲");
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(property.FindPropertyRelative("Monochrome"));
            DescriptionHelpBox("環境強のモノクロ化の度合いの初期値");

            EditorGUILayout.PropertyField(property.FindPropertyRelative("Unlit"));
            DescriptionHelpBox("環境強の無視具合の初期値");
        });

        DoFoldoutedGroup(serializedObject.FindProperty("General.ColorControl"), "Color Settings", static property =>
        {
            EditorGUILayout.PropertyField(property.FindPropertyRelative("Hue"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("Saturation"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("Brightness"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("Gamma"));
        });

        DoFoldoutedGroup(serializedObject.FindProperty("General.EmissionControl"), "Emission Settings", static property =>
        {
            EditorGUILayout.PropertyField(property.FindPropertyRelative("Strength"));
        });


        CategoryLabel("Shader Settings");
        EditorGUILayout.Space();

        DoFoldoutedGroup(serializedObject.FindProperty("LilToon"), "lilToon Settings", static property =>
        {
            EditorGUILayout.LabelField("Override Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(property.FindPropertyRelative("ShadowEnvStrength"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("VertexLightStrength"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Additional Settings", EditorStyles.boldLabel);

            EditorGUILayout.Space();
        });

        DoFoldoutedGroup(serializedObject.FindProperty("Poiyomi"), "Poiyomi Settings", property =>
        {
            EditorGUILayout.LabelField("Override Settings", EditorStyles.boldLabel);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Additional Settings", EditorStyles.boldLabel);

            EditorGUILayout.Space();
        });


        DoFoldoutedGroup(serializedObject.FindProperty("UnlitWF"), "UnlitWF Settings", property =>
        {
            EditorGUILayout.LabelField("Override Settings", EditorStyles.boldLabel);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Additional Settings", EditorStyles.boldLabel);

            EditorGUILayout.Space();
        });

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Excludes", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Excludes"), true);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Write Defaults", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("WriteDefaults"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Target Shader", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("TargetShader"));

        serializedObject.ApplyModifiedProperties();
    }

    private void DoFoldoutedGroup(SerializedProperty group, string title, Action<SerializedProperty> inner, bool insertSpaceToEnd = true)
    {
        if (!(group.isExpanded = Foldout(title, group.isExpanded)))
            return;

        EditorGUILayout.BeginVertical(Styles.Foldoutbackground.Value);
        try
        {
            inner(group);
        }
        catch { }
        EditorGUILayout.EndVertical();
        if (insertSpaceToEnd)
            EditorGUILayout.Space();
    }

    private static void CategoryLabel(string title) => EditorGUILayout.LabelField(title, Styles.CategoryLabel.Value);

    private static bool Foldout(string title, bool display)
    {
        var style = Styles.Foldout.Value;
        var rect = GUILayoutUtility.GetRect(16f, 20f, style);
        GUI.Box(rect, title, style);

        var e = Event.current;

        var toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
        if (e.type == EventType.Repaint) {
            EditorStyles.foldout.Draw(toggleRect, false, false, display, false);
        }

        if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition)) {
            display = !display;
            e.Use();
        }

        return display;
    }
    private static void DescriptionHelpBox(string message)
    {
        if (SelectedTab != Tab.DiescriptionMode)
            return;

        EditorGUILayout.HelpBox(message, MessageType.Info);
        EditorGUILayout.Space();
    }
    private static void ControledParameterField(SerializedProperty property)
    {
        EditorGUILayout.PropertyField(property);
    }

    private static class Styles
    {
        public static Lazy<GUIContent[]> TabToggles = new Lazy<GUIContent[]>(() =>
        {
            return Enum.GetNames(typeof(Tab)).Select(x => new GUIContent(x)).ToArray();
        }, false);

        public static Lazy<GUIStyle> Foldoutbackground = new Lazy<GUIStyle>(() =>
        {
            var style = new GUIStyle("HelpBox");
            style.margin = new RectOffset(15, 0, 0, 0);
            return style;
        }, false);

        public static readonly GUIStyle TabButtonStyle = "LargeButton";

        // GUI.ToolbarButtonSize.FitToContentsも設定できる
        public static readonly GUI.ToolbarButtonSize TabButtonSize = GUI.ToolbarButtonSize.Fixed;

        public static readonly Lazy<GUIStyle> Foldout = new(() => new GUIStyle("ShurikenModuleTitle")
        {
            font = EditorStyles.label.font,
            border = new RectOffset(15, 7, 4, 4),
            fixedHeight = 22,
            contentOffset = new Vector2(20f, -2f),
            fontSize = 12
        }, isThreadSafe: false);

        public static readonly Lazy<GUIStyle> CategoryLabel = new(() =>
        {
            var style = new GUIStyle(EditorStyles.label);
            style.fontStyle = FontStyle.Bold;
            style.fontSize = 14;
            style.normal.textColor = Color.white;
            return style;
        }, isThreadSafe: false);
    }
}
