using System.Collections.Generic;
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
            LightLimitChangerSettings.OnAwake = OnPlayModeEnter;
        }

        public int callbackOrder => -100010;

        internal static bool IsRunning { get; private set; }
        private static bool _isGenerated;

        internal static Dictionary<Material, Dictionary<string, string>> PoiyomiOriginalFlags;

        private static void OnPlayModeEnter(LightLimitChangerSettings settings)
        {
            VRCAvatarDescriptor avatar;
            IsRunning = true;
            if (!_isGenerated && settings.Parameters.GenerateAtBuild && (avatar = settings.gameObject.FindAvatarFromParent()) != null)
            {
                LightLimitGenerator.Generate(avatar, settings);
            }
            _isGenerated = true;
            IsRunning = false;
        }

        bool IVRCSDKPreprocessAvatarCallback.OnPreprocessAvatar(GameObject avatarGameObject)
        {
            IsRunning = true;
            var avatar = avatarGameObject.GetComponent<VRCAvatarDescriptor>();
            if (avatar.TryGetComponentInChildren<LightLimitChangerSettings>(out var settings))
            {
                if (settings.Parameters.GenerateAtBuild)
                {
                    LightLimitGenerator.Generate(avatar, settings);
                }
            }

            IsRunning = false;
            return true;
        }

        void IVRCSDKPostprocessAvatarCallback.OnPostprocessAvatar()
        {
        }
    }
}
