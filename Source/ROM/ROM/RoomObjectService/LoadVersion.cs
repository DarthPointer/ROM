using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROM.RoomObjectService
{
    /// <summary>
    /// A factory to load objects from JSON trees of a specific version.
    /// </summary>
    /// <typeparam name="TOBJ">The type of the loaded objects.</typeparam>
    /// <param name="versionId">The id of the version to use this loader for.</param>
    /// <param name="loadCall">The function to create objects from JSON trees of the version.</param>
    public class LoadVersion<TOBJ>(string versionId, Func<JToken, Room, TOBJ> loadCall)
        where TOBJ : notnull
    {
        public string VersionId { get; } = versionId;
        public Func<JToken, Room, TOBJ> LoadCall = loadCall;
    }
}
