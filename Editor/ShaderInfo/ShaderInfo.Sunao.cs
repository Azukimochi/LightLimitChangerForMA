using System;
using UnityEngine;

namespace io.github.azukimochi
{
    partial class ShaderInfo
    {
        public sealed class Sunao : ShaderInfo
        {
            public static Sunao Instance { get; } = new Sunao();

            public const string _MinimumLight = nameof(_MinimumLight);
            public const string _DirectionalLight = nameof(_DirectionalLight);
            public const string _PointLight = nameof(_PointLight);
            public const string _Unlit = nameof(_Unlit);
            public const string _SHLight = nameof(_SHLight);
            public const string _Color = nameof(_Color);
            public const string _MainTex = nameof(_MainTex);
            public const string _SubTex = nameof(_SubTex);
            public const string _SubColor = nameof(_SubColor);

            public override string[] ShaderParameters { get; } = { _MinimumLight, _DirectionalLight, _PointLight, _Unlit, _SHLight, _Color, _MainTex, _SubTex, _SubColor };

            public override Shaders ShaderType => Shaders.Sunao;

            protected override string DefaultShaderName => "Sunao Shader/Opaque";

            public override bool TryNormalizeMaterial(Material material, TextureBaker textureBaker)
            {
                bool result = false;
                bool bakeFlag = false;
                var tex = material.GetTexture(PropertyIDs[_MainTex]);
                if (tex != null) 
                {
                    textureBaker.Texture = tex;

                    {
                        var id = PropertyIDs[_Color];
                        if (DefaultParameters[id] != material.GetColor(id))
                        {
                            textureBaker.Color = material.GetColor(id);
                            material.SetColor(id, DefaultParameters[id].Color);
                            bakeFlag = true;
                        }
                    }

                    if (bakeFlag)
                    {
                        material.SetTexture(PropertyIDs[_MainTex], BakeTexture(textureBaker));
                    }

                    result |= bakeFlag;
                }

                return result;
            }

            public override bool IsTargetShader(Shader shader)
            {
                return
                    shader.name.IndexOf(nameof(Sunao), StringComparison.OrdinalIgnoreCase) != -1 ||
                    ContainsParameter(shader);
            }
        }
    }
}
