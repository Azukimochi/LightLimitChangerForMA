
namespace io.github.azukimochi;

[CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
internal sealed class MinMaxSliderAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.Vector2)
        {
            return;
        }
        using var scope = new PropertyScope(position, label, property);
        var vector = property.vector2Value;
        var attr = attribute as MinMaxSliderAttribute;

        position = EditorGUI.PrefixLabel(position, scope.Label);
        position.x -= EditorGUI.indentLevel * 15f;
        position.width += EditorGUI.indentLevel * 15f;


        float floatFieldWidth = Mathf.Max(position.width * 0.1f, 50);
        float padding = 10f;

        var left = position with { width = floatFieldWidth };
        var mid = position with { width = position.width - (floatFieldWidth * 2 + padding * 2), x = position.x + left.width + padding };
        var right = position with { width = floatFieldWidth, x = position.x + left.width + mid.width + padding * 2 };

        EditorGUI.BeginChangeCheck();
        var f = EditorGUI.FloatField(left, GUIContent.none, Mathf.Round(vector.x * 100) / 100f);
        if (EditorGUI.EndChangeCheck())
        {
            vector.x = Mathf.Clamp(f, attr.Min, vector.y);
            property.vector2Value = vector;
        }
        EditorGUI.BeginChangeCheck();
        EditorGUI.MinMaxSlider(mid, GUIContent.none, ref vector.x, ref vector.y, attr.Min, attr.Max);
        if (EditorGUI.EndChangeCheck())
        {
            property.vector2Value = vector;
        }

        EditorGUI.BeginChangeCheck();
        f = EditorGUI.FloatField(right, GUIContent.none, Mathf.Round(vector.y * 100) / 100f);
        if (EditorGUI.EndChangeCheck())
        {
            vector.y = Mathf.Clamp(f, vector.x, attr.Max);
            property.vector2Value = vector;
        }
    }
}
