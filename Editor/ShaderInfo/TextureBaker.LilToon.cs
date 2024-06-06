using UnityEngine;

namespace io.github.azukimochi
{
    public sealed class LilToonTextureBaker : TextureBaker
    {
        public Texture Mask { get; set; } = Texture2D.whiteTexture;

        public Texture GradationMap { get; set; } = Texture2D.whiteTexture;

        public Vector4 HSVG { get; set; } = new Vector4(0, 1, 1, 1);

        public float GradationStrength { get; set; } = 0;

        private Material Material
        {
            get
            {
                if (_material == null)
                    _material = new Material(Shader.Find("Hidden/ltsother_baker"));
                return _material;
            }
        }

        private Material _material;

        public override Texture2D Bake()
        {
            var hashCode = new HashCode().Append(Texture).Append(Color).Append(Mask).Append(GradationMap).Append(HSVG).Append(GradationStrength).GetHashCode();
            if (Cache.TryGetBakedTexture(hashCode, out var baked))
                return baked;

            Material.SetTexture("_MainTex", Texture);
            Material.SetColor("_Color", Color);
            Material.SetTexture("_MainColorAdjustMask", Mask);
            Material.SetTexture("_MainGradationTex", GradationMap);
            Material.SetVector("_MainTexHSVG", HSVG);
            Material.SetFloat("_MainGradationStrength", GradationStrength);

            baked = Bake(Material);

            Cache.RegisterBakedTexture(hashCode, baked);
            return baked;
        }

        public override void Reset()
        {
            base.Reset();
            Mask = Texture2D.whiteTexture;
            GradationMap = Texture2D.whiteTexture;
            HSVG = new Vector4(0, 1, 1, 1);
            GradationStrength = 0;
        }
    }

}