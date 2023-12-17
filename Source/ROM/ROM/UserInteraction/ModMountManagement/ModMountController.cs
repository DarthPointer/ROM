using BepInEx.Logging;
using Newtonsoft.Json.Linq;
using ROM.ObjectDataStorage;
using ROM.RoomObjectService;
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
        #endregion

        #region Properties
        public ModMount ModMount
        {
            get;
            private set;
        }

        public Room? ContextRoom
        {
            get
            {
                return _contextRoom;
            }
            set
            {
                _contextRoom = value;
            }
        }

        public bool CanWrite => ModMount.Mod.workshopMod == false;
        #endregion

        #region Constructors
        public ModMountController(ModMount modMount)
        {
            ModMount = modMount;
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

            UpdatableAndDeletable newObject;
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
            SpawningManager.SpawnedObjectsTracker.Add(newObjectData, new WeakReference<UpdatableAndDeletable>(newObject));
            ContextRoom.AddObject(newObject);
            ModMount.AddObjectData(newObjectData);

            try
            {
                SaveMountFile();
            }
            catch (Exception ex)
            {
                string mountSavingErrorString = $"An exception occurred while saving the updated mount file. {ex.GetType()}. " +
                    $"See logs for more info.";

                ROMPlugin.Logger?.LogError(mountSavingErrorString);
                throw new Exception(mountSavingErrorString, ex);
            }
        }

        public void DeleteObject(ObjectData objectData)
        {
            AssertContextRoomNotNull();

            if (SpawningManager.SpawnedObjectsTracker.TryGetValue(objectData, out var uadRef) &&
                    uadRef.TryGetTarget(out UpdatableAndDeletable uad))
            {
                ContextRoom.RemoveObject(uad);
                uad.Destroy();
            }

            try
            {
                SaveMountFile();
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
