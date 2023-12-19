using Newtonsoft.Json.Linq;
using ROM.UserInteraction.InroomManagement;
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

        object CreateNew(Room room);

        object Load(JToken dataJson, Room room);

        void AddToRoom(object obj, Room room);

        IEnumerable<IObjectEditorElement> GetEditorElements(object obj, Room room);

        void RemoveFromRoom(object obj, Room room);

        JToken Save(object obj);
    }

    public static class TypeOperator
    {
        internal static Dictionary<string, ITypeOperator> TypeOperators { get; } = [];

        public static void RegisterType<TOBJ>(string typeId,
            Func<Room, TOBJ> createNewCall, Func<JToken, Room, TOBJ> loadCall, Action<TOBJ, Room> addToRoomCall,
            Func<TOBJ, Room, IEnumerable<IObjectEditorElement>> getEditorElementsCall,
            Func<TOBJ, JToken> saveCall, Action<TOBJ, Room> removeFromRoomCall)
            where TOBJ : notnull
        {
            if (TypeOperators.ContainsKey(typeId))
            {
                ROMPlugin.Logger?.LogWarning($"Operator for type {typeId} already is set, overwriting.");
            }

            TypeOperators[typeId] = new TypeOperator<TOBJ>(typeId, createNewCall, loadCall, addToRoomCall, getEditorElementsCall, saveCall, removeFromRoomCall);
        }

        public static void RegisterType<TUAD>(string typeId,
            Func<Room, TUAD> createNewCall, Func<JToken, Room, TUAD> loadCall,
            Func<TUAD, Room, IEnumerable<IObjectEditorElement>> getEditorElementsCall,
            Func<TUAD, JToken> saveCall)
            where TUAD : UpdatableAndDeletable
        {
            RegisterType(typeId, createNewCall, loadCall, TypeOperatorUtils.AddUADToRoom, getEditorElementsCall, saveCall, TypeOperatorUtils.RemoveUADFromRoom);
        }
    }

    internal class TypeOperator<TOBJ>(string typeId,
        Func<Room, TOBJ> createNewCall, Func<JToken, Room, TOBJ> loadCall, Action<TOBJ, Room> addToRoomCall,
        Func<TOBJ, Room, IEnumerable<IObjectEditorElement>> getEditorElementsCall,
        Func<TOBJ, JToken> saveCall, Action<TOBJ, Room> removeFromRoomCall)
        : ITypeOperator
        where TOBJ : notnull
    {
        #region Properties
        public string TypeId { get; } = typeId;

        
        private Func<Room, TOBJ> CreateNewCall { get; } = createNewCall;
        object ITypeOperator.CreateNew(Room room) => CreateNewCall(room);

        private Func<JToken, Room, TOBJ> LoadCall { get; } = loadCall;
        object ITypeOperator.Load(JToken dataJson, Room room) => LoadCall(dataJson, room);

        private Action<TOBJ, Room> AddToRoomCall { get; } = addToRoomCall;
        void ITypeOperator.AddToRoom(object obj, Room room) => AddToRoomCall(AssureObjectType(obj), room);

        private Func<TOBJ, JToken> SaveCall { get; } = saveCall;
        JToken ITypeOperator.Save(object obj) => SaveCall(AssureObjectType(obj));

        private Func<TOBJ, Room, IEnumerable<IObjectEditorElement>> GetEditorElementsCall { get; } = getEditorElementsCall;
        IEnumerable<IObjectEditorElement> ITypeOperator.GetEditorElements(object obj, Room room) => GetEditorElementsCall(AssureObjectType(obj), room);

        private Action<TOBJ, Room> RemoveFromRoomCall { get; } = removeFromRoomCall;
        void ITypeOperator.RemoveFromRoom(object obj, Room room) => RemoveFromRoomCall(AssureObjectType(obj), room);

        private TOBJ AssureObjectType(object obj)
        {
            if (obj is TOBJ tobj)
            {
                return tobj;
            }

            throw new ArgumentException($"The object to save was of wrong type {obj.GetType()} while an instance of {typeof(TOBJ)} is required", nameof(obj));
        }
        #endregion
    }
}
