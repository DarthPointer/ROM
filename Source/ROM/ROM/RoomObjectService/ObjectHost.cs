using ROM.RoomObjectService.SpawningCondition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ROM.RoomObjectService
{
    internal class ObjectHost
    {
        #region Properties
        public WeakReference<object?> Object { get; set; } = new(null);

        public ISpawningCondition? SpawningCondition { get; set; }
        #endregion
    }
}
