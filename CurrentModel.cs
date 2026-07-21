using HarmonyLib;
using System;
using UnityEngine;
using VRM;

namespace BlackStartX.GestureManager
{
    public static class CurrentModel
    {
        public static event Action OnAvatarSwitch;
        public static GameObject gameObject { get; private set; }
        public static Transform Root { get; private set; }
    public static Transform transform => gameObject.transform;
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
            AvatarBigScreenHandlerProxy.Inst = GetComponent<AvatarBigScreenHandler>();
            AvatarGravityControllerProxy.Inst = GetComponent<AvatarGravityController>();
            UniversalBlendshapesProxy.Inst = GetComponent<UniversalBlendshapes>();
        }
        public static T GetComponent<T>() where T : Component
            => gameObject.GetComponent<T>();
        public static T AddComponent<T>() where T : Component
            => gameObject.AddComponent<T>();


        public static class AvatarBigScreenHandlerProxy
        {
            public static AvatarBigScreenHandler Inst;

            static readonly AccessTools.FieldRef<AvatarBigScreenHandler, bool> _isBigScreenActive = AccessTools.FieldRefAccess<AvatarBigScreenHandler, bool>("isBigScreenActive");
            public static bool isBigScreenActive
            {
                get => _isBigScreenActive != null ? _isBigScreenActive(Inst) : false;
            }
        }
        public static class AvatarGravityControllerProxy
        {
            public static AvatarGravityController Inst;
        }
        public static class UniversalBlendshapesProxy
        {
            public static UniversalBlendshapes Inst;

            static readonly AccessTools.FieldRef<UniversalBlendshapes, VRMBlendShapeProxy> _proxy0 = AccessTools.FieldRefAccess<UniversalBlendshapes, VRMBlendShapeProxy>("proxy0");
            public static VRMBlendShapeProxy proxy0
            {
                set => _proxy0(Inst) = value;
                get => _proxy0(Inst);
            }
        }
    }
}