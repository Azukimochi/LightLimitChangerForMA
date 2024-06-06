using UnityEngine;

namespace io.github.azukimochi
{
    internal sealed class DefaultTextureBaker : TextureBaker
    {
        private Material Material
        {
            get
            {
                if (_material == null)
                    _material = new Material(Shader.Find("Hidden/LightLimitChanger/TextureBaker/Default"));
                return _material;
            }
        }

        private Material _material;

        public override Texture2D Bake()
        {
            var hashCode = new HashCode().Append(Texture).Append(Color).GetHashCode();
            if (Cache.TryGetBakedTexture(hashCode, out var baked))
                return baked;

            Material.SetTexture("_MainTex", Texture);
            Material.SetColor("_Color", Color);

            baked = Bake(Material);

            Cache.RegisterBakedTexture(hashCode, baked);
            return baked;
        }
    }

}