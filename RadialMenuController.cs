using BlackStartX.GestureManager;
using BlackStartX.GestureManager.Editor.Modules.Vrc3;
using BlackStartX.GestureManager.Editor.Modules.Vrc3.Params;
using HarmonyLib;
using MEGME.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.Avatars.Components;
using Xamin;
using static MEGME.ModSettings;
using static VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu;
using RadialSettings = BlackStartX.GestureManager.Editor.Modules.Vrc3.RadialSlices.RadialSliceControl.RadialSettings;

namespace MEGME
{
    public class RadialMenuController : MonoBehaviour
    {
        class MenuId : ScriptableObject { }

        [SerializeField] private GestureManager Manager;

        UIDocument doc;
        VisualElement root;

        RadialMenu ExpressionsMenu;
        RadialMenu SettingsMenu;

        MenuId settingsMenuId;
        MenuId expressionsMenuId;

        Rect expressionsMenuRect = new(1017, 493, 300, 300);
        Rect settingsMenuRect = new(1017, 493, 300, 300);

        bool isExpressionsMenuRendering;
        bool isSettingsMenuRendering;

        public GameObject ExpressionsMenuToggle;
        public GameObject ExpressionsMenuUIToggle;
        public GameObject SettingsMenuUIToggle;

        MenuActions actions;


        string targetSelectorButton = "Clothes";

        bool isButtonСonfigured;

        SettingsMenuPosition MEMenuPosition;
        readonly AccessTools.FieldRef<SettingsMenuPosition, bool> lastAtRightEdge = AccessTools.FieldRefAccess<SettingsMenuPosition, bool>("lastAtRightEdge");

        Vector2 menuPosOrigin = new(1168, 632);
        Vector2 targetRes = new(1536, 1024);

        CircleSelector selector;

        int actionsEntryIndex;
        MenuEntry actionsEntryOrigin;
        int selectorButtonIndex;
        GameObject selectorButtonOrigin;


        public GameObject MenuBlur;

        readonly List<ModSettings> modSettings = new();
        static readonly Queue<List<ModSettings>> registerRequests = new();
        bool layoutChanged;

        ModuleVrc3 dummyModule;
        VRCAvatarDescriptor dummyDescriptor;

        readonly AccessTools.FieldRef<MenuActions, CircleSelector> radialMenu = AccessTools.FieldRefAccess<MenuActions, CircleSelector>("radialMenu");
        CircleSelector radialMenuOrigin;

        RectTransform outerTransform;
        UiTooltip[] tooltips;

        void Awake()
        {
            doc = GetComponent<UIDocument>();
        }
        void OnEnable()
        {
            root = doc.rootVisualElement;

            root.pickingMode = PickingMode.Ignore;
            root.style.color = Color.white; // text color is inherited from parent
        }
        void Start()
        {
            settingsMenuId = ScriptableObject.CreateInstance<MenuId>();
            expressionsMenuId = ScriptableObject.CreateInstance<MenuId>();

            MEMenuPosition = FindFirstObjectByType<SettingsMenuPosition>();

            var circleMenu = GameObject.Find("CircleMenu");
            actions = circleMenu?.GetComponentInChildren<MenuActions>();
            selector = circleMenu?.GetComponentInChildren<CircleSelector>();

            if (actions && selector && MEMenuPosition)
                SetupExpressionsMenu();

            if (actions)
                SetupSettingsMenu();
        }
        public void OnUpdate()
        {
            if (SettingsMenu != null)
            {
                while (registerRequests.Count > 0)
                {
                    var record = registerRequests.Dequeue();
                    RegisterSettingsMenu(record);
                }

                if (layoutChanged)
                {
                    RefreshId(ref settingsMenuId);

                    SettingsMenu = dummyModule.GetOrCreateRadial(settingsMenuId);
                    SettingsMenu.Controller = this;
                    SettingsMenu.OpenSettingsMenu(modSettings);

                    layoutChanged = false;
                }
            }
        }
        void OnGUI()
        {
            if (ExpressionsMenu != null)
            {
                if (isExpressionsMenuRendering)
                {
                    CalculateExpressionsMenuPosition();

                    ExpressionsMenu.Rect = expressionsMenuRect;
                    ExpressionsMenu.Render(root, expressionsMenuRect);
                }
                else
                    ExpressionsMenu.StopRendering();
            }

            if (SettingsMenu != null)
            {
                if (isSettingsMenuRendering)
                {
                    CalculateSettingsMenuPosition();

                    SettingsMenu.Rect = settingsMenuRect;
                    SettingsMenu.Render(root, settingsMenuRect);

                    if (Input.GetKeyDown(actions.radialMenuKey))
                        ToggleSettingsMenu();
                }
                else
                    SettingsMenu.StopRendering();
            }
        }
        internal void OnAvatarSwitch()
        {
            if (Manager.Module == null)
            {
                ExpressionsMenu?.StopRendering();
                ExpressionsMenu = null;

                isExpressionsMenuRendering = false;

                if (isButtonСonfigured)
                    RestoreOriginalSelectorButton();
            }
            else
            {
                ExpressionsMenu = Manager.Module.GetOrCreateRadial(expressionsMenuId);

                if (!isButtonСonfigured)
                    SetupSelectorButton();
            }
        }
        void CalculateExpressionsMenuPosition()
        {
            if (!MEMenuPosition) return;

            var screenSize = root.layout.size;

            var offsetX = lastAtRightEdge(MEMenuPosition) ? screenSize.x / 2 - targetRes.x : screenSize.x - targetRes.x;
            var offsetY = screenSize.y - targetRes.y;

            expressionsMenuRect.center = menuPosOrigin + new Vector2(offsetX, offsetY);
        }
        void CalculateSettingsMenuPosition()
        {
            var world = outerTransform.TransformPoint(outerTransform.rect.center);
            var screen = RectTransformUtility.WorldToScreenPoint(
                Camera.main,
                world
            );
            var panel = RuntimePanelUtils.ScreenToPanel(
                root.panel,
                new Vector2(
                    screen.x,
                    Screen.height - screen.y
                )
            );
            settingsMenuRect.center = panel;
        }

        public void ToggleExpressionsMenu()
        {
            isExpressionsMenuRendering = !isExpressionsMenuRendering;
            if (!isExpressionsMenuRendering)
            {
                ExpressionsMenu.ClosePuppet();
            }
        }
        public void ToggleSettingsMenu()
        {
            isSettingsMenuRendering = !isSettingsMenuRendering;
            if (isSettingsMenuRendering)
            {
                MenuBlur.SetActive(true);
                radialMenu(actions) = null;

                foreach (var t in tooltips)
                {
                    t.enabled = false;
                }
            }
            else
            {
                SettingsMenu.ClosePuppet();
                StartCoroutine(RestoreRadialMenu());
            }
        }
        IEnumerator RestoreRadialMenu() // Input.GetKeyDown(radialMenuKey) in MenuActions still recives Input event the next frame after it occur (unity moment?)
        {
            yield return null;

            MenuBlur.SetActive(false);
            radialMenu(actions) = radialMenuOrigin;

            foreach (var t in tooltips)
            {
                t.enabled = true;
            }
        }
        void SetupExpressionsMenu()
        {
            var menuEntries = actions.menuEntries;
            for (int j = 0; j < menuEntries.Count; j++)
                if (menuEntries[j].menu.name == targetSelectorButton)
                {
                    actionsEntryIndex = j;
                    actionsEntryOrigin = menuEntries[j];
                    break;
                }

            var selectorButtons = selector.Buttons;
            for (int j = 0; j < selectorButtons.Count; j++)
                if (selectorButtons[j].name == targetSelectorButton)
                {
                    selectorButtonIndex = j;
                    selectorButtonOrigin = selectorButtons[j];
                    break;
                }

            if (actionsEntryOrigin == null || selectorButtonOrigin == null)
                Debug.LogWarning($"[MEGME] ExpressionsMenu setup failed, ActionsEntry:{actionsEntryOrigin} SelectorEntry:{selectorButtonOrigin}");
        }
        void SetupSettingsMenu()
        {
            radialMenuOrigin = radialMenu(actions);

            var settings = GameObject.Find("/Settings");
            var canvas = settings?.transform.Find("SettingsMenuCanvas");
            var outerMenu = settings?.transform.Find("SettingsMenuCanvas/OuterMenu");
            var mainMenu = settings?.transform.Find("SettingsMenuCanvas/Main Menu/Viewport/Content/MenuPanel/Main Menu");
            if (!settings || !canvas || !outerMenu || !mainMenu)
            {
                Debug.LogWarning($"[MEGME] SettingsMenu setup failed, Settings:{settings} SettingsMenuCanvas:{canvas} OutherMenu:{outerMenu} MainMenu:{mainMenu}");
                return;
            }

            outerTransform = outerMenu.GetComponent<RectTransform>();
            var blurTransform = MenuBlur.GetComponent<RectTransform>();
            CopyRectTransform(outerTransform, blurTransform);

            MenuBlur.transform.SetParent(canvas, false);

            MEMenuPosition.menus.Add(new SettingsMenuPosition.MenuEntry
            {
                settingsMenu = blurTransform,
                originalX = blurTransform.anchoredPosition.x,
                originalY = blurTransform.anchoredPosition.y,
                lastApplied = blurTransform.anchoredPosition
            });

            SettingsMenuUIToggle.transform.SetParent(mainMenu, false);

            dummyDescriptor = gameObject.AddComponent<VRCAvatarDescriptor>();
            gameObject.AddComponent<Animator>();
            dummyModule = new ModuleVrc3(dummyDescriptor);

            SettingsMenu = dummyModule.GetOrCreateRadial(settingsMenuId);

            SettingsMenu.Controller = this;

            SettingsMenu.OpenSettingsMenu(modSettings);

            tooltips = canvas.GetComponentsInChildren<UiTooltip>();
        }
        static void CopyRectTransform(RectTransform from, RectTransform to)
        {
            to.localPosition = from.localPosition;
            to.localScale = from.localScale;
            to.localRotation = from.localRotation;

            to.anchorMin = from.anchorMin;
            to.anchorMax = from.anchorMax;
            to.anchoredPosition = from.anchoredPosition;

            to.sizeDelta = from.sizeDelta;
            to.pivot = from.pivot;
        }
        void RefreshId<T>(ref T id) where T : ScriptableObject
        {
            id = ScriptableObject.CreateInstance<T>();
        }
        private void SetupSelectorButton()
        {
            if (!actions || !selector) return;

            actions.menuEntries[actionsEntryIndex] = new MenuEntry
            {
                menu = ExpressionsMenuToggle,
                blockMovement = true,
                blockHandTracking = true,
                blockReaction = true,
                blockChibiMode = true
            };
            selector.Buttons[selectorButtonIndex] = ExpressionsMenuUIToggle;

            isButtonСonfigured = true;
        }

        private void RestoreOriginalSelectorButton()
        {
            if (!actions || !selector) return;

            actions.menuEntries[actionsEntryIndex] = actionsEntryOrigin;
            selector.Buttons[selectorButtonIndex] = selectorButtonOrigin;

            isButtonСonfigured = false;
        }

        public static void RegisterSettingsMenu(ModSettings setting, params ModSettings[] s) => registerRequests.Enqueue([setting, .. s]);
        void RegisterSettingsMenu(List<ModSettings> settings)
        {
            Integrate(settings, modSettings);

            void Integrate(List<ModSettings> from, List<ModSettings> to, string menuName = null)
            {
                foreach (var set in from)
                {
                    var existingMenu = to.FirstOrDefault(s =>
                        s.name == set.name && s.controlType == set.controlType && s.controlType == Control.ControlType.SubMenu);

                    if (existingMenu != null)
                    {
                        Debug.Log($"[MEGME] Expanding menu '{set.name}'");
                        Integrate(set.subSettings, existingMenu.subSettings, set.name);
                    }
                    else
                    {
                        to.Add(set);
                    }
                }
            }

            layoutChanged = true;
        }
    }
    public class ModSettings(string name, ParamBinding bind, Control.ControlType controlType, Texture2D icon = null,
        float offValue = 0, float onValue = 1, RadialSettings radialSettings = null,
        ParamBinding[] subBinds = null, List<ModSettings> subSettings = null, Control.Label[] subLabels = null)
    {
        public string name = name;

        public Control.ControlType controlType = controlType;
        public Texture2D icon = icon ?? MEGMEStyles.Void;

        public float onValue = onValue;
        public float offValue = offValue;

        public ParamBinding bind = bind;
        public ParamBinding[] subBinds = subBinds;

        public Control.Label[] subLabels = subLabels;
        public List<ModSettings> subSettings = subSettings;

        public RadialSettings radialSettings = radialSettings;

        public static ModSettings Toggle(string name, ISetting setting, Texture2D icon = null)
        {
            return new ModSettings(name, new ParamBinding(setting), Control.ControlType.Toggle, icon);
        }
        public static ModSettings Radial(string name, ISetting setting, float min = 0, float max = 1, float? checkpoint = null, DisplayType displayType = DisplayType.Percentage, Texture2D icon = null)
        {
            return new ModSettings(name, null, Control.ControlType.RadialPuppet, icon, radialSettings: new RadialSettings((RadialSettings.DisplayType)displayType, min, max, checkpoint), subBinds: [new ParamBinding(setting)]);
        }
        public static ModSettings SubMenu(string name, ModSettings subSetting, params ModSettings[] s) => SubMenu(name, null, subSetting, s);
        public static ModSettings SubMenu(string name, Texture2D icon, ModSettings subSetting, params ModSettings[] s)
        {
            return new ModSettings(name, null, Control.ControlType.SubMenu, icon, subSettings: [subSetting, .. s]);
        }

        public enum DisplayType
        {
            Percentage = RadialSettings.DisplayType.Percentage,
            Meters = RadialSettings.DisplayType.Meters,
            Absolute = RadialSettings.DisplayType.Absolute,
            Degree = RadialSettings.DisplayType.Degree
        }
        public ParamBinding[] GetBinds()
        {
            return controlType switch
            {
                Control.ControlType.Toggle => [bind],
                Control.ControlType.RadialPuppet => subBinds,
                Control.ControlType.SubMenu => GetSubBinds().ToArray(),
                _ => throw new NotImplementedException()
            };

            List<ParamBinding> GetSubBinds()
            {
                var binds = new List<ParamBinding>();
                foreach (var subs in subSettings)
                {
                    binds.AddRange(subs.GetBinds());
                }
                return binds;
            }
        }

        public class ParamBinding
        {
            public ISetting Setting;
            public Vrc3Param Param;

            public ParamBinding(ISetting setting)
            {
                var (ToFloat, FromFloat) = Converters.TryGetValue(setting.Type, out var c)
                    ? c
                    : throw new NotSupportedException($"[MEGME] Param converter for type {setting.Type} is not supported");

                void OnParamChange(Vrc3Param param, float value)
                {
                    Setting.BoxedValue = FromFloat(value);
                }
                void OnSettingChange(ISetting set)
                {
                    Param.InternalSet(ToFloat(set.BoxedValue));
                }

                var param = new Vrc3Param(setting.Key, AnimatorControllerParameterType.Float, OnParamChange);

                setting.OnChange += OnSettingChange;

                Param = param;
                Setting = setting;
            }

            static readonly Dictionary<Type, (Func<object, float> ToFloat, Func<float, object> FromFloat)> Converters = new()
            {
                [typeof(float)] = (
                    v => (float)v,
                    v => v
                ),
                [typeof(int)] = (
                    v => (int)v,
                    v => (int)v
                ),
                [typeof(bool)] = (
                    v => (bool)v ? 1f : 0f,
                    v => v != 0f
                )
            };
        }
    }
}