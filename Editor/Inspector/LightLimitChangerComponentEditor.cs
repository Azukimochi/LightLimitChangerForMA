using Target = io.github.azukimochi.LightLimitChangerComponent;
using UnityEngine.UIElements;
using System.Reflection.Emit;
using System.Reflection;
using System.Linq;
using System.Globalization;
using System.Text;

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

    public static bool IsAdvancedMode { get; set; }

    public static bool ShowDescriptions { get; set; } = true;

    private void OnEnable()
    {
    }

    public override void OnInspectorGUI()
    {
        var target = (Target)base.target;
        CategoryLabel($"{LightLimitChanger.Title} {LightLimitChanger.Version}");
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.DelayedTextField(serializedObject.FindProperty(nameof(Target.PresetName)), GUIContent.none);
            if (GUILayout.Button("S", GUILayout.Width(24)))
            {
                PresetManager.Global.Update(target.PresetName, target);
            }
            EditorGUILayout.EndHorizontal();
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

        void DoPropertyGUI<TSettings>(SerializedProperty property, string title) where TSettings: ISettings
        {
            using var scope = new ShurikenHeaderGroupScope(property, title);
            try
            {
                var settings = (TSettings)property.boxedValue;
                if (scope.IsOpened)
                {
                    OnGUI(settings, property);
                }
            }
            catch { }
        }

        DoPropertyGUI<LightingSettings>(serializedObject.FindProperty("General.LightingControl"), "Lighting Settings");
        DoPropertyGUI<ColorControlSettings>(serializedObject.FindProperty("General.ColorControl"), "Color Settings");

        CategoryLabel("Shader Settings");
        EditorGUILayout.Space();

        DoPropertyGUI<LilToonSettings>(serializedObject.FindProperty("LilToon"), "lilToon Settings");
        DoPropertyGUI<PoiyomiSettings>(serializedObject.FindProperty("Poiyomi"), "Poiyomi Settings");
        DoPropertyGUI<UnlitWFSettings>(serializedObject.FindProperty("UnlitWF"), "UnlitWF Settings");


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


    private static void CategoryLabel(string title) => EditorGUILayout.LabelField(title, Styles.CategoryLabel.Value);

    internal static void OnGUI<TSettings>(TSettings settings, SerializedProperty property) where TSettings : ISettings
        => DynamicGUIBuilder<TSettings>.OnGUI(settings, property);

    internal static class DynamicGUIBuilder<TSettings> where TSettings : ISettings
    {
        public delegate void OnGUIDelegate(TSettings settings, SerializedProperty property);

        public static readonly OnGUIDelegate OnGUI;

        static DynamicGUIBuilder()
        {
            var method = new DynamicMethod($"{typeof(TSettings).Name}_OnGUI", null, typeof(OnGUIDelegate).GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance).GetParameters().Select(x => x.ParameterType).ToArray());
            var il = method.GetILGenerator();
            var fpr = typeof(SerializedProperty).GetMethod(nameof(SerializedProperty.FindPropertyRelative), BindingFlags.Public | BindingFlags.Instance);
            var elpf = typeof(EditorGUILayout).GetMethod(nameof(EditorGUILayout.PropertyField), BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(SerializedProperty), typeof(GUIContent), typeof(GUILayoutOption[]) }, null);
            var tr = typeof(EditorGUIUtility).GetMethod(nameof(EditorGUIUtility.TrTempContent), BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);
            var elhb = typeof(EditorGUILayout).GetMethod(nameof(EditorGUILayout.HelpBox), BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string), typeof(MessageType) }, null);

            var getShowDescription = typeof(LightLimitChangerComponentEditor).GetProperty(nameof(LightLimitChangerComponentEditor.ShowDescriptions), BindingFlags.Public | BindingFlags.Static).GetMethod;
            StringBuilder sb = new();

            // showDesc = LightLimitChangerComponentEditor.ShowDescriptions;
            il.Emit(OpCodes.Call, getShowDescription);
            il.DeclareLocal(typeof(bool));
            il.Emit(OpCodes.Stloc_0);

            foreach (var field in default(TSettings).AllParameterFields())
            {
                var info = new ParameterFieldInfo(field);
                
                // _ = EditorGUILayout.PropertyField(property.FindPropertyRelative(field.Name), EditorGUIUtility.TrTempContent(field.Name));
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldstr, field.Name);
                il.Emit(OpCodes.Callvirt, fpr);
                il.Emit(OpCodes.Ldstr, StringExt.Create(sb, $"settings:{SettingsFieldInfo<TSettings>.Id}/{char.ToLowerInvariant(field.Name[0])}{field.Name.AsSpan(1)}/label"));
                il.Emit(OpCodes.Call, tr);
                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Call, elpf);
                il.Emit(OpCodes.Pop);

                // if (showDesc)
                //     EditorGUILayout.HelpBox(message, MessageType.Info);

                var marker = il.DefineLabel();
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Brfalse_S, marker);

                il.Emit(OpCodes.Ldstr, StringExt.Create(sb, $"settings:{SettingsFieldInfo<TSettings>.Id}/{char.ToLowerInvariant(field.Name[0])}{field.Name.AsSpan(1)}/description"));
                //il.Emit(OpCodes.Ldc_I4, (int)MessageType.Info);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Call, elhb);

                il.MarkLabel(marker);
            }
            il.Emit(OpCodes.Ret);

            OnGUI = method.CreateDelegate(typeof(OnGUIDelegate)) as OnGUIDelegate;
        }
    }
}
