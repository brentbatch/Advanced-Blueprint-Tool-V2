using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Model.Data
{
    public class DescriptionData
    {
        [JsonProperty("creatorId")]
        public string CreatorId { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("fileId")]
        public string FileId { get; set; }

        [JsonProperty("localId")]
        public string LocalId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        public override string ToString()
        {
            return $"{Name}; {LocalId}; {FileId}";
        }
    }
}
