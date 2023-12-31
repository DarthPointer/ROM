using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROM.UserInteraction.ObjectEditorElement.TextField
{
    public class TextFieldConfiguration<T>(
        Func<T, string> formatter, TryParseValue<T> parser, Func<T, bool> valueValidator, Func<T, T>? valueRestrictor)
        where T : notnull
    {
        public Func<T, string> Formatter { get; set; } = formatter;

        public TryParseValue<T> Parser { get; set; } = parser;

        public Func<T, bool> ValueValidator { get; set; } = valueValidator;

        public Func<T, T>? ValueRestrictor { get; set; } = valueRestrictor;
    }

    public delegate bool TryParseValue<T>(string input, out T value)
        where T : notnull;

    public class FloatTextFieldConfiguration : TextFieldConfiguration<float>
    {
        public FloatTextFieldConfiguration() : base(
            formatter: value => value.ToString("g8", CultureInfo.InvariantCulture),

            parser: (string input, out float value) => float.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out value),

            valueValidator: _ => true,
            valueRestrictor: null)
        { }
    }

    public class IntTextFieldConfiguration : TextFieldConfiguration<int>
    {
        public IntTextFieldConfiguration() : base(
            formatter: value => value.ToString(CultureInfo.InvariantCulture),

            parser: (string input, out int value) => int.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out value),

            valueValidator: _ => true,
            valueRestrictor: null)
        { }
    }

    public class StringTextFieldConfiguration : TextFieldConfiguration<string>
    {
        public StringTextFieldConfiguration() : base(
            formatter: value => value,
            parser: (string input, out string value) => { value = input; return true; },
            valueValidator: _ => true,
            valueRestrictor: null)
        { }
    }

    public static class TextFieldUtils
    {
        public static Func<T, bool> InRange<T>(T min, T max)
            where T : IComparable<T>
        {
            return value => value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0;
        }
    }
}
