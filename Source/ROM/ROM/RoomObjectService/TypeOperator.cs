using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROM.RoomObjectService
{
    internal interface ITypeOperator
    {
        string TypeId { get; }

        UpdatableAndDeletable CreateNew(Room room);

        UpdatableAndDeletable Load(JToken dataJson, Room room);

        JToken Save(UpdatableAndDeletable obj);
    }

    public static class TypeOperator
    {
        internal static Dictionary<string, ITypeOperator> TypeOperators { get; } = [];

        public static void RegisterType<TUAD>(string typeId, Func<Room, TUAD> createNewCall, Func<JToken, Room, TUAD> loadCall, Func<TUAD, JToken> saveCall)
            where TUAD : UpdatableAndDeletable
        {
            if (TypeOperators.ContainsKey(typeId))
            {
                ROMPlugin.Logger?.LogWarning($"Operator for type {typeId} already is set, overwriting.");
            }

            TypeOperators[typeId] = new TypeOperator<TUAD>(typeId, createNewCall, loadCall, saveCall);
        }
    }

    internal class TypeOperator<TUAD>(string typeId, Func<Room, TUAD> createNewCall, Func<JToken, Room, TUAD> loadCall, Func<TUAD, JToken> saveCall)
        : ITypeOperator
        where TUAD : UpdatableAndDeletable
    {
        #region Properties
        public string TypeId { get; } = typeId;

        
        private Func<Room, TUAD> CreateNewCall { get; } = createNewCall;
        UpdatableAndDeletable ITypeOperator.CreateNew(Room room) => CreateNewCall(room);

        private Func<JToken, Room, TUAD> LoadCall { get; } = loadCall;
        UpdatableAndDeletable ITypeOperator.Load(JToken dataJson, Room room) => LoadCall(dataJson, room);

        private Func<TUAD, JToken> SaveCall { get; } = saveCall;
        JToken ITypeOperator.Save(UpdatableAndDeletable obj)
        {
            if (obj is TUAD tuad)
            {
                return SaveCall(tuad);
            }

            throw new ArgumentException($"The object to save was of wrong type {obj.GetType()} while a instance of {typeof(TUAD)} is required", nameof(obj));
        }
        #endregion
    }
}
