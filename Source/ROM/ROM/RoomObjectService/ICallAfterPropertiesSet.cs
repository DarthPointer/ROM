using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROM.RoomObjectService
{
    /// <summary>
    /// Implement this interface with your object if you want to receive an automatic call after your object got all properties set.
    /// Mostly useful for objects that are loaded with default ROM-supplied methods that rely on using trivial ctor() for constructing the instances.
    /// </summary>
    public interface ICallAfterPropertiesSet
    {
        /// <summary>
        /// The method that ROM will call after creating (<see cref="ITypeOperator.CreateNew(Room)"/>)
        /// or loading (<see cref="ITypeOperator.Load(Newtonsoft.Json.Linq.JToken, Room)"/>) the object,
        /// but before <see cref="ITypeOperator.AddToRoom(object, Room)"/>. You can implement it to use like a constructor.
        /// </summary>
        void OnAfterPropertiesSet();
    }
}
