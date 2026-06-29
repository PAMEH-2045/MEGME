using CustomDancePlayer;
using HarmonyLib;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using VRC.Dynamics;

namespace BlackStartX.GestureManager
{
    [DefaultExecutionOrder(-1000)]
    public class Stuff : MonoBehaviour
    {
        // MEX3.2.0 UISetOnOff
        public void SetOnOff(GameObject obj)
        {
            if (obj != null)
                obj.SetActive(!obj.activeSelf);
        }

        void Awake()
        {
            ApplyPatch();
        }

        void ApplyPatch()
        {
            var harmony = new Harmony("MEGME");
            harmony.PatchAll();
            Debug.Log("[MEGME] Harmony patches applied");
        }
    }

    [HarmonyPatch(typeof(AvatarDanceHandler), "EnsureAnimatorReady")]
    class Patch_EnsureAnimatorReady
    {

        static readonly AccessTools.FieldRef<AvatarDanceHandler, Animator> animator = AccessTools.FieldRefAccess<AvatarDanceHandler, Animator>("animator");

        static void Postfix(AvatarDanceHandler __instance, bool __result)
        {
            if (!__result) return;

            var manager = GameObject.FindFirstObjectByType<GestureManager>();

            if (manager.Module == null) return;

            animator(__instance).runtimeAnimatorController = null; // remove ME controller
        }
    }

    [HarmonyPatch(typeof(AvatarDanceHandler), "RefreshAnimatorIfChanged")]
    class Patch_RefreshAnimatorIfChanged
    {

        static readonly AccessTools.FieldRef<AvatarDanceHandler, Animator> animator = AccessTools.FieldRefAccess<AvatarDanceHandler, Animator>("animator");
        static readonly AccessTools.FieldRef<AvatarDanceHandler, Animator> lastAnimator = AccessTools.FieldRefAccess<AvatarDanceHandler, Animator>("lastAnimator");
        static readonly AccessTools.FieldRef<AvatarDanceHandler, RuntimeAnimatorController> defaultController = AccessTools.FieldRefAccess<AvatarDanceHandler, RuntimeAnimatorController>("defaultController");
        static readonly AccessTools.FieldRef<AvatarDanceHandler, RuntimeAnimatorController> overrideController = AccessTools.FieldRefAccess<AvatarDanceHandler, RuntimeAnimatorController>("overrideController");
        static readonly AccessTools.FieldRef<AvatarDanceHandler, int> layerIndex = AccessTools.FieldRefAccess<AvatarDanceHandler, int>("layerIndex");
        static readonly AccessTools.FieldRef<AvatarDanceHandler, int> stateHash = AccessTools.FieldRefAccess<AvatarDanceHandler, int>("stateHash");

        static AvatarDanceHandler danceHandler;
        static Patch_RefreshAnimatorIfChanged()
        {
            CurrentModel.OnAvatarSwitch += OnAvatarSwitch;

            danceHandler = GameObject.FindFirstObjectByType<AvatarDanceHandler>();
        }

        static void OnAvatarSwitch()
        {
            var animatorNew = CurrentModel.Animator;

            if (animator(danceHandler) == null || animator(danceHandler) != animatorNew)
            {
                animator(danceHandler) = animatorNew;
                lastAnimator(danceHandler) = animatorNew;
                defaultController(danceHandler) = animatorNew != null ? animatorNew.runtimeAnimatorController : null;
                layerIndex(danceHandler) = animatorNew != null ? animatorNew.GetLayerIndex(danceHandler.danceLayerName) : -1;
                stateHash(danceHandler) = Animator.StringToHash(danceHandler.danceStateName);
                if (GameObject.FindFirstObjectByType<GestureManager>().Module == null)
                    overrideController(danceHandler) = null;
            }
        }
        static bool Prefix()
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(AvatarBigScreenToggleHandler), "Update")]
    class Patch_Update
    {

        static readonly AccessTools.FieldRef<AvatarBigScreenToggleHandler, AvatarBigScreenHandler> bigScreenHandler = AccessTools.FieldRefAccess<AvatarBigScreenToggleHandler, AvatarBigScreenHandler>("bigScreenHandler");
        static readonly AccessTools.FieldRef<AvatarBigScreenToggleHandler, Dictionary<Behaviour, bool>> wasEnabledBefore = AccessTools.FieldRefAccess<AvatarBigScreenToggleHandler, Dictionary<Behaviour, bool>>("wasEnabledBefore");
        static readonly AccessTools.FieldRef<AvatarBigScreenHandler, bool> isBigScreenActiveField = AccessTools.FieldRefAccess<AvatarBigScreenHandler, bool>("isBigScreenActive");

        static bool Prefix(AvatarBigScreenToggleHandler __instance)
        {
            if (!bigScreenHandler(__instance)) return false;

            bool isBigScreenActive = false;
            if (isBigScreenActiveField != null)
                isBigScreenActive = isBigScreenActiveField(bigScreenHandler(__instance));

            var behaviours = __instance.GetComponents<Behaviour>();
            foreach (var b in behaviours)
            {
                if (b == __instance || b == bigScreenHandler(__instance) || b == null) continue;
                //if (b == __instance || b == bigScreenHandler(__instance)) continue; // original version

                bool shouldDisable = __instance.settings.Exists(s => s.componentTypeName == b.GetType().FullName && s.disableInBigScreen);

                if (isBigScreenActive && shouldDisable)
                {
                    if (!wasEnabledBefore(__instance).ContainsKey(b))
                    {
                        wasEnabledBefore(__instance)[b] = b.enabled;
                        b.enabled = false;
                    }
                }
                else if (!isBigScreenActive)
                {
                    if (wasEnabledBefore(__instance).ContainsKey(b))
                    {
                        b.enabled = wasEnabledBefore(__instance)[b];
                    }
                }
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(PhysBoneManager.PhysBoneJob), "SolveChain")]
    class Patch_SolveChain
    {
        public static float3 currentForce;

        static void Prefix(ref PhysBoneManager.PhysBoneJob __instance, ref PhysBoneManager.Chain chain)
        {
            NativeArray<PhysBoneManager.Bone> bones = __instance.bones;

            for (int i = 0; i < chain.boneCount; i++)
            {
                int index = chain.boneOffset + i;
                var bone = bones[index];
                if (bone.isSimulated)
                {
                    bone.prevVelocity += currentForce;
                    bones[index] = bone;
                }
            }
        }
    }
}
