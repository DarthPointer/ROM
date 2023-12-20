using Newtonsoft.Json.Linq;
using ROM.UserInteraction.InroomManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROM.RoomObjectService
{
    public interface ITypeOperator
    {
        string TypeId { get; }

        object CreateNew(Room room);

        object Load(JToken dataJson, Room room);

        JToken Save(object obj);

        void AddToRoom(object obj, Room room);

        void RemoveFromRoom(object obj, Room room);

        IEnumerable<IObjectEditorElement> GetEditorElements(object obj, Room room);
    }

    public static class TypeOperator
    {
        internal static Dictionary<string, ITypeOperator> TypeOperators { get; } = [];

        public static void RegisterType<TOperator>(TOperator typeOperator)
            where TOperator : ITypeOperator
        {
            if (TypeOperators.ContainsKey(typeOperator.TypeId))
            {
                ROMPlugin.Logger?.LogWarning($"Operator for type {typeOperator.TypeId} already is set, overwriting.");
            }

            TypeOperators[typeOperator.TypeId] = typeOperator;
        }

        public static void RegisterType<TOperator>()
            where TOperator : ITypeOperator, new()
        {
            RegisterType(new TOperator());
        }
    }

    public abstract class TypeOperator<TOBJ> : ITypeOperator
        where TOBJ : notnull
    {
        #region Properties
        public abstract string TypeId { get; }


        public abstract TOBJ CreateNew(Room room);
        object ITypeOperator.CreateNew(Room room) => CreateNew(room);

        public abstract TOBJ Load(JToken dataJson, Room room);
        object ITypeOperator.Load(JToken dataJson, Room room) => Load(dataJson, room);

        public abstract JToken Save(TOBJ obj);
        JToken ITypeOperator.Save(object obj) => Save(AssureObjectType(obj));

        public abstract void AddToRoom(TOBJ obj, Room room);
        void ITypeOperator.AddToRoom(object obj, Room room) => AddToRoom(AssureObjectType(obj), room);

        public abstract IEnumerable<IObjectEditorElement> GetEditorElements(TOBJ obj, Room room);
        IEnumerable<IObjectEditorElement> ITypeOperator.GetEditorElements(object obj, Room room) => GetEditorElements(AssureObjectType(obj), room);

        public abstract void RemoveFromRoom(TOBJ obj, Room room);
        void ITypeOperator.RemoveFromRoom(object obj, Room room) => RemoveFromRoom(AssureObjectType(obj), room);

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
