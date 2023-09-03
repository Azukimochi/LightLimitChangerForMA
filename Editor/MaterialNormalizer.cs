using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace io.github.azukimochi
{
    internal static class MaterialNormalizer
    {
        public static bool Normalize(Material material)
        {
            var type = LightLimitGenerator.GetShaderType(material?.shader);
            if (type == 0)
                return false;

            // TODO

            return true;
        }
    }
}
