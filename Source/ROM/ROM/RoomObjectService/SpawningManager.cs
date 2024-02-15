using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;
using ROM.ObjectDataStorage;

namespace ROM.RoomObjectService
{
    internal class SpawningManager
    {
        internal static Dictionary<ObjectData, WeakReference<object>> SpawnedObjectsTracker { get; } = [];

        #region Properties
        /// <summary>
        /// The registry of all object datas to spawn.
        /// </summary>
        public ObjectRegistry ObjectRegistry
        {
            get;
            private set;
        }
        #endregion

        #region Constructors
        public SpawningManager(ObjectRegistry objectRegistry)
        {
            ObjectRegistry = objectRegistry;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Spawn objects of the room.
        /// </summary>
        /// <param name="room">The room to spawn objects in.</param>
        public void SpawnForRoom(Room room)
        {
            // sus room loaded for reasons unknown
            if (room.game == null)
                return;

            ROMPlugin.Logger?.LogInfo($"{typeof(SpawningManager)} spawns objects for room {room.abstractRoom.name}");

            foreach (ObjectData objectData in GetDatasForRoom(room))
            {
                try
                {
                    ITypeOperator typeOperator = objectData.GetTypeOperator();

                    object newObj = typeOperator.Load(objectData.DataJson, room);
                    (newObj as ICallAfterPropertiesSet)?.OnAfterPropertiesSet();

                    SpawnedObjectsTracker[objectData] = new(newObj);
                    typeOperator.AddToRoom(newObj, room);
                }
                catch (Exception ex)
                {
                    ROMPlugin.Logger?.LogError($"Exception caught while spawning {objectData.FullLogString}\n" +
                        $"{ex}");
                }
            }
        }

        /// <summary>
        /// Get all datas for the objects of the room from the registry.
        /// </summary>
        /// <param name="room">The room to get the object datas for.</param>
        /// <returns></returns>
        private IEnumerable<ObjectData> GetDatasForRoom(Room room)
        {
            string roomName = room.abstractRoom.name;
            return ObjectRegistry.ModMounts.SelectMany(mount => GetMountDatasForRoom(mount, roomName));
        }

        /// <summary>
        /// Get all datas for the objects of the room from the mod mount.
        /// </summary>
        /// <param name="modMount">The mod mount to get the datas from.</param>
        /// <param name="roomID">The roomID to search by.</param>
        /// <returns></returns>
        private static IEnumerable<ObjectData> GetMountDatasForRoom(ModMount modMount, string roomID)
        {
            if (modMount.ObjectsByRooms.TryGetValue(roomID, out List<ObjectData> mountDatas))
            {
                return mountDatas;
            }

            return Enumerable.Empty<ObjectData>();
        }
        #endregion
    }
}
