using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROM.RoomObjectService
{
    [JsonObject(MemberSerialization = MemberSerialization.OptOut)]
    public class VersionedJson
    {
        public string VersionId { get; set; } = "";

        public JToken Data { get; set; } = new JObject();
    }
}
