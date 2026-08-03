using UnityEngine;
using UnityEngine.Events;

namespace MEGME
{
    public class MenuToggle : MonoBehaviour
    {
        public UnityEvent action;
        void OnEnable() => action.Invoke();
        void OnDisable() => action.Invoke();
    }
}
