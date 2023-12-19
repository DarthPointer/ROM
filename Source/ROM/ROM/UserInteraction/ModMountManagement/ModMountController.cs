using BepInEx.Logging;
using Newtonsoft.Json.Linq;
using ROM.ObjectDataStorage;
using ROM.RoomObjectService;
using ROM.UserInteraction.InroomManagement;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROM.UserInteraction.ModMountManagement
{
    internal class ModMountController
    {
        #region Constants
        public const string MODIFY_FOLDER = "modify";
        #endregion

        #region Fields
        private Room? _contextRoom;

        private IReadOnlyList<ObjectData>? _currentRoomObjectsList;
        #endregion

        #region Properties
        public ModMount ModMount
        {
            get;
            private set;
        }

        private IMGUIWindowsContainer ChildWindowsContainer { get; }

        public Room? ContextRoom
        {
            get
            {
                return _contextRoom;
            }
            set
            {
                if (_contextRoom != value)
                {
                    _contextRoom = value;

                    UpdateCurrentRoomObjects();
                }
            }
        }

        public IReadOnlyList<ObjectData>? CurrentRoomObjectsList
        {
            get
            {
                return _currentRoomObjectsList;
            }
            private set
            {
                if (_currentRoomObjectsList != value)
                {
                    _currentRoomObjectsList = value;
                    UpdateEditObjectWindowsDict();
                }
            }
        }

        public IReadOnlyDictionary<ObjectData, EditRoomObjectWindow?> EditObjectWindows => EditObjectWindowsDict;
        private Dictionary<ObjectData, EditRoomObjectWindow?> EditObjectWindowsDict { get; set; } = [];
        private bool IsDroppingEditObjectWindows { get; set; } = false;

        public bool CanWrite => ModMount.Mod.workshopMod == false;
        #endregion

        #region Constructors
        public ModMountController(ModMount modMount, IMGUIWindowsContainer childWindowsContainer)
        {
            ModMount = modMount;
            ChildWindowsContainer = childWindowsContainer;
        }
        #endregion

        #region Methods
        public void SaveMountFile()
        {
            if (ModMount.Mod.workshopMod == true)
            {
                throw new InvalidOperationException($"{nameof(SaveMountFile)} is called for the controller of the \"{ModMount.Mod.id}\" mod mount, " +
                    $"but its mod is a downloaded workshop mod. Saving mounts and objects of workshop mods is not allowed.");
            }

            try
            {
                Directory.CreateDirectory(Path.Combine(ModMount.Mod.path, MODIFY_FOLDER));

                File.WriteAllText(Path.Combine(ModMount.Mod.path, MODIFY_FOLDER, ObjectRegistry.ROM_MOUNT_FILE_ASSET_PATH),
                    ModMount.GenerateMountModifyFileString());
            }
            catch (Exception ex)
            {
                ROMPlugin.Logger?.LogError($"Exception caught while attempting to save mount {ModMount.Mod.id}.\n{ex}");

                throw;
            }
        }
        
        public void AddObject(string newObjectFilePath, ITypeOperator typeOperator)
        {
            AssertContextRoomNotNull();

            string targetFilePath = ObjectData.GetPrimarySourceFilePath(ModMount.Mod, newObjectFilePath);
            if (File.Exists(targetFilePath))
            {
                string fileExistsErrorString  = $"File {targetFilePath} already exists.";

                ROMPlugin.Logger?.LogError(fileExistsErrorString);
                throw new Exception(fileExistsErrorString);
            }

            object newObject;
            try
            {
                newObject = typeOperator.CreateNew(ContextRoom);
                (newObject as ICallAfterPropertiesSet)?.OnAfterPropertiesSet();
            }
            catch (Exception ex)
            {
                string objectCreationErrorString = $"An exception caught while creating a new instance for typeId {typeOperator.TypeId}.\n{ex}";

                ROMPlugin.Logger?.LogError(objectCreationErrorString);
                throw new Exception(objectCreationErrorString, ex);
            }

            JToken dataJson;
            try
            {
                dataJson = typeOperator.Save(newObject);
            }
            catch (Exception ex)
            {
                string objectSavingErrorString = $"An exception caught while saving the data from the new instance for typeId {typeOperator.TypeId}.\n{ex}";

                ROMPlugin.Logger?.LogError(objectSavingErrorString);
                throw new Exception(objectSavingErrorString, ex);
            }

            ObjectData newObjectData = new()
            {
                TypeId = typeOperator.TypeId,
                RoomId = ContextRoom.abstractRoom.name,
                FilePath = newObjectFilePath,
                Mod = ModMount.Mod,
                DataJson = dataJson
            };

            try
            {
                newObjectData.Save();
            }
            catch (Exception ex)
            {
                string objectSavingErrorString = $"An exception occurred while saving the new object file. {ex.GetType()}. " +
                    $"See logs for more info.";

                ROMPlugin.Logger?.LogError(objectSavingErrorString);
                throw new Exception(objectSavingErrorString, ex);
            }

            SpawningManager.SpawnedObjectsTracker.Remove(newObjectData);
            SpawningManager.SpawnedObjectsTracker.Add(newObjectData, new WeakReference<object>(newObject));
            try
            {
                typeOperator.AddToRoom(newObject, ContextRoom);
            }
            catch (Exception ex)
            {
                string addingToRoomErrorString = $"An exception occurred while adding the new object to the room. {ex.GetType()}.";

                ROMPlugin.Logger?.LogError(addingToRoomErrorString);
                throw new Exception(addingToRoomErrorString, ex);
            }

            ModMount.AddObjectData(newObjectData);
            EditObjectWindowsDict[newObjectData] = null;

            try
            {
                SaveMountFile();
            }
            catch (Exception ex)
            {
                string mountSavingErrorString = $"An exception occurred while saving the updated mount file. {ex.GetType()}.";

                ROMPlugin.Logger?.LogError(mountSavingErrorString);
                throw new Exception(mountSavingErrorString, ex);
            }
        }

        public void DeleteObject(ObjectData objectData)
        {
            AssertContextRoomNotNull();

            if (SpawningManager.SpawnedObjectsTracker.TryGetValue(objectData, out var objRef) &&
                    objRef.TryGetTarget(out object obj))
            {
                try
                {
                    ITypeOperator typeOperator = objectData.GetTypeOperator();
                    typeOperator.RemoveFromRoom(obj, ContextRoom);
                }
                catch (Exception ex)
                {
                    string objectRemovalErrorString = $"An exception occurred while removing the object from the room. {ex.GetType()}.";

                    ROMPlugin.Logger?.LogError(objectRemovalErrorString);
                    throw new Exception(objectRemovalErrorString, ex);
                }
            }

            try
            {
                File.Delete(objectData.GetPrimarySourceFilePath());
            }
            catch (Exception ex)
            {
                string objectSavingErrorString = $"Exception occurred while deleting the object {objectData.FilePath} from the files.\n{ex}";

                ROMPlugin.Logger?.LogError(objectSavingErrorString);
                throw new Exception(objectSavingErrorString, ex);
            }

            SpawningManager.SpawnedObjectsTracker.Remove(objectData);
            ModMount.ObjectsByRooms[ContextRoom.abstractRoom.name].Remove(objectData);
            EditObjectWindowsDict[objectData]?.Close();
            EditObjectWindowsDict.Remove(objectData);
            SaveMountFile();
        }

        public void OpenWindowForObject(ObjectData objectData)
        {
            if (ContextRoom == null)
                return;

            if (EditObjectWindowsDict[objectData] == null)
            {
                if (SpawningManager.SpawnedObjectsTracker[objectData].TryGetTarget(out object roomObject))
                {
                    ITypeOperator typeOperator = objectData.GetTypeOperator();

                    EditRoomObjectWindow window = new(this, objectData, roomObject, typeOperator.GetEditorElements(roomObject, ContextRoom));
                    EditObjectWindowsDict[objectData] = window;
                    ChildWindowsContainer.AddWindow(window);

                    return;
                }

                string noSpawnedObjectFoundErrorString =
                    $"Can not create a window to edit {objectData.FullLogString} because there is no object found spawned by it.";

                ROMPlugin.Logger?.LogError(noSpawnedObjectFoundErrorString);
                throw new Exception(noSpawnedObjectFoundErrorString);
            }
        }

        public void CloseWindowForObject(ObjectData objectData)
        {
            EditObjectWindowsDict[objectData]?.Close();
        }

        public void RemoveWindowForObject(ObjectData objectData)
        {
            if (!IsDroppingEditObjectWindows)
                EditObjectWindowsDict[objectData] = null;
        }

        private void UpdateCurrentRoomObjects()
        {
            if (ContextRoom == null)
            {
                CurrentRoomObjectsList = null;
                return;
            }

            if (ModMount.ObjectsByRooms.TryGetValue(ContextRoom.abstractRoom.name, out List<ObjectData> result))
            {
                CurrentRoomObjectsList = result;
                return;
            }

            CurrentRoomObjectsList = null;
        }

        private void UpdateEditObjectWindowsDict()
        {
            IsDroppingEditObjectWindows = true;

            foreach (EditRoomObjectWindow? editRoomObjectWindow in EditObjectWindowsDict.Values)
            {
                editRoomObjectWindow?.Close();
            }

            IsDroppingEditObjectWindows = false;

            EditObjectWindowsDict = [];

            if (CurrentRoomObjectsList != null)
            {
                foreach (ObjectData objectData in CurrentRoomObjectsList)
                {
                    EditObjectWindowsDict[objectData] = null;
                }
            }
        }

        [MemberNotNull(nameof(ContextRoom))]
        private void AssertContextRoomNotNull()
        {
            if (ContextRoom == null)
            {
                string noContextRoomErrorString = $"{nameof(ModMountController)} can not operate with no context room set";

                ROMPlugin.Logger?.LogError(noContextRoomErrorString);
                throw new InvalidOperationException(noContextRoomErrorString);
            }
        }
        #endregion
    }
}
