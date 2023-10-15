using UnityEditor;
using UnityEngine;

namespace io.github.azukimochi
{
    internal static class Icons
    {
        public static Texture2D LLC => FromGUID("09c1f5650f9952f49a0fbb551e64dcad");
        public static Texture2D Enable => FromGUID("94e101a18f0647c448df7fc3193aa474");
        public static Texture2D Light => FromGUID("68c823f3911f5eb4692149fa6c48fa78");
        public static Texture2D Light_Min => FromGUID("52c2f1b2f7851494d8811a5e125e1eca");
        public static Texture2D Light_Max => FromGUID("e028715830d03fa49b3d4e62fbe1f02f");
        public static Texture2D Temp => FromGUID("94f3542912b540b47a37f7c914c92924");
        public static Texture2D Color => FromGUID("e641931350faa6c4f82ba056f31a1ef6");
        public static Texture2D Unlit => FromGUID("b0b1a25395988c64f887088f1c6748cf");
        public static Texture2D Reset => FromGUID("46b69c6755e703048845eef57e51a329");
        public static Texture2D Settings => FromGUID("75cdf4ab5baf72a4f8b6463d3720bd35");

        private static Texture2D FromGUID(string guid) => AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guid));
    }
}
