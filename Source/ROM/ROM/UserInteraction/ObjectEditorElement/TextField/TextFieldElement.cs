using ROM.UserInteraction.InroomManagement;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ROM.UserInteraction.ObjectEditorElement.TextField
{
    internal class TextFieldElement<T> : IObjectEditorElement
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
                if (_text != value)
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

        private TryParseValue<T> Parser { get; }

        private Func<T, bool> ValueValidator { get; }

        private Func<T, T>? ValueRestrictor { get; }

        public TextFieldElement(string displayName, Func<T> getter, Action<T> setter,
            TextFieldConfiguration<T> configuration)
        {
            Getter = getter;
            Setter = setter;

            SavedValue = Target;

            InputElementId = GetHashCode().ToString();
            DisplayName = displayName;
            Formatter = configuration.Formatter;
            Parser = configuration.Parser;
            ValueValidator = configuration.ValueValidator;
            ValueRestrictor = configuration.ValueRestrictor;

            _text = GetTargetString();
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
                Target = ValueRestrictor == null ?
                    input : ValueRestrictor(input);
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
    }

    public static class TextFieldElement
    {
        // A wrap to circumvent verbose typeparam in calls.
        public static IObjectEditorElement TextField<T>(string displayName, Func<T> getter, Action<T> setter, TextFieldConfiguration<T> configuration)
            where T : notnull
        {
            return new TextFieldElement<T>(displayName, getter, setter, configuration);
        }

        public static IObjectEditorElement TextField(string displayName, Func<float> getter, Action<float> setter,
            TextFieldConfiguration<float>? configuration = null)
        {
            return TextField<float>(displayName, getter, setter, configuration ?? new FloatTextFieldConfiguration());
        }

        public static IObjectEditorElement TextField(string displayName, Func<int> getter, Action<int> setter,
            TextFieldConfiguration<int>? configuration = null)
        {
            return TextField<int>(displayName, getter, setter, configuration ?? new IntTextFieldConfiguration());
        }

        public static IObjectEditorElement TextField(string displayName, Func<string> getter, Action<string> setter,
            TextFieldConfiguration<string>? configuration = null)
        {
            return TextField<string>(displayName, getter, setter, configuration ?? new StringTextFieldConfiguration());
        }
    }
}
