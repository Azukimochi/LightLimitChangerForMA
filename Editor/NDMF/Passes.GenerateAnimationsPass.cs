using System;
using gomoru.su;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using nadena.dev.ndmf.util;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace io.github.azukimochi
{
    partial class Passes
    {
        internal sealed class GenerateAnimationsPass : LightLimitChangerBasePass<GenerateAnimationsPass>
        {
            protected override void Execute(BuildContext context, Session session, LightLimitChangerObjectCache cache) => Run(session, cache);

            internal static void Run(Session session, LightLimitChangerObjectCache cache)
            {
                ReadOnlySpan<ControlAnimationContainer> animationContainers = session.Controls;
                var parameters = session.Parameters;

                foreach (var renderer in session.TargetRenderers)
                {
                    if (session.Excludes.Contains(renderer.gameObject))
                    {
                        continue;
                    }

                    var relativePath = renderer.AvatarRootPath();
                    var type = renderer.GetType();

                    foreach (var x in ShaderInfo.RegisteredShaderInfos)
                    {
                        if (!parameters.TargetShaders.Contains(x.Name))
                            continue;

                        foreach (ref readonly var container in animationContainers)
                        {
                            if (!(!parameters.OverwriteDefaultLightMinMax &&
                                  (renderer.sharedMaterials?.Length ?? 0) > 0 &&
                                  renderer.sharedMaterials[0] is Material mat &&
                                  x.IsTargetShader(mat?.shader) &&
                                  x.TryGetLightMinMaxValue(mat, out var min, out var max)))
                            {
                                min = parameters.MinLightValue;
                                max = parameters.MaxLightValue;
                            }

                            x.SetControlAnimation(container, new ControlAnimationParameters(relativePath, type, min, max));
                        }
                    }
                }

                
                var toggleTree = session.DirectBlendTree.AddAndGate("Enable");
                toggleTree.OFF = session.Controls[0].Default;
                toggleTree.Parameters = new[] { ParameterName_Toggle };
                var animationTree = toggleTree.AddDirectBlendTree(DirectBlendTree.Target.ON, "Animation");

                session.AddParameter(new ParameterConfig() { nameOrPrefix = ParameterName_Toggle, defaultValue = parameters.IsDefaultUse ? 1 : 0, syncType = ParameterSyncType.Bool });

                foreach (ref readonly var container in animationContainers)
                {
                    if (session.TargetControl.HasFlag(container.ControlType))
                    {
                        container.AddTo(cache);
                        var puppet = animationTree.AddRadialPuppet(container.Name);
                        puppet.ParameterName = container.ParameterName;
                        puppet.Animation = container.Control;

                        session.AddParameter(new ParameterConfig() { nameOrPrefix = container.ParameterName, defaultValue = container.DefaultValue, syncType = ParameterSyncType.Float });
                    }
                }
            }
        }
    }
}
