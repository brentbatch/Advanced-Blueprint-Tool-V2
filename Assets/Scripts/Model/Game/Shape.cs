using Assets.Scripts.Context;
using Assets.Scripts.Loaders;
using Assets.Scripts.Model.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Joint = Assets.Scripts.Model.Data.Joint;

namespace Assets.Scripts.Model.Game
{
    public abstract class Shape
    {
        public ModContext mod; // null == "vanilla"
        public TranslationData translation;

        public UnityEngine.Mesh[] subMeshes;
        public List<TextureInfo> TextureInfoList;

        public Shape(ModContext mod)
        {
            this.mod = mod;
        }

        internal static Shape CreateBlank(Child child)
        {
            // create fake partlistdata
            // can be a part or a block, child.bounds?
            throw new NotImplementedException();
        }
        internal static Shape CreateBlank(Joint child)
        {
            throw new NotImplementedException();
        }

        public abstract GameObject Instantiate(Transform parent);
        public abstract void ApplyTextures(GameObject gameObject);

        public abstract void LoadMesh();
        public abstract void LoadTextures();

    }
}
