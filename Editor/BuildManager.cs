using System.Collections.Generic;
using System.IO;
using nadena.dev.modular_avatar.core.editor;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase.Editor.BuildPipeline;

namespace io.github.azukimochi
{
    [InitializeOnLoad]
    public class BuildManager : IVRCSDKPreprocessAvatarCallback, IVRCSDKPostprocessAvatarCallback
    {
        static BuildManager()
        {
            // 多分これが一番早いと思います
            LightLimitChangerSettings.OnAwake = default(BuildManager).BuildOnPlayMode;
        }

        public int callbackOrder => new AvatarProcessor().callbackOrder - 1;

        internal static Dictionary<Material, Dictionary<string, string>> PoiyomiOriginalFlags;

        bool IVRCSDKPreprocessAvatarCallback.OnPreprocessAvatar(GameObject avatarGameObject)
        {
            var avatar = avatarGameObject.GetComponent<VRCAvatarDescriptor>();
            if (avatar.TryGetComponentInChildren<LightLimitChangerSettings>(out var settings))
            {
                if (settings.Parameters.GenerateAtBuild)
                {
                    LightLimitGenerator.Generate(avatar, settings);
                }
                if (settings.Parameters.AllowOverridePoiyomiAnimTag)
                {
                    LightLimitGenerator.ConfigurePoiyomiAnimated(avatar, settings);
                }

            }

            return true;
        }

        void IVRCSDKPostprocessAvatarCallback.OnPostprocessAvatar()
        {
            foreach (var settings in GameObject.FindObjectsOfType<LightLimitChangerSettings>())
            {
                LightLimitGenerator.ResetPoiyomiAnimated(settings);
            }
        }
    }

    internal static class BuildManagerHelper
    {
        public static void BuildOnPlayMode(this BuildManager _, LightLimitChangerSettings settings)
        {
            VRCAvatarDescriptor avatar;
            if (settings.Parameters.GenerateAtBuild && (avatar = settings.gameObject.FindAvatarFromParent()) != null)
            {
                LightLimitGenerator.Generate(avatar, settings);
            }
        }
    }
}
