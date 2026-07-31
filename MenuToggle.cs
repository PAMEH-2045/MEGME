using UnityEngine;
using UnityEngine.Events;

namespace BlackStartX.GestureManager
{
    public class MenuToggle : MonoBehaviour
    {
        public UnityEvent action;
        void OnEnable() => action.Invoke();
        void OnDisable() => action.Invoke();
    }
}
