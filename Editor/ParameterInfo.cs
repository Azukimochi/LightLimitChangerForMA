using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace io.github.azukimochi;

internal sealed class ParameterInfo
{
    public string Name { get; }
    public Parameter Parameter { get; }
    public Type ParameterType { get; }
    public FieldInfo FieldInfo { get; }
    public ISettings DeclaringSettings { get; }

    public ImmutableDictionary<string, string> MaterialProperties { get; }

    public ParameterInfo(ISettings settings, FieldInfo fieldInfo)
    {
        this.Name = fieldInfo.Name;
        this.Parameter = fieldInfo.GetValue(settings) as Parameter;
        this.ParameterType = fieldInfo.FieldType.GenericTypeArguments[0];
        this.FieldInfo = fieldInfo;
        this.DeclaringSettings = settings;

        MaterialProperties = fieldInfo.GetCustomAttributes<MaterialPropertyNameAttribute>(false).ToImmutableDictionary(x => x.Shader, x => x.Name);
    }
}