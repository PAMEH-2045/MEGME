using GmgAvatarDescriptor =
#if VRC_SDK_VRCSDK2 || VRC_SDK_VRCSDK3
    VRC.SDKBase.VRC_AvatarDescriptor;
#else
    UnityEngine.UI.GraphicRaycaster;
#endif
using System;
using MEGME.Settings;

namespace BlackStartX.GestureManager.Modules
{
    [Serializable]
    public class ModuleSettings
    {
        public GmgAvatarDescriptor favourite;
        public Pose initialPose = Pose.None;
        public int userIndex;

        public float cullingDistance;
        public bool isOnFriendsList;
        public bool simulateCulling;
        public bool loadStored;
        public Setting<float> isRemote = Setting<float>.Create("IsLocal");
        //public bool isRemote;
        public bool vrMode;
    }

    public enum Pose
    {
        None,
        PoseT,
        PoseIK
    }
}