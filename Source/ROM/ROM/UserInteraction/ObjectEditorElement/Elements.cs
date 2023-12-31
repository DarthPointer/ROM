using ROM.UserInteraction.InroomManagement;
using ROM.UserInteraction.ObjectEditorElement.LevelPosition;
using ROM.UserInteraction.ObjectEditorElement.Scrollbar;
using ROM.UserInteraction.ObjectEditorElement.TextField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

namespace ROM.UserInteraction.ObjectEditorElement
{
    public static class Elements
    {
        #region TextField
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
        #endregion

        #region Scrollbar
        public static IObjectEditorElement Scrollbar<T>(string displayName, Func<T> getter, Action<T> setter, ScrollbarConfiguration<T> configuration)
        {
            return new ScrollbarElement<T>(displayName, getter, setter, configuration.Left, configuration.Right,
                configuration.PosToValue, configuration.ValueToPos, configuration.ValueFormatter);
        }

        public static IObjectEditorElement Scrollbar(string displayName, Func<float> getter, Action<float> setter,
            ScrollbarConfiguration<float>? configuration = null)
        {
            return Scrollbar<float>(displayName, getter, setter, configuration ?? new FloatScrollbarConfiguration());
        }

        public static IObjectEditorElement Scrollbar(string displayName, Func<int> getter, Action<int> setter,
            int left, int right)
        {
            return Scrollbar<int>(displayName, getter, setter, new IntScrollbarConfiguration(left, right));
        }

        public static IObjectEditorElement Scrollbar<T>(string displayName, Func<T> getter, Action<T> setter, IEnumerable<Option<T>> options)
        {
            return new OptionsScrollbarElement<T>(displayName, getter, setter, options);
        }
        #endregion

        #region Checkbox
        public static IObjectEditorElement Checkbox(string displayName, Func<bool> getter, Action<bool> setter)
        {
            return new CheckboxElement(displayName, getter, setter);
        }
        #endregion

        #region CollapsableOptionSelect
        public static IObjectEditorElement CollapsableOptionSelect<T>(string header, Func<T> getter, Action<T> setter,
            IEnumerable<Option<T>> options)
        {
            return new CollapsableOptionSelectElement<T>(header, getter, setter, options);
        }

        public static IObjectEditorElement CollapsableOptionSelect<TEnum>(string header, Func<TEnum> getter, Action<TEnum> setter)
            where TEnum : Enum
        {
            IEnumerable<Option<TEnum>> options = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().
                Select(val =>
                new Option<TEnum>(val, Enum.GetName(typeof(TEnum), val)));

            return CollapsableOptionSelect(header, getter, setter, options);
        }

        public static IObjectEditorElement CollapsableOptionSelect<T>(string header, Func<T> getter, Action<T> setter,
            params Option<T>[] options)
        {
            return CollapsableOptionSelect(header, getter, setter, options as IEnumerable<Option<T>>);
        }
        #endregion

        #region Point
        public static IObjectEditorElement Point(string displayName, Func<Vector2> getter, Action<Vector2> setter)
        {
            return new PointElement(displayName, getter, setter);
        }
        #endregion
    }
}
