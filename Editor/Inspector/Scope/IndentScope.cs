namespace io.github.azukimochi;

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
