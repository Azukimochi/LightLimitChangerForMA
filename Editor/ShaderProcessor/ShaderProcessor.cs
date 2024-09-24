using System.Collections.Generic;
using System.Globalization;
using nadena.dev.modular_avatar.core;

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
    public virtual void OnMaterialCloned(Material material) 
    {
        if (!IsTargetMaterial(material)) 
            return;

        NormalizeMaterial(material);
    }

    /// <summary>
    /// マテリアルの正規化（テクスチャの焼き込みなど）を行う
    /// </summary>
    /// <param name="material"></param>
    public virtual void NormalizeMaterial(Material material) { }

    /// <summary>
    /// パラメーターの種類か名前から操作対象の名前を取得する
    /// </summary>
    public virtual string GetMaterialPropertyNameFromTypeOrName(GeneralControlType type, string name) => null;

    /// <summary>
    /// アニメーションを設定する
    /// </summary>
    public virtual void ConfigureGeneralAnimation(ConfigureGeneralAnimationContext context)
    {
        string propertyName = GetMaterialPropertyNameFromTypeOrName(context.Type, context.Name);
        if (propertyName is null)
            return;

        var (min, max) = context.Range;
        context.Renderers.AnimateAllFloat(context.AnimationClip, $"{MaterialAnimationKeyPrefix}{propertyName}", AnimationCurve.Linear(0, min, 1 / 60f, max));
    }

    /// <summary>
    /// シェーダー固有のアニメーションの設定
    /// </summary>
    public virtual void ConfigureShaderSpecificAnimation(ConfigureShaderSpecificAnimationContext context)
    {
        string propertyName = GetMaterialPropertyNameFromTypeOrName(default, context.Name);
        if (propertyName is null)
            return;

        var (min, max) = context.Range;
        context.Renderers.AnimateAllFloat(context.AnimationClip, $"{MaterialAnimationKeyPrefix}{propertyName}", AnimationCurve.Linear(0, min, 1 / 60f, max));
    }

    /// <summary>
    /// 空のアニメーションを設定する
    /// </summary>
    /// <param name="context"></param>
    public virtual void ConfigreEmptyAnimation(ConfigureEmptyAnimationContext context)
    {
        string propertyName = GetMaterialPropertyNameFromTypeOrName(context.Type, context.Name);
        if (propertyName is null)
            return;

        context.Renderers.AnimateAllFloat(context.AnimationClip, $"{MaterialAnimationKeyPrefix}{propertyName}", AnimationCurve.Constant(0, 0, context.Value));
    }

    /// <summary>
    /// シェーダー固有のメニューを生成する
    /// </summary>
    public virtual void CreateShaderSpecificControl(in CreateShaderSpecificControlContext context) {  }
}
