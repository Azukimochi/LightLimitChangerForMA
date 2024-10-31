using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using gomoru.su;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
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
    private Material[] targetMaterials;
    private ImmutableDictionary<ShaderProcessor, Material[]> shaderMaterialPair;
    private AnimatorController animatorController;

    public ReadOnlySpan<ShaderProcessor> Processors => processors.AsSpan();
    public ReadOnlySpan<Renderer> TargetRenderers => targetRenderers;
    public ReadOnlySpan<Material> TargetMaterials => targetMaterials;

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

        CloneMaterials();

        SetupAnimations();

        ConfigureSettings(Component.General.LightingControl);
        ConfigureSettings(Component.General.ColorControl);
        ConfigureSettings(Component.General.EmissionControl);
        ConfigureSettings(Component.LilToon);
        ConfigureSettings(Component.Poiyomi);

        GenerateParameters();


        var layer = blendTree.ToAnimatorControllerLayer(animatorController);
        animatorController.AddLayer(layer);

        RemoveEmptySubMenus(menuRoot);
    }

    private void CloneMaterials()
    {
        var components = AvatarRootObject.GetComponentsInChildren<Component>(true);
        var rootAnimator = AvatarRootObject.GetComponent<Animator>();
        var materials = new HashSet<Material>();
        var dict = new Dictionary<ShaderProcessor, List<Material>>();
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
                    if (p.propertyType != SerializedPropertyType.ObjectReference)
                        continue;

                    var result = Clone(p.objectReferenceValue);
                    if (result != null)
                    {
                        p.objectReferenceValue = result;
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

        targetMaterials = materials.ToArray();
        shaderMaterialPair = dict.ToImmutableDictionary(x => x.Key, x => x.Value.ToArray());

        foreach (var processor in Processors)
        {
            processor.OnMaterialCloned(targetMaterials);
        }

        foreach (var pair in shaderMaterialPair)
        {
            foreach (var mat in pair.Value.AsSpan())
            {
                pair.Key.NormalizeMaterial(mat);
            }
        }

        Object Clone(Object obj)
        {
            if (obj == null)
                return null;

            if (cache.TryGetValue(obj, out var mapped))
            {
                return mapped;
            }
            else if (obj is Material mat)
            {
                Material cloned = null;
                foreach (var processor in Processors)
                {
                    if (!processor.IsTargetMaterial(mat))
                        continue;

                    cloned = Object.Instantiate(mat);
                    cloned.name = $"{obj.name}(LLC)";
                    ObjectRegistry.RegisterReplacedObject(obj, cloned);
                    AssetDatabase.AddObjectToAsset(cloned, context.AssetContainer);

                    materials.Add(cloned);
                    cache.TryAdd(obj, cloned);
                    dict.GetOrAdd(processor, _ => new()).Add(cloned);
                    break;
                }

                if (cloned == null)
                    return null;

                return cloned;
            }

            return null;
        }
    }

    private void SetupAnimations()
    {
        var go = Component.gameObject;
        blendTree = new DirectBlendTree() { Name = LightLimitChanger.Title };
        animatorController = new AnimatorController() { name = LightLimitChanger.Title };

        AssetDatabase.AddObjectToAsset(animatorController, AssetContainer);
        animatorController.AddParameter(new() { defaultFloat = 1, name = "1", type = AnimatorControllerParameterType.Float });

        var mama = go.GetOrAddComponent<ModularAvatarMergeAnimator>();
        mama.animator = animatorController;
        mama.matchAvatarWriteDefaults = Component.WriteDefaults == WriteDefaultsSetting.MatchAvatar;
        mama.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
        mama.pathMode = MergeAnimatorPathMode.Absolute;
        mama.deleteAttachedAnimator = true;

        if (!go.TryGetComponent<ModularAvatarMenuInstaller>(out var mami))
        {
            mami = go.AddComponent<ModularAvatarMenuInstaller>();
        }

        var mam = go.AddComponent<ModularAvatarMenuItem>();
        mam.Control.type = VRCExpressionsMenu.Control.ControlType.SubMenu;
        mam.MenuSource = SubmenuSource.Children;
        menuRoot = mam;
    }

    private void GenerateParameters()
    {
        var mapa = Component.gameObject.GetOrAddComponent<ModularAvatarParameters>();
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
    }
    
    private static void RemoveEmptySubMenus(MAMenuItem menu)
    {
        if (menu.Control.type != VRCExpressionsMenu.Control.ControlType.SubMenu)
            return;

        var children = menu.transform.Cast<Transform>().Select(x => x.GetComponent<MAMenuItem>()).Where(x => x != null);
        if (!children.Any())
        {
            Object.DestroyImmediate(menu);
            return;
        }

        foreach (var child in children)
        {
            RemoveEmptySubMenus(child);
        }
    }

    private void ConfigureSettings<TSettings>(TSettings settings) where TSettings : ISettings, new()
    {
        var menuGroup = menuRoot.GetOrAdd(settings.DisplayName);
        if (typeof(TSettings).GetCustomAttribute<MenuIconAttribute>() is { } groupIconAttr)
        {
            menuGroup.Control.icon = AssetUtils.FromGUID<Texture2D>(groupIconAttr.Guid);
        }

        var parameterBuffer = (stackalloc float[8]);
        Dictionary<string, List<ParameterInfo>> entries = new();

        foreach (var field in settings.AllParameterFields())
        {
            if (field.FieldType.BaseType != typeof(Parameter))
                continue;

            var parameterInfo = new ParameterInfo(settings, field);
            var parameter = parameterInfo.Parameter;

            string key = parameterInfo.VectorFieldAttribute != null ? parameterInfo.VectorFieldAttribute.Group ?? settings.ParameterPrefix : "";
            var list = entries.GetOrAdd(key, _ => new());
            list.Add(parameterInfo);
        }

        foreach (var entry in entries)
        {
            var items = entry.Value.AsSpan();
            bool generateEmpty = entry.Value.Any(x => x.VectorFieldAttribute != null && x.Parameter.Enable && x.Parameter.IsAnimated);
            foreach (var parameterInfo in items)
            {
                var parameter = parameterInfo.Parameter;
                var t = parameterInfo.ParameterType;

                if (parameter.Enable)
                {
                    foreach (var pair in shaderMaterialPair)
                    {
                        if (!parameterInfo.MaterialProperties.TryGetValue(pair.Key.QualifiedName, out var propertyName))
                            propertyName = pair.Key.GetMaterialPropertyName(parameterInfo);

                        if (propertyName == null)
                            continue;

                        pair.Key.OverrideMaterialValue(new OverrideMaterialValueContext() { ParameterInfo = parameterInfo, PropertyName = propertyName, Materials = pair.Value });
                    }
                }

                if ((!parameter.Enable || !parameter.IsAnimated) && !generateEmpty)
                    continue;

                var group = GetBlendTreeGroup(settings.ParameterPrefix);
                ReadOnlySpan<float> values = parameterBuffer[..parameterInfo.Parameter.GetValues(parameterBuffer)];

                for(int i = 0; i < values.Length; i++)
                {
                    var value = values[i];
                    string postfix = values.Length == 1 ? "" : t == typeof(Vector4) ? $".{"xyzw"[i]}" : $".{"rgba"[i]}";
                    var name = $"{parameterInfo.Name}{postfix}";
                    var anim = new AnimationClip() { name = $"{LightLimitChanger.Title} {name}" };
                    AssetDatabase.AddObjectToAsset(anim, AssetContainer);

                    if (!parameter.IsAnimated)
                    {
                        var tree = group.AddMotion(anim);
                        var context = new ConfigureEmptyAnimationContext()
                        {
                            ParameterInfo = parameterInfo,
                            PropertyName = null,
                            Renderers = targetRenderers,
                            AnimationClip = anim,
                            Type = parameterInfo.GeneralControlType,
                        };

                        foreach (var processor in processors.AsSpan())
                        {
                            var propertyName = parameterInfo.GetPropertyName(processor);
                            if (string.IsNullOrEmpty(propertyName))
                                continue;

                            context.PropertyName = $"{propertyName}{postfix}";
                            var buffer = parameterBuffer[4..];
                            buffer = buffer[..ParameterCache<TSettings>.InitialParameters[parameterInfo.Name].GetValues(buffer)];
                            context.Value = buffer[i];

                            processor.ConfigreEmptyAnimation(context);
                        }
                    }
                    else
                    {
                        var tree = group.AddMotionTime(name);
                        tree.Animation = anim;
                        var avatarParameter = new ParameterConfig()
                        {
                            nameOrPrefix = $"{settings.ParameterPrefix}{name}",
                            defaultValue = Utils.NormalizeInRange(value, parameterInfo.Range.x, parameterInfo.Range.y),
                            syncType =
                            t == typeof(bool) ? ParameterSyncType.Bool :
                            t == typeof(int) ? ParameterSyncType.Int :
                            t == typeof(float) || t == typeof(Vector4) || t == typeof(Color) ? ParameterSyncType.Float :
                            ParameterSyncType.NotSynced,
                            saved = parameter.Saved,
                            localOnly = !parameter.Synced,
                        };
                        tree.ParameterName = avatarParameter.nameOrPrefix;
                        avatarParameters.Add(avatarParameter);

                        var context = new ConfigureGeneralAnimationContext()
                        {
                            ParameterInfo = parameterInfo,
                            PropertyName = null,
                            Renderers = targetRenderers,
                            AnimationClip = anim,
                            AvatarParameter = avatarParameter,
                            Type = parameterInfo.GeneralControlType,
                        };

                        foreach (var processor in processors.AsSpan())
                        {
                            context.Range = parameterInfo.Range; // Range is mutable

                            var propertyName = parameterInfo.GetPropertyName(processor);
                            if (string.IsNullOrEmpty(propertyName))
                                continue;

                            context.PropertyName = $"{propertyName}{postfix}";

                            if (parameterInfo.ShaderFeatureAttribute is null)
                            {
                                processor.ConfigureGeneralAnimation(context);
                            }
                            else
                            {
                                var names = parameterInfo.ShaderFeatureAttribute.QualifiedNames;
                                if (!names.Contains(processor.QualifiedName))
                                    continue;

                                processor.ConfigureShaderSpecificAnimation(context);
                            }
                            var menuPath = $"{parameterInfo.Name}{(values.Length == 1 ? "" : $"/{(char)(postfix[1] & ~0x20)}")}";
                            var menuItem = menuGroup.GetOrAdd(menuPath, menu => (VRCExpressionsMenu.Control.ControlType.RadialPuppet, avatarParameter.nameOrPrefix));
                            if (menuItem.Control.icon == null)
                            {
                                menuItem.Control.icon = parameterInfo.Icon;
                            }
                        }
                    }
                }
            }
        }
    }

    private DirectBlendTree GetBlendTreeGroup(string name)
    {
        return blendTree.Items
            .FirstOrDefault(x => x is DirectBlendTree d && d.Name == name) as DirectBlendTree 
            ?? blendTree.AddDirectBlendTree(name);
    }

    public LightLimitChangerProcessor AddProcessor<T>() where T : ShaderProcessor, new()
    {
        var t = new T();
        (t as ILightLimitChangerProcessorReceiver).Initialize(this);
        processors.Add(t);
        return this;
    }

    public void Dispose()
    {

    }
}
