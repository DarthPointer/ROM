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
    /// <summary>
    /// A static class with shortcuts to create elements in various ways.
    /// </summary>
    public static class Elements
    {
        #region TextField
        /// <summary>
        /// A wrap to circumvent verbose typeparam for <see cref="TextFieldElement{T}.TextFieldElement(string, Func{T}, Action{T}, TextFieldConfiguration{T})"/>
        /// </summary>
        /// <typeparam name="T">The type of edited value.</typeparam>
        /// <param name="displayName">Header of the element.</param>
        /// <param name="getter">The function to retrieve the current value from the object.</param>
        /// <param name="setter">The action to set the value.</param>
        /// <param name="configuration">The configuration to use for the text field.</param>
        /// <returns></returns>
        public static IObjectEditorElement TextField<T>(string displayName, Func<T> getter, Action<T> setter, TextFieldConfiguration<T> configuration)
            where T : notnull
        {
            return new TextFieldElement<T>(displayName, getter, setter, configuration);
        }

        /// <summary>
        /// Creates a float text field.
        /// </summary>
        /// <param name="displayName">Header of the element.</param>
        /// <param name="getter">The function to retrieve the current value from the object.</param>
        /// <param name="setter">The action to set the value.</param>
        /// <param name="configuration">The configuration to use for the text field.
        /// If <see langword=""="null"/> a default one will be created and used.</param>
        /// <returns></returns>
        public static IObjectEditorElement TextField(string displayName, Func<float> getter, Action<float> setter,
            TextFieldConfiguration<float>? configuration = null)
        {
            return TextField<float>(displayName, getter, setter, configuration ?? new FloatTextFieldConfiguration());
        }

        /// <summary>
        /// Creates an int text field.
        /// </summary>
        /// <param name="displayName">Header of the element.</param>
        /// <param name="getter">The function to retrieve the current value from the object.</param>
        /// <param name="setter">The action to set the value.</param>
        /// <param name="configuration">The configuration to use for the text field.
        /// If <see langword=""="null"/> a default one will be created and used.</param>
        /// <returns></returns>
        public static IObjectEditorElement TextField(string displayName, Func<int> getter, Action<int> setter,
            TextFieldConfiguration<int>? configuration = null)
        {
            return TextField<int>(displayName, getter, setter, configuration ?? new IntTextFieldConfiguration());
        }

        /// <summary>
        /// Creates a string text field.
        /// </summary>
        /// <param name="displayName">Header of the element.</param>
        /// <param name="getter">The function to retrieve the current value from the object.</param>
        /// <param name="setter">The action to set the value.</param>
        /// <param name="configuration">The configuration to use for the text field.
        /// If <see langword=""="null"/> a default one will be created and used.</param>
        /// <returns></returns>
        public static IObjectEditorElement TextField(string displayName, Func<string> getter, Action<string> setter,
            TextFieldConfiguration<string>? configuration = null)
        {
            return TextField<string>(displayName, getter, setter, configuration ?? new StringTextFieldConfiguration());
        }
        #endregion

        #region Scrollbar
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">The type of edited value.</typeparam>
        /// <param name="displayName">Header of the element.</param>
        /// <param name="getter">The function to retrieve the current value from the object.</param>
        /// <param name="setter">The action to set the value.</param>
        /// <param name="configuration">The configuration to use for the scrollbar.</param>
        /// <returns></returns>
        public static IObjectEditorElement Scrollbar<T>(string displayName, Func<T> getter, Action<T> setter, ScrollbarConfiguration<T> configuration)
        {
            return new ScrollbarElement<T>(displayName, getter, setter, configuration.Left, configuration.Right,
                configuration.PosToValue, configuration.ValueToPos, configuration.ValueFormatter);
        }

        /// <summary>
        /// Creates a float scrollbar.
        /// </summary>
        /// <param name="displayName">Header of the element.</param>
        /// <param name="getter">The function to retrieve the current value from the object.</param>
        /// <param name="setter">The action to set the value.</param>
        /// <param name="configuration">The configuration to use for the scrollbar.
        /// If <see langword=""="null"/> a default one will be created and used.</param>
        /// <returns></returns>
        public static IObjectEditorElement Scrollbar(string displayName, Func<float> getter, Action<float> setter,
            ScrollbarConfiguration<float>? configuration = null)
        {
            return Scrollbar<float>(displayName, getter, setter, configuration ?? new FloatScrollbarConfiguration());
        }

        /// <summary>
        /// Creates an int scrollbar.
        /// </summary>
        /// <param name="displayName">Header of the element.</param>
        /// <param name="getter">The function to retrieve the current value from the object.</param>
        /// <param name="setter">The action to set the value.</param>
        /// <param name="left">The leftmost value.</param>
        /// <param name="right">The rightmost value.</param>
        /// <returns></returns>
        public static IObjectEditorElement Scrollbar(string displayName, Func<int> getter, Action<int> setter,
            int left, int right)
        {
            return Scrollbar<int>(displayName, getter, setter, new IntScrollbarConfiguration(left, right));
        }

        /// <summary>
        /// Creates a scrollbar to select options.
        /// </summary>
        /// <typeparam name="T">The type of options.</typeparam>
        /// <param name="displayName">Header of the element.</param>
        /// <param name="getter">The function to retrieve the current value from the object.</param>
        /// <param name="setter">The action to set the value.</param>
        /// <param name="options">The list of options.</param>
        /// <returns></returns>
        public static IObjectEditorElement Scrollbar<T>(string displayName, Func<T> getter, Action<T> setter, IEnumerable<Option<T>> options)
        {
            return new OptionsScrollbarElement<T>(displayName, getter, setter, options);
        }
        #endregion

        #region Checkbox
        /// <summary>
        /// 
        /// </summary>
        /// <param name="displayName">Header of the element.</param>
        /// <param name="getter">The function to retrieve the current value from the object.</param>
        /// <param name="setter">The action to set the value.</param>
        /// <returns></returns>
        public static IObjectEditorElement Checkbox(string displayName, Func<bool> getter, Action<bool> setter)
        {
            return new CheckboxElement(displayName, getter, setter);
        }
        #endregion

        #region CollapsableOptionSelect
        /// <summary>
        /// A wrap to circumvent verbose typeparam for
        /// <see cref="CollapsableOptionSelectElement{T}.CollapsableOptionSelectElement(string, Func{T}, Action{T}, IEnumerable{Option{T}})"/>.
        /// </summary>
        /// <typeparam name="T">The type of options.</typeparam>
        /// <param name="displayName">Header of the element.</param>
        /// <param name="getter">The function to retrieve the current value from the object.</param>
        /// <param name="setter">The action to set the value.</param>
        /// <param name="options">The list of options.</param>
        /// <returns></returns>
        public static IObjectEditorElement CollapsableOptionSelect<T>(string header, Func<T> getter, Action<T> setter,
            IEnumerable<Option<T>> options, bool displayNullOption = true)
        {
            return new CollapsableOptionSelectElement<T>(header, getter, setter, options, displayNullOption);
        }

        /// <summary>
        /// Creates a <see cref="CollapsableOptionSelectElement{TEnum}"/> that can select from all values of <typeparamref name="TEnum"/>.
        /// </summary>
        /// <typeparam name="TEnum">The type of options.</typeparam>
        /// <param name="getter">The function to retrieve the current value from the object.</param>
        /// <param name="setter">The action to set the value.</param>
        /// <param name="options">The list of options.</param>
        /// <returns></returns>
        public static IObjectEditorElement CollapsableOptionSelect<TEnum>(string header, Func<TEnum> getter, Action<TEnum> setter)
            where TEnum : Enum
        {
            IEnumerable<Option<TEnum>> options = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().
                Select(val =>
                new Option<TEnum>(val, Enum.GetName(typeof(TEnum), val)));

            return CollapsableOptionSelect(header, getter, setter, options);
        }

        /// <summary>
        /// A wrap to create <see cref="CollapsableOptionSelectElement{T}"/> and pass list of options as params.
        /// </summary>
        /// <typeparam name="T">The type of options.</typeparam>
        /// <param name="displayName">Header of the element.</param>
        /// <param name="getter">The function to retrieve the current value from the object.</param>
        /// <param name="setter">The action to set the value.</param>
        /// <param name="options">The list of options.</param>
        /// <returns></returns>
        public static IObjectEditorElement CollapsableOptionSelect<T>(string header, Func<T> getter, Action<T> setter, bool displayNullOption,
            params Option<T>[] options)
        {
            return CollapsableOptionSelect(header, getter, setter, options as IEnumerable<Option<T>>, displayNullOption);
        }
        #endregion

        #region Point and Polygon
        public static IObjectEditorElement Point(string displayName, string displayCode, Func<Vector2> getter, Action<Vector2> setter,
            bool displayTogglePointButton = true)
        {
            return new PointElement(displayName, displayCode, getter, setter, displayTogglePointButton);
        }

        public static IObjectEditorElement Polygon(string displayName, Vector2[] vertices)
        {
            PolygonElement.PointAccessor[] accessors = new PolygonElement.PointAccessor[vertices.Length];

            for (int i = 0; i < vertices.Length; i++)
            {
                // Captured variable scope matters.
                int index = i;
                accessors[index].getter = () => vertices[index];
                accessors[index].setter = val => vertices[index] = val;
            }

            return new PolygonElement(displayName, accessors);
        }
        #endregion
    }
}
