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

internal struct ConfigureGeneralAnimationContext
{
    public string Name;
    public Parameter<float> Parameter;
    public AnimationClip AnimationClip;
    public ReadOnlyMemory<Renderer> Renderers;
    public Vector2? Range;
    public GeneralControlType Type;
}

internal struct ConfigureShaderSpecificAnimationContext
{
    public string Name;
    public Parameter<float> Parameter;
    public AnimationClip AnimationClip;
    public ReadOnlyMemory<Renderer> Renderers;
    public Vector2? Range;
}
