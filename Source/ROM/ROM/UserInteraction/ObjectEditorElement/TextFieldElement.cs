using ROM.UserInteraction.InroomManagement;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ROM.UserInteraction.ObjectEditorElement
{
    public class TextFieldElement : IObjectEditorElement
    {
        private string _text;

        private Func<float> Getter { get; }
        private Action<float> Setter { get; }

        private float Target
        {
            get
            {
                return Getter();
            }
            set
            {
                Setter(value);
            }
        }

        private float SavedValue { get; set; }

        public bool HasChanges => SavedValue != Target;

        private string Text
        {
            get
            {
                return _text;
            }
            set
            {
                if (_text  != value)
                {
                    _text = value;
                    ProcessInputString(value);
                }
            }
        }

        private string InputElementId { get; }

        private string DisplayName { get; }

        private string? ErrorText { get; set; }

        public float MinValue { get; set; } = float.MinValue;
        public float MaxValue { get; set; } = float.MaxValue;

        public TextFieldElement(Func<float> getter, Action<float> setter, string displayName)
        {
            Getter = getter;
            Setter = setter;

            SavedValue = Target;
            _text = GetTargetString();

            InputElementId = GetHashCode().ToString();
            DisplayName = displayName;
        }

        private string GetTargetString()
        {
            return Target.ToString("g8", CultureInfo.InvariantCulture);
        }

        private void ProcessInputString(string input)
        {
            ErrorText = null;

            if (float.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out float value))
            {
                ProcessInputValue(value);
                return;
            }

            ErrorText = $"Input string conversion to {typeof(float)} has failed.";
        }

        private void ProcessInputValue(float input)
        {
            if (AllowValue(input))
            {
                Target = input;
                return;
            }

            ErrorText = $"The value has to be from {MinValue} to {MaxValue}.";
        }

        private bool AllowValue(float value)
        {
            return value >= MinValue && value <= MaxValue;
        }

        public void OnSaved()
        {
            SavedValue = Target;
        }

        public void ResetChanges()
        {
            Target = SavedValue;
        }

        void IObjectEditorElement.Draw()
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();

            GUILayout.Label(DisplayName);

            if (InputElementId != GUI.GetNameOfFocusedControl())
            {
                _text = GetTargetString();
            }

            GUI.SetNextControlName(InputElementId);

            Text = GUILayout.TextField(Text);

            GUILayout.EndHorizontal();

            if (ErrorText != null)
            {
                GUILayout.Label(ErrorText);
            }

            GUILayout.EndVertical();
        }

        public void DrawPostWindow()
        { }
    }
}
