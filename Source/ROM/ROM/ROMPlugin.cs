using BepInEx;
using ROM.ObjectDataStorage;
using ROM.SpawningService;
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
        private bool _haveHooked = false;

        private ROMOptionInterface _optionInterface;
        private IMGUIWindowsContainer _windowsContainer;
        private GameObject _imguiWindowsContainerGO;

        private SpawningManager _spawningManager;
        #endregion

        #region Methods
        public void OnEnable()
        {
            if (!_haveHooked)
            {
                On.RainWorld.OnModsInit += RainWorld_OnModsInit;
                _haveHooked = true;
            }
            _imguiWindowsContainerGO = new GameObject("ROM IMGUI Windows Container", typeof(IMGUIWindowsContainer));
            _windowsContainer = _imguiWindowsContainerGO.GetComponent<IMGUIWindowsContainer>();
            _windowsContainer.DisplayWindows = true;

            _windowsContainer.AddWindow(new SampleWindow());
            _windowsContainer.AddWindow(new SampleWindow());
        }

        public void Update()
        {
        }

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            On.RainWorld.OnModsInit -= RainWorld_OnModsInit;
            orig(self);

            _optionInterface = new ROMOptionInterface();
            MachineConnector.SetRegisteredOI(ROM_MOD_ID, _optionInterface);

            ObjectRegistry objectRegistry = ObjectRegistry.CreateRegistryFromMountFileAsset(ObjectRegistry.ROM_MOUNT_FILE_ASSET_PATH, Logger);
            _spawningManager = new SpawningManager(objectRegistry, Logger);
        }
        #endregion
    }
}
