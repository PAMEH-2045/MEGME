using BlackStartX.GestureManager.Editor.Modules.Vrc3;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace BlackStartX.GestureManager
{
    public class RadialMenuController : MonoBehaviour
    {
        [SerializeField] private GestureManager Manager;

        VisualElement _root;
        RadialMenu Menu;
        SettingsMenuPosition settingsMenuPosition;
        FieldInfo lastAtRightEdgeField;

        Vector2 screenSize;
        Vector2 menuPosOrigin = new(1017, 493);
        Rect menuRectCurrent = new(1017, 493, 300, 300);
        Vector2 targetRes = new(1536, 1024);

        private void OnEnable()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;

            _root.pickingMode = PickingMode.Position;
            _root.style.color = Color.white; // text color is inherited from parent

            _root.RegisterCallback<GeometryChangedEvent>(_ => screenSize = _root.layout.size);
            _root.RegisterCallback<MouseMoveEvent>(e => Menu.mousePos = e.mousePosition);

            settingsMenuPosition = FindFirstObjectByType<SettingsMenuPosition>();
            lastAtRightEdgeField = typeof(SettingsMenuPosition).GetField("lastAtRightEdge", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        void OnGUI()
        {
            if (Manager.Module == null)
                return;

            Manager.SetDrag(!Event.current.alt);
            var _module = (ModuleVrc3)Manager.Module;

            if (settingsMenuPosition != null && lastAtRightEdgeField != null) CalculateMenuPosition();

            Menu = _module.GetOrCreateRadial(this);

            Menu.Rect = menuRectCurrent;
            Menu.Render(_root, menuRectCurrent);
        }

        // need rework ( or is it? )
        void CalculateMenuPosition()
        {
            var lastAtRightEdge = (bool)lastAtRightEdgeField.GetValue(settingsMenuPosition);

            var offsetX = lastAtRightEdge ? screenSize.x / 2 - targetRes.x : screenSize.x - targetRes.x;
            var offsetY = screenSize.y - targetRes.y;

            menuRectCurrent.position = menuPosOrigin + new Vector2(offsetX, offsetY);
        }
    }
}