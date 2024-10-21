using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace io.github.azukimochi;

[CustomPropertyDrawer(typeof(SemVer))]
internal sealed class SemVerDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        label = EditorGUI.BeginProperty(position, label, property);
        EditorGUI.TextField(position, label, property.boxedValue.ToString());
        EditorGUI.EndProperty();
    }
}
