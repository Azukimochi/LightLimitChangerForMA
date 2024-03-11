using System;
using System.Collections.Generic;
using System.Linq;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using UnityEditor;
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
                var obj = session.Settings.gameObject;
                var mergeAnimator = obj.GetOrAddComponent<ModularAvatarMergeAnimator>();
                var maParameters = obj.GetOrAddComponent<ModularAvatarParameters>();
                var menuInstaller = obj.GetOrAddComponent<ModularAvatarMenuInstaller>();

                var layer = session.DirectBlendTree.ToAnimatorControllerLayer(cache.Container);
                layer.name = "LightLimitChanger";
                layer.defaultWeight = 1;
                session.Controller.AddLayer(layer);
                session.Controller.AddParameter(new AnimatorControllerParameter() { name = session.DirectBlendTree.ParameterName, defaultInt = 1, type = AnimatorControllerParameterType.Float });

                mergeAnimator.animator = session.Controller;
                mergeAnimator.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
                mergeAnimator.pathMode = MergeAnimatorPathMode.Absolute;
                mergeAnimator.matchAvatarWriteDefaults = session.Settings.WriteDefaults == WriteDefaultsSetting.MatchAvatar;

                maParameters.parameters.AddRange(session.Controller.parameters.Select(x => new ParameterConfig()
                {
                    nameOrPrefix = x.name,
                    internalParameter = true,
                    syncType = x.type == AnimatorControllerParameterType.Float ? ParameterSyncType.Float : ParameterSyncType.Bool,
                    defaultValue = x.type == AnimatorControllerParameterType.Float ? x.defaultFloat : (x.defaultBool ? 1 : 0),
                    saved = session.Parameters.IsValueSave,
                    localOnly = x.name == ParameterName_Reset
                }));

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
