using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROM.UserInteraction
{
    /// <summary>
    /// An object to describe an option of arbitrary type.
    /// </summary>
    /// <typeparam name="TOption">The option type.</typeparam>
    /// <param name="value">The option value.</param>
    /// <param name="name">The display name of the option.</param>
    public class Option<TOption>(TOption value, string name)
    {
        /// <summary>
        /// The option value.
        /// </summary>
        public TOption Value { get; } = value;

        /// <summary>
        /// The display name of the option.
        /// </summary>
        public string Name { get; } = name;


        public static implicit operator Option<TOption>(TOption value)
        {
            string optionName = value?.ToString() ?? "Null";

            return new Option<TOption>(value, optionName);
        }
    }
}
