using System;
using System.Collections.Generic;
using System.Linq;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using UnityEditor.Animations;
using UnityEngine;
using VRC.Core;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace io.github.azukimochi
{
    partial class Passes
    {
        internal sealed class FinalizePass : LightLimitChangerBasePass<FinalizePass>
        {
            private GameObject _avatarObject;
            private Session _session;
            private LightLimitChangerObjectCache _cache;

            protected override void Execute(BuildContext context, Session session, LightLimitChangerObjectCache cache)
            {
                Run(context.AvatarRootObject, session, cache);
            }

            internal static void Run(GameObject avatarObject, Session session, LightLimitChangerObjectCache cache)
            {
                var obj = session.Settings?.gameObject;
                if (!session.IsValid())
                {
                    if (obj == null)
                        return;

                    var mami = obj.GetComponent<ModularAvatarMenuInstaller>();
                    if (mami != null)
                    {
                        GameObject.DestroyImmediate(mami);
                    }
                    return;
                }

                var mergeAnimator_wd = obj.AddComponent<ModularAvatarMergeAnimator>();
                var mergeAnimator = obj.AddComponent<ModularAvatarMergeAnimator>();
                var maParameters = obj.GetOrAddComponent<ModularAvatarParameters>();
                var menuInstaller = obj.GetOrAddComponent<ModularAvatarMenuInstaller>();


                var animator = new AnimatorController() { name = "LLC" }.AddTo(cache);
                mergeAnimator.animator = animator;
                mergeAnimator.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
                mergeAnimator.pathMode = MergeAnimatorPathMode.Absolute;
                mergeAnimator.matchAvatarWriteDefaults = session.Settings.WriteDefaults == WriteDefaultsSetting.MatchAvatar;

                var animatorWD = new AnimatorController() { name = "LLC WD ON" }.AddTo(cache);
                mergeAnimator_wd.animator = animatorWD;
                mergeAnimator_wd.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
                mergeAnimator_wd.pathMode = MergeAnimatorPathMode.Absolute;
                mergeAnimator_wd.matchAvatarWriteDefaults = false;

                // DirectBlendTree to AnimatorController layer
                {
                    var layer = session.DirectBlendTree.ToAnimatorControllerLayer(cache.Container);
                    layer.name = "LightLimitChanger";
                    layer.defaultWeight = 1;
                    animatorWD.AddLayer(layer);
                }

                if (session.Parameters.AddResetButton)
                {
                    animator.AddLayer("LightLimitChanger Reset");
                    var layer = animator.layers.Last();
                    layer.defaultWeight = 1;
                    var stateMachine = layer.stateMachine;

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

                    foreach (ref readonly var container in (ReadOnlySpan<ControlAnimationContainer>)session.Controls)
                    {
                        if (session.TargetControl.HasFlag(container.ControlType))
                        {
                            dr.parameters.Add(new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter() { type = VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Set, name = container.ParameterName, value = container.DefaultValue });
                        }
                    }

                    stateMachine.AddState(off, stateMachine.entryPosition + new Vector3(-20, 50));
                    stateMachine.AddState(on, stateMachine.entryPosition + new Vector3(-20, 100));

                    session.AddParameter(new ParameterConfig() { nameOrPrefix = ParameterName_Reset, syncType = ParameterSyncType.Bool, localOnly = true });
                }

                /*
                maParameters.parameters = session.AvatarParameters;
                foreach(ref var parameter in maParameters.parameters.AsSpan())
                {
                    parameter.saved = session.Parameters.IsValueSave;
                }
                */

                foreach(var parameter in session.AvatarParameters)
                {
                    var param = parameter;
                    param.saved = session.Parameters.IsValueSave;
                    param.internalParameter = true;
                    maParameters.parameters.Add(param);

                    var animatorParam = new AnimatorControllerParameter()
                    {
                        name = param.nameOrPrefix,
                        type = AnimatorControllerParameterType.Float,
                        defaultFloat = param.defaultValue,
                    };

                    animator.AddParameter(animatorParam);
                    animatorWD.AddParameter(animatorParam);
                }
                maParameters.parameters.Add(new ParameterConfig() { nameOrPrefix = "1", defaultValue = 1, localOnly = true, syncType = ParameterSyncType.NotSynced });
                animatorWD.AddParameter(new AnimatorControllerParameter() { name = "1", defaultFloat = 1, type = AnimatorControllerParameterType.Float });

                menuInstaller.menuToAppend = CreateMenu(session, cache);

                foreach (var component in avatarObject.GetComponentsInChildren<LightLimitChangerSettings>(true))
                {
                    component.Destroy();
                }
            }


            private static VRCExpressionsMenu CreateMenu(Session session, LightLimitChangerObjectCache cache)
            {
                var mainMenu = ScriptableObject.CreateInstance<VRCExpressionsMenu>().AddTo(cache);
                VRCExpressionsMenu additionalMenu = null;
                mainMenu.name = "Main Menu";
                mainMenu.controls = new List<VRCExpressionsMenu.Control>
                {
                    new VRCExpressionsMenu.Control
                    {
                        name = "Enable",
                        type = VRCExpressionsMenu.Control.ControlType.Toggle,
                        icon = Icons.Enable,
                        parameter = new VRCExpressionsMenu.Control.Parameter
                        {
                            name = ParameterName_Toggle,
                        },
                    },
                };

                foreach (ref readonly var control in session.Controls.AsSpan())
                {
                    if (!session.TargetControl.HasFlag(control.ControlType))
                        continue;

                    VRCExpressionsMenu menu;
                    if (!session.Parameters.IsGroupingAdditionalControls || !LightLimitControlType.AdditionalControls.HasFlag(control.ControlType))
                    {
                        menu = mainMenu;
                    }
                    else
                    {
                        if (additionalMenu == null)
                        {
                            additionalMenu = ScriptableObject.CreateInstance<VRCExpressionsMenu>().AddTo(cache);

                            mainMenu.controls.Add(new VRCExpressionsMenu.Control
                            {
                                name = "Controls",
                                type = VRCExpressionsMenu.Control.ControlType.SubMenu,
                                icon = Icons.Settings,
                                subMenu = additionalMenu,
                            });

                        }
                        menu = additionalMenu;
                    }

                    menu.controls.Add(new VRCExpressionsMenu.Control()
                    {
                        name = control.Name,
                        type = VRCExpressionsMenu.Control.ControlType.RadialPuppet,
                        icon = control.Icon,
                        subParameters = new[]
                        {
                            new VRCExpressionsMenu.Control.Parameter
                            {
                                name = control.ParameterName
                            }
                        },
                    });
                }

                if (session.Parameters.AddResetButton)
                {
                    mainMenu.controls.Add(new VRCExpressionsMenu.Control()
                    {
                        name = "Reset",
                        type = VRCExpressionsMenu.Control.ControlType.Button,
                        icon = Icons.Reset,
                        parameter = new VRCExpressionsMenu.Control.Parameter() { name = ParameterName_Reset }
                    });
                }

                var rootMenu = ScriptableObject.CreateInstance<VRCExpressionsMenu>().AddTo(cache);
                {
                    rootMenu.name = "Root Menu";
                    rootMenu.controls = new List<VRCExpressionsMenu.Control>
                    {
                        new VRCExpressionsMenu.Control
                        {
                            name = "Light Limit Changer",
                            icon = Icons.LLC,
                            type = VRCExpressionsMenu.Control.ControlType.SubMenu,
                            subMenu = mainMenu,
                        },
                    };
                };

                return rootMenu;
            }

        }
    }
}
