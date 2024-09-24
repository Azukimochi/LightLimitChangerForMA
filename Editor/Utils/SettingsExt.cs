using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace io.github.azukimochi;

internal static class SettingsExt
{
    public static bool Any<TSettings>(this TSettings settings, Func<Parameter, bool> predicate) where TSettings : ISettings
    {
        foreach (var parameter in settings.EnumerateParameters())
        {
            if (predicate(parameter))
                return true;
        }
        return false;
    }

    public static ParameterIterator<TSettings> EnumerateParameters<TSettings>(this TSettings settings) where TSettings : ISettings
        => new (settings);

    public ref struct ParameterIterator<TSettings> where TSettings : ISettings
    {
        private readonly TSettings instance;
        private int index;

        public ParameterIterator(TSettings settings)
        {
            instance = settings;
            index = -1;
        }

        public bool MoveNext()
        {
            if (index + 1 >= EnumerateParametersCache<TSettings>.ParameterCount)
                return false;

            index++;
            return true;
        }

        public readonly Parameter Current => EnumerateParametersCache<TSettings>.GetParameterByIndex(instance, index);

        public readonly ParameterIterator<TSettings> GetEnumerator() => this;
    }

    private static class EnumerateParametersCache<TSettings> where TSettings : ISettings
    {
        public delegate Parameter GetParameterByIndexDelegate(TSettings settings, int index);
        public static GetParameterByIndexDelegate GetParameterByIndex { get; }
        public static int ParameterCount { get; }

        static EnumerateParametersCache()
        {
            var type = typeof(TSettings);
            var parameters = type.GetFields(BindingFlags.Instance | BindingFlags.Public).Where(x => typeof(Parameter).IsAssignableFrom(x.FieldType)).ToArray();
            ParameterCount = parameters.Length;

            // Parameter GetParameterByIndex(TSettings settings, int index)
            var method = new DynamicMethod(nameof(GetParameterByIndex), typeof(Parameter), new[] { typeof(TSettings), typeof(int) });
            var il = method.GetILGenerator();

            il.DeclareLocal(typeof(Parameter));

            var label_def = il.DefineLabel();
            var label_end = il.DefineLabel();
            var label_cases = new Label[parameters.Length];
            foreach (ref var label in label_cases.AsSpan())
            {
                label = il.DefineLabel();
            }

            // switch (index)
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Switch, label_cases);

            il.Emit(OpCodes.Br_S, label_def);

            // case {index}: return settings.{index-th parameter};
            for (int i = 0; i < label_cases.Length; i++)
            {
                Label label = label_cases[i];
                il.MarkLabel(label);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, parameters[i]);
                il.Emit(OpCodes.Stloc_0);
                il.Emit(OpCodes.Br_S, label_end);
            }

            // default: return null;
            il.MarkLabel(label_def);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Stloc_0);

            // end
            il.MarkLabel(label_end);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);

            GetParameterByIndex = method.CreateDelegate(typeof(GetParameterByIndexDelegate)) as GetParameterByIndexDelegate;
        }
    }
}