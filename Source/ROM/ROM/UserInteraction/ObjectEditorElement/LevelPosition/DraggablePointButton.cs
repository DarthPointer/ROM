using ROM.IMGUIUtilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ROM.UserInteraction.ObjectEditorElement.LevelPosition
{
    public class DraggablePointButton
    {
        #region Statics
        static GUIStyle ButtonStyle { get; } = new();
        static GUIStyle BlackTextStyle { get; } = new();
        public DraggablePointButton()
        {
            if(ButtonStyle.normal.background == null)
            {
                Texture2D buttonTexture = CommonIMGUIUtils.GetSingleColorTexture(16, 16, Color.white with { a = 0.2f });

                for (int x = 0; x < 16; x++)
                {
                    buttonTexture.SetPixel(x, 0, Color.black);
                    buttonTexture.SetPixel(x, 15, Color.black);
                }

                for (int y = 1; y < 15; y++)
                {
                    buttonTexture.SetPixel(0, y, Color.black);
                    buttonTexture.SetPixel(15, y, Color.black);
                }


                ButtonStyle.normal.background = buttonTexture;
                ButtonStyle.active.background = buttonTexture;

                BlackTextStyle.normal.textColor = Color.black;
            }
            ButtonStyle.normal.background.Apply();   
        }
        #endregion

        #region Properties
        public string DisplayCode { get; set; } = "";

        public Vector2 Point
        {
            get
            {
                return ButtonRect.center;
            }
            set
            {
                ButtonRect = ButtonRect with { position = value - (ButtonRect.size / 2) };
            }
        }

        private Rect ButtonRect
        {
            get;
            set;
        } = new(Vector2.zero, new Vector2(24, 24));
        #endregion

        #region Methods
        public void Draw()
        {
            if (GUI.Button(ButtonRect, "", ButtonStyle))
            {
            }

            GUILayout.BeginArea(ButtonRect);

            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.Label(DisplayCode, BlackTextStyle);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            
            GUILayout.EndArea();
        }
        #endregion
    }
}
