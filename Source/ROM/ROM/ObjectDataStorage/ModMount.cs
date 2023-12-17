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
        public const char MOUNT_START_RECORD_PREFIX = '?';

        /// <summary>
        /// The instruction used in by modification patches to append strings at the end of target files.
        /// </summary>
        public const string ADD_PATCH_INSTRUCTION = "[ADD]";

        /// <summary>
        /// The character used to separate lines in strings.
        /// </summary>
        public const string NEWLINE = "\n";
        #endregion

        #region Properties
        /// <summary>
        /// The mod owning the mount. If it is not set, saving the mounted object changes will be impossible.
        /// </summary>
        public ModManager.Mod Mod
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
        public ModMount(ModManager.Mod mod)
        {
            Mod = mod;
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
            if (!ObjectsByRooms.ContainsKey(objectData.RoomId))
            {
                ObjectsByRooms.Add(objectData.RoomId, new());
            }

            ObjectsByRooms[objectData.RoomId].Add(objectData);
        }

        public string GenerateMountModifyFileString()
        {
            if (Mod == null)
            {
                string noModErrorMessage = $"Can not generate ";

                ROMPlugin.Logger?.LogError(noModErrorMessage);
                throw new InvalidOperationException(noModErrorMessage);
            }

            return string.Join(NEWLINE,
                ObjectsByRooms.Values.SelectMany(roomObjects => roomObjects).Select(obj => obj.FilePath).
                Prepend(MOUNT_START_RECORD_PREFIX + Mod.id).
                Select(str => ADD_PATCH_INSTRUCTION + str));
        }

        /// <summary>
        /// Helper method to generate a sequence of <see cref="ModMount"/>'s from a sequence of strings from the ROMmount file.
        /// </summary>
        /// <param name="mountRecordEntries">Sequence of newline-separated strings from the mount file.</param>
        public static IEnumerable<ModMount> CreateMountsFromMountRecords(IEnumerable<string> mountRecordEntries)
        {
            ROMPlugin.Logger?.LogInfo("Generating mod mounts from mount records.");

            ModMount? currentMount = null;

            foreach (string entry in mountRecordEntries)
            {
                if (string.IsNullOrEmpty(entry))
                {
                    ROMPlugin.Logger?.LogWarning("Null or empty mount record string encountered");
                    continue;
                }

                // If we have a new mount declaration.
                if (entry[0] == MOUNT_START_RECORD_PREFIX)
                {
                    // If we already have a mount-in-progress created, we yield it and wait for new to be created.
                    if (currentMount != null) yield return currentMount;
                    currentMount = null;

                    // Removing the mount decl prefix.
                    string newMountModtId = entry.Remove(0, 1);

                    if (string.IsNullOrEmpty(newMountModtId))
                    {
                        ROMPlugin.Logger?.LogError($"Can't create a mod mount with null or empty mod ID.");
                        continue;
                    }

                    // If we have a valid mount mod ID
                    ModManager.Mod? newMountMod = GetActiveModById(newMountModtId);

                    if (newMountMod == null)
                    {
                        ROMPlugin.Logger?.LogError($"No active mod found for a mount by id {newMountModtId}. " +
                            $"Objects of this mount will not be loaded.");
                        continue;
                    }

                    currentMount = new ModMount(newMountMod);
                    continue;
                }

                if (currentMount != null)
                {
                    try
                    {
                        string objectDataFilePath = AssetManager.ResolveFilePath(Path.Combine(currentMount.Mod.id, entry));

                        if (JsonConvert.DeserializeObject<ObjectData>(File.ReadAllText(objectDataFilePath)) is ObjectData newData)
                        {
                            newData.FilePath = entry;
                            newData.Mod = currentMount.Mod;
                            currentMount.AddObjectData(newData);
                            continue;
                        }

                        // Newtonsoft.JSON returned null
                        ROMPlugin.Logger?.LogError($"Could not create an object data from {entry}, json deserialization failed.");
                        continue;

                    }
                    catch (Exception ex)
                    {
                        ROMPlugin.Logger?.LogError($"Exception caught while loading object data from {entry}\n" +
                        $"{ex}");
                        continue;
                    }
                }

                // else if there is no mount to attach a new object file record.
                ROMPlugin.Logger?.LogError($"Can't register an object data for mount entry \"{entry}\" because there is no mount to attach it to. " +
                    $"There was no mount declaration before or the last mount declaration was invalid.");
                continue;
            }

            // If we already have a mount-in-progress created by the end of the records, we yield it and finish the mount loading.
            if (currentMount != null) yield return currentMount;
        }

        private static ModManager.Mod? GetActiveModById(string id)
        {
            return ModManager.ActiveMods.FirstOrDefault(mod => mod.id == id);
        }
        #endregion
    }
}
