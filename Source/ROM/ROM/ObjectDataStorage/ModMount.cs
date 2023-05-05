using System;
using System.Collections.Generic;
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
    }
}
