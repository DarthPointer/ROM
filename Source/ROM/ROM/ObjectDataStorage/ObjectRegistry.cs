using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROM.ObjectDataStorage
{
    /// <summary>
    /// The registry of all the object spawners.
    /// </summary>
    internal class ObjectRegistry
    {
        #region Constants
        public const string ROM_MOUNT_FILE_ASSET_PATH = "ROMmount.txt";
        #endregion

        #region Properties
        /// <summary>
        /// The list of all mod mounts.
        /// </summary>
        public List<ModMount> ModMounts
        {
            get;
            private set;
        }
        #endregion

        #region Constructors
        public ObjectRegistry(IEnumerable<ModMount> modMounts)
        {
            ModMounts = modMounts.ToList();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Helper method to create an <see cref="ObjectRegistry"/> from a sequence of strings from the ROMmount file.
        /// </summary>
        /// <param name="mountRecordEntries">Sequence of newline-separated strings from the mount file.</param>
        public static ObjectRegistry CreateRegistryFromMountRecords(IEnumerable<string> mountRecordEntries, ManualLogSource? logger = null)
        {
            var result = new ObjectRegistry(ModMount.CreateMountsFromMountRecords(mountRecordEntries, logger));

            logger?.LogInfo($"Object registry created. {result.ModMounts.Count} mod mount(s) loaded.");
            if (logger != null)
            {
                foreach (ModMount modMount in result.ModMounts)
                {
                    logger.LogInfo($"Mount \"{modMount.ModId}\": {modMount.ObjectsByRooms.Values.Sum(objList => objList.Count)} objects");
                }
            }

            return result;
        }

        /// <summary>
        /// Read specified asset file as ROMmount and create an <see cref="ObjectRegistry"/> from it.
        /// </summary>
        /// <param name="fileAssetPath"></param>
        /// <returns></returns>
        public static ObjectRegistry CreateRegistryFromMountFileAsset(string fileAssetPath, ManualLogSource? logger = null)
        {
            try
            {
                string mountFilePath = AssetManager.ResolveFilePath(fileAssetPath);
                return CreateRegistryFromMountRecords(File.ReadAllLines(mountFilePath), logger);
            }
            catch (Exception ex)
            {
                logger?.LogError($"Exception caught while creating an object registry from mount file.\n" +
                    $"{ex}");

                throw;
            }
        }
        #endregion
    }
}
