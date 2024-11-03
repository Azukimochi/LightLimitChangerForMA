namespace io.github.azukimochi;

internal readonly ref struct PropertyScope
{
    public readonly GUIContent Label;

    public PropertyScope(Rect totalPosition, GUIContent label, SerializedProperty property) 
        => Label = EditorGUI.BeginProperty(totalPosition, label, property);

    public void Dispose() => EditorGUI.EndProperty();
}
