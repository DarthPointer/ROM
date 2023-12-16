using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROM.RoomObjectService
{
    public interface ITypeLoader<out TUAD>
        where TUAD : UpdatableAndDeletable
    {
        TUAD Load(JToken objectData, Room room);
    }

    public class VersionedTypeLoader<TUAD>
        : ITypeLoader<TUAD>
        where TUAD : UpdatableAndDeletable
    {
        private Dictionary<string, Func<JToken, Room, TUAD>> Loaders { get; } = [];

        public Func<JToken, Room, TUAD>? DefaultLoader { get; set; }

        public VersionedTypeLoader(IEnumerable<Version> versions)
        {
            foreach (Version version in versions)
            {
                Loaders[version.VersionId] = version.Converter;
            }
        }

        public VersionedTypeLoader(params Version[] versions) : this(versions as IEnumerable<Version>)
        { }

        public TUAD Load(JToken objectData, Room room)
        {
            if (objectData.ToObject<VersionedJson>() is VersionedJson versionedJson)
            {
                if (Loaders.TryGetValue(versionedJson.VersionId, out var converter))
                {
                    return converter(versionedJson.Data, room);
                }

                if (DefaultLoader != null)
                    return DefaultLoader(versionedJson.Data, room);

                string noLoaderErrorMessage =
                    $"Failed to load object from a JSON of version {versionedJson.VersionId} " +
                    $"because there is no loader set for that version and {nameof(DefaultLoader)} is null.";

                ROMPlugin.Logger?.LogError(noLoaderErrorMessage);
                throw new Exception(noLoaderErrorMessage);
            }

            string versionedJsonErrorMessage = $"Failed to read object data JSON as {typeof(VersionedJson)}.";

            ROMPlugin.Logger?.LogError(versionedJsonErrorMessage);
            throw new Exception(versionedJsonErrorMessage);
        }

        public class Version(string versionId, Func<JToken, Room, TUAD> converter)
        {
            public string VersionId { get; } = versionId;
            public Func<JToken, Room, TUAD> Converter { get; } = converter;
        }
    }

    public static class VersionedTypeLoader
    {
        /// <summary>
        /// The most "trivial" way to construct an UAD from its data json.
        /// Requires the UAD type to provide a trivial ctor().
        /// Only use it for save versions that can be directly loaded back into the UAD.
        /// </summary>
        /// <typeparam name="TUAD"></typeparam>
        /// <param name="objectData"></param>
        /// <param name="room"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static TUAD DefaultLoad<TUAD>(JToken objectData, Room room)
            where TUAD : UpdatableAndDeletable, new()
        {
            if (objectData.ToObject<TUAD>() is TUAD uad)
            {
                uad.room = room;

                return uad;
            }

            string objectGenerationError = $"Failed to create an object of {typeof(TUAD)}";

            ROMPlugin.Logger?.LogError(objectGenerationError);
            throw new Exception(objectGenerationError);
        }
    }
}
