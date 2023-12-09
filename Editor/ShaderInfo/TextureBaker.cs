using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace io.github.azukimochi
{
    public abstract class TextureBaker
    {
        public abstract Texture2D Bake();

        public abstract void Reset();

        protected LightLimitChangerObjectCache Cache;

        public static T GetInstance<T>(LightLimitChangerObjectCache cache, bool forceReset = true) where T : TextureBaker, new()
        {
            var instance = TextureBakerCache<T>.Default;
            if (forceReset)
                instance.Reset();
            instance.Cache = cache;
            return instance;
        }

        private static class TextureBakerCache<T> where T : TextureBaker, new()
        {
            public static readonly T Default = new T();
        }
    }

    public class DefaultTextureBaker : TextureBaker
    {
        private const string TextureBakerShaderName = "Hidden/LightLimitChanger/TextureBaker";
        private static readonly int _TexturePropertyID = Shader.PropertyToID("_MainTex");
        private static readonly int _MaskPropertyID = Shader.PropertyToID($"_{nameof(Mask)}");
        private static readonly int _GradationMapPropertyID = Shader.PropertyToID($"_{nameof(GradationMap)}");
        private static readonly int _ColorPropertyID = Shader.PropertyToID($"_{nameof(Color)}");
        private static readonly int _HSVGPropertyID = Shader.PropertyToID($"_{nameof(HSVG)}");
        private static readonly int _GradationStrengthPropertyID = Shader.PropertyToID($"_{nameof(GradationStrength)}");

        protected Material Material
        {
            get
            {
                if (_material == null)
                    _material = new Material(Shader.Find(TextureBakerShaderName));
                return _material;
            }
        }
        private Material _material;

        public Texture Texture { get => Material?.GetTexture(_TexturePropertyID); set => Material?.SetTexture(_TexturePropertyID, value); }

        public Texture Mask { get => Material?.GetTexture(_MaskPropertyID); set => Material?.SetTexture(_MaskPropertyID, value); }

        public Texture GradationMap { get => Material?.GetTexture(_GradationMapPropertyID); set => Material?.SetTexture(_GradationMapPropertyID, value); }

        public Color Color { get => Material?.GetColor(_ColorPropertyID) ?? Color.black; set => Material?.SetColor(_ColorPropertyID, value); }

        public Vector4 HSVG { get => Material?.GetVector(_HSVGPropertyID) ?? Vector4.zero; set => Material?.SetVector(_HSVGPropertyID, value); }

        public float GradationStrength { get => Material?.GetFloat(_GradationStrengthPropertyID) ?? 0; set => Material?.SetFloat(_GradationStrengthPropertyID, Mathf.Clamp01(value)); }

        public override void Reset()
        {
            Texture = null;
            Mask = null;
            GradationMap = null;
            Color = Color.white;
            HSVG = new Vector4(0, 1, 1, 1);
            GradationStrength = 0;
        }

        public override Texture2D Bake()
        {
            if (Material == null)
                return null;

            var key = new BakedTextureParameters(this);
            if (Cache.TryGetBakedTexture(key, out var value) && value != null)
            {
                return value;
            }

            var (width, height) = (32, 32);
            var source = Texture;
            if (source != null)
            {
                (width, height) = (Mathf.Max(32, source.width), Mathf.Max(32, source.height));
            }

            var dest = new Texture2D(width, height, TextureFormat.RGBA32, true);
            dest.name = source?.name ?? Color.ToString();
            var rt = RenderTexture.GetTemporary(width, height);
            var temp = RenderTexture.active;
            try
            {
                Graphics.Blit(source, rt, Material);

                var request = AsyncGPUReadback.Request(rt);
                request.WaitForCompletion();
                dest.SetPixelData(request.GetData<Color>(), 0);
                dest.Apply(true);

                TextureFormat format;
                if (source is Texture2D == false)
                {
                    format = Color.a < 1 ? TextureFormat.DXT5 : TextureFormat.DXT1;
                }
                else
                {
                    var tex = source as Texture2D;
                    format = tex.format;
                    if (Color.a < 1)
                    {
                        switch (format)
                        {
                            case TextureFormat.RGB24: format = TextureFormat.RGBA32; break;
                            case TextureFormat.DXT1: format = TextureFormat.DXT5; break;
                            case TextureFormat.DXT1Crunched: format = TextureFormat.DXT5Crunched; break;
                        }
                    }
                }
                int quality = (AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(source)) as TextureImporter)?.compressionQuality ?? 50;
                EditorUtility.CompressTexture(dest, format, quality);

                Cache.RegisterBakedTexture(key, dest);
                return dest;
            }
            finally
            {
                RenderTexture.active = temp;
                RenderTexture.ReleaseTemporary(rt);
            }
        }

        private sealed class BakedTextureParameters
        {
            public BakedTextureParameters(DefaultTextureBaker baker)
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

            public override bool Equals(object obj) => obj is BakedTextureParameters other && Equals(other);

            public bool Equals(BakedTextureParameters other)
            {
                return
                    this.Texture == other.Texture &&
                    this.Mask == other.Mask &&
                    this.GradationMap == other.GradationMap &&
                    this.Color == other.Color &&
                    this.HSVG == other.HSVG &&
                    this.GradationStrength == other.GradationStrength;
            }

            public override int GetHashCode() => new HashCode()
                .Append(Texture)
                .Append(Mask)
                .Append(GradationMap)
                .Append(Color)
                .Append(HSVG)
                .Append(GradationStrength)
                .GetHashCode();
        }
    }

    internal sealed class PoiyomiTextureBaker : DefaultTextureBaker
    {
        public override Texture2D Bake()
        {
            Material?.EnableKeyword("_POIYOMI");
            return base.Bake();
        }
    }
}