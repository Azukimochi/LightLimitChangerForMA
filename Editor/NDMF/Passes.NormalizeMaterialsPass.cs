using System.Linq;
using nadena.dev.ndmf;
using UnityEngine;

namespace io.github.azukimochi
{
    partial class Passes
    {
        internal sealed class NormalizeMaterialsPass : LightLimitChangerBasePass<NormalizeMaterialsPass>
        {
            protected override void Execute(BuildContext context, Session session, LightLimitChangerObjectCache cache)
            {
                if (!session.Parameters.AllowColorTempControl && !session.Parameters.AllowSaturationControl)
                    return;

                foreach (var material in cache.MappedObjects.Select(x => x as Material).Where(x => x != null).ToArray())
                {
                    if (ShaderInfo.TryGetShaderInfo(material, out var shaderInfo))
                    {
                        shaderInfo.TryNormalizeMaterial(material, cache);
                    }
                }
            }
        }
    }
}
