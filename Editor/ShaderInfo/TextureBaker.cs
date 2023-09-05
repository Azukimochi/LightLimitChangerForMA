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

        private Material _material = new Material(_shader);
        private Dictionary<BakedTextureParameters, Texture2D> _cache = new Dictionary<BakedTextureParameters, Texture2D>();

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

        public Texture2D Bake(string path)
        {
            if (_material == null)
                return null;

            var key = new BakedTextureParameters(this);
            if (_cache.TryGetValue(key, out var value) && value != null)
            {
                return value;
            }

            var absolutePath = Path.Combine(Path.GetDirectoryName(Application.dataPath), path);
            var saveTo = new FileInfo(absolutePath);
            if (saveTo.Exists)
                return null;

            if (!saveTo.Directory.Exists)
                saveTo.Directory.Create();

            var source = Texture;
            var rt = RenderTexture.GetTemporary(source.width, source.height);

            var temp = RenderTexture.active;
            // BlitするとRenderTexture.activeに勝手に入れられてしまうらしい
            Graphics.Blit(source, rt, _material);
            RenderTexture.active = temp;

            var request = AsyncGPUReadback.Request(rt);
            request.WaitForCompletion();

            var data = ImageConversion.EncodeNativeArrayToPNG(request.GetData<Color>(), rt.graphicsFormat, (uint)source.width, (uint)source.height);
            unsafe
            {
                using (var fs = saveTo.Create())
                {
                    var buffer = Utils.ArrayPool<byte>.Rent(data.Length);
                    fixed(byte* p = buffer)
                    {
                        UnsafeUtility.MemCpy(p, data.GetUnsafeReadOnlyPtr(), data.Length);
                    }
                    fs.Write(buffer, 0, data.Length);
                    Utils.ArrayPool<byte>.Return(buffer);
                }
            }

            RenderTexture.ReleaseTemporary(rt);

            AssetDatabase.ImportAsset(path);

            var srcImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(source)) as TextureImporter;
            var dstImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            if (srcImporter != null && dstImporter != null)
            {
                EditorUtility.CopySerialized(srcImporter, dstImporter);
                dstImporter.SaveAndReimport();
            }

            var dest = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            _cache.Add(key, dest);
            return dest;
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
