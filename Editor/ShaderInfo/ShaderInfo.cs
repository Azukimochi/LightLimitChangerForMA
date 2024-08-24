using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

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

            try
            {
                var self = Assembly.GetExecutingAssembly();
                foreach (var type in AppDomain.CurrentDomain.GetAssemblies().Where(x => x != self).SelectMany(x => x.GetTypes()).Where(x => !x.IsAbstract && typeof(ShaderInfo).IsAssignableFrom(x)))
                {
                    var instance = Activator.CreateInstance(type) as ShaderInfo;
                    RegisterShaderInfo(instance);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }

            InitializeRuntimeMethods();
        }

        private static void RegisterShaderInfo(ShaderInfo info)
        {
            if (info is null)
                return;

            var count = _ShaderInfoCount;
            if (count >= _RegisteredShaderInfos.Length)
                return;

            _RegisteredShaderInfos[count] = info;
            info.ShaderType = 1 << _ShaderInfoCount;
            _ShaderInfoCount = count + 1;
        }

        private static void InitializeRuntimeMethods()
        {
            RuntimeShaderInfo.FromBitMask = ShaderInfoUtility.FromBitMask;
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

        public abstract bool TryNormalizeMaterial(Material material, LightLimitChangerObjectCache cache);

        public abstract bool IsTargetShader(Shader shader);

        public abstract void SetControlAnimation(in ControlAnimationContainer container, in ControlAnimationParameters parameters, in LightLimitChangerParameters llc_parameters);

        public virtual void AdditionalControl(Material material, in LightLimitChangerParameters parameters) { }

        public virtual bool TryGetLightMinMaxValue(Material material, out float min, out float max)
        {
            min = 0;
            max = 0;
            return false;
        }

        public virtual bool TryGetMonochromeValue(Material material, out float monochrome, out float monochromeAdditive)
        {
            monochrome = 0;
            monochromeAdditive = 1;
            return false;
        }
    }
}
