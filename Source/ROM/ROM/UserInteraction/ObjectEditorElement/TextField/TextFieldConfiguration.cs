using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROM.UserInteraction.ObjectEditorElement.TextField
{
    /// <summary>
    /// Container for <see cref="TextFieldElement{T}"/> configuration.
    /// </summary>
    /// <typeparam name="T">The type of edited value.</typeparam>
    /// <param name="formatter">The function to generate value representation string.</param>
    /// <param name="parser">The function to parse input string into a value.</param>
    /// <param name="valueValidator">The function to validate whether the parsed value is acceptable.</param>
    /// <param name="valueRestrictor">(Optional via <see langword="null"/>) The function to "snap" an "acceptable" value into desired domain.</param>
    public class TextFieldConfiguration<T>(
        Func<T, string> formatter, TryParseValue<T> parser, Func<T, bool> valueValidator, Func<T, T>? valueRestrictor)
        where T : notnull
    {
        /// <summary>
        /// The function to generate value representation string.
        /// </summary>
        public Func<T, string> Formatter { get; set; } = formatter;

        /// <summary>
        /// The function to parse input string into a value.
        /// </summary>
        public TryParseValue<T> Parser { get; set; } = parser;

        /// <summary>
        /// The function to validate whether the parsed value is acceptable.
        /// </summary>
        public Func<T, bool> ValueValidator { get; set; } = valueValidator;

        /// <summary>
        /// (Optional via <see langword="null"/>) The function to "snap" an "acceptable" value into desired domain.
        /// </summary>
        public Func<T, T>? ValueRestrictor { get; set; } = valueRestrictor;
    }

    public delegate bool TryParseValue<T>(string input, out T value)
        where T : notnull;

    /// <summary>
    /// A trivial float text field configuration without any restriction.
    /// </summary>
    public class FloatTextFieldConfiguration : TextFieldConfiguration<float>
    {
        public FloatTextFieldConfiguration() : base(
            formatter: value => value.ToString("g8", CultureInfo.InvariantCulture),

            parser: (string input, out float value) => float.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out value),

            valueValidator: _ => true,
            valueRestrictor: null)
        { }
    }

    /// <summary>
    /// A trivial int text field configuration without any restriction.
    /// </summary>
    public class IntTextFieldConfiguration : TextFieldConfiguration<int>
    {
        public IntTextFieldConfiguration() : base(
            formatter: value => value.ToString(CultureInfo.InvariantCulture),

            parser: (string input, out int value) => int.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out value),

            valueValidator: _ => true,
            valueRestrictor: null)
        { }
    }

    /// <summary>
    /// A trivial string text field configuration without any restrictions.
    /// </summary>
    public class StringTextFieldConfiguration : TextFieldConfiguration<string>
    {
        public StringTextFieldConfiguration() : base(
            formatter: value => value,
            parser: (string input, out string value) => { value = input; return true; },
            valueValidator: _ => true,
            valueRestrictor: null)
        { }
    }
}
