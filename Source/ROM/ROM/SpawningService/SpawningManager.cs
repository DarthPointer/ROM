using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;
using ROM.ObjectDataStorage;

namespace ROM.SpawningService
{
    /// <summary>
    /// The object to aggregate object spawners by type and call them for to spawn room objects.
    /// </summary>
    internal class SpawningManager
    {
        #region Properties
        /// <summary>
        /// The collection of spawners, by type ID.
        /// </summary>
        public Dictionary<string, ITypeSpawner> TypeSpawners
        {
            get;
            private set;
        }
        
        /// <summary>
        /// The registry of all object datas to spawn.
        /// </summary>
        public ObjectRegistry ObjectRegistry
        {
            get;
            private set;
        }

        private ManualLogSource? Logger
        {
            get;
            set;
        }
        #endregion

        #region Constructors
        public SpawningManager(ObjectRegistry objectRegistry, ManualLogSource? logger = null)
        {
            TypeSpawners = new();
            ObjectRegistry = objectRegistry;
            Logger = logger;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Spawn objects of the room.
        /// </summary>
        /// <param name="room">The room to spawn objects in.</param>
        public void SpawnForRoom(Room room)
        {
            Logger?.LogInfo($"{typeof(SpawningManager)} spawns objects for room {room.abstractRoom.name}");

            foreach (ObjectData objectData in GetDatasForRoom(room))
            {
                try
                {
                    if (TypeSpawners.TryGetValue(objectData.TypeID, out var typeSpawner))
                    {
                        typeSpawner.Spawn(objectData.DataJson, room);
                    }
                    else
                    {
                        Logger?.LogError(
                            $"Could not spawn object {objectData.FullLogString} because there is no spawner registered for type {objectData.TypeID}.");
                    }
                }
                catch (Exception ex)
                {
                    Logger?.LogError($"Exception caught while spawning {objectData.FullLogString}\n" +
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
