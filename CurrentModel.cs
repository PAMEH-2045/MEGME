using HarmonyLib;
using System;
using UnityEngine;
using VRM;

namespace MEGME
{
    public static class CurrentModel
    {
        public static event Action OnAvatarSwitch;
        public static GameObject gameObject { get; private set; }
        public static Transform Root { get; private set; }
        public static Transform transform => gameObject?.transform;
        public static Animator Animator { get; private set; }

        static readonly float avatarScanInterval = 0.25f;
        static float nextAvatarScan;

        public static void OnStart()
        {
            var modelRootGO = GameObject.Find("Model");
            if (modelRootGO != null)
                Root = modelRootGO.transform;
        }
        public static void OnUpdate()
        {
            if (Time.unscaledTime >= nextAvatarScan)
            {
                UpdateCurrentAvatar();
                nextAvatarScan = Time.unscaledTime + avatarScanInterval;
            }
        }
        static void UpdateCurrentAvatar()
        {
            if (!Root) return;

            for (int i = 0; i < Root.childCount; i++)
            {
                var child = Root.GetChild(i).gameObject;
                if (!child.activeInHierarchy) continue;
                if (gameObject == child) return;
                gameObject = child;

                UpdateAvatarComponents();
                OnAvatarSwitch?.Invoke();

                return;
            }
        }
        static void UpdateAvatarComponents()
        {
            CurrentModel.Animator = GetComponent<Animator>();
            AvatarBigScreenHandler.Inst = GetComponent<global::AvatarBigScreenHandler>();
            AvatarGravityController.Inst = GetComponent<global::AvatarGravityController>();
            UniversalBlendshapes.Inst = GetComponent<global::UniversalBlendshapes>();
            IKFix.Inst = GetComponent<global::IKFix>();

            DynamicBoneAvatarGravityController.Inst = AddComponent<MEGME.DynamicBoneAvatarGravityController>();
        }
        public static T GetComponent<T>() where T : Component
            => gameObject.GetComponent<T>();
        public static T AddComponent<T>() where T : Component
            => gameObject.AddComponent<T>();


        public static bool IsBigScreenActive => AvatarBigScreenHandler.isBigScreenActive;
        public class AvatarBigScreenHandler
        {
            public static global::AvatarBigScreenHandler Inst;

            static readonly AccessTools.FieldRef<global::AvatarBigScreenHandler, bool> _isBigScreenActive = AccessTools.FieldRefAccess<global::AvatarBigScreenHandler, bool>("isBigScreenActive");
            public static bool isBigScreenActive
            {
                get => _isBigScreenActive != null ? _isBigScreenActive(Inst) : false;
            }
        }
        public static class AvatarGravityController
        {
            public static global::AvatarGravityController Inst;
        }
        public static class UniversalBlendshapes
        {
            public static global::UniversalBlendshapes Inst;

            static readonly AccessTools.FieldRef<global::UniversalBlendshapes, VRMBlendShapeProxy> _proxy0 = AccessTools.FieldRefAccess<global::UniversalBlendshapes, VRMBlendShapeProxy>("proxy0");
            public static VRMBlendShapeProxy proxy0
            {
                get => _proxy0(Inst);
                set => _proxy0(Inst) = value;
            }
        }
        public static class DynamicBoneAvatarGravityController
        {
            public static MEGME.DynamicBoneAvatarGravityController Inst;
        }
        public static bool EnableIK { get => IKFix.Inst.enableIK; set => IKFix.Inst.enableIK = value; }
        public static class IKFix
        {
            public static global::IKFix Inst;
        }
    }
}