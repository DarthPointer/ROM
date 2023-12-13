using ROM.IMGUIUtilities;
using ROM.UserInteraction.ModMountManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ROM.UserInteraction.InroomManagement
{
    internal class ModMountRoomObjectsWindow : IIMGUIWindow
    {
        #region Fields
        private Rect _windowRect = new Rect(100, 100, 400, 600);
        private Vector2 _windowSize;
        private Vector2 _scrollState = Vector2.zero;

        private int _count = 0;
        private string _countString;
        #endregion

        #region Properties
        private ModMountController? ModMountController { get; set; }

        #endregion

        #region Constructors
        public ModMountRoomObjectsWindow(ModMountController modMountController)
        {
            _windowSize = _windowRect.size;

            ModMountController = modMountController;

            _countString = _count.ToString();
        }
        #endregion

        #region Methods
        void IIMGUIWindow.Display()
        {
            _windowRect.size = _windowSize;

            _windowRect = GUILayout.Window(GetHashCode(), _windowRect, ModMountWindow,
                $"{ModMountController?.ContextRoom?.abstractRoom.name ?? "NO ROOM"} objects of {ModMountController?.ModMount.ModId ?? "NO MOD ASSIGNED"} mount.");
        }

        private void ModMountWindow(int id)
        {
            GUILayout.BeginVertical();

            _scrollState = GUILayout.BeginScrollView(_scrollState);

            GUILayout.BeginVertical();

            _countString = GUILayout.TextField(_countString.ToString());
            if (int.TryParse(_countString, out int result))
            {
                _count = result;
            }

            for (int i = 0; i < _count; i++)
            {
                GUILayout.Label(i.ToString());
            }

            GUILayout.EndVertical();

            GUILayout.EndScrollView();

            _windowSize = WindowResizer.GetNewSizeByDragButton(this, _windowSize);
            GUILayout.EndVertical();

            GUI.DragWindow();
        }

        public void Close()
        {
            this.RemoveFromContainer();
            ModMountController = null;
        }
        #endregion
    }
}
