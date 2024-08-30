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
using VRC.SDK3.Avatars.ScriptableObjects;

namespace io.github.azukimochi;

internal sealed class LightLimitChangerProcessor : IDisposable
{
    public GameObject AvatarRootObject { get; }
    public LightLimitChangerComponent Component { get; }
    public Object AssetContainer { get; }

    private readonly BuildContext context;
    private readonly Dictionary<Object, Object> cache = new();
    private readonly List<ShaderProcessor> processors = new();
    private readonly HashSet<ParameterConfig> avatarParameters = new();

    private ModularAvatarMenuItem menuRoot;
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

        var llcObject = Component.gameObject;
        blendTree = new DirectBlendTree() { Name = LightLimitChanger.Title };

        var animatorController = new AnimatorController() { name = LightLimitChanger.Title };
        AssetDatabase.AddObjectToAsset(animatorController, AssetContainer);
        animatorController.AddParameter(new() { defaultFloat = 1, name = "1", type = AnimatorControllerParameterType.Float });

        var mama = llcObject.GetOrAddComponent<ModularAvatarMergeAnimator>();
        mama.animator = animatorController;
        mama.matchAvatarWriteDefaults = Component.WriteDefaults == WriteDefaultsSetting.MatchAvatar;
        mama.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
        mama.pathMode = MergeAnimatorPathMode.Absolute;
        mama.deleteAttachedAnimator = true;
        
        if (!llcObject.TryGetComponent<ModularAvatarMenuInstaller>(out var mami))
        {
            mami = llcObject.AddComponent<ModularAvatarMenuInstaller>();
        }
        
        var mam = llcObject.AddComponent<ModularAvatarMenuItem>();
        mam.Control.type = VRCExpressionsMenu.Control.ControlType.SubMenu;
        mam.MenuSource = SubmenuSource.Children;
        menuRoot = mam;

        ConfigureSettings(Component.General.LightingControl);
        ConfigureSettings(Component.General.ColorControl);
        ConfigureSettings(Component.General.EmissionControl);

        ConfigureSettings(Component.LilToon);
        ConfigureSettings(Component.Poiyomi);

        var mapa = llcObject.GetOrAddComponent<ModularAvatarParameters>();
        mapa.parameters.AddRange(avatarParameters);

        foreach (var parameter in mapa.parameters.AsSpan())
        {
            animatorController.AddParameter(new AnimatorControllerParameter()
            {
                name = parameter.nameOrPrefix,
                defaultFloat = parameter.defaultValue,
                defaultBool = parameter.defaultValue != 0,
                defaultInt = (int)parameter.defaultValue,
                type = parameter.syncType switch 
                {
                    ParameterSyncType.Int => AnimatorControllerParameterType.Int,
                    ParameterSyncType.Bool => AnimatorControllerParameterType.Bool,
                    ParameterSyncType.Float => AnimatorControllerParameterType.Float,
                    _ => default,
                },
            });
        }

        animatorController.AddLayer(blendTree.ToAnimatorControllerLayer(animatorController));
    }

    private void ConfigureSettings<TSettings>(TSettings settings) where TSettings : ISettings
    {
        var menuGroup = menuRoot.GetOrAdd(settings.DisplayName);
        if (typeof(TSettings).GetCustomAttribute<MenuIconAttribute>() is { } groupIconAttr)
        {
            menuGroup.Control.icon = AssetUtils.FromGUID<Texture2D>(groupIconAttr.Guid);
        }

        var fields = typeof(TSettings).GetFields(BindingFlags.Instance | BindingFlags.Public);
        foreach(var field in fields)
        {
            if (field.FieldType.BaseType != typeof(Parameter))
                continue;

            var t = field.FieldType.GenericTypeArguments[0];
            var parameter = field.GetValue(settings) as Parameter<float>;
            Vector2 range = Vector2.up;

            if (field.GetCustomAttribute<RangeParameterAttribute>() is { } rangeParamAttr)
            {
                var val = typeof(TSettings).GetField(rangeParamAttr.ParameterName)?.GetValue(settings) ?? null;
                if (val is Vector2 v)
                    range = v;
            }
            else if (field.GetCustomAttribute<RangeAttribute>() is { } rangeAttr)
            {
                range = new(rangeAttr.Min, rangeAttr.Max);
            }

            if (!parameter.Enable)
                continue;

            var name = field.Name;

            var group = blendTree.Items.FirstOrDefault(x => x is DirectBlendTree d && d.Name == settings.ParameterPrefix) as DirectBlendTree ?? blendTree.AddDirectBlendTree(settings.ParameterPrefix);
            var tree = group.AddMotionTime(name);
            var anim = tree.Animation = new AnimationClip() { name = $"{LightLimitChanger.Title} {name}" };
            AssetDatabase.AddObjectToAsset(anim, AssetContainer);

            var avatarParameter = new ParameterConfig()
            {
                nameOrPrefix = $"{settings.ParameterPrefix}{field.Name}",
                defaultValue = NormalizeInRange(parameter.InitialValue, range.x, range.y),
                syncType = 
                    t == typeof(bool) ? ParameterSyncType.Bool : 
                    t == typeof(int) ? ParameterSyncType.Int : 
                    t == typeof(float) ? ParameterSyncType.Float : 
                    ParameterSyncType.NotSynced,
                saved = parameter.Saved,
                localOnly = !parameter.Synced,
            };
            tree.ParameterName = avatarParameter.nameOrPrefix;
            avatarParameters.Add(avatarParameter);

            var generalType = field.GetCustomAttribute<GeneralControlAttribute>()?.Type ?? default;
            var shaderFeatureAttr = field.GetCustomAttribute<ShaderFeatureAttribute>();

            foreach (var processor in processors.AsSpan())
            {
                var context = new ConfigureGeneralAnimationContext()
                {
                    Name = name,
                    Renderers = targetRenderers,
                    AnimationClip = anim,
                    AvatarParameter = avatarParameter,
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

                    processor.ConfigureShaderSpecificAnimation(context);
                }
                var menuItem = menuGroup.GetOrAdd(name, menu => (VRCExpressionsMenu.Control.ControlType.RadialPuppet, avatarParameter.nameOrPrefix));
                if (menuItem.Control.icon == null && field.GetCustomAttribute<MenuIconAttribute>() is { } iconAttr)
                {
                    menuItem.Control.icon = AssetUtils.FromGUID<Texture2D>(iconAttr.Guid);
                }
            }
        }
    }

    private void ConfigureAnimation<TSettings>(FieldInfo parameter) where TSettings : ISettings
    {

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
                                processor.OnMaterialCloned(mat);
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
        (t as IShaderProcessor).Initialize(this);
        processors.Add(t);
        return this;
    }

    private static float NormalizeInRange(float value, float min, float max)
        => (value - min) / (max - min);
}