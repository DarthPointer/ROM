using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ROM.UserInteraction
{
    internal class SampleWindow : AbstractIMGUIWindow
    {
        #region Styles
        private static readonly GUIStyle WHITE_TEXT_STYLE;
        #endregion

        static SampleWindow()
        {
            WHITE_TEXT_STYLE = new();
            WHITE_TEXT_STYLE.normal.textColor = Color.white;
        }

        #region Fields
        private Rect _rect = new Rect(100, 100, 200, 500);
        #endregion

        #region Constructors
        public SampleWindow()
        {
        }
        #endregion

        #region Methods
        public override void Display()
        {
            _rect = GUI.Window(id: GUIUtility.GetControlID(FocusType.Passive), clientRect: _rect, func: WindowFunction, text: "Sample Header");
        }

        private void WindowFunction(int id)
        {
            GUI.Label(new Rect(0, 20, 100, 20), "sample text", WHITE_TEXT_STYLE);

            if (GUI.Button(new Rect(0, 40, 100, 30), "xd", WHITE_TEXT_STYLE))
            {
                Debug.Log("ok");
            }

            GUI.DragWindow();
        }
        #endregion
    }
}
