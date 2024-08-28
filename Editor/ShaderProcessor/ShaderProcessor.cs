using System.Collections.Generic;
using System.Globalization;
using nadena.dev.modular_avatar.core;

namespace io.github.azukimochi;

internal abstract class ShaderProcessor
{
    protected LightLimitChangerProcessor Processor => processor;
    private LightLimitChangerProcessor processor;

    public virtual void Initialize(LightLimitChangerProcessor processor) 
    { 
        this.processor = processor;
    }

    public abstract string QualifiedName { get; }

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
