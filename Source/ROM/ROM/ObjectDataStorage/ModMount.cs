using BepInEx.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROM.ObjectDataStorage
{
    /// <summary>
    /// The collection of objects provided by one mod.
    /// </summary>
    internal class ModMount
    {
        #region Constants
        /// <summary>
        /// The character used to designate new mount start in the merged ROMmount file.
        /// </summary>
        public const char MOUNT_START_RECORD_PREFIX = '!';
        #endregion

        #region Properties
        /// <summary>
        /// The StreamingAssets/mods folder name of the mod this collection represents.
        /// </summary>
        public string ModFolderName
        {
            get;
            private set;
        }

        /// <summary>
        /// The collection of spawners for the represented mod, organized by rooms.
        /// </summary>
        public Dictionary<string, List<ObjectData>> ObjectsByRooms
        {
            get;
            private set;
        }
        #endregion

        #region Constructors
        public ModMount(string modFolderName)
        {
            if (modFolderName.IndexOfAny(Path.GetInvalidFileNameChars()) > -1 || modFolderName.IndexOfAny(Path.GetInvalidPathChars()) > -1)
            {
                throw new ArgumentException($"Mod folder name \"{modFolderName}\" passed for {typeof(ModMount)} is not a valid folder name " +
                    $"(it contains characters illegal for paths or files)",
                    nameof(modFolderName));
            }

            ModFolderName = modFolderName;
            ObjectsByRooms = new();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds object data to corresponding room list. Handles the scenario when dictionary entry for the room is not created thus is the preferable way to add items.
        /// </summary>
        /// <param name="objectData"></param>
        public void AddObjectData(ObjectData objectData)
        {
            if (!ObjectsByRooms.ContainsKey(objectData.RoomID))
            {
                ObjectsByRooms.Add(objectData.RoomID, new());
            }

            ObjectsByRooms[objectData.RoomID].Add(objectData);
        }

        /// <summary>
        /// Helper method to generate a sequence of <see cref="ModMount"/>'s from a sequence of strings from the ROMmount file.
        /// </summary>
        /// <param name="mountRecordEntries">Sequence of newline-separated strings from the mount file.</param>
        public static IEnumerable<ModMount> CreateMountsFromMountRecords(IEnumerable<string> mountRecordEntries, ManualLogSource? logger = null)
        {
            logger?.LogInfo("Generating mod mounts from mount records.");

            ModMount? currentMount = null;

            foreach (string entry in mountRecordEntries)
            {
                if (string.IsNullOrEmpty(entry))
                {
                    logger?.LogWarning("Null or empty mount record strung encountered");
                    continue;
                }

                // If we have a new mount declaration.
                if (entry[0] == MOUNT_START_RECORD_PREFIX)
                {
                    // If we already have a mount-in-progress created, we yield it and wait for new to be created.
                    if (currentMount != null) yield return currentMount;
                    currentMount = null;

                    // Removing the mount decl prefix.
                    string newMountName = entry.Remove(0, 1);

                    // Can't have such a folder name, can't register following entries because no mount created.
                    if (string.IsNullOrWhiteSpace(newMountName))
                    {
                        logger?.LogError($"Can't create a mod mount with null or empty or whitespace-only mod folder name (encountered a mount record entry \"{entry}\"). " +
                            $"New mount not registered, its object files will be skipped.");
                        continue;
                    }

                    int invalidCharIndex = newMountName.IndexOfAny(Path.GetInvalidPathChars());
                    if (invalidCharIndex < 0) invalidCharIndex = newMountName.IndexOfAny(Path.GetInvalidFileNameChars());

                    // Can't have such a folder name, can't register following entries because no mount created.
                    if (invalidCharIndex > -1)
                    {
                        logger?.LogError($"Mount record entry \"{entry}\" contains the character {newMountName[invalidCharIndex]} which is invalid for a mount name. " +
                            $"New mount not registered, its object files will be skipped.");
                        continue;
                    }

                    // If we have a valid mount name
                    currentMount = new ModMount(newMountName);
                    continue;
                }

                if (currentMount != null)
                {
                    try
                    {
                        string objectDataFilePath = AssetManager.ResolveFilePath(Path.Combine(currentMount.ModFolderName, entry));

                        if (JsonConvert.DeserializeObject<ObjectData>(File.ReadAllText(objectDataFilePath)) is ObjectData newData)
                        {
                            currentMount.AddObjectData(newData);
                            continue;
                        }

                        // Newtonsoft.JSON returned null
                        logger?.LogError($"Could not create an object data from {entry}, json deserialization failed.");
                        continue;

                    }
                    catch (Exception ex)
                    {
                        logger?.LogError($"Exception caught while loading object data from {entry}\n" +
                        $"{ex}");
                        continue;
                    }
                }

                // else if there is no mount to attach a new object file record.
                logger?.LogError($"Can't register an object data for mount entry \"{entry}\" because there is no mount to attach it to. " +
                    $"There was no mount declaration before or the last mount declaration was invalid.");
                continue;
            }

            // If we already have a mount-in-progress created by the end of the records, we yield it and finish the mount loading.
            if (currentMount != null) yield return currentMount;
        }
        #endregion
    }
}
