using System;
using System.Collections.Generic;
using System.IO;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace io.github.azukimochi
{
    public sealed class TextureBaker : IDisposable
    {
        private const string TextureBakerShaderName = "Hidden/LightLimitChanger/TextureBaker";
        private static Shader _shader = Shader.Find(TextureBakerShaderName);
        private static readonly int _TexturePropertyID = Shader.PropertyToID("_MainTex");
        private static readonly int _MaskPropertyID = Shader.PropertyToID($"_{nameof(Mask)}");
        private static readonly int _GradationMapPropertyID = Shader.PropertyToID($"_{nameof(GradationMap)}");
        private static readonly int _ColorPropertyID = Shader.PropertyToID($"_{nameof(Color)}");
        private static readonly int _HSVGPropertyID = Shader.PropertyToID($"_{nameof(HSVG)}");
        private static readonly int _GradationStrengthPropertyID = Shader.PropertyToID($"_{nameof(GradationStrength)}");

        private Dictionary<BakedTextureParameters, Texture2D> _cache = new Dictionary<BakedTextureParameters, Texture2D>();
        private Material _material = new Material(_shader);
        private Object _rootArtifact;

        public TextureBaker(Object rootArtifact)
        {
            _rootArtifact = rootArtifact;
        }


        public Texture Texture { get => _material?.GetTexture(_TexturePropertyID); set => _material?.SetTexture(_TexturePropertyID, value); }

        public Texture Mask { get => _material?.GetTexture(_MaskPropertyID); set => _material?.SetTexture(_MaskPropertyID, value); }

        public Texture GradationMap { get => _material?.GetTexture(_GradationMapPropertyID); set => _material?.SetTexture(_GradationMapPropertyID, value); }

        public Color Color { get => _material?.GetColor(_ColorPropertyID) ?? Color.black; set => _material?.SetColor(_ColorPropertyID, value); }

        public Vector4 HSVG { get => _material?.GetVector(_HSVGPropertyID) ?? Vector4.zero; set => _material?.SetVector(_HSVGPropertyID, value); }

        public float GradationStrength { get => _material?.GetFloat(_GradationStrengthPropertyID) ?? 0; set => _material?.SetFloat(_GradationStrengthPropertyID, Mathf.Clamp01(value)); }

        public bool IsPoiyomiMode { get => _material?.IsKeywordEnabled("_POIYOMI") ?? false; set => _material?.EnableKeyword("_POIYOMI"); }

        public void ResetParamerter()
        {
            Texture = null;
            Mask = null;
            GradationMap = null;
            Color = Color.white;
            HSVG = new Vector4(0, 1, 1, 1);
            GradationStrength = 0; 
        }

        public Texture2D Bake()
        {
            if (_material == null)
                return null;

            var key = new BakedTextureParameters(this);
            if (_cache.TryGetValue(key, out var value) && value != null)
            {
                return value;
            }

            var (width, height) = (32, 32);
            var source = Texture;
            if (source != null)
            {
                (width, height) = (source.width, source.height);
            }

            var dest = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var rt = RenderTexture.GetTemporary(width, height);
            var temp = RenderTexture.active;
            try
            {
                Graphics.Blit(source, rt, _material);

                var request = AsyncGPUReadback.Request(rt);
                request.WaitForCompletion();
                dest.LoadRawTextureData(request.GetData<Color>());
                dest.Apply();

                var format = (source as Texture2D)?.format ?? (Color.a == 1 ? TextureFormat.DXT1Crunched : TextureFormat.DXT5);
                int quality = (AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(source)) as TextureImporter)?.compressionQuality ?? 50;
                EditorUtility.CompressTexture(dest, format, quality);

                _cache.Add(key, dest);
                dest.AddTo(_rootArtifact);
                return dest;
            }
            finally
            {
                RenderTexture.active = temp;
                RenderTexture.ReleaseTemporary(rt);
            }
        }

        public void Dispose()
        {
            Object.DestroyImmediate(_material);
            _cache.Clear();
        }

        private readonly struct BakedTextureParameters : IEquatable<BakedTextureParameters>
        {
            public BakedTextureParameters(TextureBaker baker)
            {
                Texture = baker.Texture;
                Mask = baker.Mask;
                GradationMap = baker.GradationMap;
                Color = baker.Color;
                HSVG = baker.HSVG;
                GradationStrength = baker.GradationStrength;
            }

            public readonly Texture Texture;
            public readonly Texture Mask;
            public readonly Texture GradationMap;
            public readonly Color Color;
            public readonly Vector4 HSVG;
            public readonly float GradationStrength;

            public bool Equals(BakedTextureParameters other) => Equals(in other);
            public override bool Equals(object obj) => obj is BakedTextureParameters other && Equals(in other);

            public bool Equals(in BakedTextureParameters other)
            {
                return
                    this.Texture == other.Texture &&
                    this.Mask == other.Mask &&
                    this.GradationMap == other.GradationMap &&
                    this.Color == other.Color &&
                    this.HSVG == other.HSVG &&
                    this.GradationStrength == other.GradationStrength;
            }

            public override int GetHashCode()
            {
                const int Prime1 = 1117;
                const int Prime2 = 1777;

                int hash = Prime1;
                unchecked
                {
                    hash = hash * Prime2 + Texture?.GetHashCode() ?? 0;
                    hash = hash * Prime2 + Mask?.GetHashCode() ?? 0;
                    hash = hash * Prime2 + GradationMap?.GetHashCode() ?? 0;
                    hash = hash * Prime2 + Color.GetHashCode();
                    hash = hash * Prime2 + HSVG.GetHashCode();
                    hash = hash * Prime2 + GradationStrength.GetHashCode();
                }
                return hash;
            }
        }
    }
}
