using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROM.ObjectDataStorage
{
    /// <summary>
    /// The wrap class to store one specific object data.
    /// </summary>
    internal class ObjectData
    {
        #region Properties
        /// <summary>
        /// Asset FilePath of the object.
        /// </summary>
        public string FilePath
        {
            get;
            private set;
        }

        /// <summary>
        /// The StreamingAssets/mods folder name of the mod this object comes from.
        /// </summary>
        public string ModFolderName
        {
            get;
            private set;
        }

        /// <summary>
        /// RoomID of the room for the object to be spawned in.
        /// </summary>
        public string RoomID
        {
            get;
            private set;
        }

        /// <summary>
        /// The ID of the object type to be spawned.
        /// </summary>
        public string TypeID
        {
            get;
            private set;
        }

        public string FullLogString
        {
            get
            {
                return $"{FilePath} from mod {ModFolderName} of type {TypeID} for room {RoomID}";
            }
        }

        /// <summary>
        /// The embedded schema that stores all the custom object data.
        /// </summary>
        public JsonSchema ObjectDataSchema
        {
            get;
            private set;
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return FilePath;
        }
        #endregion
    }
}
