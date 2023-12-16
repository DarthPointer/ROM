using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROM.RoomObjectService
{
    /// <summary>
    /// Implement this interface with your UAD if you want to receive an automatic call after your UAD got all properties set.
    /// Mostly useful for UADs that are loaded with default ROM-supplied methods that rely on using trivial ctor() for constructing the instances.
    /// </summary>
    public interface ICallAfterPropertiesSet
    {
        /// <summary>
        /// The method that ROM will call after creating or loading the UAD. You can implement it to use like the constructor.
        /// </summary>
        void OnAfterPropertiesSet();
    }
}
