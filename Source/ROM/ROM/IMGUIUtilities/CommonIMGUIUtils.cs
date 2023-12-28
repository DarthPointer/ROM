using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ROM.IMGUIUtilities
{
    internal static class CommonIMGUIUtils
    {
        /// <summary>
        /// Returns screen coordinates of mouse realigned to IMGUI coordinages (inverted y axis)
        /// </summary>
        /// <returns></returns>
        public static Vector2 GetScreenMouseUICoordinates()
        {
            Vector3 unityMousPos = Input.mousePosition;

            return new(unityMousPos.x, Screen.height - unityMousPos.y);
        }

        public static Texture2D GetSingleColorTexture(int x, int y, Color color)
        {
            Texture2D tex = new(x, y);
            tex.SetPixels(Enumerable.Repeat(color, x * y).ToArray());

            return tex;
        }

        public static void HorizontalLine(Texture2D? fillTexture = null, int height = 2, int spacing = 5)
        {
            fillTexture ??= GetSingleColorTexture(16, 16, Color.white);

            GUIStyle style = new();
            style.normal.background = fillTexture;

            GUILayout.Space(spacing);
            GUILayout.Label("", style, GUILayout.Height(height));
            GUILayout.Space(spacing);
        }
    }
}
