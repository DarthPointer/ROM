using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using ROM.ObjectDataStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROM.RoomObjectService
{
    /// <summary>
    /// The contract for loading room objects from configs into rooms.
    /// </summary>
    public interface ITypeCreator<out TUAD> where TUAD : UpdatableAndDeletable
    {
        /// <summary>
        /// Called when the object's room is loaded and the object should be spawned.
        /// </summary>
        /// <param name="objectDataSchema">The data schema of the object from the config.</param>
        /// <param name="room">The room object to spawn into.</param>
        TUAD Create(Room room);
    }
}
