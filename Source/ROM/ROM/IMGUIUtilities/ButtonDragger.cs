using ROM.UserInteraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace ROM.IMGUIUtilities
{
    internal static class ButtonDragger
    {
        private const int LEFT_MOUSE_BUTTON = 0;

        
        private static int? DragTargetHash { get; set; }

        private static Vector2 InitialVector { get; set; }

        private static Vector2 InitialDragPos { get; set; }

        public static Vector2 GetNewVectorByDragButton(int targetHash, Vector2 currentVector, Func<bool> buttonMethod)
        {
            // Always draw the button
            if (buttonMethod())
            {
                // If already resizing a different window
                if (DragTargetHash != null && targetHash != DragTargetHash)
                {
                    return currentVector;
                }

                // If there was no window dragged
                if (DragTargetHash == null)
                {
                    InitialDragPos = CommonIMGUIUtils.GetScreenMouseUICoordinates();
                    InitialVector = currentVector;
                    DragTargetHash = targetHash;

                    // The drag is at the same spot now so will return zero change anyway.
                    return currentVector;
                }

                // Here it is guaranteed that targetHash is the same as DragTargetHash.
                return InitialVector + CommonIMGUIUtils.GetScreenMouseUICoordinates() - InitialDragPos;
            }


            // If the button is not clicked
            else
            {
                // If we are resizing this window
                if (DragTargetHash == targetHash)
                {
                    // If the button is still held
                    if (Input.GetMouseButton(LEFT_MOUSE_BUTTON))
                    {
                        // Consume the inputs
                        //Input.ResetInputAxes();

                        // We continue the drag
                        return InitialVector + CommonIMGUIUtils.GetScreenMouseUICoordinates() - InitialDragPos;
                    }

                    // If it is not, we stop the drag
                    DragTargetHash = null;
                }

                return currentVector;
            }
        }
    }
}
