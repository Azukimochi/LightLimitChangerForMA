using System.Collections.Generic;
using UnityEngine.Rendering;

namespace io.github.azukimochi;

internal static class TextureBaker 
{
    private static Dictionary<int, Texture2D> cache = new();

    public static Texture2D GetOrBake(Texture texture, Material material, TextureFormat format = default)
    {
        if (format == default)
            format = GetTextureFormat(texture, material.Get("_Color", Color.white));

        HashCode hash = new();
        hash.Add(texture.GetHashCode());
        hash.Add(material.ComputeCRC());
        hash.Add(format);
        var hashCode = hash.ToHashCode();

        if (cache.TryGetValue(hashCode, out var result) && result != null)
            return result;

        result = Bake(texture, material, format);
        cache[hashCode] = result;
        return result;
    }

    public static Texture2D Bake(Texture texture, Material material, TextureFormat format = default)
    {
        if (material == null)
            return null;

        if (format == default)
            format = GetTextureFormat(texture, material.Get("_Color", Color.white));

        var (width, height) = (1, 1);
        var source = texture == null ? Texture2D.whiteTexture : texture;
        if (source != null)
        {
            (width, height) = (source.width, source.height);
        }

        var dest = new Texture2D(width, height, TextureFormat.RGBA32, true)
        {
            name = $"{source.name}_llc_{GUID.Generate()}"
        };

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
            EditorUtility.CompressTexture(dest, format, quality);

            return dest;
        }
        finally
        {
            RenderTexture.active = temp;
            RenderTexture.ReleaseTemporary(rt);
        }
    }

    public static TextureFormat GetTextureFormat(Texture sourceTexture, Color baseColor)
    {
        if (sourceTexture is not Texture2D tex)
        {
            return baseColor.a < 1 ? TextureFormat.DXT5 : TextureFormat.DXT1;
        }
        else
        {
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
}