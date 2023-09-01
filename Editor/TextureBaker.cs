using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Networking.Types;
using UnityEngine.Rendering;
using VRC.Core.Pool;

namespace io.github.azukimochi
{
    public readonly struct TextureBaker
    {
        private const string TextureBakerShaderName = "Hidden/LightLimitChanger/TextureBaker";
        private static Shader _shader = Shader.Find(TextureBakerShaderName);
        private static readonly int _TexturePropertyID = Shader.PropertyToID("_MainTex");
        private static readonly int _MaskPropertyID = Shader.PropertyToID("_Mask");
        private static readonly int _GradationMapPropertyID = Shader.PropertyToID("_GradationMap");
        private static readonly int _ColorPropertyID = Shader.PropertyToID($"_{nameof(Color)}");
        private static readonly int _HSVGPropertyID = Shader.PropertyToID($"_{nameof(HSVG)}");
        private static readonly int _GradationStrengthPropertyID = Shader.PropertyToID($"_{nameof(GradationStrength)}");

        public static TextureBaker Create() => new TextureBaker(new Material(_shader));

        public TextureBaker(Material material) => _material = material;

        private readonly Material _material;


        public Texture Texture { get => _material?.GetTexture(_TexturePropertyID); set => _material?.SetTexture(_TexturePropertyID, value); }

        public Texture Mask { get => _material?.GetTexture(_MaskPropertyID); set => _material?.SetTexture(_MaskPropertyID, value); }

        public Texture GradationMap { get => _material?.GetTexture(_GradationMapPropertyID); set => _material?.SetTexture(_GradationMapPropertyID, value); }

        public Color Color { get => _material?.GetColor(_ColorPropertyID) ?? Color.black; set => _material?.SetColor(_ColorPropertyID, value); }

        public Vector4 HSVG { get => _material?.GetVector(_HSVGPropertyID) ?? Vector4.zero; set => _material?.SetVector(_HSVGPropertyID, value); }

        public float GradationStrength { get => _material?.GetFloat(_GradationStrengthPropertyID) ?? 0; set => _material?.SetFloat(_GradationStrengthPropertyID, Mathf.Clamp01(value)); }

        public Texture2D Bake(string path)
        {
            if (_material == null)
                return null;

            var absolutePath = Path.Combine(Path.GetDirectoryName(Application.dataPath), path);
            var saveTo = new FileInfo(absolutePath);
            if (saveTo.Exists)
                return null;

            if (!saveTo.Directory.Exists)
                saveTo.Directory.Create();

            var source = Texture;
            var rt = RenderTexture.GetTemporary(source.width, source.height);

            Graphics.Blit(source, rt, _material);
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
            return dest;
        }
    }
}
