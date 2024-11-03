namespace io.github.azukimochi;

internal readonly ref struct ShurikenHeaderGroupScope
{
    private readonly bool InsertSpaceToEnd;
    public readonly bool IsOpened;

    public ShurikenHeaderGroupScope(SerializedProperty group, string title, bool insertSpaceToEnd = true)
    {
        InsertSpaceToEnd = insertSpaceToEnd;

        if (!(IsOpened = group.isExpanded = Foldout(title, group.isExpanded)))
            return;

        EditorGUILayout.BeginVertical(Styles.Foldoutbackground.Value);
    }

    public void Dispose()
    {
        if (!IsOpened)
            return;

        EditorGUILayout.EndVertical();
        if (InsertSpaceToEnd)
            EditorGUILayout.Space();
    }

    private static bool Foldout(string title, bool display)
    {
        var style = Styles.ShurikenTitle.Value;
        var rect = GUILayoutUtility.GetRect(16f, 20f, style);
        GUI.Box(rect, title, style);

        var e = Event.current;

        var toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
        if (e.type == EventType.Repaint)
        {
            EditorStyles.foldout.Draw(toggleRect, false, false, display, false);
        }

        if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
        {
            display = !display;
            e.Use();
        }

        return display;
    }
}