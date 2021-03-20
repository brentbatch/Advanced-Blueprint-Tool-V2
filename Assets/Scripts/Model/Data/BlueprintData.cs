using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Model.Data
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class BlueprintData
    {
        [JsonProperty("bodies")]
        public List<Body> Bodies { get; set; }

        [JsonProperty("joints")]
        public List<Joint> Joints { get; set; }

        [JsonProperty("version")]
        public int Version { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Body
    {
        [JsonProperty("childs")]
        public List<Child> Childs { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Child
    {
        [JsonProperty("bounds")]
        public Bounds Bounds { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("joints")]
        public List<Joint> Joints { get; set; }

        [JsonProperty("pos")]
        public Pos Pos { get; set; }

        [JsonProperty("shapeId")]
        public string ShapeId { get; set; }

        [JsonProperty("xaxis")]
        public int Xaxis { get; set; }

        [JsonProperty("zaxis")]
        public int Zaxis { get; set; }

        [JsonProperty("controller")]
        public Controller Controller { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Joint
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("reverse")]
        public object Reverse { get; set; }

        [JsonProperty("endAngle")]
        public int? EndAngle { get; set; }

        [JsonProperty("frames")]
        public List<Frame> Frames { get; set; }

        [JsonProperty("index")]
        public int? Index { get; set; }

        [JsonProperty("startAngle")]
        public int? StartAngle { get; set; }

        [JsonProperty("childA")]
        public int ChildA { get; set; }

        [JsonProperty("childB")]
        public int ChildB { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("posA")]
        public Pos PosA { get; set; }

        [JsonProperty("posB")]
        public Pos PosB { get; set; }

        [JsonProperty("shapeId")]
        public string ShapeId { get; set; }

        [JsonProperty("xaxisA")]
        public int XaxisA { get; set; }

        [JsonProperty("xaxisB")]
        public int XaxisB { get; set; }

        [JsonProperty("zaxisA")]
        public int ZaxisA { get; set; }

        [JsonProperty("zaxisB")]
        public int ZaxisB { get; set; }

        [JsonProperty("controller")]
        public Controller Controller { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Controller
    {
        [JsonProperty("containers")]
        public List<ControllerContainer> Containers { get; set; }

        [JsonProperty("controllers")]
        public List<ControllerController> Controllers { get; set; }

        [JsonProperty("data")]
        public string Data { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("joints")]
        public List<Joint> Joints { get; set; }

        [JsonProperty("audioEnabled")]
        public bool? AudioEnabled { get; set; }

        [JsonProperty("buttonMode")]
        public bool? ButtonMode { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("colorMode")]
        public bool? ColorMode { get; set; }

        [JsonProperty("range")]
        public int? Range { get; set; }

        [JsonProperty("active")]
        public bool? Active { get; set; }

        [JsonProperty("pitch")]
        public double? Pitch { get; set; }

        [JsonProperty("mode")]
        public int? Mode { get; set; }

        [JsonProperty("seconds")]
        public int? Seconds { get; set; }

        [JsonProperty("ticks")]
        public int? Ticks { get; set; }

        [JsonProperty("audioIndex")]
        public int? AudioIndex { get; set; }

        [JsonProperty("volume")]
        public int? Volume { get; set; }

        [JsonProperty("coneAngle")]
        public int? ConeAngle { get; set; }

        [JsonProperty("luminance")]
        public int? Luminance { get; set; }

        [JsonProperty("steering")]
        public object Steering { get; set; }

        [JsonProperty("playMode")]
        public int? PlayMode { get; set; }

        [JsonProperty("timePerFrame")]
        public double? TimePerFrame { get; set; }

        [JsonProperty("stiffnessLevel")]
        public int StiffnessLevel { get; set; }

        [JsonProperty("length")]
        public int? Length { get; set; }

        [JsonProperty("speed")]
        public int? Speed { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ControllerContainer
    {
        [JsonProperty("container")]
        public List<ContainerContainer> Container { get; set; }

        [JsonProperty("filters")]
        public List<Filter> Filters { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("slots")]
        public int Slots { get; set; }

        [JsonProperty("stackSize")]
        public int StackSize { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ContainerContainer
    {
        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("uuid")]
        public string Uuid { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Filter
    {
        [JsonProperty("uuid")]
        public string Uuid { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ControllerController
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("frames")]
        public List<Frame> Frames { get; set; }

        [JsonProperty("index")]
        public int? Index { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Frame
    {
        [JsonProperty("setting")]
        public int Setting { get; set; }

        [JsonProperty("targetAngle")]
        public int TargetAngle { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Pos
    {
        [JsonProperty("x")]
        public int X { get; set; }

        [JsonProperty("y")]
        public int Y { get; set; }

        [JsonProperty("z")]
        public int Z { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Bounds
    {
        [JsonProperty("x")]
        public int X { get; set; }

        [JsonProperty("y")]
        public int Y { get; set; }

        [JsonProperty("z")]
        public int Z { get; set; }
    }
}
