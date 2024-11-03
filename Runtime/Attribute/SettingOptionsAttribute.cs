namespace io.github.azukimochi;

[AttributeUsage(AttributeTargets.Class)]
internal sealed class SettingOptionsAttribute : Attribute
{
    public SettingOptionsAttribute(string id, string displayName, string parameterPrefix = null)
    {
        Id = id;
        DisplayName = displayName;
        ParameterPrefix = parameterPrefix;
    }

    /// <summary>
    /// ローカライズとかに使うID
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// メニューの表示名
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// パラメーター名の接頭辞
    /// </summary>
    public string ParameterPrefix { get; }
}