using ROM.IMGUIUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ROM.UserInteraction
{
    internal class ResizeableAndDraggableIMGUIWindow : IIMGUIWindow
    {
        protected Rect _windowRect = new(100, 100, 200, 300);
        private Vector2 _windowSize;

        protected virtual string HeaderText => "Resizeable and draggable window";

        public void Display()
        {
            _windowSize = _windowRect.size;

            _windowRect = GUILayout.Window(GetHashCode(), _windowRect, InternalWindowFunction, HeaderText);

            _windowRect.size = _windowSize;

            PostCall();
        }

        private void InternalWindowFunction(int id)
        {
            GUILayout.BeginVertical();

            WindowFunction(id);

            _windowSize = WindowResizer.GetNewSizeByDragButton(this, _windowSize);
            // I have no clue what the exact order and sync are for OnGUI and registered Window calls is, so I will save it always.
            _windowRect.size = _windowSize;
            GUILayout.EndVertical();

            GUI.DragWindow();
        }

        protected virtual void WindowFunction(int id)
        { }

        protected virtual void PostCall()
        { }
    }
}
