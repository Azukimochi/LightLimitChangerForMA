using System.Collections.Generic;
using System.Linq;
using nadena.dev.ndmf;
using UnityEngine;

namespace io.github.azukimochi
{
    internal static partial class Passes
    {
        internal sealed class CheckMixedShadersPass : LightLimitChangerBasePass<CheckMixedShadersPass>
        {
            protected override void Execute(BuildContext context, Session session, LightLimitChangerObjectCache cache)
            {
                var renderers = new List<(Renderer Renderer, Material Material)>();
                foreach(var renderer in session.TargetRenderers)
                {
                    if (renderer == null)
                        continue;

                    var materials = renderer.sharedMaterials;
                    if (materials.Length < 2)
                        continue;

                    for (int i = 0; i <  materials.Length; i++)
                    {
                        var material = materials[i];
                        if (material == null)
                            continue;
                        // 対象レンダラー内に対象じゃないマテリアルが存在している？🤔
                        if (!ShaderInfo.TryGetShaderInfo(material, out var shaderInfo) || !session.Parameters.TargetShaders.Contains(shaderInfo.Name))
                        {
                            renderers.Add((renderer, material));
                            break;
                        }
                    }
                }

                if (renderers.Count == 0)
                    return;

                ErrorReport.ReportError(new CustomMessage(
                    string.Join("\n", renderers.Select(x => $"{x.Renderer.name} --> {x.Material.name}")), 
                    ErrorSeverity.NonFatal, 
                    renderers.Count));
            }

            internal sealed class CustomMessage : ErrorMessage
            {
                private readonly int count;

                public CustomMessage(string details, ErrorSeverity severity, int count) : base("NDMF.info.mixedShader", severity)
                {
                    DetailsKey = details;
                    this.count = count;
                }

                public override string FormatTitle()
                {
                    var result = base.FormatTitle();
                    if (string.IsNullOrEmpty(result))
                        return result;

                    return string.Format(base.FormatTitle(), count);
                }

                public override string DetailsKey { get; }

                public override string FormatDetails() => DetailsKey;
            }
        }
    }
}
