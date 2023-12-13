using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROM.UserInteraction
{
    internal interface IIMGUIWindow
    {
        #region Methods
        void Display();
        #endregion
    }

    internal static class IIMGUIWindowExtension
    {
        public static void RemoveFromContainer(this IIMGUIWindow window)
        {
            if (IMGUIWindowsContainer.WindowContainingContainers.TryGetValue(window, out var container))
            {
                container.RemoveWindow(window);
            }
        }
    }
}
