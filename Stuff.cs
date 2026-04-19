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
    class Patch_EnsureAnimatorReady // переделать на loadedEntry
    {
        static readonly string idleLayerName = "Base Layer";
        static readonly string idleStateName = $"{idleLayerName}.Idle";

        static readonly AccessTools.FieldRef<AvatarDanceHandler, Animator> animator = AccessTools.FieldRefAccess<AvatarDanceHandler, Animator>("animator");
        static readonly AccessTools.FieldRef<AvatarDanceHandler, AnimatorOverrideController> overrideController = AccessTools.FieldRefAccess<AvatarDanceHandler, AnimatorOverrideController>("overrideController");
        static readonly AccessTools.FieldRef<ModuleVrc3, PlayableGraph> _playableGraph = AccessTools.FieldRefAccess<ModuleVrc3, PlayableGraph>("_playableGraph");
        static readonly AccessTools.FieldRef<ModuleVrc3, AnimationScriptPlayable> readUBSScriptPlayable = AccessTools.FieldRefAccess<ModuleVrc3, AnimationScriptPlayable>("readUBSScriptPlayable");

        static ModuleVrc3 module;

        static void Postfix(AvatarDanceHandler __instance, bool __result)
        {
            if (!__result) return;

            var manager = GameObject.FindFirstObjectByType<GestureManager>();
            var m = (ModuleVrc3)manager.Module;

            if (m == null) return;

            if (module != m)
            {
                module = m;

                var readUBSScriptPlayable = Patch_EnsureAnimatorReady.readUBSScriptPlayable(module);

                var overrideControllerPlayable = AnimatorControllerPlayable.Create(_playableGraph(module), overrideController(__instance));

                var controllerPlayable = (AnimatorControllerPlayable)readUBSScriptPlayable.GetInput(0);
                var idleLayerIndex = controllerPlayable.GetLayerIndex(idleLayerName);

                readUBSScriptPlayable.DisconnectInput(0);
                readUBSScriptPlayable.ConnectInput(0, overrideControllerPlayable, 0);

                overrideControllerPlayable.Play(idleStateName, idleLayerIndex, 0f);
            }

            animator(__instance).runtimeAnimatorController = null; // remove ME controller
        }
    }
}
