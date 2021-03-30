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

namespace Assets.Scripts.Model.Game
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

            GameObject subMeshGameObject = UnityEngine.Object.Instantiate(Constants.Instance.Cube, gameObject.transform);

            var pos = subMeshGameObject.transform.position;
            (pos.x, pos.y, pos.z) = (0.5f, 0.5f, 0.5f);
            subMeshGameObject.transform.position = pos;

            if (blockData.Glass == true)
            {
                subMeshGameObject.GetComponent<MeshRenderer>().material = new UnityEngine.Material(Constants.Instance.glassBlockMaterial);
                subMeshGameObject.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                subMeshGameObject.GetComponent<Renderer>().receiveShadows = false;
            }
            subMeshGameObject.GetComponent<MeshRenderer>().material.SetFloat("_Tiling", 1f / blockData.Tiling);
            return gameObject;
        }


        public override void ApplyTextures(GameObject gameObject)
        {
            if (this.TextureInfoList == null)
                LoadTextures();

            var material = gameObject.GetComponentInChildren<MeshRenderer>().material;

            var texInfo = TextureInfoList[0];
            if (texInfo.diffuse != null)
            {
                TextureLoader.Instance.GetTextureAndDoAction(
                    TextureInfoList[0].diffuse,
                    (Texture2D tex) => material.SetTexture("_MainTex", tex));
            }
            if (texInfo.normal != null)
            {
                TextureLoader.Instance.GetTextureAndDoAction(
                    TextureInfoList[0].normal,
                    (Texture2D tex) => material.SetTexture("_NorTex", tex));
            }
            if (texInfo.asg != null)
            {
                TextureLoader.Instance.GetTextureAndDoAction(
                    TextureInfoList[0].asg,
                    (Texture2D tex) => material.SetTexture("_AsgTex", tex));
            }
        }

        public override void LoadMesh()
        {
        }

        /// <summary>
        /// Loads texturepaths, ... based on BlockData
        /// </summary>
        public override void LoadTextures()
        {
            try
            {
                //var transparanttga = PathResolver.ResolvePath("$GAME_DATA/Textures/transparent.tga", mod?.ModFolderPath);
                //var nonortga = PathResolver.ResolvePath("$GAME_DATA/Textures/nonor_nor.tga", mod?.ModFolderPath);

                string dif = PathResolver.ResolvePath(blockData.Dif, mod?.ModFolderPath);
                string asg = PathResolver.ResolvePath(blockData.Asg, mod?.ModFolderPath);
                string nor = PathResolver.ResolvePath(blockData.Nor, mod?.ModFolderPath);

                if (!File.Exists(dif)) dif = null; //transparanttga;
                if (!File.Exists(asg)) asg = null; //transparanttga;
                if (!File.Exists(nor)) nor = null; //nonortga;

                TextureInfoList = new List<TextureInfo>()
                {
                    new TextureInfo()
                    {
                        diffuse = dif,
                        asg = asg,
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
