using BlackStartX.GestureManager.Editor.Modules.Vrc3;
using BlackStartX.GestureManager.Editor.Modules.Vrc3.Tools;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UniVRM10;
using VRC.SDK3.Avatars.Components;
using VRM;
using Xamin;
using static VRC.SDKBase.VRC_AvatarDescriptor;

namespace BlackStartX.GestureManager
{
    [DefaultExecutionOrder(+10)] // Should run after all possible MEManipulators
    public class GestureManagerManager : MonoBehaviour
    {
        [SerializeField] private GestureManager Manager;
        [SerializeField] private RadialMenuController radialMenuController;
        [SerializeField] private Button circleSelectorButton;

        [SerializeField] private bool bindMEController = true;

        bool bigScreenWasActive;

        AccessTools.FieldRef<AvatarTools.ClickableContacts, bool> isClickableContactsActive = AccessTools.FieldRefAccess<AvatarTools.ClickableContacts, bool>("_isActive");

        bool isButtonСonfigured;
        MenuActions menuActions;
        int actionsClothesIndex;
        MenuEntry actionsClothesEntryOrigin;
        CircleSelector circleSelector;
        int selectorClothesIndex;
        GameObject selectorClothesButtonOrigin;

        public static Queue<(string name, List<ModSettings> settings)> settingRegisterRequests = new();

        void OnEnable()
        {
            CurrentModel.OnAvatarSwitch += OnAvatarSwitch;
            CurrentModel.OnAvatarSwitch += radialMenuController.OnAvatarSwitch;
        }
        void Start()
        {
            CurrentModel.OnStart();
            CacheClothesItems();
        }
        void Update()
        {
            CurrentModel.OnUpdate();

            while (settingRegisterRequests.Count > 0)
            {
                var record = settingRegisterRequests.Dequeue();
                radialMenuController.RegisterSettingsMenu(record.name, record.settings);
            }

            if (Manager == null || Manager.Module == null) return;

            var isBigScreenActive = CurrentModel.AvatarBigScreenHandlerProxy.isBigScreenActive;
            if (bigScreenWasActive != (bigScreenWasActive = isBigScreenActive))
            {
                isClickableContactsActive(Manager.Module.AvatarTools.ContactsClickable) = isBigScreenActive;
            }
        }
        void OnDisable()
        {
            CurrentModel.OnAvatarSwitch -= OnAvatarSwitch;
            CurrentModel.OnAvatarSwitch -= radialMenuController.OnAvatarSwitch;
        }
        void CacheClothesItems()
        {
            var circleMenu = GameObject.Find("CircleMenu");

            if (circleMenu == null) return;

            menuActions = circleMenu.GetComponentInChildren<MenuActions>();
            circleSelector = circleMenu.GetComponentInChildren<CircleSelector>();

            var menuEntries = menuActions.menuEntries;
            for (int j = 0; j < menuEntries.Count; j++)
                if (menuEntries[j].menu.name == "Clothes")
                {
                    actionsClothesIndex = j;
                    actionsClothesEntryOrigin = menuEntries[j];
                    break;
                }

            var selectorButtons = circleSelector.Buttons;
            for (int j = 0; j < selectorButtons.Count; j++)
                if (selectorButtons[j].name == "Clothes")
                {
                    selectorClothesIndex = j;
                    selectorClothesButtonOrigin = selectorButtons[j];
                    break;
                }

            if (actionsClothesEntryOrigin == null || selectorClothesButtonOrigin == null)
                Debug.LogWarning("[MEGME] No Clothes buttons entries found");
        }
        private void OnAvatarSwitch()
        {
            var descriptor = CurrentModel.GetComponent<VRCAvatarDescriptor>();
            if (descriptor == null)
            {
                Manager.UnlinkModule();
                if (isButtonСonfigured) RestoreOriginalClothesButton();
                return;
            }

            var module = new ModuleVrc3(descriptor);

            if (bindMEController)
                BindMEControllerToDescriptor();

            Manager.SetModule(module);

            if (!isButtonСonfigured)
                SetupClothesButton();

            var isBlendShapesConfigured = CurrentModel.GetComponent<Vrm10Instance>()?.Runtime.Expression.ExpressionKeys.Count > 0 
                || CurrentModel.GetComponent<VRMBlendShapeProxy>()?.BlendShapeAvatar.Clips.Count > 0;
            if (!isBlendShapesConfigured)
            {
                TrySetupBlendshapes();
            }
        }

        private void SetupClothesButton()
        {
            if (!menuActions || !circleSelector) return;

            menuActions.menuEntries[actionsClothesIndex] = new MenuEntry
            {
                menu = radialMenuController.gameObject,
                blockMovement = true,
                blockHandTracking = true,
                blockReaction = true,
                blockChibiMode = true
            };
            circleSelector.Buttons[selectorClothesIndex] = circleSelectorButton.gameObject;

            isButtonСonfigured = true;
        }

        private void RestoreOriginalClothesButton()
        {
            if (!menuActions || !circleSelector) return;

            menuActions.menuEntries[actionsClothesIndex] = actionsClothesEntryOrigin;
            circleSelector.Buttons[selectorClothesIndex] = selectorClothesButtonOrigin;

            isButtonСonfigured = false;
        }

        private void BindMEControllerToDescriptor()
        {
            var animator = CurrentModel.GetComponent<Animator>();
            var controller = animator.runtimeAnimatorController;

            var descriptor = CurrentModel.GetComponent<VRCAvatarDescriptor>();
            var baseLayers = descriptor.baseAnimationLayers;

            for (int i = 0; i < baseLayers.Length; i++)
                if (baseLayers[i].type == VRCAvatarDescriptor.AnimLayerType.Base)
                {
                    baseLayers[i].animatorController = controller;
                    baseLayers[i].isDefault = false;
                    break;
                }
        }

        public static void RegisterSettingsMenu(string name, List<ModSettings> settings) => settingRegisterRequests.Enqueue((name, settings));

        private void TrySetupBlendshapes()
        {
            var descriptor = CurrentModel.GetComponent<VRCAvatarDescriptor>();

            var visemeMesh = descriptor.VisemeSkinnedMesh;
            var eyelidMesh = descriptor.customEyeLookSettings.eyelidsSkinnedMesh;

            CurrentModel.UniversalBlendshapesProxy.proxy0 = CurrentModel.AddComponent<VRMBlendShapeProxy>();
            var blendShapeAvatar = ScriptableObject.CreateInstance<BlendShapeAvatar>();

            foreach (var (preset, entry) in VRC_VRM)
            {
                SkinnedMeshRenderer targetMesh = null;
                int targetBlendShapeIndex = -1;

                //foreach (var item in entry.se)
                //{
                    
                //}

                switch (entry.Source)
                {
                    case BlendShapeSourceType.VisemeSkinnedMesh:
                        if (visemeMesh == null) break;

                        int visemeIndex = (int)entry.Viseme;

                        var blendShapeName = descriptor.VisemeBlendShapes[visemeIndex];

                        targetMesh = visemeMesh;
                        targetBlendShapeIndex = visemeMesh.sharedMesh.GetBlendShapeIndex(blendShapeName);

                        Debug.Log($"[MEGME] Preset {preset}, rule {entry.Source}, {blendShapeName} == {targetMesh.name}[{targetBlendShapeIndex}]" +
                            $"->{targetMesh.sharedMesh.GetBlendShapeName(targetBlendShapeIndex)}");
                        break;

                    case BlendShapeSourceType.EyelidsSkinnedMesh:
                        if (eyelidMesh == null) break;

                        int eyelidIndex = (int)entry.EyelidState;

                        var blendShapeIndex = descriptor.customEyeLookSettings.eyelidsBlendshapes[eyelidIndex];
                        if (blendShapeIndex < 0) break;

                        targetMesh = eyelidMesh;
                        targetBlendShapeIndex = blendShapeIndex;

                        Debug.Log($"[MEGME] Preset {preset}, rule {entry.Source}, {targetMesh.name}[{targetBlendShapeIndex}]" +
                            $"->{targetMesh.sharedMesh.GetBlendShapeName(targetBlendShapeIndex)}");
                        break;
                }

                if (targetMesh == null && entry.SearchNames != null)
                {
                    foreach (var mesh in new[] { eyelidMesh, visemeMesh })
                    {
                        if (mesh == null) continue;

                        for (int i = 0; i < mesh.sharedMesh.blendShapeCount; i++)
                        {
                            var blendShapeName = mesh.sharedMesh.GetBlendShapeName(i);
                            foreach (var search in entry.SearchNames)
                            {
                                if (StringToRegex(search).IsMatch(blendShapeName))
                                {
                                    targetBlendShapeIndex = i;
                                    targetMesh = mesh;
                                    Debug.Log($"[MEGME] Preset {preset}, rule {search}, {targetMesh.name}[{targetBlendShapeIndex}]" +
                                        $"->{targetMesh.sharedMesh.GetBlendShapeName(targetBlendShapeIndex)}");
                                    break;
                                }
                            }
                            if (targetMesh != null) break;
                        }
                        if (targetMesh != null) break;
                    }
                }

                if (targetMesh != null && targetBlendShapeIndex >= 0)
                {
                    var clip = ScriptableObject.CreateInstance<BlendShapeClip>();
                    clip.Preset = preset;
                    clip.Values = [
                        new BlendShapeBinding
                        {
                            RelativePath = RelativePathFrom(targetMesh.transform, CurrentModel.transform),
                            Index = targetBlendShapeIndex,
                            Weight = 100f
                        }
                    ];

                    blendShapeAvatar.Clips.Add(clip);
                }
            }

            CurrentModel.UniversalBlendshapesProxy.proxy0.BlendShapeAvatar = blendShapeAvatar;
        }

        public static Regex StringToRegex(string s)
        {
            var escaped = Regex.Escape(s);
            var deGlob = escaped.Replace("\\*", ".*");
            return new Regex("^" + deGlob + "$", RegexOptions.IgnoreCase);
        }

        // MEX3.3.0 UniVRM0.128.3 UniGLTF.UnityExtensions.RelativePathFrom NonExtension variant
        public static string RelativePathFrom(Transform self, Transform root)
        {
            var path = new List<String>();
            for (var current = self; current != null; current = current.parent)
            {
                if (current == root)
                {
                    return String.Join("/", path.ToArray());
                }

                path.Insert(0, current.name);
            }

            throw new Exception("no RelativePath");
        }
        // AAO_Merged_*_BSNumber
        Dictionary<BlendShapePreset, BlendShapeMapEntry> VRC_VRM = new Dictionary<BlendShapePreset, BlendShapeMapEntry>
        {
            { BlendShapePreset.Neutral, BlendShapeMapEntry.From(Viseme.sil, "vrc.v_sil", "sil", "neutral") },
            { BlendShapePreset.A, BlendShapeMapEntry.From(Viseme.aa, "vrc.v_aa", "aa", "あ","mouth*a*") },
            { BlendShapePreset.I, BlendShapeMapEntry.From(Viseme.ih, "vrc.v_ih", "ih", "い", "mouth*i*") },
            { BlendShapePreset.U, BlendShapeMapEntry.From(Viseme.ou, "vrc.v_ou", "ou", "う", "mouth*u*") },
            { BlendShapePreset.E, BlendShapeMapEntry.From(Viseme.E, "vrc.v_e", "e", "え", "mouth*e*") },
            { BlendShapePreset.O, BlendShapeMapEntry.From(Viseme.oh, "vrc.v_oh", "oh", "お", "mouth*o*") },
            { BlendShapePreset.Blink, BlendShapeMapEntry.From(EyelidState.Blink, "vrc.blink", "*blink", "まばたき", "blink_double", "eyeclosed") },
            { BlendShapePreset.Joy, BlendShapeMapEntry.From("*joy", "*happy", "にっこり") },
            { BlendShapePreset.Angry, BlendShapeMapEntry.From("*angry", "怒り") },
            { BlendShapePreset.Sorrow, BlendShapeMapEntry.From("*sorrow", "*sad", "困る") },
            { BlendShapePreset.Fun, BlendShapeMapEntry.From("*fun", "ワ") },
            { BlendShapePreset.LookUp, BlendShapeMapEntry.From(EyelidState.LookingUp, "vrc.looking_up", "*look*up*", "eye_move_up", "eye_up") },
            { BlendShapePreset.LookDown, BlendShapeMapEntry.From(EyelidState.LookingDown, "vrc.looking_down", "*look*down*", "eye_move_down", "eye_down") },
            { BlendShapePreset.LookLeft, BlendShapeMapEntry.From("*look*left*", "eyes_left", "eye_left", "eye_move_l") },
            { BlendShapePreset.LookRight, BlendShapeMapEntry.From("*look*right*", "eyes_right", "eye_right", "eye_move_r") },
            { BlendShapePreset.Blink_L, BlendShapeMapEntry.From("*blink*l", "eye_close_l", "eye_close_left", "eye_wink_l", "eyeclosedleft", "ウィンク２") },
            { BlendShapePreset.Blink_R, BlendShapeMapEntry.From("*blink*r", "eye_close_r", "eye_close_right", "eye_wink_r", "eyeclosedright", "ｳｨﾝｸ２右") },
            //{ BlendShapePreset.Joy, BlendShapeMapEntry.From(
            //    new Expr("*blink*r"),
            //    new Expr([
            //        ["eye_close_r"],
            //        ["eye_close_r"]
            //    ]),
            //    "eye_close_right",
            //    "eye_wink_r",
            //    "eyeclosedright",
            //    "ｳｨﾝｸ２右"
            //    )
            //},
        };
        struct BlendShapeMapEntry
        {
            public BlendShapeSourceType Source;
            public Viseme Viseme;
            public EyelidState EyelidState;
            public string[] SearchNames;

            public static BlendShapeMapEntry From(Viseme viseme, params string[] names)
            {
                return new BlendShapeMapEntry() { Source = BlendShapeSourceType.VisemeSkinnedMesh, Viseme = viseme, SearchNames = names };
            }
            public static BlendShapeMapEntry From(EyelidState state, params string[] names)
            {
                return new BlendShapeMapEntry() { Source = BlendShapeSourceType.EyelidsSkinnedMesh, EyelidState = state, SearchNames = names };
            }
            public static BlendShapeMapEntry From(string name, params string[] s)
            {
                return new BlendShapeMapEntry() { Source = BlendShapeSourceType.ManualSearch, SearchNames = [name, .. s] };
            }
        }
        enum BlendShapeSourceType
        {
            VisemeSkinnedMesh,
            EyelidsSkinnedMesh,
            ManualSearch
        }
        // VRCSDKA3.9.0 com.vrchat.avatars\Editor\VRCSDK\SDK3A\Components3\VRCAvatarDescriptorEditor3EyeLook.cs:300
        enum EyelidState
        {
            Blink,
            LookingUp,
            LookingDown
        }
    }
}