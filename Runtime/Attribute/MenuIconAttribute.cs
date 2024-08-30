namespace io.github.azukimochi;

/// <summary>
/// メニューに使用するアイコンを設定する
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field)]
internal sealed class MenuIconAttribute : Attribute
{
    /// <param name="guid">テクスチャのGUID</param>
    public MenuIconAttribute(string guid) => Guid = guid;

    public string Guid { get; }
}
