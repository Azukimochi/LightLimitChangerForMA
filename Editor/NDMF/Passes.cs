using System;
using System.Collections.Generic;
using System.Linq;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using nadena.dev.ndmf.fluent;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using static io.github.azukimochi.Passes;
using Object = UnityEngine.Object;

namespace io.github.azukimochi
{
    internal static partial class Passes
    {
        public static void RunningPasses(Sequence sequence)
        {
            sequence
                .Run(CollectTargetRenderers).Then
                .Run(CloningMaterials).Then
                .Run(NormalizeMaterials).Then
                .Run(GenerateAdditionalControl).Then
                .Run(GenerateAnimations).Then
                .Run(Finalize);
        }

        public readonly static CollectTargetRenderersPass CollectTargetRenderers = new CollectTargetRenderersPass();
        public readonly static CloningMaterialsPass CloningMaterials = new CloningMaterialsPass();
        public readonly static NormalizeMaterialsPass NormalizeMaterials = new NormalizeMaterialsPass();
        public readonly static GenerateAdditionalControlPass GenerateAdditionalControl = new GenerateAdditionalControlPass();
        public readonly static GenerateAnimationsPass GenerateAnimations = new GenerateAnimationsPass();
        public readonly static FinalizePass Finalize = new FinalizePass();

        internal const string ParameterName_Toggle = "LightLimitEnable";
        internal const string ParameterName_Value = "LightLimitValue";
        internal const string ParameterName_Min = "LightLimitMin";
        internal const string ParameterName_Max = "LightLimitMax";
        internal const string ParameterName_Saturation = "LightLimitSaturation";
        internal const string ParameterName_Unlit = "LightLimitUnlit";
        internal const string ParameterName_ColorTemp = "LightLimitColorTemp";
        internal const string ParameterName_Reset = "LightLimitReset";

        private static Session GetSession(BuildContext context)
        {
            var session = context.GetState<Session>();
            session.InitializeSession(context);
            return session;
        }

        private static LightLimitChangerObjectCache GetObjectCache(BuildContext context)
        {
            var cache = context.GetState<LightLimitChangerObjectCache>();
            if (cache.Context != context)
                cache.Context = context;
            return cache;
        }

        internal abstract class LightLimitChangerBasePass<TPass> : Pass<TPass> where TPass : Pass<TPass>, new()
        {
            protected override void Execute(BuildContext context)
            {
                var session = GetSession(context);
                if (!session.IsValid())
                    return;
            
                var cache = GetObjectCache(context);

                Execute(context, session, cache);
            }

            protected abstract void Execute(BuildContext context, Session session, LightLimitChangerObjectCache cache);
        }

        internal sealed class Session
        {
            public LightLimitChangerSettings Settings;
            public LightLimitChangerParameters Parameters;
            public ControlAnimationContainer[] Controls;
            public AnimatorController Controller;
            public LightLimitControlType TargetControl;
            public HashSet<Renderer> TargetRenderers;

            public HashSet<Object> Excludes;

            private bool _initialized;

            public bool IsValid() => Settings != null;

            public void InitializeSession(BuildContext context)
            {
                if (_initialized)
                    return;

                Controller = new AnimatorController() { name = "Light Limit Controller" }.AddTo(GetObjectCache(context));
                Settings = context.AvatarRootObject.GetComponentInChildren<LightLimitChangerSettings>();
                var parameters = Parameters = Settings?.Parameters ?? LightLimitChangerParameters.Default;
                Excludes = new HashSet<Object>(Settings?.Excludes);

                List<ControlAnimationContainer> controls = new List<ControlAnimationContainer>();
                if (!parameters.IsSeparateLightControl)
                {
                    controls.Add(ControlAnimationContainer.Create(LightLimitControlType.Light, "Light", ParameterName_Value, parameters.DefaultLightValue, Icons.Light));
                }
                else
                {
                    controls.Add(ControlAnimationContainer.Create(LightLimitControlType.LightMin, "Min Light", ParameterName_Min, parameters.DefaultLightValue, Icons.Light_Min));
                    controls.Add(ControlAnimationContainer.Create(LightLimitControlType.LightMax, "Max Light", ParameterName_Max, parameters.DefaultLightValue, Icons.Light_Max));
                }

                controls.AddRange(new[]
                {
                    ControlAnimationContainer.Create(LightLimitControlType.Saturation, "Saturation", ParameterName_Saturation, 0.5f, Icons.Color),
                    ControlAnimationContainer.Create(LightLimitControlType.Unlit, "Unlit", ParameterName_Unlit, 0, Icons.Unlit),
                    ControlAnimationContainer.Create(LightLimitControlType.ColorTemperature, "ColorTemp", ParameterName_ColorTemp, 0.5f, Icons.Temp),
                });

                Controls = controls.ToArray();

                var targetControl = LightLimitControlType.Light;

                if (parameters.AllowColorTempControl)
                {
                    targetControl |= LightLimitControlType.ColorTemperature;
                }
                if (parameters.AllowSaturationControl)
                {
                    targetControl |= LightLimitControlType.Saturation;
                }
                if (parameters.AllowUnlitControl)
                {
                    targetControl |= LightLimitControlType.Unlit;
                }

                TargetControl = targetControl;

                TargetRenderers = new HashSet<Renderer>();

                _initialized = true;
            }
        }

        internal sealed class CollectTargetRenderersPass : LightLimitChangerBasePass<CollectTargetRenderersPass>
        {
            protected override void Execute(BuildContext context, Session session, LightLimitChangerObjectCache cache)
            {
                var list = session.TargetRenderers;

                var temp = new HashSet<Renderer>();
                //CollectMeshRenderers(context.AvatarRootObject, session.Parameters.TargetShaders, temp);
                CollectMeshRenderersInAnimation(context.AvatarRootObject, session.Parameters.TargetShaders, temp);

                foreach(var x in temp)
                {
                    Debug.LogWarning(x);
                }
            }

            private static void CollectMeshRenderers(GameObject avatarObject, in TargetShaders targetShaders, HashSet<Renderer> list)
            {
                foreach (var renderer in avatarObject.GetComponentsInChildren<Renderer>(true))
                {
                    if (renderer.CompareTag("EditorOnly") ||
                        !(renderer is MeshRenderer || renderer is SkinnedMeshRenderer))
                    {
                        continue;
                    }

                    foreach(var material in renderer.sharedMaterials)
                    {
                        if (ShaderInfo.TryGetShaderInfo(material, out var shaderInfo) && targetShaders.Contains(shaderInfo.Name))
                        {
                            list.Add(renderer);
                            break;
                        }
                    }
                }
            }

            private static void CollectMeshRenderersInAnimation(GameObject avatarObject, in TargetShaders targetShaders, HashSet<Renderer> list)
            {
                var components = avatarObject.GetComponentsInChildren<Component>(true);
                var clips = new Dictionary<AnimationClip, Component>();
                foreach (var component in components)
                {
                    var so = new SerializedObject(component);
                    bool enterChildren = true;
                    var p = so.GetIterator();
                    while (p.Next(enterChildren))
                    {
                        if (p.propertyType == SerializedPropertyType.ObjectReference)
                        {
                            if (p.objectReferenceValue is RuntimeAnimatorController controller)
                            {
                                foreach(var x in controller.animationClips)
                                {
                                    if (!clips.ContainsKey(x))
                                        clips.Add(x, component);
                                }
                            }
                        }

                        enterChildren = p.propertyType.IsNeedToEnterChildren();
                    }
                }

                foreach(var (clip, component) in clips)
                {
                    foreach(var bind in AnimationUtility.GetObjectReferenceCurveBindings(clip) ?? Array.Empty<EditorCurveBinding>())
                    {
                        var rootObj = component.gameObject;
                        if (component is ModularAvatarMergeAnimator mamaaaa && mamaaaa.pathMode == MergeAnimatorPathMode.Absolute)
                        {
                            rootObj = avatarObject;
                        }

                        var obj = AnimationUtility.GetAnimatedObject(rootObj, bind);
                        if (obj is MeshRenderer || obj is SkinnedMeshRenderer)
                        {
                            var renderer = obj as Renderer;
                            if (AnimationUtility.GetObjectReferenceValue(rootObj, bind, out var maybeMaterial) && maybeMaterial is Material material)
                            {
                                Debug.LogError(material);
                                if (ShaderInfo.TryGetShaderInfo(material, out var shaderInfo) && targetShaders.Contains(shaderInfo.Name))
                                {
                                    list.Add(renderer);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            private sealed class GeneralEqualityComparer<T> : IEqualityComparer<T>
            {
                public Func<T, T, bool> _equals;
                public Func<T, int> _getHashCode;

                public GeneralEqualityComparer(Func<T, T, bool> equals, Func<T, int> getHashCode)
                {
                    _equals = equals;
                    _getHashCode = getHashCode;
                }

                public bool Equals(T x, T y) => _equals(x, y);

                public int GetHashCode(T obj) => _getHashCode(obj);
            }
        }
    }
}
