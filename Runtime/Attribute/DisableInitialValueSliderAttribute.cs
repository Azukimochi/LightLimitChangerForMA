namespace io.github.azukimochi;

/// <summary>
/// 初期値設定用のスライダーを無効にする
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
internal sealed class DisableInitialValueSliderAttribute : Attribute { }