#if UNITY_EDITOR

using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.Core;
using nadena.dev.modular_avatar.core;
using System.Linq;

namespace io.github.azukimochi
{
    public class LightLimitChanger : EditorWindow
    {
        public bool IsDefaultUse = false;
        public bool IsValueSave = false;
        public float DefaultLightValue = 0.5f;
        public float MaxLightValue = 1.0f;
        public float MinLightValue = 0.0f;
        public VRCAvatarDescriptor TargetAvatar;

        const string SHADER_KEY_LightMinLimit = "_LightMinLimit";
        const string SHADER_KEY_LightMaxLimit = "_LightMaxLimit";
        const string ParameterName_Toggle = "LightLimitEnable";
        const string ParameterName_Value = "LightLimitValue";
        const string ArtifactsFolderGUID = "f60eb27a31a2bb640a2b06ae64950203";

        private string infoLabel = "";

        [MenuItem("Tools/Modular Avatar/LightLimitChanger")]
        static void CreateGUI()
        {
            GetWindow<LightLimitChanger>("LightLimitChanger");
        }
        private void OnGUI()
        {
            GUILayout.Label("---- Select Avater / アバターを選択");
            TargetAvatar = EditorGUILayout.ObjectField(" Avater", TargetAvatar, typeof(VRCAvatarDescriptor), true) as VRCAvatarDescriptor;
            EditorGUILayout.Space();
            GUILayout.Label("---- Paramater / パラメータ");
            IsDefaultUse = EditorGUILayout.Toggle(" DefaultUse", IsDefaultUse);
            IsValueSave = EditorGUILayout.Toggle(" SaveValue", IsValueSave);
            MaxLightValue = EditorGUILayout.FloatField(" MaxLight", MaxLightValue);
            MinLightValue = EditorGUILayout.FloatField(" MinLight", MinLightValue);
            DefaultLightValue = EditorGUILayout.FloatField(" DefaultLight", DefaultLightValue);

            EditorGUI.BeginDisabledGroup(TargetAvatar == null);
            if (GUILayout.Button(" Generate / 生成 "))
            {
                infoLabel = "生成中・・・";
                try
                {
                    GenerateAssets();
                    infoLabel = "生成終了";
                }
                catch (Exception e)
                {
                    infoLabel = $"エラー: {e.Message}";
                }
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.Label(infoLabel);

        }

        private void GenerateAssets()
        {
            var folder = AssetDatabase.GUIDToAssetPath(ArtifactsFolderGUID);
            if (folder == null)
            {
                folder = "Assets/LightLimitChangerForMA/Artifacts/";
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    AssetDatabase.CreateFolder("Assets/LightLimitChangerForMA", "Artifacts");
                }
            }
            if (!AssetDatabase.IsValidFolder(folder))
            {
                throw new System.IO.DirectoryNotFoundException($"Artifacts folder is not found");
            }

            var avatar = TargetAvatar;

            var fileName = $"{TargetAvatar.name}_{DateTime.Now:yyyyMMddHHmmss}_{GUID.Generate()}.controller";
            var fx = new AnimatorController() { name = System.IO.Path.GetFileName(fileName) };
            AssetDatabase.CreateAsset(fx, System.IO.Path.Combine(folder, fileName));
            AnimationClip defaultAnim = null;
            AnimationClip anim = null;

            var linearCurve = AnimationCurve.Linear(0, MinLightValue, 1 / 60f, MaxLightValue);

            foreach (var renderer in avatar.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer is MeshRenderer || renderer is SkinnedMeshRenderer)
                {
                    var type = renderer.GetType();
                    bool isLilToon = false;

                    (float Min, float Max) defaultValue = (0, 1);

                    foreach(var material in renderer.sharedMaterials)
                    {
                        var shader = material?.shader;
                        if (shader != null)
                        {
                            int count = shader.GetPropertyCount();
                            for(int i = 0; i < count; i++)
                            {
                                var propertyName = shader.GetPropertyName(i);
                                if (propertyName == SHADER_KEY_LightMinLimit)
                                {
                                    isLilToon = true;
                                    defaultValue.Min = material.GetFloat(propertyName);
                                }
                                if (propertyName == SHADER_KEY_LightMaxLimit)
                                {
                                    isLilToon = true;
                                    defaultValue.Max = material.GetFloat(propertyName);
                                }
                            }
                        }
                        if (isLilToon)
                            break;
                    }
                    if (isLilToon)
                    {
                        if (defaultAnim == null || anim == null)
                        {
                            defaultAnim = new AnimationClip().AddTo(fx);
                            anim = new AnimationClip().AddTo(fx);
                        }

                        var relativePath = renderer.transform.GetRelativePath(avatar.transform);
                        defaultAnim.SetCurve(relativePath, type, $"material.{SHADER_KEY_LightMinLimit}", AnimationCurve.Constant(0, 0, defaultValue.Min));
                        defaultAnim.SetCurve(relativePath, type, $"material.{SHADER_KEY_LightMaxLimit}", AnimationCurve.Constant(0, 0, defaultValue.Max));
                        anim.SetCurve(relativePath, type, $"material.{SHADER_KEY_LightMinLimit}", linearCurve);
                        anim.SetCurve(relativePath, type, $"material.{SHADER_KEY_LightMaxLimit}", linearCurve);
                    }
                }
            }

            if (defaultAnim == null || anim == null)
            {
                return;
            }

            var layer = new AnimatorControllerLayer() { name = "Light", defaultWeight = 1, stateMachine = new AnimatorStateMachine().HideInHierarchy().AddTo(fx) };
            var stateMachine = layer.stateMachine;
            var defaultState = new AnimatorState() { name = "Default", writeDefaultValues = false, motion = defaultAnim };
            var state = new AnimatorState() { name = "Control", writeDefaultValues = false, motion = anim, timeParameterActive = true, timeParameter = ParameterName_Value };

            var condition = new AnimatorCondition[] { new AnimatorCondition() { parameter = ParameterName_Toggle, mode = AnimatorConditionMode.If, threshold = 0 } };

            var tr = new AnimatorStateTransition()
            {
                destinationState = state,
                duration = 0,
                hasExitTime = false,
                conditions = condition,
            }.HideInHierarchy().AddTo(fx);

            defaultState.AddTransition(tr);

            condition[0].mode = AnimatorConditionMode.IfNot;
            tr = new AnimatorStateTransition()
            {
                destinationState = defaultState,
                duration = 0,
                hasExitTime = false,
                conditions = condition,
            }.HideInHierarchy().AddTo(fx);

            state.AddTransition(tr);

            stateMachine.AddState(defaultState, stateMachine.entryPosition + new Vector3(0, 200));
            stateMachine.AddState(state, stateMachine.entryPosition + new Vector3(0, 350));

            fx.AddLayer(layer);
            fx.AddParameter(new AnimatorControllerParameter() { name = ParameterName_Toggle, defaultBool = IsDefaultUse, type = AnimatorControllerParameterType.Bool });
            fx.AddParameter(new AnimatorControllerParameter() { name = ParameterName_Value, defaultFloat = DefaultLightValue, type = AnimatorControllerParameterType.Float });

            //////////////////////
            //ExpressionMenuの生成

            //SubMenuの生成
            var subMenu = new VRCExpressionsMenu
            {
                controls = new List<VRCExpressionsMenu.Control>
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
                },
            }.AddTo(fx);

            //Menuの生成
            var menu = new VRCExpressionsMenu()
            {
                controls = new List<VRCExpressionsMenu.Control>
                {
                    new VRCExpressionsMenu.Control 
                    {
                        name = "Light Limit Changer",
                        type = VRCExpressionsMenu.Control.ControlType.SubMenu,
                        subMenu = new VRCExpressionsMenu
                        {
                            controls = new List<VRCExpressionsMenu.Control>
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
                            },
                        }.AddTo(fx),
                    },
                },
            }.AddTo(fx);


        }

        private void GenerateAssets(string savePath, string saveName, GameObject avater)
        {
            /////////////////////
            //アニメーションの生成
            var animLightChangeClip = new AnimationClip();
            var animLightDisableClip = new AnimationClip();

            //アニメーションをまとめて生成
            createAnimationClip(avater, animLightChangeClip, animLightDisableClip, avater.name);
            AssetDatabase.CreateAsset(animLightChangeClip, $"{savePath}_active.anim");
            AssetDatabase.CreateAsset(animLightDisableClip, $"{savePath}_deactive.anim");

            //////////////////////////////////
            //アニメーターコントローラーの生成
            var controller = AnimatorController.CreateAnimatorControllerAtPath($"{savePath}.controller");

            controller.parameters = new AnimatorControllerParameter[]
            {
            new AnimatorControllerParameter()
            {
                name = saveName,
                type = AnimatorControllerParameterType.Bool,
                defaultBool = this.IsDefaultUse
            },
            new AnimatorControllerParameter()
            {
                name = saveName + "_motion",
                type = AnimatorControllerParameterType.Float,
                defaultFloat = this.DefaultLightValue
            }
            };
            //レイヤーとステートの設定
            var layer = controller.layers[0];
            layer.name = saveName;
            layer.stateMachine.name = saveName;

            var lightActiveState = layer.stateMachine.AddState($"{saveName}_Active", new Vector3(300, 200));
            lightActiveState.motion = animLightChangeClip;
            lightActiveState.writeDefaultValues = false;
            lightActiveState.timeParameterActive = true;
            lightActiveState.timeParameter = saveName + "_motion";

            var lightDeActiveState = layer.stateMachine.AddState($"{saveName}_DeActive", new Vector3(300, 100));
            lightDeActiveState.motion = animLightDisableClip;
            lightDeActiveState.writeDefaultValues = false;
            layer.stateMachine.defaultState = lightDeActiveState;

            var toDeActiveTransition = lightActiveState.AddTransition(lightDeActiveState);
            toDeActiveTransition.exitTime = 0;
            toDeActiveTransition.duration = 0;
            toDeActiveTransition.hasExitTime = false;
            toDeActiveTransition.conditions = new AnimatorCondition[] {
                new AnimatorCondition
                {
                    mode = AnimatorConditionMode.IfNot,
                    parameter = saveName,
                    threshold = 1,
                },
            };
            var toActiveTransition = lightDeActiveState.AddTransition(lightActiveState);
            toActiveTransition.exitTime = 0;
            toActiveTransition.duration = 0;
            toActiveTransition.hasExitTime = false;
            toActiveTransition.conditions = new AnimatorCondition[] {
                new AnimatorCondition
                {
                    mode = AnimatorConditionMode.If,
                    parameter = saveName,
                    threshold = 1,
                },
            };

            //////////////////////
            //ExpressionMenuの生成

            //SubMenuの生成
            var subMenu = new VRCExpressionsMenu
            {
                controls = new List<VRCExpressionsMenu.Control>
                {
                new VRCExpressionsMenu.Control {
                        name = saveName + " Toggle",
                        type = VRCExpressionsMenu.Control.ControlType.Toggle,
                        parameter = new VRCExpressionsMenu.Control.Parameter
                        {
                            name = saveName,
                        },
                        subParameters = new VRCExpressionsMenu.Control.Parameter[] { },
                        value = 1,
                        labels = new VRCExpressionsMenu.Control.Label[] { },
                    },
                new VRCExpressionsMenu.Control {
                    name = saveName + " light",
                    type = VRCExpressionsMenu.Control.ControlType.RadialPuppet,
                    subParameters = new VRCExpressionsMenu.Control.Parameter[] {
                        new VRCExpressionsMenu.Control.Parameter
                        {
                            name = saveName + "_motion"
                        }
                    },
                    value = DefaultLightValue,
                    labels = new VRCExpressionsMenu.Control.Label[] { },
                },
             },
            };
            AssetDatabase.CreateAsset(subMenu, $"{savePath}_subMenu.asset");

            //Menuの生成
            var menu = new VRCExpressionsMenu()
            {
                controls = new List<VRCExpressionsMenu.Control>
                {
                new VRCExpressionsMenu.Control {
                        name = saveName,
                        type = VRCExpressionsMenu.Control.ControlType.SubMenu,
                        subMenu = subMenu,
                        subParameters = new VRCExpressionsMenu.Control.Parameter[] { },
                        labels = new VRCExpressionsMenu.Control.Label[] { },
                    },
             },
            };
            AssetDatabase.CreateAsset(menu, $"{savePath}.asset");

            //////////////
            //Prefabの生成

            var prefab = new GameObject(saveName);
            var menuInstaller = prefab.GetOrAddComponent<ModularAvatarMenuInstaller>();
            menuInstaller.menuToAppend = menu;
            var parameters = prefab.GetOrAddComponent<ModularAvatarParameters>();

            //MA Parameters
            parameters.parameters.Add(new ParameterConfig
            {
                nameOrPrefix = saveName,
                defaultValue = (float)Convert.ToDouble(this.IsDefaultUse),
                syncType = ParameterSyncType.Bool,
                saved = this.IsValueSave
            });
            parameters.parameters.Add(new ParameterConfig
            {
                nameOrPrefix = name = saveName + "_motion",
                defaultValue = this.DefaultLightValue,
                syncType = ParameterSyncType.Float,
                saved = this.IsValueSave
            });
            // MA MergeAnimator
            var mergeAnimator = prefab.GetOrAddComponent<ModularAvatarMergeAnimator>();
            mergeAnimator.animator = controller;
            mergeAnimator.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
            mergeAnimator.pathMode = MergeAnimatorPathMode.Absolute;
            mergeAnimator.matchAvatarWriteDefaults = true;
            PrefabUtility.SaveAsPrefabAsset(prefab, $"{savePath}.prefab");
            DestroyImmediate(prefab);

            AssetDatabase.SaveAssets();

            Debug.Log(savePath);
            Debug.Log(saveName);
        }
        private void createAnimationClip(GameObject parentObj, AnimationClip animActiveClip, AnimationClip animDeActiveClip, string avaterName)
        {
            Transform children = parentObj.GetComponent<Transform>();

            if (children.childCount == 0) return;

            foreach (Transform obj in children)
            {
                SkinnedMeshRenderer SkinnedMeshR = obj.gameObject.GetComponent<SkinnedMeshRenderer>();
                if (SkinnedMeshR != null)
                {
                    foreach (var mat in SkinnedMeshR.sharedMaterials)
                    {
                        if (mat != null && (mat.shader.name.IndexOf("lilToon") != -1 || mat.shader.name.IndexOf("motchiri") != -1))
                        {
                            string path = getObjectPath(obj).Replace(avaterName + "/", "");
                            animActiveClip.SetCurve(path, typeof(SkinnedMeshRenderer), SHADER_KEY_LightMinLimit, new AnimationCurve(new Keyframe(0 / 60.0f, MinLightValue), new Keyframe(1 / 60.0f, MaxLightValue)));
                            animActiveClip.SetCurve(path, typeof(SkinnedMeshRenderer), SHADER_KEY_LightMaxLimit, new AnimationCurve(new Keyframe(0 / 60.0f, MinLightValue), new Keyframe(1 / 60.0f, MaxLightValue)));

                            animDeActiveClip.SetCurve(path, typeof(SkinnedMeshRenderer), SHADER_KEY_LightMinLimit, new AnimationCurve(new Keyframe(0 / 60.0f, 0.0f), new Keyframe(1 / 60.0f, 0.0f)));
                            animDeActiveClip.SetCurve(path, typeof(SkinnedMeshRenderer), SHADER_KEY_LightMaxLimit, new AnimationCurve(new Keyframe(0 / 60.0f, 1.0f), new Keyframe(1 / 60.0f, 1.0f)));
                        }
                    }
                }
                createAnimationClip(obj.gameObject, animActiveClip, animDeActiveClip, avaterName);
            }
        }
        private string getObjectPath(Transform trans)
        {
            string path = trans.name;
            var parent = trans.parent;
            while (parent)
            {
                path = $"{parent.name}/{path}";
                parent = parent.parent;
            }
            return path;
        }
    }

    internal static class Utils
    {
        public static T AddTo<T>(this T obj, UnityEngine.Object asset) where T : UnityEngine.Object
        {
            AssetDatabase.AddObjectToAsset(obj, asset);
            return obj;
        }

        public static T HideInHierarchy<T>(this T obj) where T : UnityEngine.Object
        {
            obj.hideFlags |= HideFlags.HideInHierarchy;
            return obj;
        }

        public static IEnumerable<string> EnumerateProperties(this Shader shader)
        {
            var count = shader.GetPropertyCount();
            for(int i = 0; i < count; i++)
            {
                yield return shader.GetPropertyName(i);
            }
        }

        public static string GetAbsolutePath(this GameObject obj) => GetRelativePath(obj.transform, null);

        public static string GetRelativePath(this GameObject obj, GameObject root, bool includeRelativeTo = false) => obj.transform.GetRelativePath(root.transform, includeRelativeTo);

        public static string GetRelativePath(this Transform transform, Transform root, bool includeRelativeTo = false)
        {
            var buffer = _relativePathBuffer;
            if (buffer is null)
            {
                buffer = _relativePathBuffer = new string[128];
            }

            var t = transform;
            int idx = buffer.Length;
            while (t != null && t != root)
            {
                buffer[--idx] = t.name;
                t = t.parent;
            }
            if (includeRelativeTo && t != null && t == root)
            {
                buffer[--idx] = t.name;
            }

            return string.Join("/", buffer, idx, buffer.Length - idx);
        }

        private static string[] _relativePathBuffer;
    }
}

#endif
