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
        static readonly AccessTools.FieldRef<ModuleVrc3, AnimationLayerMixerPlayable> mixer = AccessTools.FieldRefAccess<ModuleVrc3, AnimationLayerMixerPlayable>("mixer");
        static readonly AccessTools.FieldRef<ModuleVrc3, Dictionary<VRCAvatarDescriptor.AnimLayerType, LayerData>> _layers = AccessTools.FieldRefAccess<ModuleVrc3, Dictionary<VRCAvatarDescriptor.AnimLayerType, LayerData>>("_layers");

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

                var mixer = Patch_EnsureAnimatorReady.mixer(module);

                var overrideControllerPlayable = AnimatorControllerPlayable.Create(_playableGraph(module), overrideController(__instance));

                var baseControllerIndex = -1;
                var controllerPlayable = _layers(module)[VRCAvatarDescriptor.AnimLayerType.Base].Playable;
                int count = mixer.GetInputCount();
                for (int i = 0; i < count; i++)
                    if (mixer.GetInput(i).Equals(controllerPlayable))
                    {
                        baseControllerIndex = i;
                        break;
                    }

                var idleLayerIndex = controllerPlayable.GetLayerIndex(idleLayerName);

                mixer.AddInput(overrideControllerPlayable, 0, 1);

                overrideControllerPlayable.Play(idleStateName, idleLayerIndex, 0f);                
            }

            animator(__instance).runtimeAnimatorController = null; // remove ME controller
        }
    }
}
