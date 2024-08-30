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
