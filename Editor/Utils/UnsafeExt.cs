using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace io.github.azukimochi;

internal static class UnsafeExt
{
    public static Span<T> AsSpan<T>(this List<T> list)
    {
        var tuple = Unsafe.As<Tuple<T[], int>>(list);
        return tuple.Item1.AsSpan(0, tuple.Item2);
    }
}