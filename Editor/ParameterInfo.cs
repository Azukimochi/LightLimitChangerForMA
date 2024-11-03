using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace io.github.azukimochi;

internal class ParameterFieldInfo
{
    public string Name { get; }
    public Type ParameterType { get; }
    public FieldInfo FieldInfo { get; }
    public ISettings DeclaringSettings { get; }
    public GeneralControlType GeneralControlType { get; }
    public ShaderFeatureAttribute ShaderFeatureAttribute { get; }
    public VectorFieldAttribute VectorFieldAttribute { get; }
    public RangeParameterAttribute RangeParameterAttribute { get;  }
    public RangeAttribute RangeAttribute { get; }
    public DisplayOptionAttribute DisplayOptionAttribute { get; }
    public Texture2D Icon { get; }

    public ParameterFieldInfo(FieldInfo fieldInfo)
    {
        this.Name = fieldInfo.Name;
        this.ParameterType = fieldInfo.FieldType.GenericTypeArguments[0];
        this.FieldInfo = fieldInfo;

        var attributes = Attribute.GetCustomAttributes(fieldInfo);

        GeneralControlType = GetAttribute<GeneralControlAttribute>(attributes)?.Type ?? default;
        ShaderFeatureAttribute = GetAttribute<ShaderFeatureAttribute>(attributes);
        VectorFieldAttribute = GetAttribute<VectorFieldAttribute>(attributes);
        RangeParameterAttribute = GetAttribute<RangeParameterAttribute>(attributes);
        RangeAttribute = GetAttribute<RangeAttribute>(attributes);
        DisplayOptionAttribute = GetAttribute<DisplayOptionAttribute>(attributes);

        if (GetAttribute<MenuIconAttribute>(attributes) is { } iconAttr)
        {
            Icon = AssetUtils.FromGUID<Texture2D>(iconAttr.Guid);
        }

        static T GetAttribute<T>(Attribute[] attributes) where T : Attribute
        {
            foreach (var attr in attributes)
            {
                if (attr is T result)
                    return result;
            }
            return default;
        }
    }
}

internal sealed class ParameterInfo : ParameterFieldInfo
{
    public Parameter Parameter { get; }
    public Vector2 Range { get; }
    public ImmutableDictionary<string, string> MaterialProperties { get; }

    public ParameterInfo(ISettings settings, FieldInfo fieldInfo) : base(fieldInfo)
    {
        this.Parameter = fieldInfo.GetValue(settings) as Parameter;
        MaterialProperties = fieldInfo.GetCustomAttributes<MaterialPropertyNameAttribute>(false).ToImmutableDictionary(x => x.Shader, x => x.Name);

        Vector2 range = Vector2.up;
        if (RangeParameterAttribute is { } rangeParamAttr)
        {
            var val = settings.GetType().GetField(rangeParamAttr.ParameterName)?.GetValue(settings) ?? null;
            if (val is Vector2 v)
                range = v;
        }
        else if (RangeAttribute is { } rangeAttr)
        {
            range = new(rangeAttr.Min, rangeAttr.Max);
        }
        Range = range;
    }
}

internal static class ParameterCache<TSettings> where TSettings : ISettings, new()
{
    public static readonly ImmutableDictionary<string, Parameter> InitialParameters;

    static ParameterCache()
    {
        var instance = new TSettings();
        InitialParameters = instance.AllParameterFields().ToImmutableDictionary(x => x.Name, x => x.GetValue(instance) as Parameter);
    }
}
