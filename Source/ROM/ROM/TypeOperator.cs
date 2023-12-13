using ROM.SpawningService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROM
{
    public class TypeOperator(string typeId, ITypeSpawner spawner)
    {
        public static Dictionary<string, TypeOperator> TypeOperators { get; } = [];

        #region Properties
        public string TypeId { get; } = typeId;

        public ITypeSpawner Spawner { get; } = spawner;
        #endregion
    }
}
