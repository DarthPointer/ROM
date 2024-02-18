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

        protected Rect _collapsedRect = new(100, 100, 0, 0);

        protected virtual string HeaderText => "Resizeable and draggable window";
        protected bool IsCollapsed { get; set; } = false;

        public void Display()
        {
            if (!IsCollapsed)
            {
                _windowSize = _windowRect.size;

                _windowRect = GUILayout.Window(GetHashCode(), _windowRect, InternalWindowFunction, HeaderText);

                _windowRect.size = _windowSize;

                _collapsedRect.position = _windowRect.position;
                _collapsedRect.x = Mathf.Max(0, _collapsedRect.x);
                _collapsedRect.y = Mathf.Max(0, _collapsedRect.y);
            }
            else
            {
                _collapsedRect = GUILayout.Window(GetHashCode(), _collapsedRect, InternalWindowFunction, HeaderText);

                _windowRect.position = _collapsedRect.position;
                _windowRect.x = Mathf.Max(0, _windowRect.x);
                _windowRect.y = Mathf.Max(0, _windowRect.y);
            }

            PostCall();
        }

        private void InternalWindowFunction(int id)
        {
            GUILayout.BeginVertical();

            WindowFunction(id);

            _windowSize = ButtonDragger.GetNewVectorByDragButton(this.GetHashCode(), _windowSize, () => GUILayout.RepeatButton("~"));
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
