namespace io.github.azukimochi;

[CustomPropertyDrawer(typeof(Parameter<>), true)]
internal sealed class ParameterDrawer : PropertyDrawer
{
    private static readonly Lazy<Vector2> FoldoutStyleSize =
        new(() => EditorStyles.foldout.CalcSize(GUIContent.none), false);

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        => Draw(position, property, label);

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        => GetPropertyHeight(property);

    public static float GetPropertyHeight(SerializedProperty property) 
        => EditorGUIUtility.singleLineHeight * (property.isExpanded && LightLimitChangerComponentEditor.SelectedTab != LightLimitChangerComponentEditor.Tab.BasicSettings ? 2 : 1) ;

    public static void DrawLayout(SerializedProperty property, GUIContent label, Vector2? range = null, bool isOverrideValue = false)
    {
        var position = EditorGUILayout.GetControlRect(label != null, GetPropertyHeight(property));
        Draw(position, property, label, range, isOverrideValue);
    }

    public static void Draw(Rect position, SerializedProperty property, GUIContent label, Vector2? range = null, bool isOverrideValue = false)
    {
        using var scope = new PropertyScope(position, label, property);
        var valueProp = property.FindPropertyRelative(isOverrideValue ? "OverrideValue" : "InitialValue");
        //var rangeProp = property.FindPropertyRelative("Range");
        var enableProp = property.FindPropertyRelative("Enable");
        position.height = EditorGUIUtility.singleLineHeight;

        var p = position;
        p.width = EditorGUIUtility.labelWidth;
        bool enable;

        if (LightLimitChangerComponentEditor.SelectedTab == LightLimitChangerComponentEditor.Tab.BasicSettings)
        {
            enable = EditorGUI.ToggleLeft(p, scope.Label, enableProp.boolValue);
            enableProp.boolValue = enable;
        }
        else
        {
            p.x += FoldoutStyleSize.Value.x;
            property.isExpanded = EditorGUI.Foldout(p, property.isExpanded, scope.Label);
            enable = enableProp.boolValue;
        }

        //if (LightLimitChangerComponentEditor.SelectedTab != LightLimitChangerComponentEditor.Tab.BasicSettings)
        //{
        //    property.isExpanded = EditorGUI.Foldout(position with { width = EditorGUIUtility.labelWidth }, property.isExpanded, GUIContent.none, true);
        //}
        p = position;
        p.x += EditorGUIUtility.labelWidth - EditorGUI.indentLevel * 15;
        p.width -= EditorGUIUtility.labelWidth - EditorGUI.indentLevel * 15;
        
        
        p = position;
        p.x += EditorGUIUtility.labelWidth - EditorGUI.indentLevel * 15;
        p.width -= EditorGUIUtility.labelWidth - EditorGUI.indentLevel * 15;

        EditorGUI.BeginDisabledGroup(!enable);
        if (range is { /* Not Null */ } r)
        {
            EditorGUI.BeginChangeCheck();
            var value = EditorGUI.Slider(p, GUIContent.none, valueProp.floatValue, r.x, r.y);
            if (EditorGUI.EndChangeCheck())
            {
                valueProp.floatValue = value;
            }
        }
        else
        {
            if (valueProp.propertyType == SerializedPropertyType.Boolean)
            {
                ToggleDrawer.Draw(p, valueProp, GUIContent.none);
            }
            else
            {
                EditorGUI.PropertyField(p, valueProp, GUIContent.none);
            }
        }
        EditorGUI.EndDisabledGroup();

        if (!property.isExpanded || LightLimitChangerComponentEditor.SelectedTab == LightLimitChangerComponentEditor.Tab.BasicSettings)
            return;

        EditorGUI.indentLevel++;
        position.y += EditorGUIUtility.singleLineHeight;
        position.x += EditorGUIUtility.labelWidth + 30;
        position.width -= EditorGUIUtility.labelWidth;

        p = position with { width = position.width / 3 };
        for (int i = 0; i < 3; i++)
        {
            var p2 = EditorStyles.toggle;
            EditorGUI.indentLevel -= 3;
            EditorGUI.BeginChangeCheck();
            var v = EditorGUI.ToggleLeft(p, enableProp.displayName, enableProp.boolValue);
            if (EditorGUI.EndChangeCheck())
            {
                enableProp.boolValue = v;
            }
            EditorGUI.indentLevel += 3;
            p.x += p.width;
            enableProp.Next(false);
        }
        EditorGUI.indentLevel--;
    }
}
