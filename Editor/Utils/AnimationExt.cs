using System.Runtime.Remoting.Contexts;
using nadena.dev.ndmf.util;

namespace io.github.azukimochi;

internal static class AnimationExt
{
    public static EditorCurveBinding ToPPtrCurve(this GameObject obj, string propertyName)
        => EditorCurveBinding.PPtrCurve(obj.AvatarRootPath(), obj.GetType(), propertyName);

    public static EditorCurveBinding ToPPtrCurve<T>(this T obj, string propertyName) where T : Component 
        => EditorCurveBinding.PPtrCurve(obj.AvatarRootPath(), obj.GetType(), propertyName);

    public static EditorCurveBinding ToFloatCurve(this GameObject obj, string propertyName)
        => EditorCurveBinding.FloatCurve(obj.AvatarRootPath(), obj.GetType(), propertyName);

    public static EditorCurveBinding ToFloatCurve<T>(this T obj, string propertyName) where T : Component
        => EditorCurveBinding.FloatCurve(obj.AvatarRootPath(), obj.GetType(), propertyName);
}

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