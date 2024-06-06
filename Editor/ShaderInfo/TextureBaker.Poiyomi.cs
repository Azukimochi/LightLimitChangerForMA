using UnityEngine;

namespace io.github.azukimochi
{
    internal sealed class PoiyomiTextureBaker : TextureBaker
    {
        public float Saturation { get; set; } = 0;

        public Texture ColorAdjustTexture { get; set; } = Texture2D.whiteTexture;

        private Material Material 
        {
            get
            {
                if (_material == null)
                    _material = new Material(Shader.Find("Hidden/LightLimitChanger/TextureBaker/Poiyomi Toon"));
                return _material;
            }
        }

        private Material _material;

        public override Texture2D Bake()
        {
            var hashCode = new HashCode().Append(Texture).Append(Color).Append(Saturation).Append(ColorAdjustTexture).GetHashCode();
            if (Cache.TryGetBakedTexture(hashCode, out var baked))
                return baked;

            Material.SetTexture("_MainTex", Texture);
            Material.SetColor("_Color", Color);
            Material.EnableKeyword("COLOR_GRADING_HDR");
            Material.SetFloat("_Saturation", Saturation);
            Material.SetTexture("_MainColorAdjustTexture", ColorAdjustTexture);
            baked = Bake(Material);

            Cache.RegisterBakedTexture(hashCode, baked);
            return baked;
        }

        public override void Reset()
        {
            base.Reset();
            Saturation = 0;
            ColorAdjustTexture = Texture2D.whiteTexture;
        }
    }

}