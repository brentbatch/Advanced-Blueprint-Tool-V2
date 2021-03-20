using Assets.Scripts.Model.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Context
{
    public class BlueprintContext
    {
        public BlueprintData Blueprint { get; set; }
        public DescriptionData Description { get; set; }
        public Texture2D Icon { get; set; }

        public string BlueprintFolderPath { get; protected set; }
        public DateTime LastEditDateTime { get; protected set; }

        public static BlueprintContext FromBlueprintPath(string path)
        {
            return FromFolderPath(Directory.GetParent(path).FullName);
        }

        public static BlueprintContext FromFolderPath(string path)
        {
            if (!Directory.Exists(path))
            {
                throw new Exception($"Blueprint folder does not exist! {path}");
            }
            if (!File.Exists($"{path}/blueprint.json"))
            {
                Debug.LogError($"Blueprint file does not exist! {path}/blueprint.json");
            }
            if (!File.Exists($"{path}/description.json"))
            {
                Debug.LogWarning($"Description file does not exist! {path}/description.json");
            }

            return new BlueprintContext
            {
                BlueprintFolderPath = path,
                LastEditDateTime = Directory.GetLastWriteTime(path)
            };
        }


        public BlueprintData LoadBlueprint()
        {
            try
            {
                Blueprint = JsonConvert.DeserializeObject<BlueprintData>(File.ReadAllText($"{this.BlueprintFolderPath}/blueprint.json"));
                return Blueprint;
            }
            catch (Exception e)
            {
                throw new Exception($"Could not load {this.BlueprintFolderPath}/blueprint.json", e);
            }
        }

        public void LoadDescription()
        {
            try
            {
                Description = JsonConvert.DeserializeObject<DescriptionData>(File.ReadAllText($"{this.BlueprintFolderPath}/description.json"));
            }
            catch (Exception e)
            {
                // todo: this can be resolved! create (inmemory) new description
                throw new Exception($"Could not load {this.BlueprintFolderPath}/description.json", e);
            }
        }

        public void LoadIcon()
        {
            Texture2D tex = new Texture2D(2,2);
            try
            {
                tex.LoadImage(File.ReadAllBytes($"{this.BlueprintFolderPath}/icon.png"));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Could not load icon, using plain texture.\nError: {e}");
            }
            finally
            {
                this.Icon = tex;
            }
        }

        public void Refresh()
        {
            var lastEditDateTime = Directory.GetLastWriteTime(this.BlueprintFolderPath);
            if (lastEditDateTime > this.LastEditDateTime)
            {
                this.LoadIcon();
                this.LoadDescription();
            }
            this.LastEditDateTime = lastEditDateTime;
        }

        public bool SaveAs()
        {
            //create new path to save at
            return Save();
        }

        public bool Save()
        {
            // use bppath
            // todo: serialize & save data

            string blueprintString = JsonConvert.SerializeObject(this.Blueprint, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });


            return true;
        }
    }
}
