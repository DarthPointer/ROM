using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROM.UserInteraction
{
    internal class Option<TOption>(TOption value, string name)
    {
        public TOption Value { get; } = value;

        public string Name { get; } = name;
    }
}
