using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROM.RoomObjectService
{
    /// <summary>
    /// A set of generic shortcuts for object handling.
    /// </summary>
    public static class TypeOperatorUtils
    {
        /// <summary>
        /// A shortcut to create a <see cref="VersionedLoader{TOBJ}"/>.
        /// </summary>
        /// <typeparam name="TOBJ"></typeparam>
        /// <param name="defaultLoad"></param>
        /// <param name="supportedVersions"></param>
        /// <returns></returns>
        public static VersionedLoader<TOBJ> CreateVersionedLoader<TOBJ>
            (Func<JToken, Room, TOBJ>? defaultLoad = null, IEnumerable<LoadVersion<TOBJ>> supportedVersions = null!)
            where TOBJ : notnull
        {
             return new VersionedLoader<TOBJ>(defaultLoad, supportedVersions ?? Enumerable.Empty<LoadVersion<TOBJ>>());
        }

        /// <summary>
        /// A shortcut to create a <see cref="VersionedLoader{TOBJ}"/>.
        /// </summary>
        /// <typeparam name="TOBJ"></typeparam>
        /// <param name="defaultLoad"></param>
        /// <param name="supportedVersions"></param>
        /// <returns></returns>
        public static VersionedLoader<TOBJ> CreateVersionedLoader<TOBJ>
            (Func<JToken, Room, TOBJ>? defaultLoad = null, params LoadVersion<TOBJ>[] supportedVersions)
            where TOBJ : notnull
        {
            return CreateVersionedLoader(defaultLoad, supportedVersions as IEnumerable<LoadVersion<TOBJ>>);
        }

        /// <summary>
        /// The default conversion of JSON tree into an object.
        /// </summary>
        /// <typeparam name="TOBJ">The type of the object.</typeparam>
        /// <param name="data">The JSON tree.</param>
        /// <param name="room">The object's room.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static TOBJ TrivialLoad<TOBJ>(JToken data, Room room)
            where TOBJ : notnull, new()
        {
            if (data.ToObject<TOBJ>() is TOBJ obj)
            {
                if (obj is UpdatableAndDeletable uad)
                {
                    uad.room = room;
                }

                return obj;
            }

            string notParsedAsObjectError = $"Failed to parse the received {nameof(data)} as {nameof(TOBJ)}.";

            ROMPlugin.Logger?.LogError(notParsedAsObjectError);
            throw new ArgumentException(notParsedAsObjectError, nameof(data));
        }

        /// <summary>
        /// The default conversion of an object to JSON tree. Does not serialize fields and properties declared in
        /// <see cref="UpdatableAndDeletable"/> to prevent dependency loops. Using this method requires
        /// setting correct serialization attributes to avoid reference loops and other serialization errors.
        /// </summary>
        /// <param name="obj">The object to save into a JSON tree.</param>
        /// <returns>A JSON tree with object's data.</returns>
        public static JToken TrivialSave(object obj)
        {
            try
            {
                if (obj is UpdatableAndDeletable uad)
                {
                    return JToken.FromObject(uad, UADContractResolver.SeiralizerInstance);
                }
                return JToken.FromObject(obj);
            }
            catch (JsonSerializationException ex)
            {
                string serializationExceptionError = $"A serialization exception has occurred while serializing a {obj.GetType()}. " +
                    $"Make sure you have provided it and its members with proper serialization attributes.";

                ROMPlugin.Logger?.LogError(serializationExceptionError + '\n' + ex);
                throw new Exception(serializationExceptionError, ex);
            }
        }

        /// <summary>
        /// A wrap to use <see cref="TrivialSave(object)"/> and return a versioned JSON.
        /// </summary>
        /// <typeparam name="TOBJ">The object to save into a JSON.</typeparam>
        /// <param name="versionId">A JSON tree of <see cref="VersionedJson"/>.</param>
        /// <returns></returns>
        public static Func<TOBJ, JToken> GetTrivialVersionedSaveCall<TOBJ>(string versionId)
            where TOBJ : notnull
        {
            return obj => GetTrivialVersionedSave(obj, versionId);
        }

        private static JToken GetTrivialVersionedSave(object obj, string versionId)
        {
            return JToken.FromObject(new VersionedJson
            {
                VersionId = versionId,
                Data = TrivialSave(obj)
            });
        }

        private class UADContractResolver : DefaultContractResolver
        {
            internal static JsonSerializer SeiralizerInstance { get; } =
                new()
                {
                    ContractResolver = new UADContractResolver()
                };

            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                IList<JsonProperty> baseResult = base.CreateProperties(type, memberSerialization);
                return baseResult.Where(AcceptUADProperty).ToList();
            }

            private static bool AcceptUADProperty(JsonProperty property)
            {
                return property.DeclaringType != typeof(UpdatableAndDeletable);
            }
        }
    }
}
