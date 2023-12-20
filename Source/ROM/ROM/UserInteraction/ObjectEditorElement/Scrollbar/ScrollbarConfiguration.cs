using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ROM.UserInteraction.ObjectEditorElement.Scrollbar
{
    public class ScrollbarConfiguration<T>(float left, float right, Func<float, T> posToValue, Func<T, float> ValueToPos, Func<T, string> valueFormatter)
        where T : notnull
    {
        public float Left { get; set; } = left;
        public float Right { get; set; } = right;

        public Func<float, T> PosToValue { get; set; } = posToValue;
        public Func<T, float> ValueToPos { get; set; } = ValueToPos;

        public Func<T, string> ValueFormatter { get; set; } = valueFormatter;
    }

    public class FloatScrollbarConfiguration : ScrollbarConfiguration<float>
    {
        public FloatScrollbarConfiguration() : base(0, 1, f => f, f => f, value => value.ToString("g8", CultureInfo.InvariantCulture))
        { }
    }

    public class IntScrollbarConfiguration : ScrollbarConfiguration<int>
    {
        public IntScrollbarConfiguration(int left, int right) :
            base(left, right, Mathf.RoundToInt, val => val, value => value.ToString(CultureInfo.InvariantCulture))
        { }
    }
}
