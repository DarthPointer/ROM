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
    public class TextFieldElement<T> : IObjectEditorElement
    where T : notnull
    {
        private string _text;

        private Func<T> Getter { get; }
        private Action<T> Setter { get; }

        private T Target
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

        private T SavedValue { get; set; }

        public bool HasChanges => !SavedValue.Equals(Target);

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

        private Func<T, string> Formatter { get; }

        private TryParseValue Parser { get; }

        private Func<T, bool> ValueValidator { get; }

        public TextFieldElement(string displayName, Func<T> getter, Action<T> setter,
            Func<T, string> formatter, TryParseValue parser, Func<T, bool> valueValidator)
        {
            Getter = getter;
            Setter = setter;

            SavedValue = Target;
            _text = GetTargetString();

            InputElementId = GetHashCode().ToString();
            DisplayName = displayName;
            Formatter = formatter;
            Parser = parser;
            ValueValidator = valueValidator;
        }

        private string GetTargetString()
        {
            return Formatter(Target);
        }

        private void ProcessInputString(string input)
        {
            ErrorText = null;

            if (Parser(input, out T value))
            {
                ProcessInputValue(value);
                return;
            }

            ErrorText = $"Input string conversion to {typeof(T)} has failed.";
        }

        private void ProcessInputValue(T input)
        {
            if (ValueValidator(input))
            {
                Target = input;
                return;
            }

            ErrorText = $"The value {Formatter(input)} has been rejected by the validator.";
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

        public delegate bool TryParseValue(string input, out T value);
    }
}
