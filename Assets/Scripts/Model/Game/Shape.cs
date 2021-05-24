using Assets.Scripts.Context;
using Assets.Scripts.Loaders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Data = Assets.Scripts.Model.Data;

namespace Assets.Scripts.Model.Game
{
    public abstract class Shape
    {
        public ModContext mod; // null == "vanilla"
        public Data.TranslationData translation;

        public UnityEngine.Mesh[] subMeshes;
        public List<TextureInfo> TextureInfoList;

        public Shape(ModContext mod)
        {
            this.mod = mod;
        }

        internal static Shape CreateBlank(Data.Child child)
        {
            if (child.Bounds == null)
            {
                Data.PartData partData = new Data.PartData()
                {
                    Uuid = child.ShapeId,
                    RenderableObject = JObject.FromObject(new Data.Renderable()
                    {
                        LodList = new List<Data.Lod>()
                        {
                            new Data.Lod()
                            {
                                SubMeshList = new List<Data.SubMesh>()
                                {
                                    new Data.SubMesh()
                                    {
                                        TextureList = new List<string>()
                                        {
                                            "$GAME_DATA/Textures/error.tga"
                                        },
                                        Material = "PoseAnimDifAsgNor"
                                    }
                                },
                                Mesh = "$GAME_DATA/Mesh/cube.mesh"
                            }
                        }
                    }),
                    Color = "df7000",
                    Density = 250, // default ?
                    Box = new Data.Box() { X = 1, Y = 1, Z = 1 },
                };

                return new Part(partData);
            }
            else
            {
                Data.BlockData blockData = new Data.BlockData()
                {
                    Uuid = child.ShapeId,
                    Dif = "$GAME_DATA/Textures/error.tga",
                    Color = "df7000",
                    Density = 250
                };
                return new Block(blockData);
            }

        }
        internal static Shape CreateBlank(Data.Joint joint)
        {
            throw new NotImplementedException($"Could not find part/block for {joint.ShapeId}");
        }

        public abstract GameObject Instantiate(Transform parent);
        public abstract void ApplyTextures(GameObject gameObject);

        public abstract void LoadMesh();
        public abstract void LoadTextures();

        public abstract int GetWeight(int volume);
    }
}
