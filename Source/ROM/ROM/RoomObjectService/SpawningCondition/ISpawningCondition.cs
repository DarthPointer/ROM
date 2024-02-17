using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROM.RoomObjectService.SpawningCondition
{
    public interface ISpawningCondition
    {
        bool ShouldSpawn(Room room);
    }
}
