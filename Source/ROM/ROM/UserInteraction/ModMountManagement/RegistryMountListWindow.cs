using BepInEx.Logging;
using Menu;
using ROM.ObjectDataStorage;
using ROM.UserInteraction.InroomManagement;
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

        //private static readonly GUIStyle DEFAULT_WHITE_TEXT;

        //static RegistryMountListWindow()
        //{
        //    DEFAULT_WHITE_TEXT = new GUIStyle();
        //    DEFAULT_WHITE_TEXT.normal.textColor = Color.white;
        //}
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

        private IMGUIWindowsContainer ChildWindowsContainer { get; }

        private bool IsCollapsed
        {
            get;
            set;
        }

        private List<ModManager.Mod> ActiveLocalModsWithNoMountLoaded
        {
            get;
            set;
        } = new();

        private List<StoredModMountController> ModMountControllers { get; set; } = new();

        private Room? PreviousFrameRoom { get; set; } = null;

        private WeakReference<RoomCamera?> TrackedCamera { get; set; } = new(null);
        #endregion

        #region Constructors
        public RegistryMountListWindow(ObjectRegistry objectRegistry, IMGUIWindowsContainer childWindowsContainer)
        {
            ChildWindowsContainer = childWindowsContainer;

            On.RoomCamera.ctor += RoomCamera_ctor;

            _windowRect = new Rect(10, 10, 0, 0);

            ObjectRegistry = objectRegistry;
            ModMountControllers = ObjectRegistry.ModMounts.Select(mount => new StoredModMountController(new(mount))).ToList();

            PopulateActiveLocalModsWithNoMountLoaded();
        }

        private void RoomCamera_ctor(On.RoomCamera.orig_ctor orig, RoomCamera self, RainWorldGame game, int cameraNumber)
        {
            orig(self, game, cameraNumber);

            if (cameraNumber == 0)
            {
                TrackedCamera = new(self);
            }
        }
        #endregion

        #region Methods
        #region Display
        void IIMGUIWindow.Display()
        {
            Room? newRoom = TryGetRoomFromTrackedCamera(TrackedCamera);

            if (newRoom != PreviousFrameRoom)
            {
                foreach (StoredModMountController storedModMountController in ModMountControllers)
                {
                    storedModMountController.Controller.ContextRoom = newRoom;
                }

                PreviousFrameRoom = newRoom;
            }

            _windowRect = GUILayout.Window(GetHashCode(), _windowRect, MountListWindow, "ROM Mod Mounts",
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

            foreach (StoredModMountController storedModMountController in ModMountControllers)
            {
                LoadedMountElement(storedModMountController);
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        private void LoadedMountElement(StoredModMountController mountEntry)
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label($"{mountEntry.Controller.ModMount.ModId}: loaded objects for {mountEntry.Controller.ModMount.ObjectsByRooms.Count} rooms");

            if (mountEntry.AssignedWindow == null)
            {
                if (GUILayout.Button($"Open"))
                {
                    mountEntry.AssignedWindow = new(mountEntry.Controller);
                    ChildWindowsContainer.AddWindow(mountEntry.AssignedWindow);
                }
            }
            else
            {
                if (GUILayout.Button($"Close"))
                {
                    mountEntry.AssignedWindow.Close();
                    mountEntry.AssignedWindow = null;
                }
            }

            GUILayout.EndHorizontal();
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

        #region Data
        private static Room? TryGetRoomFromTrackedCamera(WeakReference<RoomCamera?> roomCameraReference)
        {
            if (roomCameraReference.TryGetTarget(out RoomCamera? roomCamera))
            {
                return roomCamera?.room;
            }

            return null;
        }

        private void PopulateActiveLocalModsWithNoMountLoaded()
        {
            HashSet<string> loadedMounts = new(ObjectRegistry.ModMounts.Select(mount => mount.ModId));

            ActiveLocalModsWithNoMountLoaded = ModManager.ActiveMods.
                Where(mod => !mod.workshopMod && !loadedMounts.Contains(mod.id)).ToList();
        }

        private void CreateMountForMod(ModManager.Mod mod)
        {
            ModMount newMount = new ModMount(mod.id);

            ModMountController controller = new(newMount);

            bool mountSavedSuccessfully = true;
            try
            {
                controller.SaveMountFile();
            }
            catch (Exception ex)
            {
                mountSavedSuccessfully = false;

                ROMPlugin.Logger?.LogError($"Exception caught while saving the new mount for mod \"{mod.id}\".\n{ex}");
            }

            if (mountSavedSuccessfully)
            {
                ModMountControllers.Add(new(new(newMount)));
                controller.ContextRoom = PreviousFrameRoom;
                ObjectRegistry.ModMounts.Add(newMount);
                ActiveLocalModsWithNoMountLoaded.Remove(mod);
            }
        }
        #endregion
        #endregion

        private class StoredModMountController(ModMountController controller)
        {
            public ModMountController Controller { get; } = controller;
            public ModMountRoomObjectsWindow? AssignedWindow { get; set; }
        }
    }
}
