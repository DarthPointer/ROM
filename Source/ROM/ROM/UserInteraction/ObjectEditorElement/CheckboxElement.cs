using ROM.UserInteraction.InroomManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ROM.UserInteraction.ObjectEditorElement
{
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
