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
        #region Fields
        private Configurable<KeyCode> _toggleRoomSceneUIKeyConfigurable;
        #endregion

        #region Properties
        public Configurable<KeyCode> ToggleRoomSceneUIKeyConfigurable
        {
            get
            {
                return _toggleRoomSceneUIKeyConfigurable;
            }
            private set
            {
                _toggleRoomSceneUIKeyConfigurable = value;
            }
        }
        #endregion

        #region Constructors
        public ROMOptionInterface()
        {
            ToggleRoomSceneUIKeyConfigurable = config.Bind("ROM_ToggleRoomSceneUIKey", KeyCode.R);            
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
