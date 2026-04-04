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

    #region unused
    //[HarmonyPatch(typeof(AvatarDanceHandler), "SmoothPlayFlow")]
    //class Patch_SmoothPlayFlow
    //{
    //    bool inited = false;

    //    static Action<AvatarDanceHandler> FreezeAnimator;
    //    static Action<AvatarDanceHandler> PauseAudio;
    //    static Action<AvatarDanceHandler, bool> SetDancing;
    //    static Action<AvatarDanceHandler, bool> SetWaiting;
    //    static Action<AvatarDanceHandler> UnfreezeAnimator;
    //    static Action<AvatarDanceHandler, object> UnloadEntry;
    //    static Func<AvatarDanceHandler, bool> EnsureAnimatorReady;
    //    static Func<AvatarDanceHandler, bool> IsFullyInWaiting;
    //    static Func<AvatarDanceHandler, RuntimeAnimatorController, string, AnimationClip> FindPlaceholderClip;

    //    static AccessTools.FieldRef<AvatarDanceHandler, bool> holdDuringTransition;
    //    static AccessTools.FieldRef<AvatarDanceHandler, object> loadedEntry;
    //    static AccessTools.FieldRef<AvatarDanceHandler, string> placeholderClipName;
    //    static AccessTools.FieldRef<AvatarDanceHandler, AnimationClip> placeholderClipCached;
    //    static AccessTools.FieldRef<AvatarDanceHandler, RuntimeAnimatorController> defaultController;
    //    static FieldInfo entriesField;



    //    void Init()
    //    {
    //        if (inited) return;
    //        inited = true;

    //        holdDuringTransition = AccessTools.FieldRefAccess<AvatarDanceHandler, bool>("holdDuringTransition");
    //        loadedEntry = AccessTools.FieldRefAccess<AvatarDanceHandler, object>("loadedEntry");
    //        placeholderClipName = AccessTools.FieldRefAccess<AvatarDanceHandler, string>("placeholderClipName");
    //        defaultController = AccessTools.FieldRefAccess<AvatarDanceHandler, RuntimeAnimatorController>("defaultController");
    //        placeholderClipCached = AccessTools.FieldRefAccess<AvatarDanceHandler, AnimationClip>("placeholderClipCached");
    //        entriesField = typeof(AvatarDanceHandler).GetField("entries", BindingFlags.Instance | BindingFlags.NonPublic);

    //        var FreezeAnimatorMethod = AccessTools.Method(typeof(AvatarDanceHandler), "FreezeAnimator");
    //        FreezeAnimator = (Action<AvatarDanceHandler>)Delegate.CreateDelegate(typeof(Action<AvatarDanceHandler>), FreezeAnimatorMethod);
    //        var PauseAudioMethod = AccessTools.Method(typeof(AvatarDanceHandler), "PauseAudio");
    //        PauseAudio = (Action<AvatarDanceHandler>)Delegate.CreateDelegate(typeof(Action<AvatarDanceHandler>), PauseAudioMethod);
    //        var SetDancingMethod = AccessTools.Method(typeof(AvatarDanceHandler), "SetDancing");
    //        SetDancing = (Action<AvatarDanceHandler, bool>)Delegate.CreateDelegate(typeof(Action<AvatarDanceHandler, bool>), SetDancingMethod);
    //        var SetWaitingMethod = AccessTools.Method(typeof(AvatarDanceHandler), "SetWaiting");
    //        SetWaiting = (Action<AvatarDanceHandler, bool>)Delegate.CreateDelegate(typeof(Action<AvatarDanceHandler, bool>), SetWaitingMethod);
    //        var IsFullyInWaitingMethod = AccessTools.Method(typeof(AvatarDanceHandler), "IsFullyInWaiting");
    //        IsFullyInWaiting = (Func<AvatarDanceHandler, bool>)Delegate.CreateDelegate(typeof(Func<AvatarDanceHandler, bool>), IsFullyInWaitingMethod);
    //        var UnfreezeAnimatorMethod = AccessTools.Method(typeof(AvatarDanceHandler), "UnfreezeAnimator");
    //        UnfreezeAnimator = (Action<AvatarDanceHandler>)Delegate.CreateDelegate(typeof(Action<AvatarDanceHandler>), UnfreezeAnimatorMethod);
    //        var EnsureAnimatorReadyMethod = AccessTools.Method(typeof(AvatarDanceHandler), "EnsureAnimatorReady");
    //        EnsureAnimatorReady = (Func<AvatarDanceHandler, bool>)Delegate.CreateDelegate(typeof(Func<AvatarDanceHandler, bool>), EnsureAnimatorReadyMethod);
    //        var FindPlaceholderClipMethod = AccessTools.Method(typeof(AvatarDanceHandler), "FindPlaceholderClip");
    //        FindPlaceholderClip = (Func<AvatarDanceHandler, RuntimeAnimatorController, string, AnimationClip>)Delegate.CreateDelegate(typeof(Func<AvatarDanceHandler, RuntimeAnimatorController, string, AnimationClip>), FindPlaceholderClipMethod);
    //        var UnloadEntryMethod = AccessTools.Method(typeof(AvatarDanceHandler), "UnloadEntry");
    //        UnloadEntry = (Action<AvatarDanceHandler, object>)Delegate.CreateDelegate(typeof(Action<AvatarDanceHandler, object>), UnloadEntryMethod);
    //    }
    //    static bool Prefix(AvatarDanceHandler __instance, int index, ref IEnumerator __result)
    //    {
    //        __result = CustomSmoothPlayFlow(__instance, index);
    //        return false;
    //    }

    //    static IEnumerator CustomSmoothPlayFlow(AvatarDanceHandler __instance, int index)
    //    {
    //        Debug.LogWarning("SmoothPlayFlow Start"); //
    //        #region Reflection

    //        var entries = (IList)entriesField.GetValue(__instance);

    //        #endregion

    //        holdDuringTransition(__instance) = true;
    //        FreezeAnimator(__instance);
    //        PauseAudio(__instance);

    //        SetDancing(__instance, false);
    //        SetWaiting(__instance, true);

    //        float timeout = 2f;
    //        float t0 = Time.unscaledTime;
    //        while (!IsFullyInWaiting(__instance) && Time.unscaledTime - t0 < timeout)
    //            yield return null;

    //        var prev = loadedEntry;

    //        var e = entries[index];
    //        if (e.bundle == null)
    //        {
    //            string bp = string.IsNullOrEmpty(e.bundlePath) ? e.path : e.bundlePath;
    //            e.bundle = AssetBundle.LoadFromFile(bp);
    //            if (e.bundle == null) { UnfreezeAnimator(__instance); holdDuringTransition(__instance) = false; yield break; }
    //        }
    //        if (e.clip == null) e.clip = e.bundle.LoadAllAssets<AnimationClip>().FirstOrDefault();
    //        if (e.audio == null) e.audio = e.bundle.LoadAllAssets<AudioClip>().FirstOrDefault();

    //        if (!EnsureAnimatorReady(__instance)) { UnfreezeAnimator(__instance); holdDuringTransition(__instance) = false; yield break; }

    //        if (placeholderClipCached(__instance) == null) placeholderClipCached(__instance) = FindPlaceholderClip(__instance, defaultController(__instance), placeholderClipName(__instance));
    //        if (overrideController == null || placeholderClipCached(__instance) == null) { UnfreezeAnimator(__instance); holdDuringTransition(__instance) = false; yield break; }

    //        overrideController[placeholderClipName] = e.clip != null ? e.clip : placeholderClipCached(__instance);
    //        Debug.Log(e.clip); //
    //        if (prev != null && prev != e)
    //        {
    //            UnloadEntry(__instance, prev);
    //            StartCoroutine(UnloadUnusedAssetsRoutine());
    //        }

    //        if (audioSource == null) EnsureAudioSource();
    //        if (audioSource != null)
    //        {
    //            audioSource.Stop();
    //            audioSource.clip = e.audio;
    //            audioSource.time = 0f;
    //            audioSource.loop = false;
    //        }

    //        currentTotalSeconds = e.audio != null ? e.audio.length : (e.clip != null ? e.clip.length : 0f);
    //        playStartTime = Time.time;
    //        isPlaying = true;

    //        currentIndex = index;
    //        loadedEntry = e;
    //        UpdatePlayingNowLabel(e.id);
    //        UpdateAuthorLabel(e.author);
    //        UpdateTimeLabels(0f, currentTotalSeconds);

    //        SetWaiting(false);
    //        SetDancing(true);
    //        UnfreezeAnimator();
    //        ResumeAudio();

    //        holdDuringTransition = false;
    //        playRoutine = null;
    //        Debug.LogWarning("SmoothPlayFlow End"); //
    //    }
    //}
    #endregion  

    [HarmonyPatch(typeof(AvatarDanceHandler), "EnsureAnimatorReady")]
    class Patch_EnsureAnimatorReady // переделать на loadedEntry
    {
        static string idleLayerName = "Base Layer";
        static string idleStateName = $"{idleLayerName}.Idle";

        static ModuleVrc3 module;
        static void Postfix(AvatarDanceHandler __instance, bool __result)
        {
            if (!__result) return;

            var manager = GameObject.FindFirstObjectByType<GestureManager>();
            var m = (ModuleVrc3)manager.Module;
            //Debug.Log($"[Postfix] module {m}"); //
            if (m == null) return;

            var animator = AccessTools.FieldRefAccess<AvatarDanceHandler, Animator>("animator")(__instance);
            //Debug.Log($"[Postfix] animator {animator}"); //
            if (module != m)
            {
                module = m;

                var overrideController = AccessTools.FieldRefAccess<AvatarDanceHandler, AnimatorOverrideController>("overrideController")(__instance);
                var _playableGraph = AccessTools.FieldRefAccess<ModuleVrc3, PlayableGraph>("_playableGraph")(module);
                var mixer = AccessTools.FieldRefAccess<ModuleVrc3, AnimationLayerMixerPlayable>("mixer")(module);
                var _layers = AccessTools.FieldRefAccess<ModuleVrc3, Dictionary<VRCAvatarDescriptor.AnimLayerType, LayerData>>("_layers")(module);

                var overrideControllerPlayable = AnimatorControllerPlayable.Create(_playableGraph, overrideController);
                //Debug.Log($"[Postfix] overrideControllerPlayable {overrideControllerPlayable}"); // 
                var baseControllerIndex = -1;
                var controllerPlayable = _layers[VRCAvatarDescriptor.AnimLayerType.Base].Playable;
                int count = mixer.GetInputCount();
                for (int i = 0; i < count; i++)
                    if (mixer.GetInput(i).Equals(controllerPlayable))
                    {
                        baseControllerIndex = i;
                        break;
                    }

                //Debug.Log($"[Postfix] baseControllerIndex {baseControllerIndex}");
                var idleLayerIndex = controllerPlayable.GetLayerIndex(idleLayerName);

                mixer.DisconnectInput(baseControllerIndex);
                mixer.ConnectInput(baseControllerIndex, overrideControllerPlayable, idleLayerIndex, 1);

                overrideControllerPlayable.Play(idleStateName, idleLayerIndex, 0f);                
            }

            animator.runtimeAnimatorController = null; // remove ME controller
        }
    }


}
