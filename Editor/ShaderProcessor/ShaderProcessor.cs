using System.Collections.Generic;
using System.Globalization;
using nadena.dev.modular_avatar.core;

namespace io.github.azukimochi;

internal interface IShaderProcessor
{
    void Initialize(LightLimitChangerProcessor processor);
}

internal abstract class ShaderProcessor : IShaderProcessor
{
    protected const string MaterialAnimationKeyPrefix = "material.";

    protected LightLimitChangerProcessor Processor => processor;
    private LightLimitChangerProcessor processor;

    void IShaderProcessor.Initialize(LightLimitChangerProcessor processor) 
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
    /// テクスチャの焼き込みなどはここで行う
    /// </summary>
    /// <param name="material"></param>
    public virtual void OnMaterialCloned(Material material) { }

    /// <summary>
    /// アニメーションを設定する
    /// </summary>
    public virtual void ConfigureGeneralAnimation(ConfigureGeneralAnimationContext context) { }

    /// <summary>
    /// シェーダー固有のアニメーションの設定
    /// </summary>
    public virtual void ConfigureShaderSpecificAnimation(ConfigureShaderSpecificAnimationContext context) { }

    /// <summary>
    /// シェーダー固有のメニューを生成する
    /// </summary>
    public virtual void CreateShaderSpecificControl(in CreateShaderSpecificControlContext context) {  }
}
