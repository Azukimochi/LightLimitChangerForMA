using nadena.dev.modular_avatar.core;
using System.Collections.Generic;

namespace io.github.azukimochi;

internal struct CreateShaderSpecificControlContext 
{
    public string Name;
    public Parameter<float> Parameter;
    public ModularAvatarMenuItem RootMenu;
    public ModularAvatarMenuItem ParentMenu;
    public List<ParameterConfig> AvatarParameters;
}

internal abstract class ConfigureAnimationContextBase
{
    public ParameterInfo ParameterInfo { get; init; }
    public string PropertyName { get; set; }
    public AnimationClip AnimationClip { get; init; }
    public ReadOnlyMemory<Renderer> Renderers { get; init; }
}

internal class ConfigureEmptyAnimationContext : ConfigureAnimationContextBase
{
    public GeneralControlType Type { get; init; }
    public float Value { get; set; }
}

internal class ConfigureShaderSpecificAnimationContext : ConfigureAnimationContextBase
{
    public ParameterConfig AvatarParameter { get; init; }
    public Vector2 Range { get; set; }
}

internal sealed class ConfigureGeneralAnimationContext : ConfigureShaderSpecificAnimationContext
{
    public GeneralControlType Type { get; init; }
}

internal struct OverrideMaterialValueContext
{
    public ParameterInfo ParameterInfo;
    public string PropertyName;
    public Material[] Materials;
}