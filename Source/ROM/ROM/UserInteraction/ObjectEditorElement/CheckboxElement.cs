using ROM.UserInteraction.InroomManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ROM.UserInteraction.ObjectEditorElement
{
    /// <summary>
    /// A simple checkbox to toggle a bool value.
    /// </summary>
    public class CheckboxElement : IObjectEditorElement
    {
        private Func<bool> Getter { get; }
        private Action<bool> Setter { get; }

        private bool Target
        {
            get
            {
                return Getter();
            }
            set
            {
                Setter(value);
            }
        }

        private bool SavedValue { get; set; }

        private string DisplayName;

        public bool HasChanges => SavedValue != Target;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="displayName">Header of the element.</param>
        /// <param name="getter">The function to retrieve the current value from the object.</param>
        /// <param name="setter">The action to set the value.</param>
        public CheckboxElement(string displayName, Func<bool> getter, Action<bool> setter)
        {
            Getter = getter;
            Setter = setter;
            DisplayName = displayName;

            SavedValue = Target;
        }

        public void Draw()
        {
            Target = GUILayout.Toggle(Target, DisplayName);
        }

        public void OnSaved()
        {
            SavedValue = Target;
        }

        public void ResetChanges()
        {
            Target = SavedValue;
        }

        public void DrawPostWindow()
        { }
    }
}
