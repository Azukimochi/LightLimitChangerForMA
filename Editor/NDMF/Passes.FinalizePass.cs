using System.Collections.Generic;
using System.Linq;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
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
            protected override void Execute(BuildContext context, Session session, LightLimitChangerObjectCache cache)
            {
                var obj = session.Settings.gameObject;
                var mergeAnimator = obj.GetOrAddComponent<ModularAvatarMergeAnimator>();
                var maParameters = obj.GetOrAddComponent<ModularAvatarParameters>();
                var menuInstaller = obj.GetOrAddComponent<ModularAvatarMenuInstaller>();

                mergeAnimator.animator = session.Controller;
                mergeAnimator.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
                mergeAnimator.pathMode = MergeAnimatorPathMode.Absolute;
                mergeAnimator.matchAvatarWriteDefaults = true;

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

                foreach(var component in context.AvatarRootObject.GetComponentsInChildren<LightLimitChangerSettings>(true))
                {
                    component.Destroy();
                }
            }


            private static VRCExpressionsMenu CreateMenu(Session session, LightLimitChangerObjectCache cache)
            {
                var mainMenu = ScriptableObject.CreateInstance<VRCExpressionsMenu>().AddTo(cache);
                mainMenu.name = "Main Menu";
                mainMenu.controls = new List<VRCExpressionsMenu.Control>
                {
                    new VRCExpressionsMenu.Control
                    {
                        name = "Enable",
                        type = VRCExpressionsMenu.Control.ControlType.Toggle,
                        parameter = new VRCExpressionsMenu.Control.Parameter
                        {
                            name = ParameterName_Toggle,
                        },
                    },
                    new VRCExpressionsMenu.Control
                    {
                        name = "Light",
                        type = VRCExpressionsMenu.Control.ControlType.RadialPuppet,
                        subParameters = new VRCExpressionsMenu.Control.Parameter[]
                        {
                            new VRCExpressionsMenu.Control.Parameter
                            {
                                name = ParameterName_Value
                            }
                        },
                    },
                };

                if (session.Parameters.AllowColorTempControl)
                {
                    mainMenu.controls.Add(new VRCExpressionsMenu.Control()
                    {
                        name = "ColorTemperature",
                        type = VRCExpressionsMenu.Control.ControlType.RadialPuppet,
                        subParameters = new VRCExpressionsMenu.Control.Parameter[]
                        {
                        new VRCExpressionsMenu.Control.Parameter
                        {
                            name = ParameterName_ColorTemp
                        }
                        },
                    });
                }

                if (session.Parameters.AllowSaturationControl)
                {
                    mainMenu.controls.Add(new VRCExpressionsMenu.Control()
                    {
                        name = "Saturation",
                        type = VRCExpressionsMenu.Control.ControlType.RadialPuppet,
                        subParameters = new VRCExpressionsMenu.Control.Parameter[]
                        {
                        new VRCExpressionsMenu.Control.Parameter
                        {
                            name = ParameterName_Saturation
                        }
                        },
                    });
                }

                if (session.Parameters.AllowUnlitControl)
                {
                    mainMenu.controls.Add(new VRCExpressionsMenu.Control()
                    {
                        name = "Unlit",
                        type = VRCExpressionsMenu.Control.ControlType.RadialPuppet,
                        subParameters = new VRCExpressionsMenu.Control.Parameter[]
                        {
                        new VRCExpressionsMenu.Control.Parameter
                        {
                            name = ParameterName_Unlit
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
