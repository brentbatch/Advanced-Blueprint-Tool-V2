using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Model.Data
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class UpgradeResourcesData
    {
        [JsonProperty("upgrade")]
        public List<List<List<string>>> Upgrade { get; set; }
    }
}
