using UnityEngine;
using UnityEngine.EventSystems;

namespace BlackStartX.GestureManager
{
    public class Stuff : MonoBehaviour
    {
        // MEX3.2.0 UISetOnOff
        public void SetOnOff(GameObject obj)
        {
            if (obj != null)
                obj.SetActive(!obj.activeSelf);
        }
        // MEX3.3.0 Assets/MATE ENGINE - Scripts/Settings/DeselectOnClick.cs:Deselect()
        public void Deselect()
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
