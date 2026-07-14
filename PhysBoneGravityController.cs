using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

namespace BlackStartX.GestureManager
{
    public class PhysBoneGravityController : MonoBehaviour
    {

        [SerializeField]
        private float inputRange = 1.5f;
        [SerializeField]
        private float outputRange = 0.015f;

        private Vector2Int previousWindowPos;
        private float3 currentForce;

        private IntPtr unityHWND;

        void Start()
        {
            previousWindowPos = GetWindowPosition();
            unityHWND = Process.GetCurrentProcess().MainWindowHandle;
        }

        void Update()
        {
            Vector2Int currentWindowPos = GetWindowPosition();
            Vector2Int delta = currentWindowPos - previousWindowPos;
            previousWindowPos = currentWindowPos;

            if (delta != Vector2Int.zero)
            {
                currentForce = new float3(
                    Map(-delta.x / Screen.dpi),
                    Map(delta.y / Screen.dpi),
                    0
                );
            }
            else
            {
                currentForce = float3.zero;
            }

            PhysBoneExternalForce.currentForce = currentForce;
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