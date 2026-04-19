using BlackStartX.GestureManager.Data;
using BlackStartX.GestureManager.Editor.Modules.Vrc3;
using CustomDancePlayer;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using VRC.SDK3.Avatars.Components;
using static BlackStartX.GestureManager.Editor.Modules.Vrc3.ModuleVrc3;

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
            Debug.Log("[MEGME] Harmony patch applied");
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
}
