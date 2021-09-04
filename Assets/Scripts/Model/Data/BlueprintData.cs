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
        public List<JointReference> Joints { get; set; }

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
    public class Joint // blueprint.joints[]
    {
        [JsonProperty("childA")]
        public int ChildA { get; set; }

        [JsonProperty("childB")]
        public int ChildB { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("controller")]
        public Controller Controller { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

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
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class JointReference // child.joints[]
    {
        [JsonProperty("id")]
        public int Id { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Controller
    {
        [JsonProperty("containers")]
        public List<ControllerContainer> Containers { get; set; } // shape (controller)

        [JsonProperty("controllers")]
        public List<ControllerController> Controllers { get; set; } // shape (controller)

        [JsonProperty("data")]
        public string Data { get; set; } // random shit im not parsing for scripted or nonscripted parts ¯\_(ツ)_/¯

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("joints")]
        public List<ControllerJoint> Joints { get; set; }

        [JsonProperty("audioEnabled")]
        public bool? AudioEnabled { get; set; } // totebot heads

        [JsonProperty("buttonMode")]
        public bool? ButtonMode { get; set; } // sensor

        [JsonProperty("color")]
        public string Color { get; set; } // for sensor, what color does it check

        [JsonProperty("colorMode")]
        public bool? ColorMode { get; set; } // sensor

        [JsonProperty("range")]
        public int? Range { get; set; } // sensor

        [JsonProperty("active")]
        public bool? Active { get; set; } // logic gate

        [JsonProperty("pitch")]
        public double? Pitch { get; set; } // totebot heads

        [JsonProperty("mode")]
        public int? Mode { get; set; } // logic gate

        [JsonProperty("seconds")]
        public int? Seconds { get; set; } // timer

        [JsonProperty("ticks")]
        public int? Ticks { get; set; } // timer

        [JsonProperty("audioIndex")]
        public int? AudioIndex { get; set; } // totebot heads

        [JsonProperty("volume")]
        public int? Volume { get; set; } // totebot heads

        [JsonProperty("coneAngle")]
        public int? ConeAngle { get; set; } // light

        [JsonProperty("luminance")]
        public int? Luminance { get; set; } // light

        [JsonProperty("steering")]
        public List<ControllerSteering> Steering { get; set; } // driverseat

        [JsonProperty("playMode")]
        public int? PlayMode { get; set; } // controller

        [JsonProperty("timePerFrame")]
        public double? TimePerFrame { get; set; } // controller

        [JsonProperty("stiffnessLevel")]
        public int StiffnessLevel { get; set; } // joint (spring)

        [JsonProperty("length")]
        public int? Length { get; set; } // joint (piston)

        [JsonProperty("speed")]
        public int? Speed { get; set; } // joint (piston)

        [JsonProperty("level")]
        public int? Level { get; set; } // thruster level
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ControllerSteering
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("leftAngleLimit")]
        public double LeftAngleLimit { get; set; }

        [JsonProperty("leftAngleSpeed")]
        public double LeftAngleSpeed { get; set; }

        [JsonProperty("rightAngleLimit")]
        public double RightAngleLimit { get; set; }

        [JsonProperty("rightAngleSpeed")]
        public double RightAngleSpeed { get; set; }

        [JsonProperty("unlocked")]
        public bool Unlocked { get; set; }
    } // a driverseat property

    #region child/joint.controller.container stuff
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ControllerContainer
    {
        [JsonProperty("container")]
        public List<Container> Container { get; set; }

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
    public class Container
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
    #endregion

    #region child/joint.controller.controllers stuff
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ControllerController // almost only used for the 'id' part, but when on a 'controller' shape Frames and Index is also used.
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("frames")]
        public List<ControllerControllerFrame> Frames { get; set; }

        [JsonProperty("index")]
        public int Index { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ControllerControllerFrame
    {
        [JsonProperty("setting")]
        public int Setting { get; set; }
    }
    #endregion

    #region child/joint.controller.joint stuff
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ControllerJoint // almost only used for the 'id' part, but when on a 'controller' shape Frames and Index is also used.
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("reverse")]
        public bool Reverse { get; set; }

        [JsonProperty("frames")]
        public List<ControllerJointFrame> Frames { get; set; }

        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("startAngle")]
        public int StartAngle { get; set; }

        [JsonProperty("endAngle")]
        public int EndAngle { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ControllerJointFrame
    {
        [JsonProperty("targetAngle")]
        public int TargetAngle { get; set; }
    }

    #endregion

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
