using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ROM.UserInteraction.InroomManagement
{
    public interface IEditRoomObjectWindow
    {
        
    }

    internal class EditRoomObjectWindow : ResizeableAndDraggableIMGUIWindow, IEditRoomObjectWindow
    {
        #region Fields
        #endregion

        #region Properties
        private IReadOnlyList<IObjectEditorElement> EditorElements { get; }
        #endregion

        #region Constructors
        public EditRoomObjectWindow(IEnumerable<IObjectEditorElement> editorElements)
        {
            _windowRect = new(100, 100, 300, 300);

            EditorElements = editorElements.ToList();
        }
        #endregion

        protected override void WindowFunction(int id)
        {
            
        }
    }
}
