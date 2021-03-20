﻿using Assets.Scripts.Context;
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
            return gameObject;
        }


        public override void ApplyTextures(GameObject gameObject)
        {
            if (this.materialInfoList == null)
                LoadTextures();

            var material = gameObject.GetComponent<MeshRenderer>().material;
            material.SetTexture("_MainTex", materialInfoList[0].diffuse);
            material.SetTexture("_NorTex", materialInfoList[0].normal);
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

                materialInfoList = new List<MaterialInfo>()
                {
                    new MaterialInfo()
                    {
                        material = "DifAsgNor",
                        diffuse = LoadTexture(dif),
                        //normal = LoadTexture(nor)
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
