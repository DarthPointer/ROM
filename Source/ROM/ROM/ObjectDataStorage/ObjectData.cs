using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROM.ObjectDataStorage
{
    /// <summary>
    /// The wrap class to store one specific object data.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptOut)]
    internal class ObjectData
    {
        #region Properties
        /// <summary>
        /// Asset FilePath of the object.
        /// </summary>
        [JsonIgnore]
        public string FilePath
        {
            get;
            set;
        } = "";

        /// <summary>
        /// The mod owning the mount file that includes this object.
        /// </summary>
        [JsonIgnore]
        public ModManager.Mod? Mod
        {
            get;
            set;
        }

        /// <summary>
        /// RoomID of the room for the object to be spawned in.
        /// </summary>
        public string RoomId
        {
            get;
            set;
        } = "";

        /// <summary>
        /// The ID of the object type to be spawned.
        /// </summary>
        public string TypeId
        {
            get;
            set;
        } = "";

        [JsonIgnore]
        public string FullLogString
        {
            get
            {
                return $"{FilePath} from mod {Mod?.id ?? "MOD NOT SET"} of type {TypeId} for room {RoomId}";
            }
        }

        /// <summary>
        /// The embedded object that stores all the custom object data.
        /// </summary>
        public JToken DataJson
        {
            get;
            set;
        } = new JObject();
        #endregion

        #region Methods
        public override string ToString()
        {
            return FilePath;
        }

        public void Save()
        {
            if (Mod == null)
            {
                ROMPlugin.Logger?.LogError($"Can not save {FullLogString} because its mod is not set.");
                return;
            }

            if (Mod.workshopMod)
            {
                ROMPlugin.Logger?.LogError($"Can not save {FullLogString} because its mod is downloaded from the workshop.");
                return;
            }

            string str = JsonConvert.SerializeObject(this, Formatting.Indented);

            string saveFilePath = GetPrimarySourceFilePath();
            Directory.CreateDirectory(Path.GetDirectoryName(saveFilePath));
            File.WriteAllText(saveFilePath, str);
        }

        public string GetPrimarySourceFilePath()
        {
            if (Mod == null)
            {
                string noModErrorMessage = $"Can not get file path for {FullLogString} because its mod is not set.";

                ROMPlugin.Logger?.LogError(noModErrorMessage);
                throw new InvalidOperationException(noModErrorMessage);
            }

            return GetPrimarySourceFilePath(Mod, FilePath);
        }

        public static string GetPrimarySourceFilePath(ModManager.Mod mod, string objectFilePath)
        {
            return Path.Combine(mod.path, mod.id, objectFilePath);
        }
        #endregion
    }
}
