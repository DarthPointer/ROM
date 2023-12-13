using ROM.UserInteraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace ROM.IMGUIUtilities
{
    internal static class WindowResizer
    {
        private const int LEFT_MOUSE_BUTTON = 0;

        private static IIMGUIWindow? CurrentlyResizedWindow { get; set; }

        private static Vector2 InitialSize { get; set; }

        private static Vector2 InitialDragPos { get; set; }

        public static Vector2 GetNewSizeByDragButton(IIMGUIWindow windowToResize, Vector2 currentSize)
        {
            // Always draw the button
            if (GUILayout.RepeatButton("~"))
            {
                // If already resizing a different window
                if (CurrentlyResizedWindow != null && windowToResize != CurrentlyResizedWindow)
                {
                    return currentSize;
                }

                // If there was no window dragged
                if (CurrentlyResizedWindow == null)
                {
                    InitialDragPos = GetMouseUICoordinates();
                    InitialSize = currentSize;
                    CurrentlyResizedWindow = windowToResize;

                    // The drag is at the same spot now so will return zero change anyway.
                    return currentSize;
                }

                // Here it is guaranteed that CurrentlyResizedWindow is the windowToResize
                return InitialSize + GetMouseUICoordinates() - InitialDragPos;
            }


            // If the button is not clicked
            else
            {
                // If we are resizing this window
                if (CurrentlyResizedWindow == windowToResize)
                {
                    // If the button is still held
                    if (Input.GetMouseButton(LEFT_MOUSE_BUTTON))
                    {
                        // Consume the inputs
                        //Input.ResetInputAxes();

                        // We continue the drag
                        return InitialSize + GetMouseUICoordinates() - InitialDragPos;
                    }

                    // If it is not, we stop the drag
                    CurrentlyResizedWindow = null;
                }

                return currentSize;
            }
        }

        private static Vector2 GetMouseUICoordinates()
        {
            Vector3 unityMousPos = Input.mousePosition;

            return new(unityMousPos.x, Screen.height - unityMousPos.y);
        }
    }
}
