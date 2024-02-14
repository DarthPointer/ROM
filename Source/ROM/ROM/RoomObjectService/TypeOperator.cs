using Newtonsoft.Json.Linq;
using ROM.UserInteraction.InroomManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ROM.RoomObjectService
{
    /// <summary>
    /// The interface for all type operators - "factories" and "maintainers" of the room objects.
    /// This interface is non-generic and exposes methods with <see cref="object"/> as return or argument type.
    /// Please use <see cref="TypeOperator{TOBJ}"/> as the base class for your operators instead of this interface,
    /// unless you really know what you are doing.
    /// </summary>
    public interface ITypeOperator
    {
        /// <summary>
        /// The id string the type is associated with. Must be unique across different operators.
        /// </summary>
        string TypeId { get; }

        /// <summary>
        /// A factory call to create a brand new object with ROM UI.
        /// </summary>
        /// <param name="room">The room the object is created for.</param>
        /// <param name="currentCameraRect">Use the current camera rect to set initial level positions in a friendly manner (within the camera).</param>
        /// <returns>The created object.</returns>
        object CreateNew(Room room, Rect currentCameraRect);

        /// <summary>
        /// A factory call to load the object from its JSON.
        /// </summary>
        /// <param name="dataJson">JSON tree containing object data previously saved.</param>
        /// <param name="room">The room the object is loaded for.</param>
        /// <returns>The loaded object.</returns>
        object Load(JToken dataJson, Room room);

        /// <summary>
        /// The call to create a JSON tree containing the object's data.
        /// </summary>
        /// <param name="obj">The room object to save.</param>
        /// <returns>The JSON tree containing the object's data.</returns>
        JToken Save(object obj);

        /// <summary>
        /// The call invoked after the object is Created (<see cref="CreateNew(Room)"/>) or Loaded (<see cref="Load(JToken, Room)"/>).
        /// Useful for common initialization logic that has to be run after both creating or loading an object.
        /// </summary>
        /// <param name="obj">The object created or loaded.</param>
        /// <param name="room">The object's room.</param>
        void AddToRoom(object obj, Room room);

        /// <summary>
        /// The call to remove the object from the room, invoked when the object is deleted with ROM UI.
        /// NOTE: This is NOT called for objects when the room is unloaded into abstract state. This call is specifically used to
        /// clean the room up from the object and its effects when the object is deleted via ROM UI.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="room"></param>
        void RemoveFromRoom(object obj, Room room);

        /// <summary>
        /// The method to emit editor elements linked to edit the object.
        /// </summary>
        /// <param name="obj">The object to edit.</param>
        /// <param name="room">The object's room.</param>
        /// <returns>Instances of <see cref="IObjectEditorElement"/> that edit the <paramref name="obj"/></returns>
        IEnumerable<IObjectEditorElement> GetEditorElements(object obj, Room room);
    }

    public static class TypeOperator
    {
        internal static Dictionary<string, ITypeOperator> TypeOperators { get; } = [];

        /// <summary>
        /// Registers a type operator, uses <see cref="ITypeOperator.TypeId"/> as the key to associate it with.
        /// The keys must be unique across different type operators. If there already is an operator registered with same TypeId,
        /// a warning will be logged and the previous operator will be overwritten. If that is the case, make sure you don't register
        /// your type twice. If the error is logged and you are sure you only register the type once, then there is another mod that
        /// registers its type with the colliding TypeId.
        /// </summary>
        /// <param name="typeOperator">The operator of the type to register.</param>
        public static void RegisterType(ITypeOperator typeOperator)
        {
            if (TypeOperators.ContainsKey(typeOperator.TypeId))
            {
                ROMPlugin.Logger?.LogWarning($"Operator for type {typeOperator.TypeId} already is set, overwriting.");
            }

            TypeOperators[typeOperator.TypeId] = typeOperator;
        }

        /// <summary>
        /// Registers a type operator, uses <see cref="ITypeOperator.TypeId"/> as the key to associate it with.
        /// The keys must be unique across different type operators. If there already is an operator registered with same TypeId,
        /// a warning will be logged and the previous operator will be overwritten. If that is the case, make sure you don't register
        /// your type twice. If the error is logged and you are sure you only register the type once, then there is another mod that
        /// registers its type with the colliding TypeId.
        /// This method is a shortcut for type operators that don't need any special setup and can be created with a trivial ctor.
        /// </summary>
        /// <typeparam name="TOperator">The type of the operator.</typeparam>
        public static void RegisterType<TOperator>()
            where TOperator : ITypeOperator, new()
        {
            RegisterType(new TOperator());
        }
    }

    /// <summary>
    /// The generic typed wrap for <see cref="ITypeOperator"/>. Using this class as base for type operator assures that the objects it creates
    /// and receives are of correct type, given there is no <see cref="TypeId"/> collision with another operator.
    /// </summary>
    /// <typeparam name="TOBJ">The type of the room objects created and maintained by the operator.</typeparam>
    public abstract class TypeOperator<TOBJ> : ITypeOperator
        where TOBJ : notnull
    {
        #region Properties
        /// <summary>
        /// The id string the type is associated with. Must be unique across different operators.
        /// </summary>
        public abstract string TypeId { get; }

        /// <summary>
        /// A factory call to create a brand new object with ROM UI.
        /// </summary>
        /// <param name="room">The room the object is created for.</param>
        /// <param name="currentCameraRect">Use the current camera rect to set initial level positions in a friendly manner (within the camera).</param>
        /// <returns>The created object.</returns>
        public abstract TOBJ CreateNew(Room room, Rect currentCameraRect);
        object ITypeOperator.CreateNew(Room room, Rect currentCameraRect) => CreateNew(room, currentCameraRect);

        /// <summary>
        /// A factory call to load the object from its JSON.
        /// </summary>
        /// <param name="dataJson">JSON tree containing object data previously saved.</param>
        /// <param name="room">The room the object is loaded for.</param>
        /// <returns>The loaded object.</returns>
        public abstract TOBJ Load(JToken dataJson, Room room);
        object ITypeOperator.Load(JToken dataJson, Room room) => Load(dataJson, room);

        /// <summary>
        /// The call to create a JSON tree containing the object's data.
        /// </summary>
        /// <param name="obj">The room object to save.</param>
        /// <returns>The JSON tree containing the object's data.</returns>
        public abstract JToken Save(TOBJ obj);
        JToken ITypeOperator.Save(object obj) => Save(AssureObjectType(obj));

        /// <summary>
        /// The call invoked after the object is Created (<see cref="CreateNew(Room)"/>) or Loaded (<see cref="Load(JToken, Room)"/>).
        /// Useful for common initialization logic that has to be run after both creating or loading an object.
        /// </summary>
        /// <param name="obj">The object created or loaded.</param>
        /// <param name="room">The object's room.</param>
        public abstract void AddToRoom(TOBJ obj, Room room);
        void ITypeOperator.AddToRoom(object obj, Room room) => AddToRoom(AssureObjectType(obj), room);

        /// <summary>
        /// The method to emit editor elements linked to edit the object.
        /// </summary>
        /// <param name="obj">The object to edit.</param>
        /// <param name="room">The object's room.</param>
        /// <returns>Instances of <see cref="IObjectEditorElement"/> that edit the <paramref name="obj"/></returns>
        public abstract IEnumerable<IObjectEditorElement> GetEditorElements(TOBJ obj, Room room);
        IEnumerable<IObjectEditorElement> ITypeOperator.GetEditorElements(object obj, Room room) => GetEditorElements(AssureObjectType(obj), room);

        /// <summary>
        /// The call to remove the object from the room, invoked when the object is deleted with ROM UI.
        /// NOTE: This is NOT called for objects when the room is unloaded into abstract state. This call is specifically used to
        /// clean the room up from the object and its effects when the object is deleted via ROM UI.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="room"></param>
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
