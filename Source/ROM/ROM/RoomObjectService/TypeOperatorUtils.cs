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
        internal class VersionedLoader<TUAD>
            where TUAD : UpdatableAndDeletable
        {
            public Func<JToken, Room, TUAD>? DefaultLoader { get; }
            public Dictionary<string, Func<JToken, Room, TUAD>> SupportedVersionLoaders { get; }

            public VersionedLoader(Func<JToken, Room, TUAD>? defaultLoader, IEnumerable<LoadVersion<TUAD>> supportedVersions)
            {
                DefaultLoader = defaultLoader;

                SupportedVersionLoaders = [];
                foreach (LoadVersion<TUAD> version in supportedVersions)
                {
                    if (SupportedVersionLoaders.ContainsKey(version.VersionId))
                    {
                        ROMPlugin.Logger?.LogWarning($"Duplicate supported version {version.VersionId} supplied (UAD type {typeof(TUAD)}). Overwriting.");
                    }

                    SupportedVersionLoaders[version.VersionId] = version.LoadCall;
                }
            }

            public TUAD Load(JToken objectData, Room room)
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

                    string versionNotSupportedError = $"The versioned loader provided for {typeof(TUAD)} can not process the received {nameof(objectData)} " +
                        $"because its version {versionedJson.VersionId} is not supported and no default loader is specified.";

                    ROMPlugin.Logger?.LogError(versionNotSupportedError);
                    throw new ArgumentException(versionNotSupportedError, nameof(objectData));
                }

                string notAVersionedJsonError = $"Json data received for the creation of {typeof(TUAD)} was not parsable as {typeof(VersionedJson)}.";

                ROMPlugin.Logger?.LogError(notAVersionedJsonError);
                throw new ArgumentException(notAVersionedJsonError, nameof(objectData));
            }
        }

        public class LoadVersion<TUAD>(string versionId, Func<JToken, Room, TUAD> loadCall)
            where TUAD : UpdatableAndDeletable
        {
            public string VersionId { get; } = versionId;
            public Func<JToken, Room, TUAD> LoadCall = loadCall;
        }

        public static Func<JToken, Room, TUAD> GetVersionedLoadCall<TUAD>
            (Func<JToken, Room, TUAD>? defaultLoad = null, IEnumerable<LoadVersion<TUAD>> supportedVersions = null!)
            where TUAD : UpdatableAndDeletable
        {
             return new VersionedLoader<TUAD>(defaultLoad, supportedVersions ?? Enumerable.Empty<LoadVersion<TUAD>>()).Load;
        }

        public static Func<JToken, Room, TUAD> GetVersionedLoadCall<TUAD>
            (Func<JToken, Room, TUAD>? defaultLoad = null, params LoadVersion<TUAD>[] supportedVersions)
            where TUAD : UpdatableAndDeletable
        {
            return GetVersionedLoadCall(defaultLoad, supportedVersions as IEnumerable<LoadVersion<TUAD>>);
        }

        public static TUAD TrivialLoad<TUAD>(JToken data, Room room)
            where TUAD : UpdatableAndDeletable, new()
        {
            if (data.ToObject<TUAD>() is TUAD tuad)
            {
                tuad.room = room;
                return tuad;
            }

            string notParsedAsObjectError = $"Failed to parse the received {nameof(data)} as {nameof(TUAD)}.";

            ROMPlugin.Logger?.LogError(notParsedAsObjectError);
            throw new ArgumentException(notParsedAsObjectError, nameof(data));
        }

        public static JToken TrivialSave<TUAD>(TUAD obj)
            where TUAD : UpdatableAndDeletable
        {
            try
            {
                return JToken.FromObject(obj, UADContractResolver.SeiralizerInstance);
            }
            catch (JsonSerializationException ex)
            {
                string serializationExceptionError = $"A serialization exception has occurred while serializing a {typeof(TUAD)}. " +
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
