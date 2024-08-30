namespace io.github.azukimochi;

[CustomPropertyDrawer(typeof(RangeParameterAttribute))]
internal sealed class RangeParameterDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return ParameterDrawer.GetPropertyHeight(property);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var attr = attribute as RangeParameterAttribute;
        var path = property.propertyPath;
        var idx = path.LastIndexOf(".");
        if (idx == -1)
            return;
        idx += 1;
        string propertyPath = string.Create(idx + attr.ParameterName.Length, (Memory: path.AsMemory(0, idx), attr.ParameterName), (span, state) =>
        {
            state.Memory.Span.CopyTo(span);
            state.ParameterName.AsSpan().CopyTo(span[state.Memory.Length..]);
        });

        var rangeParameter = property.serializedObject.FindProperty(propertyPath);
        if (rangeParameter == null)
            return;

        ParameterDrawer.Draw(position, property, label, rangeParameter.vector2Value);
    }
}
