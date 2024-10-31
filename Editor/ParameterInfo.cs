using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace io.github.azukimochi;

internal sealed class ParameterInfo
{
    public string Name { get; }
    public Parameter Parameter { get; }
    public Type ParameterType { get; }
    public FieldInfo FieldInfo { get; }
    public ISettings DeclaringSettings { get; }
    public Vector2 Range { get; }
    public GeneralControlType GeneralControlType { get; }
    public ShaderFeatureAttribute ShaderFeatureAttribute { get; }
    public VectorFieldAttribute VectorFieldAttribute { get; }
    public Texture2D Icon { get; }

    public ImmutableDictionary<string, string> MaterialProperties { get; }

    public ParameterInfo(ISettings settings, FieldInfo fieldInfo)
    {
        this.Name = fieldInfo.Name;
        this.Parameter = fieldInfo.GetValue(settings) as Parameter;
        this.ParameterType = fieldInfo.FieldType.GenericTypeArguments[0];
        this.FieldInfo = fieldInfo;
        this.DeclaringSettings = settings;

        MaterialProperties = fieldInfo.GetCustomAttributes<MaterialPropertyNameAttribute>(false).ToImmutableDictionary(x => x.Shader, x => x.Name);

        Vector2 range = Vector2.up;
        if (fieldInfo.GetCustomAttribute<RangeParameterAttribute>() is { } rangeParamAttr)
        {
            var val = settings.GetType().GetField(rangeParamAttr.ParameterName)?.GetValue(settings) ?? null;
            if (val is Vector2 v)
                range = v;
        }
        else if (fieldInfo.GetCustomAttribute<RangeAttribute>() is { } rangeAttr)
        {
            range = new(rangeAttr.Min, rangeAttr.Max);
        }
        Range = range;

        GeneralControlType = fieldInfo.GetCustomAttribute<GeneralControlAttribute>()?.Type ?? default;
        ShaderFeatureAttribute = fieldInfo.GetCustomAttribute<ShaderFeatureAttribute>();
        VectorFieldAttribute = fieldInfo.GetCustomAttribute<VectorFieldAttribute>();

        if (fieldInfo.GetCustomAttribute<MenuIconAttribute>() is { } iconAttr)
        {
            Icon = AssetUtils.FromGUID<Texture2D>(iconAttr.Guid);
        }
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