using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ROM.UserInteraction.ObjectEditorElement.Scrollbar
{
    /// <summary>
    /// A configuration data container for <see cref="ScrollbarElement{T}"/>
    /// </summary>
    /// <typeparam name="T">The type of the value represented by the scrollbar.</typeparam>
    /// <param name="left">The leftmost coordinate of the scrollbar.</param>
    /// <param name="right">The rightmost coordinate of the scrollbar.</param>
    /// <param name="posToValue">The function to convert current scroll coordinate to a value of the represented type.</param>
    /// <param name="ValueToPos">The function to convert a value of the represented type to a coordinate.</param>
    /// <param name="valueFormatter">The function to generate value representation string.</param>
    public class ScrollbarConfiguration<T>(float left, float right, Func<float, T> posToValue, Func<T, float> ValueToPos, Func<T, string> valueFormatter)
    {
        /// <summary>
        /// The leftmost coordinate of the scrollbar.
        /// </summary>
        public float Left { get; set; } = left;
        /// <summary>
        /// The rightmost coordinate of the scrollbar.
        /// </summary>
        public float Right { get; set; } = right;

        /// <summary>
        /// The function to convert current scroll coordinate to a value of the represented type
        /// </summary>
        public Func<float, T> PosToValue { get; set; } = posToValue;
        /// <summary>
        /// The function to convert a value of the represented type to a coordinate.
        /// </summary>
        public Func<T, float> ValueToPos { get; set; } = ValueToPos;

        /// <summary>
        /// The function to generate value representation string.
        /// </summary>
        public Func<T, string> ValueFormatter { get; set; } = valueFormatter;
    }

    /// <summary>
    /// A trivial float scrollbar configuration to range the value between 0 and 1.
    /// </summary>
    public class FloatScrollbarConfiguration : ScrollbarConfiguration<float>
    {
        public FloatScrollbarConfiguration() : base(0, 1, f => f, f => f, value => value.ToString("g8", CultureInfo.InvariantCulture))
        { }
    }

    /// <summary>
    /// A trivial int scrollbar configuration to range the value between 2 integers.
    /// </summary>
    public class IntScrollbarConfiguration : ScrollbarConfiguration<int>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="left">The value at the left edge.</param>
        /// <param name="right">The value at the right edge.</param>
        public IntScrollbarConfiguration(int left, int right) :
            base(left, right, Mathf.RoundToInt, val => val, value => value.ToString(CultureInfo.InvariantCulture))
        { }
    }
}
