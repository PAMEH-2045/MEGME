using BlackStartX.GestureManager.Editor.Modules.Vrc3;
using BlackStartX.GestureManager.Editor.Modules.Vrc3.Params;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
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
        RadialMenu Menu;
        SettingsMenuPosition settingsMenuPosition;
        AccessTools.FieldRef<SettingsMenuPosition, bool> lastAtRightEdge = AccessTools.FieldRefAccess<SettingsMenuPosition, bool>("lastAtRightEdge");

        Vector2 screenSize;
        Vector2 menuPosOrigin = new(1017, 493);
        Rect menuRectCurrent = new(1017, 493, 300, 300);
        Vector2 targetRes = new(1536, 1024);

        public static Dictionary<string, List<ModSettings>> ModSettings = new();

        void Awake()
        {
            doc = GetComponent<UIDocument>();

            settingsMenuPosition = FindFirstObjectByType<SettingsMenuPosition>();
        }
        void OnEnable()
        {
            _root = doc.rootVisualElement;

            _root.pickingMode = PickingMode.Position;
            _root.style.color = Color.white; // text color is inherited from parent

            _root.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            _root.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        }

        void OnGUI()
        {
            if (Manager.Module == null)
                return;

            Manager.SetDrag(!Event.current.alt);

            if (settingsMenuPosition != null) CalculateMenuPosition();

            Menu.Rect = menuRectCurrent;
            Menu.Render(_root, menuRectCurrent);
        }
        void OnDisable()
        {
            _root.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            _root.UnregisterCallback<MouseMoveEvent>(OnMouseMove);

            Menu.ClosePuppet();
        }
        internal void OnAvatarSwitch()
        {
            if (Manager.Module == null)
                return;

            Menu = Manager.Module.GetOrCreateRadial(this);
        }
        void CalculateMenuPosition()
        {
            var offsetX = lastAtRightEdge(settingsMenuPosition) ? screenSize.x / 2 - targetRes.x : screenSize.x - targetRes.x;
            var offsetY = screenSize.y - targetRes.y;

            menuRectCurrent.position = menuPosOrigin + new Vector2(offsetX, offsetY);
        }
        void OnMouseMove(MouseMoveEvent e) => Menu.mousePos = e.mousePosition;
        void OnGeometryChanged(GeometryChangedEvent e) => screenSize = _root.layout.size;

        public void RegisterSettingsMenu(string name, List<ModSettings> settings)
        {
            foreach (var set in settings)
            {
                set.GetBind().Init();
            }

            if (ModSettings.TryGetValue(name, out var sets))
            {
                sets.AddRange(settings);
                Debug.Log($"[MEGME] Expanded existing settings menu '{name}'");
            }
            else
            {
                ModSettings.Add(name, settings);
                Debug.Log($"[MEGME] Registered new settings menu '{name}', with total of {ModSettings.Count}");
            }
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

        public static ModSettings Toggle(string name, FieldRef toggleField, Texture2D icon = null)
        {
            return new ModSettings(name, new ParamBinding(toggleField), Control.ControlType.Toggle, icon);
        }
        public static ModSettings Radial(string name, FieldRef radialField, float min = 0, float max = 1, float? checkpoint = null, DisplayType displayType = DisplayType.Percentage, Texture2D icon = null)
        {
            return new ModSettings(name, null, Control.ControlType.RadialPuppet, icon, radialSettings: new RadialSettings((RadialSettings.DisplayType)displayType, min, max, checkpoint), subBinds: [new ParamBinding(radialField)]);
        }
        public static ModSettings SubMenu(string name, List<ModSettings> subSettings, Texture2D icon = null)
        {
            return new ModSettings(name, null, Control.ControlType.SubMenu, icon, subSettings: subSettings);
        }

        public enum DisplayType
        {
            Percentage = RadialSettings.DisplayType.Percentage,
            Meters = RadialSettings.DisplayType.Meters,
            Absolute = RadialSettings.DisplayType.Absolute,
            Degree = RadialSettings.DisplayType.Degree
        }
        public ParamBinding GetBind()
        {
            return controlType switch
            {
                Control.ControlType.Toggle => bind,
                Control.ControlType.RadialPuppet => subBinds[0],
                _ => throw new NotImplementedException()
            };
        }
        public void UpdateParamValue()
        {
            var value = GetFieldValue();
            SetParamValue(value);
        }
        public float GetFieldValue()
        {
            var target = GetBind();
            return target.FieldRef.Value;
        }
        public void SetParamValue(float value)
        {
            var target = GetBind();
            target.Param.InternalSet(value);
        }

        public class ParamBinding(FieldRef FieldRef)
        {
            public FieldRef FieldRef = FieldRef;
            public Vrc3Param Param;

            public void Init()
            {
                Param = ParamFromFieldRef(FieldRef);

                if (SettingsCacheHandler.Cache.TryGetValue(Param.Name, out var value))
                {
                    FieldRef.Value = value;
                }
            }

            Vrc3Param ParamFromFieldRef(FieldRef fieldRef)
            {
                var field = fieldRef.field;
                var inst = fieldRef.inst;

                void OnChange(Vrc3Param param, float value)
                {
                    fieldRef.Value = value;

                    SettingsCacheHandler.Cache[param.Name] = value;
                    SettingsCacheHandler.MarkDirty();
                }

                var param = new Vrc3Param($"{field.DeclaringType.FullName}.{field.Name}", AnimatorControllerParameterType.Float, OnChange);

                return param;
            }
        }
        public class FieldRef(object inst, FieldInfo field)
        {
            public object inst = inst;
            public FieldInfo field = field;

            Func<object> GetTarget = inst switch
            {
                Func<object> getInst => getInst,
                not null => () => inst,
                null => field.IsStatic ? () => null : throw new TargetException()
            };
            Func<float, object> Convert = field.FieldType switch
            {
                Type t when t == typeof(float) => v => v,
                Type t when t == typeof(int) => v => (int)v,
                Type t when t == typeof(bool) => v => v != 0f,
                _ => throw new NotSupportedException()
            };
            Func<object, float> ToFloat = field.FieldType switch
            {
                Type t when t == typeof(float) => v => (float)v,
                Type t when t == typeof(int) => v => (float)v,
                Type t when t == typeof(bool) => v => (bool)v ? 1f : 0f,
                _ => throw new NotSupportedException()
            };

            public float Value
            {
                get => ToFloat(field.GetValue(GetTarget()));
                set => field.SetValue(GetTarget(), Convert(value));
            }
        }
    }
}