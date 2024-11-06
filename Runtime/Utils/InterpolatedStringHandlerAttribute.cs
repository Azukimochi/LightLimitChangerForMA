using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
internal sealed class InterpolatedStringHandlerAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
internal sealed class InterpolatedStringHandlerArgumentAttribute : Attribute
{
    public InterpolatedStringHandlerArgumentAttribute(string argument) { }
    public InterpolatedStringHandlerArgumentAttribute(params string[] arguments) { }
}
