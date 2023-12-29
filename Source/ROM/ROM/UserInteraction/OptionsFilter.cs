using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROM.UserInteraction
{
    internal class OptionsFilter<TOption>
    {
        #region Fields
        private string _searchFilter = "";
        private List<Option<TOption>> _allOptionsList = [];
        #endregion

        #region Properties
        private List<Option<TOption>> AllOptionsList
        {
            get
            {
                return _allOptionsList;
            }
            set
            {
                if (_allOptionsList != value)
                {
                    _allOptionsList = value;
                    UpdateFilteredOptions();
                }
            }
        }

        public IReadOnlyList<Option<TOption>> AllOptions => AllOptionsList;

        public string SearchFilter
        {
            get
            {
                return _searchFilter;
            }
            set
            {
                if (_searchFilter != value)
                {
                    _searchFilter = value;
                    UpdateFilteredOptions();
                }
            }
        }

        public IReadOnlyList<Option<TOption>> FilteredOptions { get; private set; }
        #endregion

        #region Constructors
        public OptionsFilter(IEnumerable<Option<TOption>>? options = null)
        {
            SetOptions(options ?? Enumerable.Empty<Option<TOption>>());
            UpdateFilteredOptions();
        }
        #endregion

        #region Methods
        [MemberNotNull(nameof(FilteredOptions))]
        private void UpdateFilteredOptions()
        {
            if (string.IsNullOrEmpty(SearchFilter))
            {
                FilteredOptions = AllOptions.ToList();
                return;
            }

            FilteredOptions = AllOptions.Where(option => option.Name.ToLower().Contains(SearchFilter.ToLower())).ToList();
        }

        public void SetOptions(IEnumerable<Option<TOption>> options)
        {
            AllOptionsList = options.ToList();
        }

        public void AddOption(Option<TOption> option)
        {
            AllOptionsList.Add(option);
            UpdateFilteredOptions();
        }

        public bool RemoveOption(Option<TOption> option)
        {
            bool result = AllOptionsList.Remove(option);
            UpdateFilteredOptions();

            return result;
        }

        public int RemoveOption(TOption option)
        {
            int result = AllOptionsList.RemoveAll(opt => Equals(opt, option));

            if (result != 0)
                UpdateFilteredOptions();

            return result;
        }
        #endregion
    }
}
