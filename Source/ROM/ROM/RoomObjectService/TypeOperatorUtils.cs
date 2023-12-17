using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROM.RoomObjectService
{
    public static class TypeOperatorUtils
    {
        internal class VersionedLoader<TOBJ>
            where TOBJ : notnull
        {
            public Func<JToken, Room, TOBJ>? DefaultLoader { get; }
            public Dictionary<string, Func<JToken, Room, TOBJ>> SupportedVersionLoaders { get; }

            public VersionedLoader(Func<JToken, Room, TOBJ>? defaultLoader, IEnumerable<LoadVersion<TOBJ>> supportedVersions)
            {
                DefaultLoader = defaultLoader;

                SupportedVersionLoaders = [];
                foreach (LoadVersion<TOBJ> version in supportedVersions)
                {
                    if (SupportedVersionLoaders.ContainsKey(version.VersionId))
                    {
                        ROMPlugin.Logger?.LogWarning($"Duplicate supported version {version.VersionId} supplied (type {typeof(TOBJ)}). Overwriting.");
                    }

                    SupportedVersionLoaders[version.VersionId] = version.LoadCall;
                }
            }

            public TOBJ Load(JToken objectData, Room room)
            {
                if (objectData.ToObject<VersionedJson>() is VersionedJson versionedJson)
                {
                    if (SupportedVersionLoaders.TryGetValue(versionedJson.VersionId, out var loader))
                    {
                        return loader(versionedJson.Data, room);
                    }

                    if (DefaultLoader != null)
                    {
                        return DefaultLoader(versionedJson.Data, room);
                    }

                    string versionNotSupportedError = $"The versioned loader provided for {typeof(TOBJ)} can not process the received {nameof(objectData)} " +
                        $"because its version {versionedJson.VersionId} is not supported and no default loader is specified.";

                    ROMPlugin.Logger?.LogError(versionNotSupportedError);
                    throw new ArgumentException(versionNotSupportedError, nameof(objectData));
                }

                string notAVersionedJsonError = $"Json data received for the creation of {typeof(TOBJ)} was not parsable as {typeof(VersionedJson)}.";

                ROMPlugin.Logger?.LogError(notAVersionedJsonError);
                throw new ArgumentException(notAVersionedJsonError, nameof(objectData));
            }
        }

        public class LoadVersion<TOBJ>(string versionId, Func<JToken, Room, TOBJ> loadCall)
            where TOBJ : notnull
        {
            public string VersionId { get; } = versionId;
            public Func<JToken, Room, TOBJ> LoadCall = loadCall;
        }

        public static Func<JToken, Room, TOBJ> GetVersionedLoadCall<TOBJ>
            (Func<JToken, Room, TOBJ>? defaultLoad = null, IEnumerable<LoadVersion<TOBJ>> supportedVersions = null!)
            where TOBJ : notnull
        {
             return new VersionedLoader<TOBJ>(defaultLoad, supportedVersions ?? Enumerable.Empty<LoadVersion<TOBJ>>()).Load;
        }

        public static Func<JToken, Room, TOBJ> GetVersionedLoadCall<TOBJ>
            (Func<JToken, Room, TOBJ>? defaultLoad = null, params LoadVersion<TOBJ>[] supportedVersions)
            where TOBJ : notnull
        {
            return GetVersionedLoadCall(defaultLoad, supportedVersions as IEnumerable<LoadVersion<TOBJ>>);
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

        public static JToken TrivialSave(UpdatableAndDeletable datableAndDeletable)
        {
            try
            {
                return JToken.FromObject(datableAndDeletable, UADContractResolver.SeiralizerInstance);
            }
            catch (JsonSerializationException ex)
            {
                string serializationExceptionError = $"A serialization exception has occurred while serializing a {datableAndDeletable.GetType()}. " +
                    $"Make sure you have provided it and its members with proper serialization attributes.";

                ROMPlugin.Logger?.LogError(serializationExceptionError + '\n' + ex);
                throw new Exception(serializationExceptionError, ex);
            }
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
