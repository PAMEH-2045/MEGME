#if VRC_SDK_VRCSDK3
using BlackStartX.GestureManager.Library;
using System.Collections.Generic;
using System.IO;
using System.Text;
//using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using AnimLayerType = VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType;
using BlendableAnimatorLayer = VRC.SDKBase.VRC_AnimatorLayerControl.BlendableLayer;
using BlendablePlayableLayer = VRC.SDKBase.VRC_PlayableLayerControl.BlendableLayer;
using TrackingType = VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType;
using ValueType = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.ValueType;
using System.Reflection;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3
{
    public static class ModuleVrc3Styles
    {
        private static GUIStyle _url;
        private static GUIStyle _urlPro;

        private static Texture2D _emojis;
        private static Texture2D _option;
        private static Texture2D _expressions;
        private static Texture2D _tools;
        private static Texture2D _back;
        private static Texture2D _backHome;
        private static Texture2D _default;
        private static Texture2D _gear;
        private static Texture2D _reset;
        private static Texture2D _twoAxis;
        private static Texture2D _fourAxis;
        private static Texture2D _radial;
        private static Texture2D _toggle;
        private static Texture2D _resetHeight;
        private static Texture2D _avatarHeight;
        private static Texture2D _releasePoses;
        private static Texture2D _runningParam;
        private static Texture2D _axisUp;
        private static Texture2D _axisRight;
        private static Texture2D _axisDown;
        private static Texture2D _axisLeft;
        private static Texture2D _supportLike;
        private static Texture2D _supportGold;
        private static Texture2D _supportHeart;
        private static Texture2D _toolCamera;
        private static Texture2D _toolClick;
        private static Texture2D _toolPose;
        private static Texture2D _earmuffs;
        private static Texture2D _fallingSpeed;
        private static Texture2D _fullBody;
        private static Texture2D _generic;
        private static Texture2D _gestureLeftWeight;
        private static Texture2D _gestureRightWeight;
        private static Texture2D _grounded;
        private static Texture2D _handsOnly;
        private static Texture2D _headHands;
        private static Texture2D _poseIk;
        private static Texture2D _muteSelf;
        private static Texture2D _seated;
        private static Texture2D _poseT;
        private static Texture2D _upright;
        private static Texture2D _velocity;
        private static Texture2D _visemes;
        private static Texture2D _vRMode;
        private static Texture2D _afk;
        private static Texture2D _fourPoint;
        private static Texture2D _uninitialized;
        private static Texture2D _isLocal;
        private static Texture2D _extras;
        private static Texture2D _isOnFriendsList;

        internal static GUIStyle Url => _url ??= new GUIStyle(GUI.skin.label) { padding = new RectOffset(-6, -6, 1, 0), normal = { textColor = Color.blue } };
        internal static GUIStyle UrlPro => _urlPro ??= new GUIStyle(GUI.skin.label) { padding = new RectOffset(-6, -6, 1, 0), normal = { textColor = Color.cyan } };

        internal static Texture2D Emojis => !_emojis ? _emojis = LoadEmbeddedTexture("Vrc3.BSX_GM_Emojis.png") : _emojis;
        internal static Texture2D Option => !_option ? _option = LoadEmbeddedTexture("Vrc3.BSX_GM_Option.png") : _option;
        internal static Texture2D Expressions => !_expressions ? _expressions = LoadEmbeddedTexture("Vrc3.BSX_GM_Expressions.png") : _expressions;
        internal static Texture2D Tools => !_tools ? _tools = LoadEmbeddedTexture("Vrc3.BSX_GM_Tools.png") : _tools;
        internal static Texture2D Back => !_back ? _back = LoadEmbeddedTexture("Vrc3.BSX_GM_Back.png") : _back;
        internal static Texture2D BackHome => !_backHome ? _backHome = LoadEmbeddedTexture("Vrc3.BSX_GM_BackHome.png") : _backHome;
        internal static Texture2D Default => !_default ? _default = LoadEmbeddedTexture("Vrc3.BSX_GM_Default.png") : _default;
        internal static Texture2D Gear => !_gear ? _gear = LoadEmbeddedTexture("Vrc3.BSX_GM_Gear.png") : _gear;
        internal static Texture2D Reset => !_reset ? _reset = LoadEmbeddedTexture("Vrc3.BSX_GM_Reset.png") : _reset;
        internal static Texture2D TwoAxis => !_twoAxis ? _twoAxis = LoadEmbeddedTexture("Vrc3.BSX_GM_2_Axis.png") : _twoAxis;
        internal static Texture2D FourAxis => !_fourAxis ? _fourAxis = LoadEmbeddedTexture("Vrc3.BSX_GM_4_Axis.png") : _fourAxis;
        internal static Texture2D Radial => !_radial ? _radial = LoadEmbeddedTexture("Vrc3.BSX_GM_Radial.png") : _radial;
        internal static Texture2D Toggle => !_toggle ? _toggle = LoadEmbeddedTexture("Vrc3.BSX_GM_Toggle.png") : _toggle;
        internal static Texture2D ResetHeight => !_resetHeight ? _resetHeight = LoadEmbeddedTexture("Vrc3.BSX_GM_Reset_Height.png") : _resetHeight;
        internal static Texture2D AvatarHeight => !_avatarHeight ? _avatarHeight = LoadEmbeddedTexture("Vrc3.BSX_GM_Avatar_Height.png") : _avatarHeight;
        internal static Texture2D ReleasePoses => !_releasePoses ? _releasePoses = LoadEmbeddedTexture("Vrc3.BSX_GM_Release_Poses.png") : _releasePoses;
        internal static Texture2D RunningParam => !_runningParam ? _runningParam = LoadEmbeddedTexture("Vrc3.BSX_GM_Running_Param.png") : _runningParam;
        internal static Texture2D AxisUp => !_axisUp ? _axisUp = LoadEmbeddedTexture("Vrc3.BSX_GM_Axis_Up.png") : _axisUp;
        internal static Texture2D AxisRight => !_axisRight ? _axisRight = LoadEmbeddedTexture("Vrc3.BSX_GM_Axis_Right.png") : _axisRight;
        internal static Texture2D AxisDown => !_axisDown ? _axisDown = LoadEmbeddedTexture("Vrc3.BSX_GM_Axis_Down.png") : _axisDown;
        internal static Texture2D AxisLeft => !_axisLeft ? _axisLeft = LoadEmbeddedTexture("Vrc3.BSX_GM_Axis_Left.png") : _axisLeft;
        internal static Texture2D SupportLike => !_supportLike ? _supportLike = LoadEmbeddedTexture("Vrc3.BSX_GM_Support_Like.png") : _supportLike;
        internal static Texture2D SupportGold => !_supportGold ? _supportGold = LoadEmbeddedTexture("Vrc3.BSX_GM_Support_Gold.png") : _supportGold;
        internal static Texture2D SupportHeart => !_supportHeart ? _supportHeart = LoadEmbeddedTexture("Vrc3.BSX_GM_Support_Heart.png") : _supportHeart;
        internal static Texture2D ToolCamera => !_toolCamera ? _toolCamera = LoadEmbeddedTexture("Vrc3.BSX_GM_Tool_Camera.png") : _toolCamera;
        internal static Texture2D ToolClick => !_toolClick ? _toolClick = LoadEmbeddedTexture("Vrc3.BSX_GM_Tool_Click.png") : _toolClick;
        internal static Texture2D ToolPose => !_toolPose ? _toolPose = LoadEmbeddedTexture("Vrc3.BSX_GM_Tool_Pose.png") : _toolPose;
        internal static Texture2D Earmuffs => !_earmuffs ? _earmuffs = LoadEmbeddedTexture("Vrc3.BSX_GM_Earmuffs.png") : _earmuffs;
        internal static Texture2D FallingSpeed => !_fallingSpeed ? _fallingSpeed = LoadEmbeddedTexture("Vrc3.BSX_GM_FallingSpeed.png") : _fallingSpeed;
        internal static Texture2D FullBody => !_fullBody ? _fullBody = LoadEmbeddedTexture("Vrc3.BSX_GM_FullBody.png") : _fullBody;
        internal static Texture2D Generic => !_generic ? _generic = LoadEmbeddedTexture("Vrc3.BSX_GM_Generic.png") : _generic;
        internal static Texture2D GestureLeftWeight => !_gestureLeftWeight ? _gestureLeftWeight = LoadEmbeddedTexture("Vrc3.BSX_GM_GestureLeftWeight.png") : _gestureLeftWeight;
        internal static Texture2D GestureRightWeight => !_gestureRightWeight ? _gestureRightWeight = LoadEmbeddedTexture("Vrc3.BSX_GM_GestureRightWeight.png") : _gestureRightWeight;
        internal static Texture2D Grounded => !_grounded ? _grounded = LoadEmbeddedTexture("Vrc3.BSX_GM_Grounded.png") : _grounded;
        internal static Texture2D HandsOnly => !_handsOnly ? _handsOnly = LoadEmbeddedTexture("Vrc3.BSX_GM_HandsOnly.png") : _handsOnly;
        internal static Texture2D HeadHands => !_headHands ? _headHands = LoadEmbeddedTexture("Vrc3.BSX_GM_HeadHands.png") : _headHands;
        internal static Texture2D PoseIK => !_poseIk ? _poseIk = LoadEmbeddedTexture("Vrc3.BSX_GM_Pose_IK.png") : _poseIk;
        internal static Texture2D MuteSelf => !_muteSelf ? _muteSelf = LoadEmbeddedTexture("Vrc3.BSX_GM_MuteSelf.png") : _muteSelf;
        internal static Texture2D Seated => !_seated ? _seated = LoadEmbeddedTexture("Vrc3.BSX_GM_Seated.png") : _seated;
        internal static Texture2D PoseT => !_poseT ? _poseT = LoadEmbeddedTexture("Vrc3.BSX_GM_Pose_T.png") : _poseT;
        internal static Texture2D Upright => !_upright ? _upright = LoadEmbeddedTexture("Vrc3.BSX_GM_Upright.png") : _upright;
        internal static Texture2D Velocity => !_velocity ? _velocity = LoadEmbeddedTexture("Vrc3.BSX_GM_Velocity.png") : _velocity;
        internal static Texture2D Visemes => !_visemes ? _visemes = LoadEmbeddedTexture("Vrc3.BSX_GM_Visemes.png") : _visemes;
        internal static Texture2D VRMode => !_vRMode ? _vRMode = LoadEmbeddedTexture("Vrc3.BSX_GM_VRMode.png") : _vRMode;
        internal static Texture2D Afk => !_afk ? _afk = LoadEmbeddedTexture("Vrc3.BSX_GM_AFK.png") : _afk;
        internal static Texture2D FourPoint => !_fourPoint ? _fourPoint = LoadEmbeddedTexture("Vrc3.BSX_GM_FourPoint.png") : _fourPoint;
        internal static Texture2D Uninitialized => !_uninitialized ? _uninitialized = LoadEmbeddedTexture("Vrc3.BSX_GM_Uninitialized.png") : _uninitialized;
        internal static Texture2D IsLocal => !_isLocal ? _isLocal = LoadEmbeddedTexture("Vrc3.BSX_GM_IsLocal.png") : _isLocal;
        internal static Texture2D Extras => !_extras ? _extras = LoadEmbeddedTexture("Vrc3.BSX_GM_Extras.png") : _extras;
        internal static Texture2D IsOnFriendsList => !_isOnFriendsList ? _isOnFriendsList = LoadEmbeddedTexture("Vrc3.BSX_GM_IsOnFriendsList.png") : _isOnFriendsList;

        public static Texture2D LoadEmbeddedTexture(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            resourceName = $"{assembly.GetName().Name}.Resources.{resourceName}";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new FileNotFoundException(resourceName);

                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);

                Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                tex.LoadImage(buffer);
                return tex;
            }
        }

        public static class Data
        {
            private const string VrcSdk3ControllerPath = "Vrc3/Controllers/";
            private const string VrcSdk3RestorePath = "Vrc3/Controllers/Restore/";

            //private static AnimatorController ControllerOfPath(string path) => Resources.Load<AnimatorController>(VrcSdk3ControllerPath + path);
            private static RuntimeAnimatorController ControllerOfPath(string path) => Resources.Load<RuntimeAnimatorController>(VrcSdk3ControllerPath + path);

            private static TextAsset RestoreOfPath(string path) => Resources.Load<TextAsset>(VrcSdk3RestorePath + path);

            internal static TextAsset RestoreOf(AnimLayerType type) => RestoreOfPath(NameOf[type]);

            //internal static AnimatorController ControllerOf(AnimLayerType type) => ControllerOfPath(NameOf[type]);
            internal static RuntimeAnimatorController ControllerOf(AnimLayerType type) => ControllerOfPath(NameOf[type]);

            internal static int LayerSort(VRCAvatarDescriptor.CustomAnimLayer x, VRCAvatarDescriptor.CustomAnimLayer y) => SortValue[x.type] - SortValue[y.type];

            internal static AvatarMask MaskOf(AnimLayerType type) => type switch
            {
                AnimLayerType.Gesture => Masks.Hands,
                AnimLayerType.IKPose => Masks.Armature,
                AnimLayerType.TPose => Masks.Armature,
                AnimLayerType.FX => Masks.Empty,
                AnimLayerType.Deprecated0 => null,
                AnimLayerType.Additive => null,
                AnimLayerType.Sitting => null,
                AnimLayerType.Action => null,
                AnimLayerType.Base => null,
                _ => null
            };

            private static readonly Dictionary<AnimLayerType, string> NameOf = new()
            {
                { AnimLayerType.FX, "GmgFxLayer" },
                { AnimLayerType.Base, "GmgBaseLayer" },
                { AnimLayerType.TPose, "GmgUtilityTPose" },
                { AnimLayerType.Action, "GmgActionLayer" },
                { AnimLayerType.IKPose, "GmgUtilityIKPose" },
                { AnimLayerType.Gesture, "GmgGestureLayer" },
                { AnimLayerType.Sitting, "GmgSittingLayer" },
                { AnimLayerType.Additive, "GmgAdditiveLayer" }
            };

            internal static readonly Dictionary<BlendableAnimatorLayer, AnimLayerType> AnimatorToLayer = new()
            {
                { BlendableAnimatorLayer.FX, AnimLayerType.FX },
                { BlendableAnimatorLayer.Action, AnimLayerType.Action },
                { BlendableAnimatorLayer.Gesture, AnimLayerType.Gesture },
                { BlendableAnimatorLayer.Additive, AnimLayerType.Additive }
            };

            internal static readonly Dictionary<BlendablePlayableLayer, AnimLayerType> PlayableToLayer = new()
            {
                { BlendablePlayableLayer.FX, AnimLayerType.FX },
                { BlendablePlayableLayer.Action, AnimLayerType.Action },
                { BlendablePlayableLayer.Gesture, AnimLayerType.Gesture },
                { BlendablePlayableLayer.Additive, AnimLayerType.Additive }
            };

            internal static readonly Dictionary<AnimLayerType, int> SortValue = new()
            {
                { AnimLayerType.Base, 0 },
                { AnimLayerType.Additive, 1 },
                { AnimLayerType.Sitting, 2 },
                { AnimLayerType.TPose, 3 },
                { AnimLayerType.IKPose, 4 },
                { AnimLayerType.Gesture, 5 },
                { AnimLayerType.Action, 6 },
                { AnimLayerType.FX, 7 }
            };

            internal static readonly Dictionary<ValueType, AnimatorControllerParameterType> TypeOf = new()
            {
                { ValueType.Int, AnimatorControllerParameterType.Int },
                { ValueType.Bool, AnimatorControllerParameterType.Bool },
                { ValueType.Float, AnimatorControllerParameterType.Float }
            };

            public static Dictionary<string, TrackingType> DefaultTrackingState => new()
            {
                { "Head", TrackingType.Tracking },
                { "Left Hand", TrackingType.Tracking },
                { "Right Hand", TrackingType.Tracking },
                { "Hip", TrackingType.Tracking },
                { "Left Foot", TrackingType.Tracking },
                { "Right Foot", TrackingType.Tracking },
                { "Left Fingers", TrackingType.Tracking },
                { "Right Fingers", TrackingType.Tracking },
                { "Eye & Eyelid", TrackingType.Tracking },
                { "Mouth & Jaw", TrackingType.Tracking }
            };
        }

        private static class Masks
        {
            private static AvatarMask _empty;
            private static AvatarMask _hands;
            private static AvatarMask _armature;

            internal static AvatarMask Empty => !_empty ? _empty = CreateEmptyMask() : _empty;

            internal static AvatarMask Hands => !_hands ? _hands = CreateHandsMask() : _hands;

            internal static AvatarMask Armature => !_armature ? _armature = CreateArmatureMask() : _armature;

            private static AvatarMask CreateEmptyMask() => GmgAvatarMaskHelper.CreateEmptyMask("Empty");

            private static AvatarMask CreateHandsMask() => GmgAvatarMaskHelper.CreateMaskWith("Hands", new[]
            {
                AvatarMaskBodyPart.LeftFingers,
                AvatarMaskBodyPart.RightFingers
            });

            private static AvatarMask CreateArmatureMask() => GmgAvatarMaskHelper.CreateMaskWithout("Armature", new[]
            {
                AvatarMaskBodyPart.LeftFootIK,
                AvatarMaskBodyPart.LeftHandIK,
                AvatarMaskBodyPart.RightFootIK,
                AvatarMaskBodyPart.RightHandIK
            });
        }
    }
}
#endif