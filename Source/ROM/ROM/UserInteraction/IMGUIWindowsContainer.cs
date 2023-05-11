using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ROM.UserInteraction
{
    internal class IMGUIWindowsContainer : MonoBehaviour
    {
        #region Fields
        private List<IIMGUIWindow> _windows;
        #endregion

        #region Properties
        public bool DisplayWindows
        {
            get;
            set;
        }
        #endregion

        #region Constructors
        public IMGUIWindowsContainer()
        {
            _windows = new();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds a new window to manage with this container. Container can't contain a window twice, duplicate addition attempt will not change the object and will return <see langword="false"/>.
        /// </summary>
        /// <param name="window">The window to add.</param>
        /// <returns><see langword="true"/> if the window was unique and added successfully. <see langword="false"/> if the window has already been present and the objects were not altered.</returns>
        public bool AddWindow(IIMGUIWindow window)
        {
            if (_windows.Contains(window))
            {
                return false;
            }

            _windows.Add(window);
            return true;
        }

        /// <summary>
        /// Removes the specified window from the container so that the container no longer manages the window.
        /// </summary>
        /// <param name="window">The window to remove.</param>
        /// <returns><see langword="true"/> if the window was removed successfully. <see langword="false"/> if the window was not present in the collection and the objects stayed intact.</returns>
        public bool RemoveWindow(IIMGUIWindow window)
        {
            return _windows.Remove(window);
        }

        /// <summary>
        /// Remove all windows from its list, so that they won't be drawn by this manager anymore.
        /// </summary>
        public void RemoveAllWindows()
        {
            _windows.Clear();
        }

        /// <summary>
        /// Unity call the container uses to display the windows it manages. Don't call this manually.
        /// </summary>
        [Obsolete(message: "ffs don't call this manually, Unity should call it on its own when needed.", error: true)]
        public void OnGUI()
        {
            if (!DisplayWindows) return;

            foreach (IIMGUIWindow window in _windows)
            {
                window.Display();
            }
        }
        #endregion
    }
}
