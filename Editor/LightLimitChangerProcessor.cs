using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using gomoru.su;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using nadena.dev.ndmf.util;
using UnityEditor.Animations;
using UnityEngine;
using VRC.Core;
using VRC.SDK3.Avatars.Components;

namespace io.github.azukimochi;

internal sealed class LightLimitChangerProcessor : IDisposable
{
    public GameObject AvatarRootObject { get; }
    public LightLimitChangerComponent Component { get; }
    public Object AssetContainer { get; }

    private readonly BuildContext context;
    private readonly Dictionary<Object, Object> cache = new();
    private readonly List<ShaderProcessor> processors = new();

    private DirectBlendTree blendTree;
    private Renderer[] targetRenderers;

    public LightLimitChangerProcessor(BuildContext context)
    {
        this.context = context;
        AvatarRootObject = context.AvatarRootObject;
        Component = context.AvatarRootObject.GetComponentInChildren<LightLimitChangerComponent>(false);
        AssetContainer = context.AssetContainer;
    }

    public LightLimitChangerProcessor(GameObject avatarRootObject, Object assetContainer)
    {
        AvatarRootObject = avatarRootObject;
        Component = avatarRootObject.GetComponentInChildren<LightLimitChangerComponent>(false);
        AssetContainer = assetContainer;
    }

    public void Run()
    {
        if (Component == null)
            return;

        var excludes = Component.Excludes.ToHashSet();
        targetRenderers = AvatarRootObject.GetComponentsInChildren<Renderer>()
            .Where(x => !excludes.Contains(x.gameObject))
            .ToArray();

        blendTree = new DirectBlendTree() { Name = LightLimitChanger.Title };

        ConfigureAnimation(Component.General.Lighting, x => x.MinLight);
        ConfigureAnimation(Component.General.Lighting, x => x.MaxLight);
        ConfigureAnimation(Component.General.Lighting, x => x.Monochrome);
        ConfigureAnimation(Component.General.Lighting, x => x.Unlit);

        var animatorController = new AnimatorController() { name = LightLimitChanger.Title };
        AssetDatabase.AddObjectToAsset(animatorController, AssetContainer);

        var llcObject = Component.gameObject;
        var mama = llcObject.GetOrAddComponent<ModularAvatarMergeAnimator>();
        mama.animator = animatorController;
        mama.matchAvatarWriteDefaults = Component.WriteDefaults == WriteDefaultsSetting.MatchAvatar;
        mama.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
        mama.pathMode = MergeAnimatorPathMode.Absolute;
        mama.deleteAttachedAnimator = true;

        animatorController.AddLayer(blendTree.ToAnimatorControllerLayer(animatorController));

    }

    private bool ConfigureAnimation<TSettings, T>(TSettings settings, Expression<Func<TSettings, Parameter<T>>> parameterSelector) where TSettings : ISettings
    {
        var targetField = (parameterSelector.Body as MemberExpression).Member as FieldInfo;
        var parameter = targetField.GetValue(settings) as Parameter<T>;
        Vector2 range = default;

        if (targetField.GetCustomAttribute<RangeParameterAttribute>() is { } rangeAttr)
        {
            var val = typeof(TSettings).GetField(rangeAttr.ParameterName)?.GetValue(settings) ?? null;
            if (val is Vector2 v)
                range = v;
        }

        if (!parameter.Enable)
            return false;

        var name = targetField.Name;

        var tree = blendTree.AddMotionTime(name);
        var anim = tree.Animation = new AnimationClip() { name = $"{LightLimitChanger.Title} {name}" };
        AssetDatabase.AddObjectToAsset(anim, AssetContainer);

        var generalType = targetField.GetCustomAttribute<GeneralControlAttribute>()?.Type ?? default;
        var shaderFeatureAttr = targetField.GetCustomAttribute<ShaderFeatureAttribute>();

        foreach (var processor in processors.AsSpan())
        {
            var context = new ConfigureGeneralAnimationContext()
            {
                Name = name,
                Renderers = targetRenderers,
                AnimationClip = anim,
                Parameter = parameter.ToFloat(),
                Range = range,
                Type = generalType,
            };

            if (shaderFeatureAttr is null)
            {
                processor.ConfigureGeneralAnimation(context);
            }
            else
            {
                var names = shaderFeatureAttr.QualifiedNames;
                if (!names.Contains(processor.QualifiedName))
                    continue;

                processor.ConfigureShaderSpecificAnimation(Unsafe.As<ConfigureGeneralAnimationContext, ConfigureShaderSpecificAnimationContext>(ref context));
            }
        }
        return true;
    }

    private void CloneMaterials()
    {
        var components = AvatarRootObject.GetComponentsInChildren<Component>(true);
        var rootAnimator = AvatarRootObject.GetComponent<Animator>();
        foreach (var x in components)
        {
            if (x == rootAnimator || x == Component) continue;

            var so = new SerializedObject(x);

            bool enterChildren = true;
            var p = so.GetIterator();
            while (p.Next(enterChildren))
            {
                try
                {
                    if (p.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        var obj = p.objectReferenceValue;
                        if (obj == null) continue;

                        if (cache.TryGetValue(obj, out var mapped))
                        {
                            p.objectReferenceValue = mapped;
                        }
                        else if (obj is Material mat)
                        {
                            foreach(var processor in processors)
                            {

                            }
                        }
                        else if (obj is RuntimeAnimatorController runtimeAnimatorController)
                        {
                            if (cache.TryGetValue(runtimeAnimatorController, out var mappedController))
                            {
                                p.objectReferenceValue = mappedController;
                            }

                        }
                    }
                }
                finally
                {
                    enterChildren = p.propertyType switch
                    {
                        SerializedPropertyType.String or
                        SerializedPropertyType.Integer or
                        SerializedPropertyType.Boolean or
                        SerializedPropertyType.Float or
                        SerializedPropertyType.Color or
                        SerializedPropertyType.ObjectReference or
                        SerializedPropertyType.LayerMask or
                        SerializedPropertyType.Enum or
                        SerializedPropertyType.Vector2 or
                        SerializedPropertyType.Vector3 or
                        SerializedPropertyType.Vector4 or
                        SerializedPropertyType.Rect or
                        SerializedPropertyType.ArraySize or
                        SerializedPropertyType.Character or
                        SerializedPropertyType.AnimationCurve or
                        SerializedPropertyType.Bounds or
                        SerializedPropertyType.Gradient or
                        SerializedPropertyType.Quaternion or
                        SerializedPropertyType.FixedBufferSize or
                        SerializedPropertyType.Vector2Int or
                        SerializedPropertyType.Vector3Int or
                        SerializedPropertyType.RectInt or
                        SerializedPropertyType.BoundsInt
                            => false,

                        _ => true,
                    };
                    so.ApplyModifiedProperties();
                }
            }
        }
    }

    public void Dispose()
    {

    }

    public LightLimitChangerProcessor AddProcessor<T>() where T : ShaderProcessor, new()
    {
        var t = new T();
        t.Initialize(this);
        processors.Add(t);
        return this;
    }
}