using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace io.github.azukimochi;

internal static class ILGeneratorExt
{
    public static void If(this ILGenerator il, Action @true)
    {
        var label = il.DefineLabel();
        il.Emit(OpCodes.Brfalse_S, label);
        @true();
        il.MarkLabel(label);
    }

    public static void GetProperty<T>(this ILGenerator il, string name)
    {
        var prop = ReflectionCache<T>.GetProperty(name);
        var method = prop.GetMethod;
        if (method.IsStatic || (typeof(T).IsSealed && !method.IsVirtual))
            il.Emit(OpCodes.Call, method);
        else
            il.Emit(OpCodes.Callvirt, method);
    }

    public static void Int(this ILGenerator il, int value)
        => il.Emit(value switch
        {
            0 => (OpCodes.Ldc_I4_0, null),
            1 => (OpCodes.Ldc_I4_1, null),
            2 => (OpCodes.Ldc_I4_2, null),
            3 => (OpCodes.Ldc_I4_3, null),
            4 => (OpCodes.Ldc_I4_4, null),
            5 => (OpCodes.Ldc_I4_5, null),
            6 => (OpCodes.Ldc_I4_6, null),
            7 => (OpCodes.Ldc_I4_7, null),
            8 => (OpCodes.Ldc_I4_8, null),
            <= byte.MaxValue
              => (OpCodes.Ldc_I4_S, value),
            _ => (OpCodes.Ldc_I4, value),
        });

    public static void Float(this ILGenerator il, float value)
        => il.Emit(OpCodes.Ldc_R4, value);

    public static void Ldstr(this  ILGenerator il, string str)
        => il.Emit(OpCodes.Ldstr, str);

    public static void NewObj<T>(this ILGenerator il, T _, BindingFlags? bindingFlags = null) where T : Delegate
        => il.NewObj<T>(bindingFlags);

    public static void NewObj<T>(this ILGenerator il, BindingFlags? bindingFlags = null) where T : Delegate
    {
        bindingFlags ??= BindingFlags.Public | BindingFlags.Instance;
        var invoke = typeof(T).GetMethod("Invoke");
        var targetType = invoke.ReturnType;
        var @params = invoke.GetParameters();

        il.Emit(OpCodes.Newobj, targetType.GetConstructor(bindingFlags.Value, null, @params.Select(x => x.ParameterType).ToArray(), null));
    }

    public static void Call<T>(this ILGenerator il, T method) where T : Delegate
    {
        var body = method.Method;
        if (body.IsStatic || (body.DeclaringType.IsSealed && !body.IsVirtual))
            il.Emit(OpCodes.Call, body);
        else
            il.Emit(OpCodes.Callvirt, body);
    }

    public static void Call<T, TDelegate>(this ILGenerator il, Expression<Func<T, TDelegate>> expression) where TDelegate : Delegate
    {
        var body = (((expression.Body as UnaryExpression)
            ?.Operand as MethodCallExpression)
            ?.Object as ConstantExpression)
            ?.Value as MethodInfo;

        if (body.DeclaringType.IsSealed && !body.IsVirtual)
            il.Emit(OpCodes.Call, body);
        else
            il.Emit(OpCodes.Callvirt, body);
    }

    public static void Ldarg(this ILGenerator il, int index)
        => il.Emit(index switch
        {
            0 => (OpCodes.Ldarg_0, null),
            1 => (OpCodes.Ldarg_1, null),
            2 => (OpCodes.Ldarg_2, null),
            3 => (OpCodes.Ldarg_3, null),
            <= byte.MaxValue 
              => (OpCodes.Ldarg_S, index),
            _ => (OpCodes.Ldarg, index),
        });

    public static void Ldarga(this ILGenerator il, int index)
        => il.Emit(index switch
        {
            <= byte.MaxValue => (OpCodes.Ldarga_S, index),
            _ => (OpCodes.Ldarga, index),
        });

    public static void Ldloc(this ILGenerator il, LocalBuilder local)
        => il.Ldloc(local.LocalIndex);

    public static void Ldloc(this ILGenerator il, int index)
        => il.Emit(index switch
        {
            0 => (OpCodes.Ldloc_0, null),
            1 => (OpCodes.Ldloc_1, null),
            2 => (OpCodes.Ldloc_2, null),
            3 => (OpCodes.Ldloc_3, null),
            <= byte.MaxValue 
              => (OpCodes.Ldloc_S, index),
            _ => (OpCodes.Ldloc, index),
        });

    public static void Ldloca(this ILGenerator il, LocalBuilder local)
        => il.Ldloca(local.LocalIndex);

    public static void Ldloca(this ILGenerator il, int index)
        => il.Emit(index switch
        {
            <= byte.MaxValue
              => (OpCodes.Ldloca_S, index),
            _ => (OpCodes.Ldloca, index),
        });

    public static void Stloc(this ILGenerator il, LocalBuilder local)
        => il.Stloc(local.LocalIndex);

    public static void Stloc(this ILGenerator il, int index)
        => il.Emit(index switch
        {
            0 => (OpCodes.Stloc_0, null),
            1 => (OpCodes.Stloc_1, null),
            2 => (OpCodes.Stloc_2, null),
            3 => (OpCodes.Stloc_3, null),
            <= byte.MaxValue
              => (OpCodes.Stloc_S, index),
            _ => (OpCodes.Stloc, index),
        });

    private static void Emit(this ILGenerator il, (OpCode Operand, int? Index) pair)
    {
        if (pair.Index == null)
        {
            il.Emit(pair.Operand);
        }
        else
        {
            il.Emit(pair.Operand, pair.Index.Value);
        }
    }

    private static class DelegateInfo<T> where T : Delegate
    {
        public static readonly MethodInfo Body = typeof(T).GetMethod("Invoke");
    }

    private static class ReflectionCache<T>
    {
        public static readonly ImmutableDictionary<string, List<MemberInfo>> Members;

        static ReflectionCache()
        {
            Members = typeof(T)
                .GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
                .GroupBy(x => x.Name, x => x)
                .Select(x => (x.Key, x.ToList()))
                .ToImmutableDictionary(x => x.Key, x => x.Item2);
        }

        public static ReadOnlySpan<MethodInfo> GetMethods(string name)
            => GetMembers<MethodInfo>(name);

        public static ReadOnlySpan<TMemberInfo> GetMembers<TMemberInfo>(string name) where TMemberInfo : MemberInfo
        {
            if (!Members.TryGetValue(name, out var list))
                return default;

            var span = list.AsSpan();
            if (span.IsEmpty || span[0] is not TMemberInfo)
                return default;

            return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<MemberInfo, TMemberInfo>(ref MemoryMarshal.GetReference(span)), span.Length);
        }

        public static PropertyInfo GetProperty(string name)
        {
            if (!Members.TryGetValue(name, out var list))
                return null;
            return list[0] as PropertyInfo;
        }
    }
}