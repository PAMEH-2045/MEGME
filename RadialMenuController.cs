using BlackStartX.GestureManager.Editor.Modules.Vrc3;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UIElements;

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
            var _module = (ModuleVrc3)Manager.Module;

            if (settingsMenuPosition != null) CalculateMenuPosition();

            Menu = _module.GetOrCreateRadial(this);

            Menu.Rect = menuRectCurrent;
            Menu.Render(_root, menuRectCurrent);
        }
        void OnDisable()
        {
            _root.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            _root.UnregisterCallback<MouseMoveEvent>(OnMouseMove);

            Menu.ClosePuppet();
        }
        void CalculateMenuPosition()
        {
            var offsetX = lastAtRightEdge(settingsMenuPosition) ? screenSize.x / 2 - targetRes.x : screenSize.x - targetRes.x;
            var offsetY = screenSize.y - targetRes.y;

            menuRectCurrent.position = menuPosOrigin + new Vector2(offsetX, offsetY);
        }
        void OnMouseMove(MouseMoveEvent e) => Menu.mousePos = e.mousePosition;
        void OnGeometryChanged(GeometryChangedEvent e) => screenSize = _root.layout.size;
    }
}