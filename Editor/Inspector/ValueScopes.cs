using System;
using System.Collections.Generic;
using System.Text;

namespace io.github.azukimochi;

internal ref struct PropertyScope
{
    public GUIContent Label;

    public PropertyScope(Rect totalPosition, GUIContent label, SerializedProperty property) 
        => Label = EditorGUI.BeginProperty(totalPosition, label, property);

    public void Dispose() => EditorGUI.EndProperty();
}