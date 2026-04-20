using CustomDancePlayer;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

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
}
