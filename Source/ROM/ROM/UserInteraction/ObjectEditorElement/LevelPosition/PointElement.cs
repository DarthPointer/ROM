using ROM.IMGUIUtilities;
using ROM.UserInteraction.InroomManagement;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ROM.UserInteraction.ObjectEditorElement.LevelPosition.SpaceConversions;

namespace ROM.UserInteraction.ObjectEditorElement.LevelPosition
{
    public class PointElement : IObjectEditorElement
    {
        #region Fields
        private string _xText = "";
        private string _yText = "";
        #endregion

        #region Properties
        public string DisplayName { get; set; }
        public string DisplayCode
        {
            get => DraggablePointButton.DisplayCode;
            set => DraggablePointButton.DisplayCode = value;
        }

        private Func<Vector2> Getter { get; }
        private Action<Vector2> Setter { get; }

        private float TargetX
        {
            get => Target.x;
            set => Target = Target with { x = value };
        }
        private float TargetY
        {
            get => Target.y;
            set => Target = Target with { y = value };
        }

        public Vector2 Target
        {
            get => Getter();
            set => Setter(value);
        }

        private Vector2 SavedValue { get; set; }

        private string XText
        {
            get
            {
                return _xText;
            }
            set
            {
                if (_xText != value)
                {
                    _xText = value;
                    ProcessXTextSet();
                }
            }
        }
        private bool ParseXError { get; set; } = false;

        private string YText
        {
            get
            {
                return _yText;
            }
            set
            {
                if (_yText != value)
                {
                    _yText = value;
                    ProcessYTextSet();
                }
            }
        }
        private bool ParseYError { get; set; } = false;

        public bool HasChanges => SavedValue != Target;

        private string XInputElementId { get; }
        private string YInputElementId { get; }

        private DraggablePointButton DraggablePointButton { get; }

        private bool DisplayTogglePointButton { get; }
        public bool DrawPoint { get; set; }
        #endregion

        #region Constructors
        public PointElement(string displayName, string displayCode,
            Func<Vector2> getter, Action<Vector2> setter, bool displayTogglePointButton)
        {
            DraggablePointButton = new();

            DisplayName = displayName;
            DisplayCode = displayCode;

            Getter = getter;
            Setter = setter;

            SavedValue = Target;

            XInputElementId = GetHashCode().ToString() + ".X";
            YInputElementId = GetHashCode().ToString() + ".Y";

            DisplayTogglePointButton = displayTogglePointButton;
        }
        #endregion

        #region Methods
        private static string FormatCoordinate(float coordinate)
            => coordinate.ToString("0.#", CultureInfo.InvariantCulture);

        private static bool TryParseCoordinate(string input, out float coordinate)
        {
            return float.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out coordinate);
        }

        private void ProcessXTextSet()
        {
            if (TryParseCoordinate(XText, out float value))
            {
                TargetX = value;
                ParseXError = false;
                return;
            }

            ParseXError = true;
        }

        private void ProcessYTextSet()
        {
            if (TryParseCoordinate(YText, out float value))
            {
                TargetY = value;
                ParseYError = false;
                return;
            }

            ParseYError = true;
        }

        public void Draw(RoomCamera? roomCamera)
        {
            CommonIMGUIUtils.HorizontalLine();
            GUILayout.Label($"{DisplayName} ({DisplayCode})");

            GUILayout.BeginHorizontal();

            string focusedControlId = GUI.GetNameOfFocusedControl();
            if (focusedControlId != XInputElementId)
            {
                _xText = FormatCoordinate(TargetX);
                ParseXError = false;
            }
            if (focusedControlId != YInputElementId)
            {
                _yText = FormatCoordinate(TargetY);
                ParseYError = false;
            }

            GUILayout.Label("X:");
            GUI.SetNextControlName(XInputElementId);
            XText = GUILayout.TextField(XText, GUILayout.Width(100));
            GUILayout.FlexibleSpace();

            GUILayout.Label("Y:");
            GUI.SetNextControlName(YInputElementId);
            YText = GUILayout.TextField(YText, GUILayout.Width(100));
            GUILayout.FlexibleSpace();

            if (DisplayTogglePointButton)
            {
                if (GUILayout.Button(DrawPoint ? "-" : "+"))
                {
                    DrawPoint = !DrawPoint;
                }
            }

            GUILayout.EndHorizontal();

            if (ParseXError || ParseYError)
            {
                List<string> badInputs = [];

                if (ParseXError)
                    badInputs.Add(XText);
                if (ParseYError)
                    badInputs.Add(YText);

                GUILayout.Label($"Error parsing following string(s) as coordinate(s): " +
                    $"{string.Join(", ", badInputs)}");
            }

            CommonIMGUIUtils.HorizontalLine();
        }

        public void OnSaved()
        {
            SavedValue = Target;
        }

        public void ResetChanges()
        {
            Target = SavedValue;
        }

        public void DrawPostWindow(RoomCamera? roomCamera)
        {
            if (DrawPoint && roomCamera != null)
            {
                DraggablePointButton.Point = IMGUIRoomSpaceToScreenSpace(Target, roomCamera);
                DraggablePointButton.Draw();

                if (DraggablePointButton.WasDragged)
                    Target = IMGUIScreenSpaceToRoomSpace(DraggablePointButton.Point, roomCamera);
            }
        }

        public void ReceiveFContainer(FContainer? container)
        { }

        public void Terminate()
        { }
        #endregion
    }
}
