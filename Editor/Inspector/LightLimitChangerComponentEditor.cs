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
    private bool isOpenLightingSetting;
    private bool isOpenColorSetting;
    private bool isOpenEmissionSetting;
    private bool isOpenLilToonSetting;
    private bool isOpenPoiyomiSetting;
    private bool isOpenUnlitWFSetting;

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
        CategoryLabelStyle("General Settings");
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("General.AllowParameterController"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("General.OverwriteMaterialParameters"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("General.OverwriteMeshSettings"));
        EditorGUILayout.Space();
        
        isOpenLightingSetting = Foldout("Lighting Settings", isOpenLightingSetting);
        if(isOpenLightingSetting) {

            using (new EditorGUILayout.VerticalScope(Styles.Foldoutbackground.Value))
            {
                ControledParameterField(serializedObject.FindProperty("General.LightingControl.MaxLight"));
                
                DescriptionHelpBox("明るさの上限値");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("General.LightingControl.MinLight"));
                DescriptionHelpBox("明るさの下限値");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("General.LightingControl.MaxLightRange"));
                DescriptionHelpBox("明るさの上限の範囲");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("General.LightingControl.MinLightRange"));
                DescriptionHelpBox("明るさの下限の範囲");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("General.LightingControl.Monochrome"));
                DescriptionHelpBox("環境強のモノクロ化の度合いの初期値");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("General.LightingControl.Unlit"));
                DescriptionHelpBox("環境強の無視具合の初期値");
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        
        isOpenColorSetting = Foldout("Color Settings", isOpenColorSetting);
        if(isOpenColorSetting) {
            using (new EditorGUILayout.VerticalScope(Styles.Foldoutbackground.Value))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("General.ColorControl.Hue"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("General.ColorControl.Saturation"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("General.ColorControl.Brightness"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("General.ColorControl.Gamma"));
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        
        isOpenEmissionSetting = Foldout("Emission Settings", isOpenEmissionSetting);
        if(isOpenEmissionSetting) {
            using (new EditorGUILayout.VerticalScope(Styles.Foldoutbackground.Value))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("General.EmissionControl.Strength"));
            }
        }

        CategoryLabelStyle("Shader Settings");
        EditorGUILayout.Space();
        
        isOpenLilToonSetting = Foldout("lilToon Settings", isOpenLilToonSetting);
        if(isOpenLilToonSetting) {
            using (new EditorGUILayout.VerticalScope(Styles.Foldoutbackground.Value))
            {
                EditorGUILayout.LabelField("Override Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("LilToon.ShadowEnvStrength"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("LilToon.VertexLightStrength"));
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Additional Settings", EditorStyles.boldLabel);
                
                EditorGUILayout.Space();
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        
        isOpenPoiyomiSetting = Foldout("Poiyomi Settings", isOpenPoiyomiSetting);
        if(isOpenPoiyomiSetting) {
            using (new EditorGUILayout.VerticalScope(Styles.Foldoutbackground.Value))
            {
                EditorGUILayout.LabelField("Override Settings", EditorStyles.boldLabel);
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Additional Settings", EditorStyles.boldLabel);
                
                EditorGUILayout.Space();
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        
        isOpenUnlitWFSetting = Foldout("UnlitWF Settings", isOpenUnlitWFSetting);
        if(isOpenUnlitWFSetting) {
            using (new EditorGUILayout.VerticalScope(Styles.Foldoutbackground.Value))
            {
                EditorGUILayout.LabelField("Override Settings", EditorStyles.boldLabel);
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Additional Settings", EditorStyles.boldLabel);
                
                EditorGUILayout.Space();
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

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

        
    }
    private static void CategoryLabelStyle(string title)
    {
        
        GUIStyle label = new GUIStyle(EditorStyles.label);
        label.fontStyle = FontStyle.Bold;
        label.normal.textColor = Color.white;
        label.fontSize = 14;
        EditorGUILayout.LabelField(title, label);
    }
    
    private static bool Foldout(string title, bool display)
    {
        var style = new GUIStyle("ShurikenModuleTitle");
        style.font = new GUIStyle(EditorStyles.label).font;
        style.border = new RectOffset(15, 7, 4, 4);
        style.fixedHeight = 22;
        style.contentOffset = new Vector2(20f, -2f);
        style.fontSize = 12;

        var rect = GUILayoutUtility.GetRect(16f, 22f, style);
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
        if (SelectedTab == Tab.DiescriptionMode)
        {
            EditorGUILayout.HelpBox(message, MessageType.Info);
        }
    }
    private static void ControledParameterField(SerializedProperty property)
    {
        EditorGUILayout.PropertyField(property);
    }
}
