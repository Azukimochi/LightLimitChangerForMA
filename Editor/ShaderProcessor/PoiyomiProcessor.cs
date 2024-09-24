
using nadena.dev.ndmf;

namespace io.github.azukimochi;

internal sealed class PoiyomiProcessor : ShaderProcessor
{
    public override string QualifiedName => BuiltinSupportedShaders.Poiyomi;

    public override bool IsTargetMaterial(Material material)
    {
        var shaderName = material.shader.name;
        bool isPoiyomi = shaderName.Contains("poiyomi", StringComparison.OrdinalIgnoreCase);
        if (!isPoiyomi)
            return false;

        if (shaderName.Contains("7.3"))
        {
            // 古くてパラメーターが違うのでサポートしない、したくない；
            ErrorMessage.Throw("error/poiyomi/old-version");
            return false; 
        }

        return true;
    }
}
