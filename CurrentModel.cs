using System;
using UnityEngine;
using HarmonyLib;

namespace BlackStartX.GestureManager
{
    public static class CurrentModel
    {
        public static event Action OnAvatarSwitch;
        public static GameObject ModelGO { get; private set; }
        public static Transform ModelRoot { get; private set; }

        static readonly float avatarScanInterval = 0.25f;
        static float nextAvatarScan;

        public static void OnAwake()
        {
            var modelRootGO = GameObject.Find("Model");
            if (modelRootGO != null)
                ModelRoot = modelRootGO.transform;
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
            if (!ModelRoot) return;

            for (int i = 0; i < ModelRoot.childCount; i++)
            {
                var child = ModelRoot.GetChild(i).gameObject;
                if (!child.activeInHierarchy) continue;
                if (ModelGO == child) return;
                ModelGO = child;

                UpdateAvatarComponents();
                OnAvatarSwitch?.Invoke();

                return;
            }
        }
        static void UpdateAvatarComponents()
        {
            AvatarBigScreenHandlerProxy.Inst = GetComponent<AvatarBigScreenHandler>();
        }
        public static T GetComponent<T>() where T : Component
            => ModelGO.GetComponent<T>();


        public static class AvatarBigScreenHandlerProxy
        {
            public static AvatarBigScreenHandler Inst;

            static readonly AccessTools.FieldRef<AvatarBigScreenHandler, bool> _isBigScreenActive = AccessTools.FieldRefAccess<AvatarBigScreenHandler, bool>("isBigScreenActive");
            public static bool isBigScreenActive
            {
                get => _isBigScreenActive(Inst);
            }
        }
    }
}