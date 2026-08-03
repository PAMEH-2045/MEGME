using BlackStartX.GestureManager;
using CustomDancePlayer;
using HarmonyLib;
using SFB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UniVRM10;
using VRM;

namespace MEGME
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

                    Debug.Log($"[MEGME] Patch {type.Name} applied");
                }
                catch (Exception e)
                {
                    var attr = HarmonyMethodExtensions.GetMergedFromType(type);

                    var declaringType = attr.declaringType;
                    var methodName = attr.methodName;

                    if (type.GetCustomAttribute<OptionalPatchAttribute>() != null)
                    {
                        Debug.LogError($"[MEGME] Optional harmony patch {type.FullName} failed for {declaringType}.{methodName}\n {e}");

                        if (attr.category is string category)
                        {
                            harmony.UnpatchCategory(category);
                        }
                        else
                        {
                            processor?.Unpatch();
                        }
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

    [HarmonyPatch]
    class FixGraphConflict
    {
        /**
         * Without removing, runtimeAnimatorController's graph would fight with GM's graph for animtor, resulting in unpredictable behavior
         */
        [HarmonyPrefix]
        [HarmonyPatch(typeof(AvatarDanceHandler), "EnsureAnimatorReady")]
        static void RemoveControllerIfGMActive(bool __result, Animator ___animator)
        {
            if (!__result) return;

            var manager = GameObject.FindFirstObjectByType<GestureManager>();

            if (manager.Module == null) return;

            ___animator.runtimeAnimatorController = null; // remove ME controller
        }
    }

    [HarmonyPatch, OptionalPatch]
    class FixRedundantFindAvatarSmartExecution
    {
        /**
         * Since animator.runtimeAnimatorController is always null the condition is always true, so unity-heavy FindAvatarSmart() is called every frame, reducing preformance
         */
        static readonly AccessTools.FieldRef<AvatarDanceHandler, Animator> animator = AccessTools.FieldRefAccess<AvatarDanceHandler, Animator>("animator");
        static readonly AccessTools.FieldRef<AvatarDanceHandler, Animator> lastAnimator = AccessTools.FieldRefAccess<AvatarDanceHandler, Animator>("lastAnimator");
        static readonly AccessTools.FieldRef<AvatarDanceHandler, RuntimeAnimatorController> defaultController = AccessTools.FieldRefAccess<AvatarDanceHandler, RuntimeAnimatorController>("defaultController");
        static readonly AccessTools.FieldRef<AvatarDanceHandler, RuntimeAnimatorController> overrideController = AccessTools.FieldRefAccess<AvatarDanceHandler, RuntimeAnimatorController>("overrideController");
        static readonly AccessTools.FieldRef<AvatarDanceHandler, int> layerIndex = AccessTools.FieldRefAccess<AvatarDanceHandler, int>("layerIndex");
        static readonly AccessTools.FieldRef<AvatarDanceHandler, int> stateHash = AccessTools.FieldRefAccess<AvatarDanceHandler, int>("stateHash");

        static AvatarDanceHandler danceHandler;
        static FixRedundantFindAvatarSmartExecution() => SceneManager.sceneLoaded += (scene, mode) =>
        {
            CurrentModel.OnAvatarSwitch += OnAvatarSwitch;

            danceHandler = GameObject.FindFirstObjectByType<AvatarDanceHandler>();
        };

        static void OnAvatarSwitch() // null reference if vrc model loaded first on app startup
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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AvatarDanceHandler), "RefreshAnimatorIfChanged")]
        static bool BlockMethodExecution()
        {
            return false;
        }
    }

    [HarmonyPatch, OptionalPatch]
    class FixNullReferenceOnMissingScript
    {
        /**
         * https://github.com/shinyflvre/Mate-Engine/issues/535
         */
        static readonly AccessTools.FieldRef<AvatarBigScreenHandler, bool> isBigScreenActiveField = AccessTools.FieldRefAccess<AvatarBigScreenHandler, bool>("isBigScreenActive");

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AvatarBigScreenToggleHandler), "Update")]
        static bool ReplaceWithNullCheck(AvatarBigScreenToggleHandler __instance, AvatarBigScreenHandler ___bigScreenHandler, Dictionary<Behaviour, bool> ___wasEnabledBefore)
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

    [HarmonyPatch, OptionalPatch]
    class VRCASupport
    {
        static readonly Action<VRMLoader, string> LoadAssetBundleModel = AccessTools.MethodDelegate<Action<VRMLoader, string>>(
            AccessTools.Method(typeof(VRMLoader), "LoadAssetBundleModel"));

        [HarmonyPrefix]
        [HarmonyPatch(typeof(VRMLoader), "LoadVRM")]
        static bool AddVRCALoad(VRMLoader __instance, string path)
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

        static readonly Action<VRMLoader, string> LoadVRM = AccessTools.MethodDelegate<Action<VRMLoader, string>>(
            AccessTools.Method(typeof(VRMLoader), "LoadVRM"));

        [HarmonyPrefix]
        [HarmonyPatch(typeof(VRMLoader), "OpenFileDialogAndLoadVRM")]
        static bool ShowVRCAInExplorer(VRMLoader __instance, ref bool ___isLoading)
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

    [HarmonyPatch, OptionalPatch]
    class FixVRM1Eyetracking
    {
        /**
         * https://github.com/shinyflvre/Mate-Engine/issues/536
         */
        [HarmonyPrefix]
        [HarmonyPatch(typeof(AvatarMouseTracking), "DoEye")]
        static bool ReplaceWithVRM1Eyetracking(Vrm10Instance ___vrm10, Camera ___mainCam, Animator ___animator,
            Transform ___leftEyeBone, Transform ___rightEyeBone, Transform ___eyeCenter, Transform ___leftEyeDriver, Transform ___rightEyeDriver,
             float ___eyeYawLimit, float ___eyePitchLimit, float ___eyeSmoothness)
        {
            var mouse = Input.mousePosition;
            var world = ___mainCam.ScreenToWorldPoint(new Vector3(mouse.x, mouse.y, ___mainCam.nearClipPlane));
            if (!___leftEyeBone || !___rightEyeBone || !___eyeCenter) return false;
            ___eyeCenter.position = (___leftEyeBone.position + ___rightEyeBone.position) * 0.5f;
            var dir = (world - ___eyeCenter.position).normalized;
            var localDir = ___animator.GetBoneTransform(HumanBodyBones.Head).InverseTransformDirection(dir);
            float eyeYaw = Mathf.Clamp(Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg, -___eyeYawLimit, ___eyeYawLimit);
            float eyePitch = Mathf.Clamp(Mathf.Asin(localDir.y) * Mathf.Rad2Deg, -___eyePitchLimit, ___eyePitchLimit);
            if (___vrm10)
            {
                var smoothYaw = Mathf.Lerp(___vrm10.Runtime.LookAt.Yaw, eyeYaw, Time.deltaTime * ___eyeSmoothness);
                var smoothPitch = Mathf.Lerp(___vrm10.Runtime.LookAt.Pitch, eyePitch, Time.deltaTime * ___eyeSmoothness);

                ___vrm10.Runtime.LookAt.SetYawPitchManually(smoothYaw, smoothPitch);
                return false;
            }
            var eyeRot = Quaternion.Euler(-eyePitch, eyeYaw, 0f);
            ___leftEyeDriver.localRotation = Quaternion.Slerp(___leftEyeDriver.localRotation, eyeRot, Time.deltaTime * ___eyeSmoothness);
            ___rightEyeDriver.localRotation = Quaternion.Slerp(___rightEyeDriver.localRotation, eyeRot, Time.deltaTime * ___eyeSmoothness);

            return false;
        }
    }

    [HarmonyPatch, OptionalPatch]
    class FixSpringBoneJittering
    {
        /**
         * https://github.com/shinyflvre/Mate-Engine/issues/526
         */

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AvatarGravityController), "Start")]
        static void OverrideDefaultValue(ref float ___impactMultiplier)
        {
            ___impactMultiplier = 1.5f;
        }

        static readonly Func<AvatarGravityController, Vector2Int> GetWindowPosition = AccessTools.MethodDelegate<Func<AvatarGravityController, Vector2Int>>(
            AccessTools.Method(typeof(AvatarGravityController), "GetWindowPosition"));

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AvatarGravityController), "Update")]
        static bool ReplaceWithNoForceNormalization(AvatarGravityController __instance, ref Vector2Int ___previousWindowPos, ref Vector3 ___currentForce, float ___impactMultiplier,
            List<VRMSpringBone> ___springBones, List<VRM10SpringBoneJoint> ___springBoneJoints, Vrm10Instance ___vrm10Instance)
        {
            Vector2Int currentWindowPos = GetWindowPosition(__instance);
            Vector2Int delta = currentWindowPos - ___previousWindowPos;
            ___previousWindowPos = currentWindowPos;

            if (delta != Vector2Int.zero)
            {
                ___currentForce = new Vector3(
                    -delta.x / Screen.dpi,
                    delta.y / Screen.dpi,
                    0
                ) * ___impactMultiplier;
            }
            else
            {
                ___currentForce = Vector3.zero;
            }

            // VRM0: set external force
            foreach (var spring in ___springBones)
            {
                if (spring != null)
                    spring.ExternalForce = ___currentForce;
            }

            // VRM1: apply gravity dir/power and notify runtime
            foreach (var joint in ___springBoneJoints)
            {
                if (joint == null) continue;

                joint.m_gravityDir = ___currentForce.normalized;
                joint.m_gravityPower = ___currentForce.magnitude;

                if (___vrm10Instance != null && ___vrm10Instance.Runtime != null)
                {
                    ___vrm10Instance.Runtime.SpringBone.SetJointLevel(joint.transform, joint.Blittable);
                }
            }

            return false;
        }
    }

    [HarmonyPatch, OptionalPatch]
    class FixSaveLoadHandlerStattering
    {
        static bool isDirty;
        static SaveLoadHandler saveLoadHandler;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SaveLoadHandler), "Awake")]
        static void Init(SaveLoadHandler __instance)
        {
            __instance.StartCoroutine(AutoSave());
            saveLoadHandler = __instance;
        }

        static IEnumerator AutoSave()
        {
            while (true)
            {
                if (isDirty)
                {
                    ActualSaveToDisk(saveLoadHandler);
                    isDirty = false;
                }

                yield return new WaitForSeconds(5);
            }
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(SaveLoadHandler), nameof(SaveLoadHandler.SaveToDisk))]
        static void ActualSaveToDisk(SaveLoadHandler instance) => 
            throw new NotImplementedException();

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SaveLoadHandler), nameof(SaveLoadHandler.SaveToDisk))]
        static bool ReplaceWithMarkDirtyInsteadOfSaving()
        {
            isDirty = true;
            return false;
        }

        static Transform _root;
        static Transform Root => _root ??= GameObject.Find("Model").transform;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SaveLoadHandler), nameof(SaveLoadHandler.ApplyAllSettingsToAllAvatars))]
        static bool ReplaceWithoutFindObjectsOfTypeAll()
        {
            var data = SaveLoadHandler.Instance.data;
            var avatars = Root.GetComponentsInChildren<AvatarAnimatorController>(true);
            //var avatars = Resources.FindObjectsOfTypeAll<AvatarAnimatorController>();

            foreach (var avatar in avatars)
            {
                avatar.SOUND_THRESHOLD = data.soundThreshold;
                avatar.IDLE_SWITCH_TIME = data.idleSwitchTime;
                avatar.IDLE_TRANSITION_TIME = data.idleTransitionTime;
                avatar.enableDancing = data.enableDancing;
                avatar.allowedApps = new List<string>(data.allowedApps);
                avatar.transform.localScale = Vector3.one * data.avatarSize;
                avatar.DANCE_SWITCH_TIME = data.danceSwitchTime;
                avatar.DANCE_TRANSITION_TIME = data.danceTransitionTime;
                avatar.enableDanceSwitch = data.enableDanceSwitch;
                avatar.enableHusbandoMode = data.enableHusbandoMode;

                foreach (var tracker in avatar.GetComponentsInChildren<AvatarMouseTracking>(true))
                {
                    tracker.enableMouseTracking = data.enableMouseTracking;
                    tracker.headBlend = data.headBlend;
                    tracker.spineBlend = data.spineBlend;
                    tracker.eyeBlend = data.eyeBlend;
                }

                foreach (var ik in avatar.GetComponentsInChildren<IKFix>(true))
                    ik.enableIK = data.enableIK;

                foreach (var handler in avatar.GetComponentsInChildren<AvatarParticleHandler>(true))
                {
                    handler.featureEnabled = data.enableParticles;
                    handler.enabled = data.enableParticles;
                    handler.selectedTheme = data.selectedParticleTheme;
                    try { handler.SetTheme(data.selectedParticleTheme); } catch { }
                }

                foreach (var holder in avatar.GetComponentsInChildren<HandHolder>(true))
                    holder.enableHandHolding = data.enableHandHolding;

                if (avatar.animator != null &&
                    avatar.animator.isActiveAndEnabled &&
                    avatar.animator.runtimeAnimatorController != null)
                {
                    avatar.animator.SetBool("isDancing", false);
                    avatar.animator.SetBool("isDragging", false);
                    avatar.isDancing = false;
                    avatar.isDragging = false;
                }

                foreach (var food in Root.GetComponentsInChildren<AvatarFoodController>(true))
                //foreach (var food in Resources.FindObjectsOfTypeAll<AvatarFoodController>())
                    food.SetFeatureEnabled(SaveLoadHandler.Instance.data.enableFeedSystem);

                foreach (var handler in Root.GetComponentsInChildren<AvatarWindowHandler>(true))
                //foreach (var handler in Resources.FindObjectsOfTypeAll<AvatarWindowHandler>())
                    handler.windowSitYOffset = data.windowSitYOffset;

                foreach (var loco in Root.GetComponentsInChildren<AvatarLocomotionController>(true))
                //foreach (var loco in Resources.FindObjectsOfTypeAll<AvatarLocomotionController>())
                    loco.EnableLocomotion = data.enableLocomotion;

            }

            return false;
        }
    }
}
