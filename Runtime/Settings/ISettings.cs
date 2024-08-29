namespace io.github.azukimochi;

internal interface ISettings
{
    /// <summary>
    /// パラメーター名の接頭辞
    /// </summary>
    string ParameterPrefix { get; }

    /// <summary>
    /// メニューの表示名
    /// </summary>
    string DisplayName { get; }
}
