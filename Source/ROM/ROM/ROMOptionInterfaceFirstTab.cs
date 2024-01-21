using Menu.Remix.MixedUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ROM
{
    internal class ROMOptionInterfaceFirstTab : OpTab
    {
        #region Constructors
        public ROMOptionInterfaceFirstTab(ROMOptionInterface owner, string name = "") : base(owner, name)
        {
            AddItems(
                new OpLabel(new Vector2(140, 230), new Vector2(140, 30), text: "ROM UI Toggle"),
                new OpKeyBinder(owner.ToggleROMUIKeyConfigurable, new Vector2(200, 200), new Vector2(20, 20), controllerNo: OpKeyBinder.BindController.AnyController),
                new OpLabel(new Vector2(100, 170), new Vector2(220, 30), text: "Use this key to toggle ROM UI, all the interaction is there.")
                );
        }
        #endregion
    }
}
