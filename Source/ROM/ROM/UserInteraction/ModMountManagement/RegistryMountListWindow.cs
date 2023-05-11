using BepInEx.Logging;
using Menu;
using ROM.ObjectDataStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ROM.UserInteraction.ModMountManagement
{
    internal class RegistryMountListWindow : IIMGUIWindow
    {
        #region Constants
        private const float WINDOW_WIDTH = 500;
        private const float WINDOW_HEIGHT = 500;
        private const float WINDOW_HEIGHT_COLLAPSED = 30;

        private static readonly Rect COLLAPSE_BUTTON_RECT = new(470, 1, 25, 18);
        private static readonly Rect LOADED_MOUNT_LIST_RECT = new(5, 20, 490, 250);
        private static readonly Rect LOADED_MOUNT_LIST_INSIDE_RECT = new(0, 0, 1000, 1000);

        private static readonly GUIStyle DEFAULT_WHITE_TEXT;

        static RegistryMountListWindow()
        {
            DEFAULT_WHITE_TEXT = new GUIStyle();
            DEFAULT_WHITE_TEXT.normal.textColor = Color.white;

            LOADED_MOUNT_LIST_RECT = new();
        }
        #endregion

        #region Fields
        private Rect _windowRect;
        private Vector2 _loadedMountListScroll = Vector2.zero;
        private Vector2 _activeLocalModsWithNoMountLoadedScroll = Vector2.zero;
        #endregion

        #region Properties
        private ObjectRegistry ObjectRegistry
        {
            get;
            set;
        }

        private bool IsCollapsed
        {
            get;
            set;
        }

        private List<ModManager.Mod> ActiveLocalModsWithNoMountLoaded
        {
            get;
            set;
        }

        private ManualLogSource? Logger
        {
            get;
            set;
        }
        #endregion

        #region Constructors
        public RegistryMountListWindow(ObjectRegistry objectRegistry, ManualLogSource? logger = null)
        {
            Logger = logger;

            _windowRect = new Rect(10, 10, 0, 0);

            ObjectRegistry = objectRegistry;

            PopulateActiveLocalModsWithNoMountLoaded();
        }
        #endregion

        #region Methods
        #region View
        void IIMGUIWindow.Display()
        {
            _windowRect = GUILayout.Window(GUIUtility.GetControlID(FocusType.Passive), _windowRect, MountListWindow, "ROM Mod Mounts",
                GUILayout.Width(WINDOW_WIDTH), GUILayout.Height(IsCollapsed ? WINDOW_HEIGHT_COLLAPSED : WINDOW_HEIGHT));
        }

        private void MountListWindow(int id)
        {
            GUILayout.BeginVertical();
            CollapseButton();

            if (!IsCollapsed)
            {
                LoadedMountList();
                ModsForMountCreation();
            }

            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private void LoadedMountList()
        {
            _loadedMountListScroll = GUILayout.BeginScrollView(_loadedMountListScroll, GUILayout.MaxHeight(300));
            GUILayout.BeginVertical();

            foreach (ModMount modMount in ObjectRegistry.ModMounts)
            {
                GUILayout.Label($"{modMount.ModId}: loaded objects for {modMount.ObjectsByRooms.Count} rooms");
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        private void ModsForMountCreation()
        {
            _activeLocalModsWithNoMountLoadedScroll = GUILayout.BeginScrollView(_activeLocalModsWithNoMountLoadedScroll);
            GUILayout.BeginVertical();

            foreach (ModManager.Mod mod in ActiveLocalModsWithNoMountLoaded)
            {
                GUILayout.BeginHorizontal();

                GUILayout.Label($"{mod.name}");
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Create Mount"))
                {
                    CreateMountForMod(mod);
                    
                    // We have to break here because the mod should be removed from the list of local mods with no mount.
                    // It can be worked around in a different manner to proceed to the end and remove the mod after the cycle, but for now this will do.
                    break;
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        private void CollapseButton()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("-"))
            {
                IsCollapsed = !IsCollapsed;
            }

            GUILayout.EndHorizontal();
        }
        #endregion

        #region ViewModel
        private void PopulateActiveLocalModsWithNoMountLoaded()
        {
            HashSet<string> loadedMounts = new(ObjectRegistry.ModMounts.Select(mount => mount.ModId));

            ActiveLocalModsWithNoMountLoaded = ModManager.ActiveMods.
                Where(mod => !mod.workshopMod && !loadedMounts.Contains(mod.id)).ToList();
        }

        private void CreateMountForMod(ModManager.Mod mod)
        {
            ModMount newMount = new ModMount(mod.id);

            ModMountController controller = new(newMount, Logger);

            bool mountSavedSuccessfully = true;
            try
            {
                controller.SaveMountFile();
            }
            catch (Exception ex)
            {
                mountSavedSuccessfully = false;

                Logger?.LogError($"Exception caught while saving the new mount for mod \"{mod.id}\".\n{ex}");
            }

            if (mountSavedSuccessfully)
            {
                ObjectRegistry.ModMounts.Add(newMount);
                ActiveLocalModsWithNoMountLoaded.Remove(mod);
            }
        }
        #endregion
        #endregion
    }
}
