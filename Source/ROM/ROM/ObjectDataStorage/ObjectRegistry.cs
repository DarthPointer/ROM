using System;
using System.Collections.Generic;
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
    }
}
