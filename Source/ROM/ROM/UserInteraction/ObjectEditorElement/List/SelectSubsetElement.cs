using ROM.IMGUIUtilities;
using ROM.UserInteraction.InroomManagement;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ROM.UserInteraction.ObjectEditorElement.List
{
    public class SelectSubsetElement<T> : IObjectEditorElement
    {
        #region Fields
        private Vector2 _addNewOptionScrollState;
        #endregion

        #region Properties
        private string DisplayName { get; }

        public bool IsCollapsed { get; set; } = true;

        public bool HasChanges => SelectedList.HasChanges;

        private Func<List<T>> Getter { get; }
        private Action<List<T>> Setter { get; }

        private List<T> Target
        {
            get => Getter();
            set
            {
                if (value != Getter())
                {
                    Setter(value);
                    OnTargetChanged();
                }
            }
        }

        private OptionsFilter<T> OptionsFilter { get; set; }
        private ListElement<T> SelectedList { get; }

        private Dictionary<T, Option<T>> OptionsByValue { get; }

        private Option<T>? OptionToAdd { get; set; }
        #endregion

        #region Constructors
        public SelectSubsetElement(string displayName, Func<List<T>> getter, Action<List<T>> setter, IEnumerable<Option<T>> optionsSet)
        {
            DisplayName = displayName;

            List<Option<T>> allOptions = optionsSet.ToList();

            Getter = getter;
            Setter = setter;

            OptionsByValue = allOptions.ToDictionary(opt => opt.Value, opt => opt);
            OnTargetChanged();

            SelectedList = new ListElement<T>($"Selected values",
                getter: () => Target,
                setter: val => Target = val,
                CreateSelectedOptionElement,
                drawCollapseButton: false);

            SelectedList.IsCollapsed = false;
        }
        #endregion

        #region Methods
        private IObjectEditorElement CreateSelectedOptionElement(T optionValue, Action deleteCall)
        {
            Action customDeleteCall = () =>
            {
                deleteCall();
                OnSelectedOptionDeleted(optionValue);
            };

            if (OptionsByValue.TryGetValue(optionValue, out Option<T> option))
            {
                return new SelectedSubsetListItemElement<T>(option, customDeleteCall);
            }

            return new SelectedSubsetListItemElement<T>(optionValue, customDeleteCall);
        }

        private void OnSelectedOptionDeleted(T optionValue)
        {
            if (OptionsByValue.TryGetValue(optionValue, out Option<T> option))
            {
                OptionsFilter.AddOption(option);
            }
        }

        [MemberNotNull(nameof(OptionsFilter))]
        private void OnTargetChanged()
        {
            OptionsFilter = new OptionsFilter<T>(OptionsByValue.Values.Where(option => !Target.Contains(option.Value)));
        }

        public void Draw(RoomCamera? roomCamera)
        {
            CommonIMGUIUtils.HorizontalLine();

            GUILayout.BeginHorizontal();
            GUILayout.Label(DisplayName);

            if (GUILayout.Button(IsCollapsed ? "+" : "-")) IsCollapsed = !IsCollapsed;
            GUILayout.EndHorizontal();

            if (!IsCollapsed)
            {
                OptionsFilter.SearchFilter = GUILayout.TextArea(OptionsFilter.SearchFilter);

                _addNewOptionScrollState = GUILayout.BeginScrollView(_addNewOptionScrollState, GUILayout.Height(200));
                GUILayout.BeginVertical();

                foreach (Option<T> option in OptionsFilter.FilteredOptions)
                {
                    DrawOptionToAdd(option);
                }

                AddSelectedOption();

                GUILayout.EndVertical();
                GUILayout.EndScrollView();

                SelectedList.Draw(roomCamera);
            }

            CommonIMGUIUtils.HorizontalLine();
        }

        private void DrawOptionToAdd(Option<T> option)
        {
            if (GUILayout.Button(option.Name))
            {
                OptionToAdd = option;
            }
        }

        private void AddSelectedOption()
        {
            if (OptionToAdd != null)
            {
                OptionsFilter.RemoveOption(OptionToAdd);

                OptionToAdd = null;
            }
        }

        public void DrawPostWindow(RoomCamera? roomCamera)
        {
            SelectedList.DrawPostWindow(roomCamera);
        }

        public void OnSaved()
        {
            SelectedList.OnSaved();
        }

        public void ReceiveFContainer(FContainer? container)
        {
            SelectedList.ReceiveFContainer(container);
        }

        public void ResetChanges()
        {
            SelectedList.ResetChanges();
        }

        public void Terminate()
        {
            SelectedList.Terminate();
        }
        #endregion
    }
}
