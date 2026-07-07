using CustomDancePlayer;
using HarmonyLib;
using SFB;
using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using VRC.Dynamics;
using System.Runtime.CompilerServices;

namespace BlackStartX.GestureManager
{
    [AttributeUsage(AttributeTargets.Class)]
    internal class OptionalPatchAttribute : Attribute { }
    internal class Patcher
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void ApplyPatches()
        {
            var harmony = new Harmony("MEGME");

            Assembly assembly = Assembly.GetExecutingAssembly();

            AccessTools.GetTypesFromAssembly(assembly).DoIf((Type type) => type.HasHarmonyAttribute(), delegate (Type type)
            {
                PatchClassProcessor processor = null;
                try
                {
                    processor = harmony.CreateClassProcessor(type);
                    processor.Patch();
                    
                    RuntimeHelpers.RunClassConstructor(type.TypeHandle);
                }
                catch (Exception e)
                {
                    var declaringType = type.GetCustomAttribute<HarmonyAttribute>().info.declaringType;
                    var methodName = type.GetCustomAttribute<HarmonyAttribute>().info.methodName;

                    if (type.GetCustomAttribute<OptionalPatchAttribute>() != null)
                    {
                        Debug.LogError($"[MEGME] Optional harmony patch {type.FullName} failed for {declaringType}.{methodName}\n {e}");

                        processor?.Unpatch();
                    }
                    else
                    {
                        Debug.LogError($"[MEGME] Critical harmony patch {type.FullName} failed for {declaringType}.{methodName}");

                        harmony.UnpatchAll();

                        throw;
                    }
                }
            });

            Debug.Log("[MEGME] Harmony patches applied");
        }
    }

    [HarmonyPatch(typeof(AvatarDanceHandler), "EnsureAnimatorReady")]
    class Patch_EnsureAnimatorReady
    {
        static void Postfix(bool __result, ref Animator ___animator)
        {
            if (!__result) return;

            var manager = GameObject.FindFirstObjectByType<GestureManager>();

            if (manager.Module == null) return;

            ___animator.runtimeAnimatorController = null; // remove ME controller
        }
    }

    [OptionalPatch]
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

    [OptionalPatch]
    [HarmonyPatch(typeof(AvatarBigScreenToggleHandler), "Update")]
    class Patch_Update
    {
        /**
         * https://github.com/shinyflvre/Mate-Engine/issues/535
         */
        static readonly AccessTools.FieldRef<AvatarBigScreenHandler, bool> isBigScreenActiveField = AccessTools.FieldRefAccess<AvatarBigScreenHandler, bool>("isBigScreenActive");
        static bool Prefix(AvatarBigScreenToggleHandler __instance, ref AvatarBigScreenHandler ___bigScreenHandler, ref Dictionary<Behaviour, bool> ___wasEnabledBefore)
        {
            if (!___bigScreenHandler) return false;

            bool isBigScreenActive = false;
            if (isBigScreenActiveField != null)
                isBigScreenActive = isBigScreenActiveField(___bigScreenHandler);

            var behaviours = __instance.GetComponents<Behaviour>();
            foreach (var b in behaviours)
            {
                if (b == __instance || b == ___bigScreenHandler || b == null) continue;
                //if (b == this || b == bigScreenHandler) continue; // original version

                bool shouldDisable = __instance.settings.Exists(s => s.componentTypeName == b.GetType().FullName && s.disableInBigScreen);

                if (isBigScreenActive && shouldDisable)
                {
                    if (!___wasEnabledBefore.ContainsKey(b))
                    {
                        ___wasEnabledBefore[b] = b.enabled;
                        b.enabled = false;
                    }
                }
                else if (!isBigScreenActive)
                {
                    if (___wasEnabledBefore.ContainsKey(b))
                    {
                        b.enabled = ___wasEnabledBefore[b];
                    }
                }
            }

            return false;
        }
    }

    [OptionalPatch]
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

    [OptionalPatch]
    [HarmonyPatch(typeof(VRMLoader), "LoadVRM")]
    class Patch_LoadVRM
    {
        static readonly Action<VRMLoader, string> LoadAssetBundleModel = AccessTools.MethodDelegate<Action<VRMLoader, string>>(
            AccessTools.Method(typeof(VRMLoader), "LoadAssetBundleModel"));

        static bool Prefix(VRMLoader __instance, string path)
        {
            if (path.EndsWith(".vrca", StringComparison.OrdinalIgnoreCase))
            {
                LoadAssetBundleModel(__instance, path);
                if (SaveLoadHandler.Instance != null)
                {
                    SaveLoadHandler.Instance.data.selectedModelPath = path;
                    SaveLoadHandler.Instance.SaveToDisk();
                }

                return false;
            }

            return true;
        }
    }

    [OptionalPatch]
    [HarmonyPatch(typeof(VRMLoader), "OpenFileDialogAndLoadVRM")]
    class Patch_OpenFileDialogAndLoadVRM
    {
        static readonly Action<VRMLoader, string> LoadVRM = AccessTools.MethodDelegate<Action<VRMLoader, string>>(
            AccessTools.Method(typeof(VRMLoader), "LoadVRM"));

        static bool Prefix(VRMLoader __instance, ref bool ___isLoading)
        {
            if (___isLoading) return false;

            ___isLoading = true;
            var extensions = new[] { new ExtensionFilter("Model Files", "vrm", "me", "prefab", "vrca") };
            string[] paths = StandaloneFileBrowser.OpenFilePanel("Select Model File", "", extensions, false);
            if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
                LoadVRM(__instance, paths[0]);

            ___isLoading = false;

            return false;
        }
    }
}
