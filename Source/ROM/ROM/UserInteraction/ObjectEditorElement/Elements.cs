using ROM.UserInteraction.InroomManagement;
using ROM.UserInteraction.ObjectEditorElement.TextField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROM.UserInteraction.ObjectEditorElement
{
    public static class Elements
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
