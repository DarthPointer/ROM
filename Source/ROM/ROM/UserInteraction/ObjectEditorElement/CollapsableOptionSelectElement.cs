using Mono.Cecil;
using ROM.IMGUIUtilities;
using ROM.UserInteraction.InroomManagement;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ROM.UserInteraction.ObjectEditorElement
{
    public class CollapsableOptionSelectElement<T> : IObjectEditorElement
    {
        private readonly static GUIStyle BoldText = new();

        static CollapsableOptionSelectElement()
        {
            BoldText.fontStyle = FontStyle.Bold;
            // why store these at different levels??
            BoldText.normal.textColor = Color.white;
        }

        #region Fields
        private string _searchboxText = "";
        private Vector2 _optionsScrollState = Vector2.zero;
        private bool _isExpanded = false;
        #endregion

        #region Properties
        private string SearchboxText
        {
            get
            {
                return _searchboxText;
            }
            set
            {
                if (_searchboxText != value)
                {
                    _searchboxText = value;
                    Controller.SearchFilter = value;
                }
            }
        }

        private bool IsExpanded
        {
            get
            {
                return _isExpanded;
            }
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    if (!value)
                    {
                        _optionsScrollState = Vector2.zero;
                    }
                }
            }
        }

        private string Header { get; }

        private Func<T> Getter { get; }
        private Action<T> Setter { get; }

        private T Target
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

        private OptionsFilter<T> Controller { get; }

        private Dictionary<T, Option<T>> OptionsByValues { get; }
        private Dictionary<string, Option<T>> OptionsByName { get; }
        private Option<T>? NullOption { get; }

        private T SavedOption { get; set; }

        public bool HasChanges => !Equals(SavedOption, Target);
        #endregion

        #region Constructors
        public CollapsableOptionSelectElement(string header, Func<T> getter, Action<T> setter, IEnumerable<Option<T>> options)
        {
            Header = header;
            Controller = new OptionsFilter<T>(options);

            OptionsByValues = [];
            OptionsByName = [];

            // Silently avoiding exceptions in the case of duplicate values
            foreach (Option<T> option in options)
            {
                if (option.Value is null)
                {
                    NullOption = option;
                }
                else
                {
                    OptionsByValues[option.Value] = option;
                }

                OptionsByName[option.Name] = option;
            }

            Getter = getter;
            Setter = setter;

            SavedOption = Target;
        }
        #endregion

        #region Methods
        private bool TryGetCurrentOption([NotNullWhen(true)] out Option<T>? option)
        {
            option = null;

            if (Target is null)
            {
                if (NullOption == null)
                {
                    ROMPlugin.Logger?.LogError($"{nameof(CollapsableOptionSelectElement<T>)} has its target set as null " +
                        $"but no null-value option was provided at its creation.");
                    return false;
                }

                option = NullOption;
                return true;
            }

            if (OptionsByValues.TryGetValue(Target, out Option<T> opt))
            {
                option = opt;
                return true;
            }

            ROMPlugin.Logger?.LogError($"{nameof(CollapsableOptionSelectElement<T>)} was not provided an option with value {Target}");
            return false;
        }

        private void Select(Option<T> option)
        {
            Target = option.Value;
        }

        public void Draw()
        {
            CommonIMGUIUtils.HorizontalLine();
            GUILayout.Label(Header);

            DrawSelectedOption();

            if (IsExpanded)
            {
                DrawSearchAndSelect();
            }

            CommonIMGUIUtils.HorizontalLine();
        }

        private void DrawSelectedOption()
        {
            string optionText;

            if (TryGetCurrentOption(out Option<T>? selectedOption))
            {
                optionText = selectedOption.Name;
            }
            else
            {
                optionText = "Target value is invalid!";
            }

            GUILayout.BeginHorizontal();

            GUILayout.Label(optionText, BoldText);
            GUILayout.FlexibleSpace();
            DrawToggleExpandButton();

            GUILayout.EndHorizontal();
        }

        private void DrawToggleExpandButton()
        {
            string buttonText = IsExpanded ? "-" : "+";

            if (GUILayout.Button(buttonText))
            {
                IsExpanded = !IsExpanded;
            }
        }

        private void DrawSearchAndSelect()
        {
            SearchboxText = GUILayout.TextField(SearchboxText);

            _optionsScrollState = GUILayout.BeginScrollView(_optionsScrollState, GUILayout.Height(200));
            GUILayout.BeginVertical();

            foreach (Option<T> option in Controller.FilteredOptions)
            {
                DrawOptionToSelect(option);
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        private void DrawOptionToSelect(Option<T> option)
        {
            if (GUILayout.Button(option.Name))
            {
                Select(option);
            }
        }

        public void OnSaved()
        {
            SavedOption = Target;
        }

        public void ResetChanges()
        {
            Target = SavedOption;
        }

        public void DrawPostWindow()
        { }
        #endregion
    }
}
