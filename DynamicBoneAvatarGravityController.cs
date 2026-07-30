using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BlackStartX.GestureManager
{
    public class DynamicBoneAvatarGravityController : MonoBehaviour
    {
        public static float inputRange = 1.5f;
        public static float outputRange = 0.015f;

        private Vector2Int previousWindowPos;
        private Vector3 currentForce;

        private IntPtr unityHWND;

        private DynamicBone[] bones = [];

        void Start()
        {
            previousWindowPos = GetWindowPosition();
            unityHWND = Process.GetCurrentProcess().MainWindowHandle;

            bones = GetComponentsInChildren<DynamicBone>(true);
            if (bones.Length == 0)
                enabled = false;
        }

        void Update()
        {
            Vector2Int currentWindowPos = GetWindowPosition();
            Vector2Int delta = currentWindowPos - previousWindowPos;
            previousWindowPos = currentWindowPos;

            if (delta != Vector2Int.zero)
            {
                currentForce = new Vector3(
                    Map(-delta.x / Screen.dpi),
                    Map(delta.y / Screen.dpi),
                    0
                );
            }
            else
            {
                currentForce = Vector3.zero;
            }

            foreach (var bone in bones)
            {
                bone.m_Force = currentForce;
            }
        }
        float Map(float value)
        {
            float sign = Mathf.Sign(value);
            float absValue = Mathf.Abs(value);

            var t = Mathf.Clamp01(absValue / inputRange);
            return sign * t * outputRange;
        }

        #region Windows API

        private Vector2Int GetWindowPosition()
        {
            GetWindowRect(unityHWND, out RECT rect);
            return new Vector2Int(rect.left, rect.top);
        }

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left, top, right, bottom;
        }

        #endregion
    }
}