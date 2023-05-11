using Menu.Remix.MixedUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ROM
{
    public class ROMOptionInterface : OptionInterface
    {
        #region Properties
        public Configurable<KeyCode> ToggleROMUIKeyConfigurable
        {
            get;
            private set;
        }
        #endregion

        #region Constructors
        public ROMOptionInterface()
        {
            ToggleROMUIKeyConfigurable = config.Bind("ROM_ToggleUIKey", KeyCode.R);            
        }
        #endregion

        #region Methods
        public override void Initialize()
        {
            ROMOptionInterfaceFirstTab firstTab = new(this, "ROM Primary Settings");

            Tabs = new OpTab[]
            {
                firstTab
            };
        }
        #endregion
    }
}
