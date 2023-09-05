using System;
using System.Linq;
using UnityEngine;

namespace io.github.azukimochi
{
    partial class ShaderInfo
    {
        public sealed class LilToon : ShaderInfo
        {
            public static LilToon Instance { get; } = new LilToon();

            public const string _LightMinLimit = nameof(_LightMinLimit);
            public const string _LightMaxLimit = nameof(_LightMaxLimit);
            public const string _AsUnlit = nameof(_AsUnlit);
            public const string _MainTexHSVG = nameof(_MainTexHSVG);
            public const string _Color = nameof(_Color);
            public const string _Color2nd = nameof(_Color2nd);
            public const string _Color3rd = nameof(_Color3rd);
            public const string _MainTex = nameof(_MainTex);
            public const string _Main2ndTex = nameof(_Main2ndTex);
            public const string _Main3rdTex = nameof(_Main3rdTex);
            public const string _MainGradationTex = nameof(_MainGradationTex);
            public const string _MainGradationStrength = nameof(_MainGradationStrength);
            public const string _MainColorAdjustMask = nameof(_MainColorAdjustMask);

            public override string[] ShaderParameters { get; } = { _LightMinLimit, _LightMaxLimit, _AsUnlit, _MainTexHSVG, _Color, _Color2nd, _Color3rd, _MainTex, _Main2ndTex, _Main3rdTex, _MainGradationStrength, _MainGradationTex, _MainColorAdjustMask };

            public override Shaders ShaderType => Shaders.lilToon;

            protected override string DefaultShaderName => "lilToon";

            public override bool TryNormalizeMaterial(Material material, TextureBaker textureBaker)
            {
                bool bakeFlag = false;
                bool result = false;

                // MainTexture
                var tex = material.GetTexture(PropertyIDs[_MainTex]);
                if (tex != null)
                {
                    textureBaker.Texture = tex;
                    bool isColorAdjusted = false;

                    // MainColor
                    {
                        var id = PropertyIDs[_Color];
                        if (DefaultParameters[id] != material.GetColor(id))
                        {
                            textureBaker.Color = material.GetColor(id);
                            material.SetColor(id, DefaultParameters[id].Color);
                            bakeFlag = true;
                        }
                    }

                    // HSV / Gamma
                    {
                        var id = PropertyIDs[_MainTexHSVG];
                        if (DefaultParameters[id] != material.GetVector(id))
                        {
                            textureBaker.HSVG = material.GetVector(id);
                            material.SetVector(id, DefaultParameters[id].Vector);
                            bakeFlag = true;
                            isColorAdjusted = true;
                        }
                    }

                    // Gradation
                    {
                        var id = PropertyIDs[_MainGradationTex];
                        var id2 = PropertyIDs[_MainGradationStrength];
                        if (material.GetTexture(id) != null && DefaultParameters[id2] != material.GetFloat(id2))
                        {
                            textureBaker.GradationMap = material.GetTexture(id);
                            textureBaker.GradationStrength = material.GetFloat(id2);
                            material.SetTexture(id, null);
                            material.SetFloat(id2, DefaultParameters[id2].Float);
                            bakeFlag = true;
                            isColorAdjusted = true;
                        }
                    }

                    // Color Adujust Mask
                    {
                        var id = PropertyIDs[_MainColorAdjustMask];
                        if (material.GetTexture(id) != null && isColorAdjusted)
                        {
                            textureBaker.Mask = material.GetTexture(id);
                            material.SetTexture(id, null);
                            bakeFlag = true;
                        }
                    }

                    // Run Bake
                    if (bakeFlag)
                    {
                        material.SetTexture(PropertyIDs[_MainTex], BakeTexture(textureBaker));
                    }

                    result |= bakeFlag;
                }

                // 2nd Texture
                tex = material.GetTexture(PropertyIDs[_Main2ndTex]);
                if (tex != null)
                {
                    if (bakeFlag)
                    {
                        textureBaker.ResetParamerter();
                        bakeFlag = false;
                    }

                    textureBaker.Texture = tex;

                    var id = PropertyIDs[_Color2nd];
                    if (DefaultParameters[id] != material.GetColor(id))
                    {
                        textureBaker.Color = material.GetColor(id);
                        material.SetColor(id, DefaultParameters[id].Color);
                        bakeFlag = true;
                    }

                    if (bakeFlag)
                    {
                        material.SetTexture(PropertyIDs[_Main2ndTex], BakeTexture(textureBaker));
                    }
                    result |= bakeFlag;
                }

                // 3rd Texture
                tex = material.GetTexture(PropertyIDs[_Main3rdTex]);
                if (tex != null)
                {
                    if (bakeFlag)
                    {
                        textureBaker.ResetParamerter();
                        bakeFlag = false;
                    }

                    textureBaker.Texture = tex;

                    var id = PropertyIDs[_Color3rd];
                    if (DefaultParameters[id] != material.GetColor(id))
                    {
                        textureBaker.Color = material.GetColor(id);
                        material.SetColor(id, DefaultParameters[id].Color);
                        bakeFlag = true;
                    }

                    if (bakeFlag)
                    {
                        material.SetTexture(PropertyIDs[_Main3rdTex], BakeTexture(textureBaker));
                    }
                    result |= bakeFlag;
                }

                return result;
            }

            public override bool IsTargetShader(Shader shader)
            {
                return
                    shader.name.IndexOf(nameof(LilToon), StringComparison.OrdinalIgnoreCase) != -1 ||
                    ContainsParameter(shader);
            }
        }
    }
}
