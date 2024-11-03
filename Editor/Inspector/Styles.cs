using System.Linq;
using static io.github.azukimochi.LightLimitChangerComponentEditor;

namespace io.github.azukimochi;

internal static class Styles
{
    public static Lazy<GUIContent[]> TabToggles = new Lazy<GUIContent[]>(() =>
    {
        return Enum.GetNames(typeof(Tab)).Select(x => new GUIContent(x)).ToArray();
    }, false);

    public static Lazy<GUIStyle> Foldoutbackground = new Lazy<GUIStyle>(() =>
    {
        var style = new GUIStyle("HelpBox");
        style.margin = new RectOffset(15, 0, 0, 0);
        return style;
    }, false);

    public static readonly GUIStyle TabButtonStyle = "LargeButton";

    // GUI.ToolbarButtonSize.FitToContentsも設定できる
    public static readonly GUI.ToolbarButtonSize TabButtonSize = GUI.ToolbarButtonSize.Fixed;

    public static readonly Lazy<GUIStyle> ShurikenTitle = new(() => new GUIStyle("ShurikenModuleTitle")
    {
        font = EditorStyles.label.font,
        border = new RectOffset(15, 7, 4, 4),
        fixedHeight = 22,
        contentOffset = new Vector2(20f, -2f),
        fontSize = 12
    }, isThreadSafe: false);

    public static readonly Lazy<GUIStyle> CategoryLabel = new(() =>
    {
        var style = new GUIStyle(EditorStyles.label);
        style.fontStyle = FontStyle.Bold;
        style.fontSize = 14;
        style.normal.textColor = Color.white;
        style.richText = true;
        return style;
    }, isThreadSafe: false);
}