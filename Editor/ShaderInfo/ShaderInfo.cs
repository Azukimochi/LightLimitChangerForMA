using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace io.github.azukimochi
{
    internal abstract partial class ShaderInfo
    {
        protected ShaderInfo() { }

        public static ReadOnlySpan<ShaderInfo> ShaderInfos => _ShaderInfos;

        private static readonly ShaderInfo[] _ShaderInfos = 
        { 
            LilToon.Instance, 
            Sunao.Instance, 
            Poiyomi.Instance
        };

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

        public static bool TryGetShaderType(Material material, out Shaders shaderType)
        {
            bool result = TryGetShaderInfo(material, out var info);
            shaderType = result ? info.ShaderType : 0;
            return result;
        }


        public Dictionary<string, int> PropertyIDs
        {
            get
            {
                if (_propertyIds == null)
                {
                    _propertyIds = ShaderParameters.ToDictionary(x => x, Shader.PropertyToID);
                }
                return _propertyIds;
            }
        }
        private Dictionary<string, int> _propertyIds;

        public Dictionary<int, ShaderPropertyValue> DefaultParameters
        {
            get
            {
                if (_defaultParameters == null)
                {
                    var shader = Shader.Find(DefaultShaderName);
                    if (shader != null)
                    {
                        var count = shader.GetPropertyCount();
                        var dict = new Dictionary<int, ShaderPropertyValue>(count);
                        for(int i = 0; i < count; i++)
                        {
                            ShaderPropertyValue value;
                            var id = shader.GetPropertyNameId(i);
                            if (dict.ContainsKey(id))
                                continue;

                            var type = shader.GetPropertyType(i);
                            switch (type)
                            {
                                case ShaderPropertyType.Float:
                                case ShaderPropertyType.Range:
                                    value = new ShaderPropertyValue(type, shader.GetPropertyDefaultFloatValue(i));
                                    break;
                                case ShaderPropertyType.Color:
                                case ShaderPropertyType.Vector:
                                    value = new ShaderPropertyValue(type, shader.GetPropertyDefaultVectorValue(i));
                                    break;
                                default:
                                    continue;
                            }
                            dict.Add(id, value);
                        }
                        _defaultParameters = dict;
                    }
                }
                return _defaultParameters;
            }
        }
        private Dictionary<int, ShaderPropertyValue> _defaultParameters;

        protected bool ContainsParameter(Shader shader)
        {
            var propertyIds = PropertyIDs;
            return propertyIds.Keys.Intersect(shader.EnumeratePropertyNames()).Count() == propertyIds.Count;
        }

        protected static Texture2D BakeTexture(TextureBaker baker)
        {
            var path = Path.Combine(Utils.GetGeneratedAssetsFolder(), $"{baker.Texture?.name ?? $"{baker.Color}"}_{GUID.Generate()}.png");
            return baker.Bake(path);
        }


        protected abstract string DefaultShaderName { get; }

        public abstract string[] ShaderParameters { get; }

        public abstract Shaders ShaderType { get; }

        public abstract bool TryNormalizeMaterial(Material material, TextureBaker textureBaker);

        public abstract bool IsTargetShader(Shader shader);
    }
}
