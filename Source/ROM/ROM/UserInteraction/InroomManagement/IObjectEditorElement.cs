using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROM.UserInteraction.InroomManagement
{
    /// <summary>
    /// An interface to implement to create elements for object edit window.
    /// </summary>
    public interface IObjectEditorElement
    {
        /// <summary>
        /// Whether the element has unsaved changes. Used to notify user if they try to close the object editor window with unsaved changes.
        /// </summary>
        bool HasChanges { get; }

        /// <summary>
        /// The call inside the IMGUI window to draw the element.
        /// </summary>
        void Draw();

        /// <summary>
        /// Called when the current state of room object is saved successfully.
        /// </summary>
        void OnSaved();

        /// <summary>
        /// The call to reset the data to last saved state.
        /// </summary>
        void ResetChanges();

        /// <summary>
        /// The call inside OnGUI to draw elements outside the object editor window such as other windows or detached elements.
        /// </summary>
        void DrawPostWindow();
    }
}
