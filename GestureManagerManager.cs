using BlackStartX.GestureManager.Editor.Modules;
using BlackStartX.GestureManager.Editor.Modules.Vrc3;
using BlackStartX.GestureManager.Editor.Modules.Vrc3.Tools;
using HarmonyLib;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using Xamin;

namespace BlackStartX.GestureManager
{
    public class GestureManagerManager : MonoBehaviour
    {
        [SerializeField] private GestureManager Manager;
        [SerializeField] private RadialMenuController radialMenuController;
        [SerializeField] private Button circleSelectorButton;

        [SerializeField] private bool bindMEController = true;

        bool bigScreenWasActive;

        AccessTools.FieldRef<AvatarTools.ClickableContacts, bool> isClickableContactsActive = AccessTools.FieldRefAccess<AvatarTools.ClickableContacts, bool>("_isActive");
        
        bool isButtonСonfigured;
        RuntimeAnimatorController avatarControllerME;
        MenuActions menuActions;
        int actionsClothesIndex;
        MenuEntry actionsClothesEntryOrigin;
        CircleSelector circleSelector;
        int selectorClothesIndex;
        GameObject selectorClothesButtonOrigin;


        void Awake()
        {
            CurrentModel.OnAwake();
        }
        void OnEnable()
        {
            CurrentModel.OnAvatarSwitch += OnAvatarSwitch;
        }
        void Start()
        {
            CacheClothesItems();

            avatarControllerME = FindFirstObjectByType<VRMLoader>().animatorController;
        }
        void Update()
        {
            CurrentModel.OnUpdate();

            if (Manager == null || Manager.Module == null) return;

            var isBigScreenActive = CurrentModel.AvatarBigScreenHandlerProxy.isBigScreenActive;
            if (bigScreenWasActive != (bigScreenWasActive = isBigScreenActive))
            {
                var m = (ModuleVrc3)Manager.Module; // some day it will be changed
                isClickableContactsActive(m.AvatarTools.ContactsClickable) = isBigScreenActive;
            }
        }
        void OnDisable()
        {
            CurrentModel.OnAvatarSwitch -= OnAvatarSwitch;
        }
        void CacheClothesItems()
        {
            var circleMenu = GameObject.Find("CircleMenu");

            if (circleMenu == null)  return;

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
            var module = ModuleHelper.GetModuleFor(CurrentModel.ModelGO);
            if (module == null)
            {
                Manager.UnlinkModule();
                if (isButtonСonfigured) RestoreOriginalClothesButton();
                return;
            }

            if (bindMEController)
                BindMEControllerToDescriptor();

            Manager.SetModule(module);

            if (!isButtonСonfigured)
                SetupClothesButton();
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
            var descriptor = CurrentModel.GetComponent<VRCAvatarDescriptor>();
            var baseLayers = descriptor.baseAnimationLayers;

            for (int i = 0; i < baseLayers.Length; i++)
                if (baseLayers[i].type == VRCAvatarDescriptor.AnimLayerType.Base)
                {
                    baseLayers[i].animatorController = avatarControllerME;
                    baseLayers[i].isDefault = false;
                    break;
                }
        }
    }
}