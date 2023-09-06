using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace io.github.azukimochi
{
    [InitializeOnLoad]
    internal abstract partial class ShaderInfo
    {
        protected ShaderInfo() { }

        static ShaderInfo()
        {
            RegisterShaderInfo(LilToon.Instance);
            RegisterShaderInfo(Sunao.Instance);
            RegisterShaderInfo(Poiyomi.Instance);

            // TODO: Register extended ShaderInfo from plugins
        }

        public static void RegisterShaderInfo(ShaderInfo info)
        {
            var count = _ShaderInfoCount;
            if (count >= _ShaderInfos.Length)
                return;

            _ShaderInfos[count] = info;
            info.ShaderType = (Shaders)(1ul << count);
            _ShaderInfoCount = count + 1;
        }

        public static IEnumerable<ShaderInfo> ShaderInfos => _ShaderInfos.Take(_ShaderInfoCount);

        private static readonly ShaderInfo[] _ShaderInfos = new ShaderInfo[64];
        private static int _ShaderInfoCount = 0;

        public static bool TryGetShaderInfo(Material material, out ShaderInfo shaderInfo)
        {
            if (material != null)
            {
                var shader = material.shader;
                if (shader != null)
                {
                    foreach (var info in _ShaderInfos)
                    {
                        if (info.IsTargetShader(shader))
                        {
                            shaderInfo = info;
                            return true;
                        }
                    }
                }
            }
            shaderInfo = null;
            return false;
        }

        public static bool TryGetShaderInfo(Shaders shaderType, out ShaderInfo shaderInfo)
        {
            foreach (var info in _ShaderInfos)
            {
                if (info.ShaderType == shaderType)
                {
                    shaderInfo = info;
                    return true;
                }
            }
            shaderInfo = null;
            return false;
        }

        public static bool TryGetShaderType(Material material, out Shaders shaderType)
        {
            bool result = TryGetShaderInfo(material, out var info);
            shaderType = result ? info.ShaderType : 0;
            return result;
        }

        public Shaders ShaderType { get; private set; }

        public abstract bool TryNormalizeMaterial(Material material, TextureBaker textureBaker);

        public abstract bool IsTargetShader(Shader shader);

        public abstract void SetControlAnimation(in ControlAnimationContainer container, in ControlAnimationParameters parameters);

    }
}
