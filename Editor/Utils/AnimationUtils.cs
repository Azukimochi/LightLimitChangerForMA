namespace io.github.azukimochi;

internal static class AnimationUtils
{
    public static void AnimateAllFloat<T>(this ReadOnlyMemory<T> memory, AnimationClip animationClip, string propertyName, AnimationCurve curve) where T : Component
        => memory.Span.AnimateAllFloat(animationClip, propertyName, curve);

    public static void AnimateAllFloat<T>(this ReadOnlySpan<T> span, AnimationClip animationClip, string propertyName, AnimationCurve curve) where T : Component
    {
        foreach(var item in span)
        {
            AnimationUtility.SetEditorCurve(animationClip, item.ToFloatCurve(propertyName), curve);
        }
    }
}