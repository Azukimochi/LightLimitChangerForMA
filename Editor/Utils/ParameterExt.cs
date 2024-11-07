using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using nadena.dev.ndmf;
using NUnit.Framework.Interfaces;
using UnityEngine;

namespace io.github.azukimochi;

internal static class ParameterExt
{
    public static int GetValues(this Parameter parameter, Span<float> destination)
    {
        return parameter switch
        {
            Parameter<int> x => Int32(x, destination),
            Parameter<bool> x => Bool(x, destination),
            Parameter<float> x => Single(x, destination),
            Parameter<Vector4> x => Vector4OrColor(x, destination),
            Parameter<Color> x => Vector4OrColor(x, destination),
            _ => 0,
        };

        static int Int32(Parameter<int> parameter, Span<float> destination)
        {
            destination[0] = parameter.Value;
            return 1;
        }
        static int Bool(Parameter<bool> parameter, Span<float> destination)
        {
            destination[0] = parameter.Value ? 1 : 0;
            return 1;
        }
        static int Single(Parameter<float> parameter, Span<float> destination)
        {
            destination[0] = parameter.Value;
            return 1;
        }
        static int Vector4OrColor<T>(Parameter<T> parameter, Span<float> destination)
        {
            var value = parameter.Value;
            MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, float>(ref value), 4).CopyTo(destination);
            return 4;
        }
    }

    internal static T GetValueDirect<T>(this Parameter parameter) => (parameter as Parameter<T>).Value;

    /// <summary>
    /// 操作対象のパラメーター名を取得する<br/>
    /// ParameterにMaterialPropertyNameAttr. が付いてればそっちから、なかったらProcessorに問い合わせる
    /// </summary>
    /// <param name="parameterInfo"></param>
    /// <param name="processor"></param>
    /// <returns></returns>
    public static string GetPropertyName(this ParameterInfo parameterInfo, ShaderProcessor processor)
    {
        if (!parameterInfo.MaterialProperties.TryGetValue(processor.QualifiedName, out var propertyName))
            propertyName = processor.GetMaterialPropertyName(parameterInfo);
        return propertyName;
    }

    internal static IEnumerable<FieldInfo> AllParameterFields<TSettings>(this TSettings _) where TSettings : ISettings
    {
        var fields = typeof(TSettings).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        return fields.Where(x => typeof(Parameter).IsAssignableFrom(x.FieldType) && x.IsPublic || (x.IsPrivate && x.GetCustomAttribute<SerializeField>() != null));
    }

    public static string GetName(this ParameterInfo parameterInfo, int index = 0)
    {
        string postfix;
        var t = parameterInfo.ParameterType;
        if (t == typeof(Vector4) || t == typeof(Color))
        {
            postfix = t == typeof(Vector4) ? $".{"xyzw"[index]}" : $".{"rgba"[index]}";
        }
        else
        {
            postfix = "";
        }
        return $"{parameterInfo.Name}{postfix}";
    }
}