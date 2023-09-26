using System.Linq;
using nadena.dev.ndmf;
using UnityEngine;

namespace io.github.azukimochi
{
    partial class Passes
    {
        internal sealed class GenerateAdditionalControlPass : LightLimitChangerBasePass<GenerateAdditionalControlPass>
        {
            protected override void Execute(BuildContext context, Session session, LightLimitChangerObjectCache cache)
            {
                foreach (var material in cache.MappedObjects.Select(x => x as Material).Where(x => x != null).ToArray())
                {
                    if (ShaderInfo.TryGetShaderInfo(material, out var shaderInfo))
                    {
                        shaderInfo.AdditionalControl(material, session.Parameters);
                    }
                }
            }
        }
    }
}
