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
    public static class TypeOperatorUtils
    {
        public class LoadVersion<TOBJ>(string versionId, Func<JToken, Room, TOBJ> loadCall)
            where TOBJ : notnull
        {
            public string VersionId { get; } = versionId;
            public Func<JToken, Room, TOBJ> LoadCall = loadCall;
        }

        public static VersionedLoader<TOBJ> CreateVersionedLoader<TOBJ>
            (Func<JToken, Room, TOBJ>? defaultLoad = null, IEnumerable<LoadVersion<TOBJ>> supportedVersions = null!)
            where TOBJ : notnull
        {
             return new VersionedLoader<TOBJ>(defaultLoad, supportedVersions ?? Enumerable.Empty<LoadVersion<TOBJ>>());
        }

        public static VersionedLoader<TOBJ> CreateVersionedLoader<TOBJ>
            (Func<JToken, Room, TOBJ>? defaultLoad = null, params LoadVersion<TOBJ>[] supportedVersions)
            where TOBJ : notnull
        {
            return CreateVersionedLoader(defaultLoad, supportedVersions as IEnumerable<LoadVersion<TOBJ>>);
        }

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

        public static void AddUADToRoom(UpdatableAndDeletable uad, Room room)
        {
            room.AddObject(uad);
        }

        public static void RemoveUADFromRoom(UpdatableAndDeletable uad, Room room)
        {
            room.RemoveObject(uad);
            uad.Destroy();
        }

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
