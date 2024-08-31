using System;
using System.Collections.Generic;
using System.Text;

namespace io.github.azukimochi;

internal readonly ref struct PropertyScope
{
    public readonly GUIContent Label;

    public PropertyScope(Rect totalPosition, GUIContent label, SerializedProperty property) 
        => Label = EditorGUI.BeginProperty(totalPosition, label, property);

    public void Dispose() => EditorGUI.EndProperty();
}

internal readonly ref struct IndentScope
{
    public readonly int PreviousLevel;

    public IndentScope(int level)
    {
        PreviousLevel = EditorGUI.indentLevel;
        EditorGUI.indentLevel = level;
    }

    public void Dispose()
    {
        EditorGUI.indentLevel = PreviousLevel;
    }

    public static IndentScope Increment(int level = 1) => new(EditorGUI.indentLevel + level);
}

internal readonly ref struct DisableScope
{
    public DisableScope(bool disabled)
    {
        EditorGUI.BeginDisabledGroup(disabled);
    }

    public void Dispose()
    {
        EditorGUI.EndDisabledGroup();
    }

    public static DisableScope Disable() => new(true);

    public static DisableScope Enable() => new(false);

    public static DisableScope If(bool disabled) => new(disabled);
}