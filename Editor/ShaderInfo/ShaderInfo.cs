using System;
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

        private static void RegisterShaderInfo(ShaderInfo info)
        {
            var count = _ShaderInfoCount;
            if (count >= _RegisteredShaderInfos.Length)
                return;

            _RegisteredShaderInfos[count] = info;
            info.ShaderType = 1 << _ShaderInfoCount;
            _ShaderInfoCount = count + 1;
        }

        public static ReadOnlySpan<ShaderInfo> RegisteredShaderInfos => _RegisteredShaderInfos.AsSpan(0, _ShaderInfoCount);

        private static readonly ShaderInfo[] _RegisteredShaderInfos = new ShaderInfo[31];
        private static int _ShaderInfoCount = 0;

        public static string[] RegisteredShaderInfoNames
        {
            get
            {
                if (_registeredShaderInfoNames?.Length != _ShaderInfoCount)
                {
                    _registeredShaderInfoNames = _RegisteredShaderInfos.Take(_ShaderInfoCount).Select(x => x.Name).ToArray();
                }
                return _registeredShaderInfoNames;
            }
        }
        private static string[] _registeredShaderInfoNames;

        public static bool TryGetShaderInfo(Material material, out ShaderInfo shaderInfo)
        {
            if (material?.shader != null)
            {
                foreach (var info in RegisteredShaderInfos)
                {
                    if (info.IsTargetShader(material.shader))
                    {
                        shaderInfo = info;
                        return true;
                    }
                }
            }
            shaderInfo = null;
            return false;
        }

        public virtual string Name => GetType().Name;

        public int ShaderType { get; private set; }

        public abstract bool TryNormalizeMaterial(Material material, UnityEngine.Object assetContainer);

        public abstract bool IsTargetShader(Shader shader);

        public abstract void SetControlAnimation(in ControlAnimationContainer container, in ControlAnimationParameters parameters);

        public virtual void AdditionalControl(Material material, in LightLimitChangerParameters parameters) { }
    }
}
