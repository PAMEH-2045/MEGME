using BlackStartX.GestureManager.Editor.Modules.Vrc3;
using BlackStartX.GestureManager.Editor.Modules.Vrc3.Params;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.Avatars.Components;
using static BlackStartX.GestureManager.Editor.Modules.Vrc3.RadialSlices.RadialSliceControl;
using static BlackStartX.GestureManager.ModSettings;
using static VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu;

namespace BlackStartX.GestureManager
{
    public class RadialMenuController : MonoBehaviour
    {
        [SerializeField] private GestureManager Manager;

        VisualElement _root;
        UIDocument doc;
        RadialMenu ExpressionsMenu;
        SettingsMenuPosition settingsMenuPosition;
        AccessTools.FieldRef<SettingsMenuPosition, bool> lastAtRightEdge = AccessTools.FieldRefAccess<SettingsMenuPosition, bool>("lastAtRightEdge");

        Vector2 menuPosOrigin = new(1168, 632);
        Vector2 targetRes = new(1536, 1024);
        Rect expressionsMenuRect = new(1017, 493, 300, 300);
        Rect settingsMenuRect = new(1017, 493, 300, 300);

        List<ModSettings> ModSettings = new();
        RadialMenu SettingsMenu;

        bool isExpressionsMenuRendering;
        bool isSettingsMenuRendering;

        public GameObject SettingsMenuToggle;
        public GameObject ExpressionsMenuToggle;

        public GameObject MenuBlur;

        static Queue<List<ModSettings>> registerRequests = new();

        bool layoutChanged;

        ModuleVrc3 dummyModule;
        VRCAvatarDescriptor dummyDescriptor;

        AccessTools.FieldRef<MenuActions, Xamin.CircleSelector> radialMenu = AccessTools.FieldRefAccess<MenuActions, Xamin.CircleSelector>("radialMenu");
        Xamin.CircleSelector radialMenuOrig;
        MenuActions menuActions;

        RectTransform outerTransform;
        UiTooltip[] tooltips;

        void Awake()
        {
            doc = GetComponent<UIDocument>();
        }
        void OnEnable()
        {
            _root = doc.rootVisualElement;

            _root.pickingMode = PickingMode.Ignore;
            _root.style.color = Color.white; // text color is inherited from parent
        }
        void Start()
        {
            settingsMenuPosition = FindFirstObjectByType<SettingsMenuPosition>();

            menuActions = GameObject.Find("CircleMenu")?.GetComponentInChildren<MenuActions>();

            if (settingsMenuPosition && menuActions)
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
                    dummyModule.ReloadRadials();

                    SettingsMenu = dummyModule.GetOrCreateRadial(this);
                    SettingsMenu.Controller = this;
                    SettingsMenu.OpenSettingsMenu(ModSettings);

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
                    ExpressionsMenu.Render(_root, expressionsMenuRect);
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
                    SettingsMenu.Render(_root, settingsMenuRect);

                    if (Input.GetKeyDown(menuActions.radialMenuKey))
                        ToggleSettingsMenu();
                }
                else
                    SettingsMenu.StopRendering();
            }
        }
        internal void OnAvatarSwitch()
        {
            if (Manager.Module == null)
                return;

            ExpressionsMenu = Manager.Module.GetOrCreateRadial(this);
        }
        void CalculateExpressionsMenuPosition()
        {
            if (!settingsMenuPosition) return;

            var screenSize = _root.layout.size;

            var offsetX = lastAtRightEdge(settingsMenuPosition) ? screenSize.x / 2 - targetRes.x : screenSize.x - targetRes.x;
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
                _root.panel,
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
                radialMenu(menuActions) = null;
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
            radialMenu(menuActions) = radialMenuOrig;
            foreach (var t in tooltips)
            {
                t.enabled = true;
            }
        }

        void SetupSettingsMenu()
        {
            radialMenuOrig = radialMenu(menuActions);

            var settings = GameObject.Find("/Settings");
            var canvas = settings?.transform.Find("SettingsMenuCanvas");
            var outerMenu = settings?.transform.Find("SettingsMenuCanvas/OuterMenu");
            var mainMenu = settings?.transform.Find("SettingsMenuCanvas/Main Menu/Viewport/Content/MenuPanel/Main Menu");
            if (!settings || !canvas || !outerMenu || !mainMenu)
            {
                Debug.Log($"[MEGME] SettingsMenu setup failed, Settings:{settings} SettingsMenuCanvas:{canvas} OutherMenu:{outerMenu} MainMenu:{mainMenu}");
                return;
            }

            outerTransform = outerMenu.GetComponent<RectTransform>();
            var blurTransform = MenuBlur.GetComponent<RectTransform>();
            CopyRectTransform(outerTransform, blurTransform);

            MenuBlur.transform.SetParent(canvas, false);

            settingsMenuPosition.menus.Add(new SettingsMenuPosition.MenuEntry
            {
                settingsMenu = blurTransform,
                originalX = blurTransform.anchoredPosition.x,
                originalY = blurTransform.anchoredPosition.y,
                lastApplied = blurTransform.anchoredPosition
            });

            SettingsMenuToggle.transform.SetParent(mainMenu, false);

            dummyDescriptor = gameObject.AddComponent<VRCAvatarDescriptor>();
            dummyModule = new ModuleVrc3(dummyDescriptor);

            SettingsMenu = dummyModule.GetOrCreateRadial(this);

            SettingsMenu.Controller = this;

            SettingsMenu.OpenSettingsMenu(ModSettings);

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

        public static void RegisterSettingsMenu(ModSettings setting, params ModSettings[] s) => registerRequests.Enqueue([setting, .. s]);

        void RegisterSettingsMenu(List<ModSettings> settings)
        {
            Integrate(settings, ModSettings);

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
                        try
                        {
                            var binds = set.GetBinds();
                            foreach (var bind in binds)
                                bind.Init();

                            Debug.Log($"[MEGME] Added new element '{set.name}' of type {set.controlType}" +
                                $"{(binds.Length != 1 ? $"({binds.Length})" : "")} to {menuName ?? "Root"} menu");
                        }
                        catch (Exception e)
                        {
                            to.Remove(set);

                            Debug.LogError($"[MEGME] Registration of element '{set.name}:{set.controlType}" +
                                $"{(set.subBinds != null ? $"({set.subBinds.Length})" : "")}' to {menuName ?? "Root"} failed:{e}");
                        }
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
        public Texture2D icon = icon ?? EResources.Load<Texture2D>("Void");
        public float onValue = onValue;
        public ParamBinding bind = bind;
        public float offValue = offValue;
        public ParamBinding[] subBinds = subBinds;
        public List<ModSettings> subSettings = subSettings;
        public Control.Label[] subLabels = subLabels;
        public Control.ControlType controlType = controlType;
        public RadialSettings radialSettings = radialSettings;

        public static ModSettings Toggle(string name, ValueRef toggleField, Texture2D icon = null)
        {
            return new ModSettings(name, new ParamBinding(toggleField), Control.ControlType.Toggle, icon);
        }
        public static ModSettings Radial(string name, ValueRef radialField, float min = 0, float max = 1, float? checkpoint = null, DisplayType displayType = DisplayType.Percentage, Texture2D icon = null)
        {
            return new ModSettings(name, null, Control.ControlType.RadialPuppet, icon, radialSettings: new RadialSettings((RadialSettings.DisplayType)displayType, min, max, checkpoint), subBinds: [new ParamBinding(radialField)]);
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
        public void UpdateParamValue()
        {
            var value = GetFieldValue();
            SetParamValue(value);
        }
        public float GetFieldValue()
        {
            var target = GetBinds()[0];
            return target.ValueRef.Value;
        }
        public void SetParamValue(float value)
        {
            var target = GetBinds()[0];
            target.Param.InternalSet(value);
        }

        public class ParamBinding(ValueRef valueRef)
        {
            public ValueRef ValueRef = valueRef;
            public Vrc3Param Param;

            public void Init()
            {
                Param = ParamFromValueRef(ValueRef);

                ApplyStored();

                CurrentModel.OnAvatarSwitch += ApplyStored;
            }
            void ApplyStored()
            {
                if (SettingsCacheHandler.Cache.TryGetValue(Param.Name, out var value))
                {
                    ValueRef.Value = value;
                }
            }
            Vrc3Param ParamFromValueRef(ValueRef valueRef)
            {
                void OnChange(Vrc3Param param, float value)
                {
                    valueRef.Value = value;

                    SettingsCacheHandler.Cache[param.Name] = value;
                    SettingsCacheHandler.MarkDirty();
                }

                var param = new Vrc3Param($"{valueRef.info.ReflectedType.FullName}.{valueRef.info.Name}", AnimatorControllerParameterType.Float, OnChange);

                return param;
            }
        }
        public class ValueRef
        {
            readonly Func<float> Get;
            readonly Action<float> Set;

            public readonly MemberInfo info;

            public float Value
            {
                get => Get();
                set => Set(value);
            }

            ValueRef(MemberInfo info, Type type, Func<object> get, Action<object> set)
            {
                var (ToFloat, FromFloat) = Converters.TryGetValue(type, out var c)
                    ? c
                    : throw new NotSupportedException();

                this.info = info;

                Get = () => ToFloat(get());
                Set = v => set(FromFloat(v));
            }

            public static ValueRef From(object instOrLookup, FieldInfo field)
            {
                var GetTarget = GetLookup(instOrLookup, field.IsStatic);
                return new ValueRef(
                    field,
                    field.FieldType,
                    () => field.GetValue(GetTarget()),
                    v => field.SetValue(GetTarget(), v)
                );
            }
            public static ValueRef From(object instOrLookup, PropertyInfo property)
            {
                var GetTarget = GetLookup(instOrLookup, property.GetMethod.IsStatic);
                return new ValueRef(
                    property,
                    property.PropertyType,
                    () => property.GetValue(GetTarget()),
                    v => property.SetValue(GetTarget(), v)
                );
            }

            static Func<object> GetLookup(object inst, bool isStatic)
            {
                return inst switch
                {
                    Func<object> getInst => getInst,
                    not null             => () => inst,
                    null when isStatic   => () => null,
                    _ => throw new TargetException()
                };
            }

            readonly Dictionary<Type, (Func<object, float> ToFloat, Func<float, object> FromFloat)> Converters = new() {
                [typeof(float)] = (
                    v => (float)v,
                    v => v
                ),
                [typeof(int)]   = (
                    v => (int)v,
                    v => (int)v
                ),
                [typeof(bool)]  = (
                    v => (bool)v ? 1f : 0f,
                    v => v != 0f
                )
            };
        }
    }
}