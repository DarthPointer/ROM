using BepInEx.Logging;
using ROM.ObjectDataStorage;
using System;
using System.Collections.Generic;
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

        #region Properties
        public ModMount ModMount
        {
            get;
            private set;
        }

        private ModManager.Mod? SourceMod
        {
            get;
            set;
        }

        private ManualLogSource? Logger
        {
            get;
            set;
        }

        public bool CanWrite => SourceMod?.workshopMod == false;
        #endregion

        #region Constructors
        public ModMountController(ModMount modMount, ManualLogSource? logger = null)
        {
            Logger = logger;

            ModMount = modMount;

            SourceMod = ModManager.ActiveMods.FirstOrDefault(mod => mod.id == modMount.ModId);

            if (SourceMod == null)
            {
                Logger?.LogWarning($"ModMount had {nameof(modMount.ModId)} \"{modMount.ModId}\", but no active mod with such an ID was found. " +
                    $"Editing the objects of this mount will be impossible.");
            }
        }
        #endregion

        #region Methods
        public void SaveMountFile()
        {
            if (SourceMod == null)
            {
                throw new InvalidOperationException($"{nameof(SaveMountFile)} is called for the controller of the \"{ModMount.ModId}\" mod mount, but its mod was null. " +
                    $"This likely is caused by Mod IDs specified in the ROM Mount and modinfo being different.");
            }

            if (SourceMod.workshopMod == true)
            {
                throw new InvalidOperationException($"{nameof(SaveMountFile)} is called for the controller of the \"{ModMount.ModId}\" mod mount, " +
                    $"but its mod is a downloaded workshop mod. Saving mounts and objects of workshop mods is not allowed.");
            }

            try
            {
                Directory.CreateDirectory(Path.Combine(SourceMod.path, MODIFY_FOLDER));

                File.WriteAllText(Path.Combine(SourceMod.path, MODIFY_FOLDER, ObjectRegistry.ROM_MOUNT_FILE_ASSET_PATH),
                    ModMount.GenerateMountModifyFileString());
            }
            catch (Exception ex)
            {
                Logger?.LogError($"Exception caught while attempting to save mount {ModMount.ModId}.\n{ex}");

                throw;
            }
        }
        #endregion
    }
}
