using System;
using gomoru.su;
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

                foreach (ref readonly var container in animationContainers)
                {
                    if (session.TargetControl.HasFlag(container.ControlType))
                    {
                        container.AddTo(cache);
                        var puppet = animationTree.AddRadialPuppet(container.Name);
                        puppet.ParameterName = container.ParameterName;
                        puppet.Animation = container.Control;

                        controller.AddParameter(new AnimatorControllerParameter() { name = container.ParameterName, defaultFloat = container.DefaultValue, type = AnimatorControllerParameterType.Float });
                    }
                }

                if (!session.Parameters.AddResetButton)
                    return;

                AnimatorStateMachine stateMachine;
                var layer = new AnimatorControllerLayer()
                {
                    name = "Reset",
                    stateMachine = stateMachine = new AnimatorStateMachine().HideInHierarchy().AddTo(cache),
                    defaultWeight = 1,
                };
                var blank = new AnimationClip() { name = "Blank" }.HideInHierarchy().AddTo(cache);
                var off = new AnimatorState() { name = "Off", writeDefaultValues = session.Settings.WriteDefaults == WriteDefaultsSetting.ON, motion = blank }.HideInHierarchy().AddTo(cache);
                var on = new AnimatorState() { name = "On", writeDefaultValues = session.Settings.WriteDefaults == WriteDefaultsSetting.ON, motion = blank }.HideInHierarchy().AddTo(cache);

                var cond = new AnimatorCondition[] { new AnimatorCondition() { mode = AnimatorConditionMode.If, parameter = ParameterName_Reset } };

                var t = new AnimatorStateTransition()
                {
                    destinationState = on,
                    duration = 0,
                    hasExitTime = false,
                    conditions = cond
                }.HideInHierarchy().AddTo(cache);

                off.AddTransition(t);

                cond[0].mode = AnimatorConditionMode.IfNot;
                t = new AnimatorStateTransition()
                {
                    destinationState = off,
                    duration = 0,
                    hasExitTime = false,
                    conditions = cond
                }.HideInHierarchy().AddTo(cache);

                on.AddTransition(t);

                var dr = on.AddStateMachineBehaviour<VRCAvatarParameterDriver>();

                foreach (ref readonly var container in animationContainers)
                {
                    if (session.TargetControl.HasFlag(container.ControlType))
                    {
                        dr.parameters.Add(new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter() { type = VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Set, name = container.ParameterName, value = container.DefaultValue });
                    }
                }

                stateMachine.AddState(off, stateMachine.entryPosition + new Vector3(-20, 50));
                stateMachine.AddState(on, stateMachine.entryPosition + new Vector3(-20, 100));

                session.Controller.AddParameter(ParameterName_Reset, AnimatorControllerParameterType.Bool);
            }
        }
    }
}
