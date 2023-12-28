using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROM.UserInteraction
{
    public class Option<TOption>(TOption value, string name)
    {
        public TOption Value { get; } = value;

        public string Name { get; } = name;


        public static implicit operator Option<TOption>(TOption value)
        {
            string optionName = value?.ToString() ?? "Null";

            return new Option<TOption>(value, optionName);
        }
    }
}
