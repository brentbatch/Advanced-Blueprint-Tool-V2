using Assets.Scripts.Loaders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Model.Data
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class PartListData
    {
        [JsonProperty("partList")]
        public List<PartData> PartList { get; set; }

        [JsonProperty("blockList")]
        public List<BlockData> BlockList { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class PartData
    {
        [JsonProperty("uuid")]
        public string Uuid { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("legacyId")]
        public int LegacyId { get; set; }
        
        public Renderable Renderable { get; set; }

        public void LoadRenderableData(string modFolderPath)
        {
            if (RenderableObject is string renderablePath)
            {
                string path = PathResolver.ResolvePath(renderablePath, modFolderPath);
                string text = System.IO.File.ReadAllText(path);
                Renderable = JsonConvert.DeserializeObject<Renderable>(text);
            }
            else
            {
                string text = RenderableObject.ToString();
                Renderable = JsonConvert.DeserializeObject<Renderable>(text);
            }
        }

        [JsonProperty("renderable")]
        public object RenderableObject { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("cylinder")]
        public Cylinder Cylinder { get; set; }

        [JsonProperty("rotationSet")]
        public string RotationSet { get; set; }

        [JsonProperty("sticky")]
        public string Sticky { get; set; }

        [JsonProperty("physicsMaterial")]
        public string PhysicsMaterial { get; set; }

        [JsonProperty("ratings")]
        public Ratings Ratings { get; set; }

        [JsonProperty("flammable")]
        public bool Flammable { get; set; }

        [JsonProperty("scripted")]
        public Scripted Scripted { get; set; }

        [JsonProperty("box")]
        public Box Box { get; set; }

        [JsonProperty("density")]
        public double? Density { get; set; }

        [JsonProperty("qualityLevel")]
        public int? QualityLevel { get; set; }

        [JsonProperty("hull")]
        public Hull Hull { get; set; }

        [JsonProperty("friction")]
        public float? Friction { get; set; }

        [JsonProperty("prop")]
        public bool? Prop { get; set; }

        [JsonProperty("sphere")]
        public Sphere Sphere { get; set; }

        [JsonProperty("restitution")]
        public double? Restitution { get; set; }

        [JsonProperty("previewRotation")]
        public List<int> PreviewRotation { get; set; }

        [JsonProperty("preview")]
        public Preview Preview { get; set; }

        [JsonProperty("showInInventory")]
        public bool? ShowInInventory { get; set; }

        [JsonProperty("bearing")]
        public Bearing Bearing { get; set; }

        [JsonProperty("stackSize")]
        public int? StackSize { get; set; }

        [JsonProperty("spring")]
        public Spring Spring { get; set; }

        [JsonProperty("steering")]
        public Steering Steering { get; set; }

        [JsonProperty("seat")]
        public Seat Seat { get; set; }

        [JsonProperty("gasEngine")]
        public GasEngine GasEngine { get; set; }

        [JsonProperty("electricEngine")]
        public ElectricEngine ElectricEngine { get; set; }

        [JsonProperty("thruster")]
        public Thruster Thruster { get; set; }

        [JsonProperty("controller")]
        public PartController Controller { get; set; }

        [JsonProperty("lever")]
        public Lever Lever { get; set; }

        [JsonProperty("button")]
        public Button Button { get; set; }

        [JsonProperty("sensor")]
        public Sensor Sensor { get; set; }

        [JsonProperty("radio")]
        public Radio Radio { get; set; }

        [JsonProperty("horn")]
        public Horn Horn { get; set; }

        [JsonProperty("logic")]
        public Logic Logic { get; set; }

        [JsonProperty("timer")]
        public Timer Timer { get; set; }

        [JsonProperty("tone")]
        public Tone Tone { get; set; }

        [JsonProperty("piston")]
        public Piston Piston { get; set; }

        [JsonProperty("simpleInteractive")]
        public SimpleInteractive SimpleInteractive { get; set; }

        public override bool Equals(object obj)
        {
            return obj is PartData data &&
                   EqualityComparer<Renderable>.Default.Equals(Renderable, data.Renderable);
        }

        public override int GetHashCode()
        {
            return -386662187 + EqualityComparer<Renderable>.Default.GetHashCode(Renderable);
        }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Cylinder
    {
        [JsonProperty("diameter")]
        public int Diameter { get; set; }

        [JsonProperty("depth")]
        public int Depth { get; set; }

        [JsonProperty("margin")]
        public double Margin { get; set; }

        [JsonProperty("axis")]
        public string Axis { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Ratings
    {
        [JsonProperty("density")]
        public int Density { get; set; }

        [JsonProperty("durability")]
        public int Durability { get; set; }

        [JsonProperty("friction")]
        public int Friction { get; set; }

        [JsonProperty("buoyancy")]
        public int Buoyancy { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Data
    {
        [JsonProperty("destructionLevel")]
        public int DestructionLevel { get; set; }

        [JsonProperty("destructionRadius")]
        public double DestructionRadius { get; set; }

        [JsonProperty("impulseRadius")]
        public double ImpulseRadius { get; set; }

        [JsonProperty("impulseMagnitude")]
        public double ImpulseMagnitude { get; set; }

        [JsonProperty("effectExplosion")]
        public string EffectExplosion { get; set; }

        [JsonProperty("effectActivate")]
        public string EffectActivate { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Scripted
    {
        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("classname")]
        public string Classname { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Box
    {
        [JsonProperty("x")]
        public float X { get; set; }

        [JsonProperty("y")]
        public float Y { get; set; }

        [JsonProperty("z")]
        public float Z { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class PointList
    {
        [JsonProperty("x")]
        public double X { get; set; }

        [JsonProperty("y")]
        public double Y { get; set; }

        [JsonProperty("z")]
        public double Z { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Hull
    {
        [JsonProperty("x")]
        public float X { get; set; }

        [JsonProperty("y")]
        public float Y { get; set; }

        [JsonProperty("z")]
        public float Z { get; set; }

        [JsonProperty("margin")]
        public double Margin { get; set; }

        [JsonProperty("pointList")]
        public List<PointList> PointList { get; set; }

        [JsonProperty("col")]
        public string Col { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Sphere
    {
        [JsonProperty("diameter")]
        public int Diameter { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Preview
    {
        [JsonProperty("effect")]
        public string Effect { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Bearing
    {
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class StiffnessLevel
    {
        [JsonProperty("stiffness")]
        public int Stiffness { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Spring
    {
        [JsonProperty("length")]
        public int Length { get; set; }

        [JsonProperty("defaultStiffnessLevel")]
        public int DefaultStiffnessLevel { get; set; }

        [JsonProperty("stiffnessLevels")]
        public List<StiffnessLevel> StiffnessLevels { get; set; }

        [JsonProperty("effect")]
        public string Effect { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Offset
    {
        [JsonProperty("x")]
        public double X { get; set; }

        [JsonProperty("y")]
        public double Y { get; set; }

        [JsonProperty("z")]
        public double Z { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Bone
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("offset")]
        public Offset Offset { get; set; }

        [JsonProperty("freeRotation")]
        public bool? FreeRotation { get; set; }

        [JsonProperty("steers")]
        public bool? Steers { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Steering
    {
        [JsonProperty("bones")]
        public List<Bone> Bones { get; set; }

        [JsonProperty("ragdollFile")]
        public string RagdollFile { get; set; }

        [JsonProperty("steerAngle")]
        public double SteerAngle { get; set; }

        [JsonProperty("enterAudio")]
        public string EnterAudio { get; set; }

        [JsonProperty("exitAudio")]
        public string ExitAudio { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Seat
    {
        [JsonProperty("bones")]
        public List<Bone> Bones { get; set; }

        [JsonProperty("ragdollFile")]
        public string RagdollFile { get; set; }

        [JsonProperty("enterAudio")]
        public string EnterAudio { get; set; }

        [JsonProperty("exitAudio")]
        public string ExitAudio { get; set; }

        [JsonProperty("effect")]
        public string Effect { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Gear
    {
        [JsonProperty("velocity")]
        public float Velocity { get; set; }

        [JsonProperty("power")]
        public float Power { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class GasEngine
    {
        [JsonProperty("gears")]
        public List<Gear> Gears { get; set; }

        [JsonProperty("effect")]
        public string Effect { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ElectricEngine
    {
        [JsonProperty("gears")]
        public List<Gear> Gears { get; set; }

        [JsonProperty("effect")]
        public string Effect { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Level
    {
        [JsonProperty("averageForce")]
        public double AverageForce { get; set; }

        [JsonProperty("forceVariation")]
        public double ForceVariation { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Thruster
    {
        [JsonProperty("levels")]
        public List<Level> Levels { get; set; }

        [JsonProperty("effect")]
        public string Effect { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class PartController
    {
        [JsonProperty("effect")]
        public string Effect { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Lever
    {
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Button
    {
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Sensor
    {
        [JsonProperty("onEffect")]
        public string OnEffect { get; set; }

        [JsonProperty("offEffect")]
        public string OffEffect { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Radio
    {
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Horn
    {
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Logic
    {
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Timer
    {
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Audio
    {
        [JsonProperty("event")]
        public string Event { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Tone
    {
        [JsonProperty("audio")]
        public List<Audio> Audio { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class SpeedLevel
    {
        [JsonProperty("speed")]
        public double Speed { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Piston
    {
        [JsonProperty("length")]
        public int Length { get; set; }

        [JsonProperty("defaultRange")]
        public int DefaultRange { get; set; }

        [JsonProperty("speedLevels")]
        public List<SpeedLevel> SpeedLevels { get; set; }

        [JsonProperty("defaultSpeedLevel")]
        public int DefaultSpeedLevel { get; set; }

        [JsonProperty("force")]
        public double Force { get; set; }

        [JsonProperty("effect")]
        public string Effect { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class SimpleInteractive
    {
        [JsonProperty("sustainedAudio")]
        public string SustainedAudio { get; set; }

        [JsonProperty("animationTime")]
        public double AnimationTime { get; set; }
    }



    // ---------------------------------- blocklist

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class BlockData
    {
        [JsonProperty("uuid")]
        public string Uuid { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("legacyId")]
        public int LegacyId { get; set; }

        [JsonProperty("dif")]
        public string Dif { get; set; }

        [JsonProperty("asg")]
        public string Asg { get; set; }

        [JsonProperty("nor")]
        public string Nor { get; set; }

        [JsonProperty("tiling")]
        public int Tiling { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("physicsMaterial")]
        public string PhysicsMaterial { get; set; }

        [JsonProperty("ratings")]
        public Ratings Ratings { get; set; }

        [JsonProperty("flammable")]
        public bool Flammable { get; set; }

        [JsonProperty("density")]
        public double Density { get; set; }

        [JsonProperty("qualityLevel")]
        public int QualityLevel { get; set; }

        [JsonProperty("restrictions")]
        public Restrictions Restrictions { get; set; }

        [JsonProperty("alpha")]
        public bool? Alpha { get; set; }

        [JsonProperty("glass")]
        public bool? Glass { get; set; }

        [JsonProperty("max_stack_count")]
        public int? MaxStackCount { get; set; }

        [JsonProperty("friction")]
        public double? Friction { get; set; }
    }


    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Restrictions
    {
        [JsonProperty("destructable")]
        public bool Destructable { get; set; }

        [JsonProperty("erasable")]
        public bool Erasable { get; set; }

        [JsonProperty("buildable")]
        public bool Buildable { get; set; }

        [JsonProperty("convertibleToDynamic")]
        public bool ConvertibleToDynamic { get; set; }

        [JsonProperty("liftable")]
        public bool Liftable { get; set; }

        [JsonProperty("paintable")]
        public bool Paintable { get; set; }
    }


    // ----------------------- renderable info


    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Renderable
    {
        [JsonProperty("lodList")]
        public List<Lod> LodList { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Lod
    {
        [JsonProperty("subMeshList")]
        public List<SubMesh> SubMeshList { get; set; }

        [JsonProperty("mesh")]
        public string Mesh { get; set; }

        [JsonProperty("pose0")]
        public string Pose0 { get; set; }

        [JsonProperty("includes")]
        public List<string> Includes { get; set; }

        [JsonProperty("custom")]
        public Custom Custom { get; set; }

        [JsonProperty("pose1")]
        public string Pose1 { get; set; }

        [JsonProperty("pose2")]
        public string Pose2 { get; set; }

        [JsonProperty("minViewSize")]
        public int? MinViewSize { get; set; }

        [JsonProperty("subMeshMap")]
        public Dictionary<string, SubMesh> SubMeshMap { get; set; }
    }


    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class SubMesh
    {
        [JsonProperty("material")]
        public string Material { get; set; }

        [JsonProperty("textureList")]
        public List<string> TextureList { get; set; }

        [JsonProperty("custom")]
        public Custom Custom { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Custom
    {
        [JsonProperty("uv0")]
        public Uv0 Uv0 { get; set; }

        [JsonProperty("skeleton")]
        public Skeleton Skeleton { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Uv0
    {
        [JsonProperty("u")]
        public double U { get; set; }

        [JsonProperty("v")]
        public double V { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Skeleton
    {
        [JsonProperty("anim0")]
        public string Anim0 { get; set; }
    }
}
