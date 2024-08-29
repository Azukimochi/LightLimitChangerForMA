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

internal class ConfigureShaderSpecificAnimationContext
{
    public string Name { get; init; }
    public ParameterConfig AvatarParameter { get; init; }
    public AnimationClip AnimationClip { get; init; }
    public ReadOnlyMemory<Renderer> Renderers { get; init; }
    public Vector2 Range { get; init; }
}

internal sealed class ConfigureGeneralAnimationContext : ConfigureShaderSpecificAnimationContext
{
    public GeneralControlType Type { get; init; }
}