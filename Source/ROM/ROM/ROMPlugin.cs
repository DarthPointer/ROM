using BepInEx;
using ROM.UserInteraction;
using System.Security;
using System.Security.Permissions;
using UnityEngine;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[module: UnverifiableCode]
#pragma warning restore CS0618

namespace ROM
{
    [BepInPlugin(GUID: ROM_MOD_ID, Name: "Room Object Manager", Version: "0.0.0")]
    public class ROMPlugin : BaseUnityPlugin
    {
        #region Constants
        public const string ROM_MOD_ID = "DarthPointer.ROM";
        #endregion

        #region Fields
        private ROMOptionInterface _optionInterface;
        private IMGUIWindowsContainer _windowsContainer;
        private GameObject _imguiWindowsContainerGO;
        #endregion

        #region Methods
        public void OnEnable()
        {
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
        }

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);

            _optionInterface = new ROMOptionInterface();
            MachineConnector.SetRegisteredOI(ROM_MOD_ID, _optionInterface);

            _imguiWindowsContainerGO = new GameObject("ROM IMGUI Windows Container", typeof(IMGUIWindowsContainer));
            _windowsContainer = _imguiWindowsContainerGO.GetComponent<IMGUIWindowsContainer>();
            _windowsContainer.DisplayWindows = true;

            _windowsContainer.AddWindow(new SampleWindow());
            _windowsContainer.AddWindow(new SampleWindow());
        }
        #endregion
    }
}
