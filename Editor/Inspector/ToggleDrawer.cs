
namespace io.github.azukimochi;

internal sealed class ToggleDrawer
{
    private static GUIContent[] labels = { new("OFF"), new("ON") };

    public static void Draw(Rect position, SerializedProperty property, GUIContent label)
    {
        label = EditorGUI.BeginProperty(position, label, property);
        bool v = property.boolValue;

        EditorGUI.BeginChangeCheck();
        int x = EditorGUI.Popup(position, label, v ? 1 : 0, labels);
        if (EditorGUI.EndChangeCheck())
        {
            property.boolValue = x != 0;
        }

        EditorGUI.EndProperty();
    }
}