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

        void IVRCSDKPostprocessAvatarCallback.OnPostprocessAvatar()
        {
        }

        bool IVRCSDKPreprocessAvatarCallback.OnPreprocessAvatar(GameObject avatarGameObject)
        {
            if (avatarGameObject.TryGetComponentInChildren<LightLimitChangerSettings>(out var settings))
            {
                if (settings.Parameters.GenerateAtBuild)
                {
                    LightLimitGenerator.Generate(avatarGameObject.GetComponent<VRCAvatarDescriptor>(), settings);
                }
            }

            return true;
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
