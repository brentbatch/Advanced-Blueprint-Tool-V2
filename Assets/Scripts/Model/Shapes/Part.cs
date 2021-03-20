﻿using Assets.Scripts.Context;
using Assets.Scripts.Loaders;
using Assets.Scripts.Model.BlueprintObject;
using Assets.Scripts.Model.Data;
using Assets.Scripts.Unity;
using Assimp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Model.Shapes
{
    public class Part : Shape
    {
        protected PartData partData;

        public UnityEngine.Mesh[] pose0;
        public UnityEngine.Mesh[] pose1;
        public UnityEngine.Mesh[] pose2;

        public Part(PartData partData, ModContext mod = null) : base(mod)
        {
            this.partData = partData;
            this.partData.LoadRenderable(mod?.ModFolderPath); // load json file
        }

        public override GameObject Instantiate(Transform parent)
        {
            var gameObject = UnityEngine.Object.Instantiate(Constants.Instance.Part, parent);
            gameObject.GetComponent<ChildObject>().shape = this;

            if (this.subMeshes == null)
                LoadMesh();

            for (int i = 0; i < subMeshes.Length; i++)
            {
                var subMesh = subMeshes[i];
                GameObject subMeshGameObject = UnityEngine.Object.Instantiate(Constants.Instance.SubMesh, gameObject.transform);
                subMeshGameObject.GetComponent<MeshFilter>().mesh = subMesh;
                subMeshGameObject.GetComponent<MeshRenderer>().material = new UnityEngine.Material(PartLoader.Instance.material);
                // todo: CORRECT to the right material based on partdata
            }
            return gameObject;
        }

        public override void ApplyTextures(GameObject gameObject)
        {
            if (materialInfoList == null)
                LoadTextures();

            MeshRenderer[] meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < subMeshes.Length && i < meshRenderers.Length; i++)
            {
                var material = meshRenderers[i].material;
                int index = i % materialInfoList.Count;
                material.SetTexture("_MainTex", materialInfoList[index].diffuse);
                material.SetTexture("_NorTex", materialInfoList[index].normal);
                 //material.SetTexture("_AsgTex", materialInfoList[index].asg);
            }
        }


        /// <summary>
        /// loads models, materials, ... based on info in PartData
        /// </summary>
        public override void LoadMesh()
        {
            try
            {
                var lod = partData.Renderable.LodList.First();

                Scene meshScene = LoadScene(PathResolver.ResolvePath(lod.Mesh, this.mod?.ModFolderPath));
                this.subMeshes = meshScene.Meshes.Select(mesh => ConvertMesh(mesh)).ToArray();

                if (lod.Pose0 != null && false)
                {
                    Scene pose0Scene = LoadScene(PathResolver.ResolvePath(lod.Pose0, this.mod?.ModFolderPath));
                    this.pose0 = pose0Scene.Meshes.Select(mesh => ConvertMesh(mesh)).ToArray();
                }
                if (lod.Pose1 != null && false)
                {
                    Scene pose1Scene = LoadScene(PathResolver.ResolvePath(lod.Pose1, this.mod?.ModFolderPath));
                    this.pose1 = pose1Scene.Meshes.Select(mesh => ConvertMesh(mesh)).ToArray();
                }
                if (lod.Pose2 != null && false)
                {
                    Scene pose2Scene = LoadScene(PathResolver.ResolvePath(lod.Pose2, this.mod?.ModFolderPath));
                    this.pose2 = pose2Scene.Meshes.Select(mesh => ConvertMesh(mesh)).ToArray();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed loading model for part {partData.Uuid}\nError: {e} \nTrace: {StackTraceUtility.ExtractStringFromException(e)}");
                // use cuboid from prefab, stretch if bounds found in partdata
                // use diffuse & normal from prefab
            }
        }

        public override void LoadTextures()
        {
            try // CACHING!!!!
            {
                var lod = partData.Renderable.LodList.First();
                Scene meshScene = LoadScene(PathResolver.ResolvePath(lod.Mesh, this.mod?.ModFolderPath));

                var transparanttga = PathResolver.ResolvePath("$GAME_DATA/Textures/transparent.tga");
                var nonortga = PathResolver.ResolvePath("$GAME_DATA/Textures/nonor_nor.tga");

                this.materialInfoList = new List<MaterialInfo>();

                if (lod.SubMeshList != null)
                {
                    foreach (SubMesh subMesh in lod.SubMeshList)
                    {
                        string dif = subMesh.TextureList.Count > 0 ? PathResolver.ResolvePath(subMesh.TextureList[0], mod?.ModFolderPath) : transparanttga;
                        string nor = subMesh.TextureList.Count > 2 ? PathResolver.ResolvePath(subMesh.TextureList[2], mod?.ModFolderPath) : nonortga;

                        if (!File.Exists(dif)) dif = transparanttga;
                        if (!File.Exists(nor)) nor = nonortga;

                        materialInfoList.Add(new MaterialInfo()
                        {
                            material = subMesh.Material,
                            diffuse = LoadTexture(dif),
                            normal = LoadTexture(nor)
                        });
                    }
                }
                if (lod.SubMeshMap != null)
                {
                    foreach (var material in meshScene.Materials)
                    {
                        if (lod.SubMeshMap.TryGetValue(material.Name, out SubMesh subMesh))
                        {
                            string dif = subMesh.TextureList.Count > 0 ? PathResolver.ResolvePath(subMesh.TextureList[0], mod?.ModFolderPath) : transparanttga;
                            string nor = subMesh.TextureList.Count > 2 ? PathResolver.ResolvePath(subMesh.TextureList[2], mod?.ModFolderPath) : nonortga;

                            if (!File.Exists(dif)) dif = transparanttga;
                            if (!File.Exists(nor)) nor = nonortga;

                            materialInfoList.Add(new MaterialInfo()
                            {
                                material = subMesh.Material,
                                diffuse = LoadTexture(dif),
                                normal = LoadTexture(nor)
                            });
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Debug.LogError($"Failed loading textures for part {partData.Uuid}\nError: {e} \nTrace: {StackTraceUtility.ExtractStringFromException(e)}");

            }
        }


        protected Scene LoadScene(string meshPath)
        {
            //importer.SetConfig(new Assimp.Configs.MeshVertexLimitConfig(60000));
            //importer.SetConfig(new Assimp.Configs.MeshTriangleLimitConfig(60000));
            //importer.SetConfig(new Assimp.Configs.RemoveDegeneratePrimitivesConfig(true));
            //importer.SetConfig(new Assimp.Configs.SortByPrimitiveTypeConfig(Assimp.PrimitiveType.Line | Assimp.PrimitiveType.Point));
            //Assimp.PostProcessSteps postProcessSteps = Assimp.PostProcessPreset.TargetRealTimeMaximumQuality | Assimp.PostProcessSteps.MakeLeftHanded | Assimp.PostProcessSteps.FlipWindingOrder;

            Scene scene = PartLoader.importer.ImportFile(meshPath);

            return scene;
        }

        protected UnityEngine.Mesh ConvertMesh(Assimp.Mesh mesh)
        {
            UnityEngine.Mesh unityMesh = new UnityEngine.Mesh
            {
                vertices = mesh.Vertices.Select(v => new Vector3(-v.X, v.Y, v.Z)).ToArray(),
                triangles = GetTriangles(mesh.Faces).ToArray(),
                normals = mesh.Normals?.Select(n => new Vector3(-n.X, n.Y, n.Z)).ToArray(), 
                //tangents = mesh.Tangents?.Select(t => new Vector4(t.X, t.Y, t.Z, 1)).ToArray()
            };
            for (int i = 0; i < mesh.TextureCoordinateChannelCount && i < 8; i++)
            {
                unityMesh.SetUVs(i, mesh.TextureCoordinateChannels[i]?.Select(coord => new Vector2(coord.X, coord.Y)).ToArray());
            }
            return unityMesh;
        }

        protected IEnumerable<int> GetTriangles(List<Face> faces)
        {
            foreach (Face face in faces)
            {
                switch (face.IndexCount)
                {
                    case 3:
                        yield return (int)face.Indices[0];
                        yield return (int)face.Indices[2];
                        yield return (int)face.Indices[1];
                        break;
                    case 4:
                        yield return (int)face.Indices[0];
                        yield return (int)face.Indices[2];
                        yield return (int)face.Indices[1];

                        yield return (int)face.Indices[3];
                        yield return (int)face.Indices[2];
                        yield return (int)face.Indices[0];
                        break;
                    case 5:
                        yield return (int)face.Indices[0];
                        yield return (int)face.Indices[2];
                        yield return (int)face.Indices[1];

                        yield return (int)face.Indices[3];
                        yield return (int)face.Indices[2];
                        yield return (int)face.Indices[0];

                        yield return (int)face.Indices[4];
                        yield return (int)face.Indices[3];
                        yield return (int)face.Indices[0];
                        break;
                    default:
                        //Debug.LogWarning($"not implemented: more than 5 vertices in one face! vertices found: {face.IndexCount}");
                        break;
                }
            }
        }

    }
}
