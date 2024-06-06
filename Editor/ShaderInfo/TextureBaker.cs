using nadena.dev.ndmf;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace io.github.azukimochi
{
    public abstract class TextureBaker
    {
        public delegate TextureFormat TextureFormatFactoryDelegate(Texture sourceTexture, Color baseColor, Texture2D bakedTexture);

        public virtual Texture Texture { get; set; } = Texture2D.whiteTexture;

        public virtual Color Color { get; set; } = Color.white;

        protected LightLimitChangerObjectCache Cache { get; private set; }

        public abstract Texture2D Bake();

        protected Texture2D Bake(Material material, TextureFormatFactoryDelegate formatFactory = null)
        {
            if (material == null)
                return null;

            if (formatFactory == null)
                formatFactory = DefaultTextureFormatFactory;

            var (width, height) = (32, 32);
            var source = Texture;
            if (source != null)
            {
                (width, height) = (Mathf.Max(32, source.width), Mathf.Max(32, source.height));
            }

            var dest = new Texture2D(width, height, TextureFormat.RGBA32, true)
            {
                name = source?.name ?? Color.ToString()
            };
            if (source != null)
                ObjectRegistry.RegisterReplacedObject(source, dest);
            dest.name = source?.name ?? Color.ToString();
            var rt = RenderTexture.GetTemporary(width, height);
            var temp = RenderTexture.active;
            try
            {
                Graphics.Blit(source, rt, material);

                var request = AsyncGPUReadback.Request(rt, 0, TextureFormat.RGBA32);
                request.WaitForCompletion();
                using (var data = request.GetData<Color>())
                    dest.SetPixelData(data, 0);
                dest.Apply(true);

                int quality = (AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(source)) as TextureImporter)?.compressionQuality ?? 50;
                EditorUtility.CompressTexture(dest, formatFactory(source, Color, dest), quality);

                return dest;
            }
            finally
            {
                RenderTexture.active = temp;
                RenderTexture.ReleaseTemporary(rt);
            }
        }

        public virtual void Reset()
        {
            Texture = Texture2D.whiteTexture;
            Color = Color.white;
        }

        private /* static */ TextureFormat DefaultTextureFormatFactory(Texture sourceTexture, Color baseColor, Texture2D bakedTexture)
        {
            if (sourceTexture is Texture2D == false)
            {
                return baseColor.a < 1 ? TextureFormat.DXT5 : TextureFormat.DXT1;
            }
            else
            {
                var tex = sourceTexture as Texture2D;
                var format = tex.format;
                if (baseColor.a < 1)
                {
                    switch (format)
                    {
                        case TextureFormat.RGB24: format = TextureFormat.RGBA32; break;
                        case TextureFormat.DXT1: format = TextureFormat.DXT5; break;
                        case TextureFormat.DXT1Crunched: format = TextureFormat.DXT5Crunched; break;
                    }
                }
                return format;
            }
        }

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

}