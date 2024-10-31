using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf.util;

namespace io.github.azukimochi;

internal interface ILightLimitChangerProcessorReceiver
{
    void Initialize(LightLimitChangerProcessor processor);
}

internal abstract class ShaderProcessor : ILightLimitChangerProcessorReceiver
{
    protected const string MaterialAnimationKeyPrefix = "material.";

    protected LightLimitChangerProcessor Processor => processor;
    private LightLimitChangerProcessor processor;

    protected LightLimitChangerComponent Component => processor.Component;

    void ILightLimitChangerProcessorReceiver.Initialize(LightLimitChangerProcessor processor) 
    { 
        this.processor = processor;
    }

    /// <summary>
    /// プロセッサーのID
    /// </summary>
    public abstract string QualifiedName { get; }

    /// <summary>
    /// 表示名　いつか使うかも
    /// </summary>
    public virtual string DisplayName => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(QualifiedName);

    /// <summary>
    /// 入力されたマテリアルが対象かどうかを判定する
    /// </summary>
    /// <param name="material"></param>
    /// <returns></returns>
    public virtual bool IsTargetMaterial(Material material) => false;

    /// <summary>
    /// マテリアルが複製された際に呼び出される
    /// </summary>
    /// <param name="material"></param>
    public virtual void OnMaterialCloned(Span<Material> material) { }

    /// <summary>
    /// マテリアルの正規化（テクスチャの焼き込みなど）を行う
    /// </summary>
    /// <param name="material"></param>
    public virtual void NormalizeMaterial(Material material) { }

    /// <summary>
    /// パラメーターの種類か名前から操作対象の名前を取得する
    /// </summary>
    public virtual string GetMaterialPropertyName(ParameterInfo parameterInfo) => null;

    /// <summary>
    /// アニメーションを設定する
    /// </summary>
    public virtual void ConfigureGeneralAnimation(ConfigureGeneralAnimationContext context)
    {
        string propertyName = context.PropertyName;

        var (min, max) = context.Range;
        context.Renderers.AnimateAllFloat(context.AnimationClip, $"{MaterialAnimationKeyPrefix}{propertyName}", AnimationCurve.Linear(0, min, 1 / 60f, max));
    }

    /// <summary>
    /// シェーダー固有のアニメーションの設定
    /// </summary>
    public virtual void ConfigureShaderSpecificAnimation(ConfigureShaderSpecificAnimationContext context)
    {
        string propertyName = context.PropertyName;

        var (min, max) = context.Range;
        context.Renderers.AnimateAllFloat(context.AnimationClip, $"{MaterialAnimationKeyPrefix}{propertyName}", AnimationCurve.Linear(0, min, 1 / 60f, max));
    }

    /// <summary>
    /// 空のアニメーションを設定する
    /// </summary>
    /// <param name="context"></param>
    public virtual void ConfigreEmptyAnimation(ConfigureEmptyAnimationContext context)
    {
        string propertyName = context.PropertyName;
        context.Renderers.AnimateAllFloat(context.AnimationClip, $"{MaterialAnimationKeyPrefix}{propertyName}", AnimationCurve.Constant(0, 0, context.Value));
    }

    /// <summary>
    /// シェーダー固有のメニューを生成する
    /// </summary>
    public virtual void CreateShaderSpecificControl(in CreateShaderSpecificControlContext context) {  }


    public virtual void OverrideMaterialValue(in OverrideMaterialValueContext context)
    {
        var t = context.ParameterInfo.ParameterType;
        var propertyName = context.PropertyName;
        var parameter = context.ParameterInfo.Parameter;

        if (t == typeof(int))
        {
            var value = parameter.GetValueDirect<int>();
            foreach (var mat in context.Materials)
                mat.SetInt(propertyName, value);
        }
        else if (t == typeof(float))
        {
            var value = parameter.GetValueDirect<float>();
            foreach (var mat in context.Materials)
                mat.SetFloat(propertyName, value);
        }
        else if (t == typeof(bool))
        {
            var value = parameter.GetValueDirect<bool>() ? 1 : 0;
            foreach (var mat in context.Materials)
                mat.SetInt(propertyName, value);
        }
        else if (t == typeof(Vector4))
        {
            var value = parameter.GetValueDirect<Vector4>();
            foreach (var mat in context.Materials)
                mat.SetVector(propertyName, value);
        }
        else if (t == typeof(Color))
        {
            var value = parameter.GetValueDirect<Color>();
            foreach (var mat in context.Materials)
                mat.SetColor(propertyName, value);
        }
    }

    public override bool Equals(object obj) => obj is ShaderProcessor proc && proc.QualifiedName == this.QualifiedName;

    public override int GetHashCode() => this.QualifiedName.GetHashCode();
}
