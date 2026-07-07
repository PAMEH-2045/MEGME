using UnityEngine;

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
    }
}
