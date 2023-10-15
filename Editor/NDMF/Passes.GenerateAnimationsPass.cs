using System;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using nadena.dev.ndmf.util;
using UnityEditor.Animations;
using UnityEngine;
using VRC.Core;
using VRC.SDK3.Avatars.Components;

namespace io.github.azukimochi
{
    partial class Passes
    {
        internal sealed class GenerateAnimationsPass : LightLimitChangerBasePass<GenerateAnimationsPass>
        {
            protected override void Execute(BuildContext context, Session session, LightLimitChangerObjectCache cache)
            {
                var controller = session.Controller;

                ReadOnlySpan<ControlAnimationContainer> animationContainers = session.Controls;

                var parameters = session.Parameters;

                foreach (var renderer in context.AvatarRootObject.GetComponentsInChildren<Renderer>(true))
                {
                    if (!(renderer is MeshRenderer || renderer is SkinnedMeshRenderer) || session.Excludes.Contains(renderer.gameObject)) 
                    {
                        continue;
                    }

                    var relativePath = renderer.AvatarRootPath();
                    var type = renderer.GetType();

                    var controlParameters = new ControlAnimationParameters(relativePath, type, parameters.MinLightValue, parameters.MaxLightValue);

                    foreach (var x in ShaderInfo.RegisteredShaderInfos)
                    {
                        if (!parameters.TargetShaders.Contains(x.Name))
                            continue;

                        foreach (ref readonly var container in animationContainers)
                        {
                            x.SetControlAnimation(container, controlParameters);
                        }
                    }
                }

                controller.AddParameter(new AnimatorControllerParameter() { name = ParameterName_Toggle, defaultBool = parameters.IsDefaultUse, type = AnimatorControllerParameterType.Bool });
                
                foreach (ref readonly var container in animationContainers)
                {
                    if (session.TargetControl.HasFlag(container.ControlType))
                    {
                        container.AddTo(cache);
                        AddLayer(session, cache, container, container.ParameterName);

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
                var off = new AnimatorState() { name = "Off", writeDefaultValues = false, motion = blank }.HideInHierarchy().AddTo(cache);
                var on = new AnimatorState() { name = "On", writeDefaultValues = false, motion = blank }.HideInHierarchy().AddTo(cache);

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

                session.Controller.AddLayer(layer);
            }

            private static void AddLayer(Session session, LightLimitChangerObjectCache cache, ControlAnimationContainer container, string parameterName)
            {
                var layer = new AnimatorControllerLayer() { name = container.Name, defaultWeight = 1, stateMachine = new AnimatorStateMachine().HideInHierarchy().AddTo(cache) };
                var stateMachine = layer.stateMachine;
                var defaultState = new AnimatorState() { name = "Default", writeDefaultValues = false, motion = container.Default }.HideInHierarchy().AddTo(cache);
                var state = new AnimatorState() { name = "Control", writeDefaultValues = false, motion = container.Control, timeParameterActive = true, timeParameter = parameterName }.HideInHierarchy().AddTo(cache);

                var condition = new AnimatorCondition[] { new AnimatorCondition() { parameter = ParameterName_Toggle, mode = AnimatorConditionMode.If, threshold = 0 } };

                var tr = new AnimatorStateTransition()
                {
                    destinationState = state,
                    duration = 0,
                    hasExitTime = false,
                    conditions = condition,
                }.HideInHierarchy().AddTo(cache);

                defaultState.AddTransition(tr);

                condition[0].mode = AnimatorConditionMode.IfNot;
                tr = new AnimatorStateTransition()
                {
                    destinationState = defaultState,
                    duration = 0,
                    hasExitTime = false,
                    conditions = condition,
                }.HideInHierarchy().AddTo(cache);

                state.AddTransition(tr);

                stateMachine.AddState(defaultState, stateMachine.entryPosition + new Vector3(-20, 50));
                stateMachine.AddState(state, stateMachine.entryPosition + new Vector3(-20, 100));

                session.Controller.AddLayer(layer);
            }
        }
    }
}
