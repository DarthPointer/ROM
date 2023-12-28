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
        #endregion

        #region Properties
        public IReadOnlyList<Option<TOption>> Options { get; }

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
        public OptionsFilter(IEnumerable<Option<TOption>> options)
        {
            Options = options.ToList();
            UpdateFilteredOptions();
        }
        #endregion

        #region Methods
        [MemberNotNull(nameof(FilteredOptions))]
        private void UpdateFilteredOptions()
        {
            if (string.IsNullOrEmpty(SearchFilter))
            {
                FilteredOptions = Options.ToList();
                return;
            }

            FilteredOptions = Options.Where(option => option.Name.ToLower().Contains(SearchFilter.ToLower())).ToList();
        }
        #endregion
    }
}
