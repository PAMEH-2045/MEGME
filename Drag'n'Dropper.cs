using Kirurobo;
using System.IO;
using System.Linq;
using UnityEngine;

namespace BlackStartX.GestureManager
{
    class Drag_n_Dropper : MonoBehaviour
    {
        UniWindowController controller;
        VRMLoader loader;

        string[] extentions = [".vrm", ".me", ".prefab", ".vrca"];

        void Awake()
        {
            controller = FindFirstObjectByType<UniWindowController>();
            loader = FindFirstObjectByType<VRMLoader>();
        }
        void OnEnable() => controller.OnDropFiles += OnDropFiles;
        void OnDisable() => controller.OnDropFiles -= OnDropFiles;

        void OnDropFiles(string[] files)
        {
            if (files.Length > 0 && !string.IsNullOrEmpty(files[0]))
            {
                var ext = Path.GetExtension(files[0]).ToLower();
                if (extentions.Contains(ext))
                    loader.LoadVRM(files[0]);
            }
        }
    }
}
