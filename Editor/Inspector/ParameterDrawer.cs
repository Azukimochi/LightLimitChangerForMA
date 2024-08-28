namespace io.github.azukimochi;

[CustomPropertyDrawer(typeof(Parameter<>), true)]
internal sealed class ParameterDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        => Draw(position, property, label);

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        => GetPropertyHeight(property);

    public static float GetPropertyHeight(SerializedProperty property) 
        => EditorGUIUtility.singleLineHeight * (property.isExpanded ? 2 : 1);

    public static void Draw(Rect position, SerializedProperty property, GUIContent label, Vector2? range = null)
    {
        using var scope = new PropertyScope(position, label, property);
        var valueProp = property.FindPropertyRelative("InitialValue");
        //var rangeProp = property.FindPropertyRelative("Range");
        var enableProp = property.FindPropertyRelative("Enable");
        position.height = EditorGUIUtility.singleLineHeight;

        EditorGUI.BeginChangeCheck();
        var enable = EditorGUI.ToggleLeft(position, scope.Label, enableProp.boolValue);
        if (EditorGUI.EndChangeCheck())
        {
            enableProp.boolValue = enable;
        }

        property.isExpanded = EditorGUI.Foldout(position with { width = EditorGUIUtility.labelWidth }, property.isExpanded, GUIContent.none, true);

        var p = position;
        p.x += EditorGUIUtility.labelWidth - EditorGUI.indentLevel * 15;
        p.width -= EditorGUIUtility.labelWidth - EditorGUI.indentLevel * 15;

        EditorGUI.BeginDisabledGroup(!enable);
        if (range is { /* Not Null */ } r)
        {
            EditorGUI.BeginChangeCheck();
            var value = EditorGUI.Slider(p, GUIContent.none, valueProp.floatValue, r.x, r.y);
            EditorGUI.Slider(p, GUIContent.none, property.FindPropertyRelative("OverrideValue").floatValue, r.x, r.y);
            if (EditorGUI.EndChangeCheck())
            {
                valueProp.floatValue = value;
            }
        }
        else
        {
            EditorGUI.PropertyField(p, valueProp, GUIContent.none);
        }
        EditorGUI.EndDisabledGroup();

        if (!property.isExpanded)
            return;
        EditorGUI.indentLevel++;
        position.y += EditorGUIUtility.singleLineHeight;
        position.x += EditorGUIUtility.labelWidth;
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
