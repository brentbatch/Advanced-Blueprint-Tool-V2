using Assets.Scripts.Context;
using Assets.Scripts.Loaders;
using Assets.Scripts.Model.BlueprintObject;
using Assets.Scripts.Model.Data;
using Assets.Scripts.Unity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Model.Shapes
{
    public class Block : Shape
    {
        protected BlockData blockData;

        public Block(BlockData blockData, ModContext mod = null) : base(mod) 
        {
            this.blockData = blockData;
        }

        public override GameObject Instantiate(Transform parent)
        {
            var gameObject = UnityEngine.Object.Instantiate(Constants.Instance.Block, parent);
            gameObject.GetComponent<ChildObject>().shape = this;

            GameObject subMeshGameObject = UnityEngine.Object.Instantiate(Constants.Instance.Cube, gameObject.transform);

            var pos = subMeshGameObject.transform.position;
            (pos.x, pos.y, pos.z) = (0.5f, 0.5f, 0.5f);
            subMeshGameObject.transform.position = pos;

            // todo: box collider edit
            if (blockData.Glass == true)
            {
                subMeshGameObject.GetComponent<MeshRenderer>().material = new UnityEngine.Material(Constants.Instance.glassBlockMaterial);
            }
            subMeshGameObject.GetComponent<MeshRenderer>().material.SetFloat("_Tiling", 1f / blockData.Tiling);
            return gameObject;
        }


        public override void ApplyTextures(GameObject gameObject)
        {
            if (this.TextureInfoList == null)
                LoadTextures();

            var material = gameObject.GetComponentInChildren<MeshRenderer>().material;

            TextureLoader.Instance.GetTextureAndDoAction(
                TextureInfoList[0].diffuse,
                (Texture2D tex) => material.SetTexture("_MainTex", tex));

            TextureLoader.Instance.GetTextureAndDoAction(
                TextureInfoList[0].normal,
                (Texture2D tex) => material.SetTexture("_NorTex", tex));
            //material.SetTexture("_AsgTex", materialInfoList[0].asg);
        }

        public override void LoadMesh()
        {
        }

        /// <summary>
        /// Loads texture, material, ... based on BlockData
        /// </summary>
        public override void LoadTextures()
        {
            try // caching!!!
            {
                var transparanttga = PathResolver.ResolvePath("$GAME_DATA/Textures/transparent.tga", mod?.ModFolderPath);
                var nonortga = PathResolver.ResolvePath("$GAME_DATA/Textures/nonor_nor.tga", mod?.ModFolderPath);

                string dif = PathResolver.ResolvePath(blockData.Dif, mod?.ModFolderPath);
                string nor = PathResolver.ResolvePath(blockData.Nor, mod?.ModFolderPath);

                if (!File.Exists(dif)) dif = transparanttga;
                if (!File.Exists(nor)) nor = nonortga;

                TextureInfoList = new List<TextureInfo>()
                {
                    new TextureInfo()
                    {
                        diffuse = dif,
                        normal = nor
                    }
                };
            }
            catch (Exception e)
            {
                Debug.LogError($"An error occurred when loading data for block {translation?.Title}\nError: {e}");
            }
        }

    }
}
