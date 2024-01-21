using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ROM.RoomObjectService.TypeOperatorUtils;

namespace ROM.RoomObjectService
{
    /// <summary>
    /// A class to contain a list loaders and use them according to the <see cref="VersionedJson.VersionId"/> in the received JSON trees.
    /// </summary>
    /// <typeparam name="TOBJ">The type of the objects loaded from JSON trees.</typeparam>
    public class VersionedLoader<TOBJ>
            where TOBJ : notnull
    {
        /// <summary>
        /// The load call to use if there is no <see cref="LoadVersion{TOBJ}"/> found with a matching <see cref="LoadVersion{TOBJ}.VersionId"/>.
        /// </summary>
        public Func<JToken, Room, TOBJ>? DefaultLoader { get; }
        /// <summary>
        /// The dictionary of supported versions.
        /// </summary>
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
}
