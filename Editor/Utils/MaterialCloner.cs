using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Anatawa12.AvatarOptimizer;
using nadena.dev.ndmf;
using UnityEditor.Animations;

namespace io.github.azukimochi;

internal sealed class MaterialCloner : DeepCloneHelper
{
    public Dictionary<ShaderProcessor, List<Material>> ShaderMaterialPair { get; } = new();

    private readonly LightLimitChangerProcessor processor;

    public MaterialCloner(LightLimitChangerProcessor processor)
    {
        this.processor = processor;
    }

    public Dictionary<Material, Material> Cloned { get; } = new();

    protected override Dictionary<Object, Object> GetCache(Type type)
    {
        if (type == typeof(Material))
        //  return Cloned as Dictionary<Object, Object>           // Cannot convert type 'Dictionary<Material, Material>' to 'Dictionary<Object, Object>' */
            return Unsafe.As<Dictionary<Object, Object>>(Cloned); // 👍
        return base.GetCache(type);
    }

    protected override Object CustomClone(Object o)
    {
        if (o is Material mat)
        {
            ShaderProcessor targetProcessor = null;
            foreach(var processor in this.processor.Processors)
            {
                if (!processor.IsTargetMaterial(mat))
                    continue;
                targetProcessor = processor;
                break;
            }
            if (targetProcessor is null)
                return null;

            Material newMat;
            using (MaterialEditorReflection.BeginNoApplyMaterialPropertyDrawers())
                newMat = new Material(mat);
            newMat.parent = null;
            newMat.name = $"{o.name}(LLC Clone)";
            ObjectRegistry.RegisterReplacedObject(mat, newMat);

            ShaderMaterialPair.GetOrAdd(targetProcessor, _ => new()).Add(newMat);

            return newMat;
        }

        return null;
    }

    protected override ComponentSupport GetComponentSupport(Object o)
    {
        return o switch
        {
            Material or
            Motion or 
            AnimatorController or 
            AnimatorOverrideController or 
            AnimatorState or 
            AnimatorStateMachine or 
            AnimatorTransitionBase or 
            StateMachineBehaviour or 
            AvatarMask => ComponentSupport.Clone,
            _ => ComponentSupport.NoClone,
        };
    }
}

// https://github.com/anatawa12/AvatarOptimizer/blob/d7178a4df250dae5ef33b05ed60c8e35f49879f9/Editor/Processors/DupliacteAssets.cs#L152-L188
// Originally under MIT License
// Copyright(c) 2022 anatawa12
static class MaterialEditorReflection
{
    static MaterialEditorReflection()
    {
        DisableApplyMaterialPropertyDrawersPropertyInfo = typeof(EditorMaterialUtility).GetProperty(
            "disableApplyMaterialPropertyDrawers", BindingFlags.Static | BindingFlags.NonPublic)!;
    }

    public static readonly PropertyInfo DisableApplyMaterialPropertyDrawersPropertyInfo;

    public static DisableApplyMaterialPropertyDisposable BeginNoApplyMaterialPropertyDrawers()
    {
        return new DisableApplyMaterialPropertyDisposable(true);
    }

    public static bool DisableApplyMaterialPropertyDrawers
    {
        get => (bool)DisableApplyMaterialPropertyDrawersPropertyInfo.GetValue(null);
        set => DisableApplyMaterialPropertyDrawersPropertyInfo.SetValue(null, value);
    }

    public readonly struct DisableApplyMaterialPropertyDisposable : IDisposable
    {
        private readonly bool _originalValue;

        public DisableApplyMaterialPropertyDisposable(bool value)
        {
            _originalValue = DisableApplyMaterialPropertyDrawers;
            DisableApplyMaterialPropertyDrawers = value;
        }

        public void Dispose()
        {
            DisableApplyMaterialPropertyDrawers = _originalValue;
        }
    }
}