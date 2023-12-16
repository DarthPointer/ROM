using BepInEx.Logging;
using Newtonsoft.Json.Linq;
using ROM.ObjectDataStorage;
using ROM.RoomObjectService;
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

        private ModManager.Mod? SourceMod => ModMount.Mod;

        public bool CanWrite => SourceMod?.workshopMod == false;
        #endregion

        #region Constructors
        public ModMountController(ModMount modMount)
        {
            ModMount = modMount;

            if (SourceMod == null)
            {
                ROMPlugin.Logger?.LogWarning($"ModMount has no mod assigned. " +
                    $"Saving the changes of objects of this mount will be impossible.");
            }
        }
        #endregion

        #region Methods
        public void SaveMountFile()
        {
            if (SourceMod == null)
            {
                throw new InvalidOperationException($"{nameof(SaveMountFile)} is called for a controller with no mod assigned. " +
                    $"This likely is caused by Mod IDs specified in the ROM Mount and modinfo being different.");
            }

            if (SourceMod.workshopMod == true)
            {
                throw new InvalidOperationException($"{nameof(SaveMountFile)} is called for the controller of the \"{SourceMod.id}\" mod mount, " +
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
                ROMPlugin.Logger?.LogError($"Exception caught while attempting to save mount {SourceMod.id}.\n{ex}");

                throw;
            }
        }
        #endregion
    }
}
