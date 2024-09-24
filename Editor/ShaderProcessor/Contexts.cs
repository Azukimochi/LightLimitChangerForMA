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
    public string Name { get; init; }
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