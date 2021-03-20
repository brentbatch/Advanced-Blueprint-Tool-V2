using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Model.Data
{
    public class ShapeSetListData
    {
        [JsonProperty("shapeSetList")]
        public List<string> ShapeSetList { get; set; }
    }
}
