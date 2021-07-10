using Assets.Scripts.Loaders;
using Assets.Scripts.Model.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Context
{
    public class ModContext
    {
        public DescriptionData Description { get; set; }
        public string ModFolderPath { get; protected set; }

        public ConcurrentBag<string> partUuids = new ConcurrentBag<string>();


        public static bool IsValidModPath(string modPath) =>
            File.Exists(Path.Combine(modPath, "description.json")) &&
            Directory.Exists(Path.Combine(modPath, "Objects", "Database", "ShapeSets"));

        public static bool IsValidShapeListPath(string shapeListPath) =>
            Path.GetExtension(shapeListPath).ToLower() == ".json";


        public static ModContext FromFolderPath(string path) => new ModContext()
            {
                ModFolderPath = path,
                Description = LoadDescription(path)
            };

        private static DescriptionData LoadDescription(string path)
        {
            // try to load the description.
            try
            {
                return JsonConvert.DeserializeObject<DescriptionData>(
                    File.ReadAllText($"{path}/description.json"));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Found a mod with unloadable description.json, creating replacement.\nError: {e}");
                var newguid = Guid.NewGuid().ToString();
                return new DescriptionData()
                {
                    Description = "unloadable mod description (deserialisation error)",
                    LocalId = newguid,
                    Name = $"Unnamed mod {newguid}"
                };
            }
        }


        public void LoadModShapes()
        {
            Parallel.ForEach(Directory.GetFiles(Path.Combine(this.ModFolderPath, "Objects", "Database", "ShapeSets")),
                shapeListFilePath =>
                {
                    if (IsValidShapeListPath(shapeListFilePath))
                    {
                        try
                        {
                            var partListData = JsonConvert.DeserializeObject<PartListData>(File.ReadAllText(shapeListFilePath));

                            PartLoader.LoadShapes(partListData, this);

                            if (partListData.PartList != null)
                                foreach (var part in partListData.PartList)
                                {
                                    this.partUuids.Add(part.Uuid);
                                }
                            if (partListData.BlockList != null)
                                foreach (var block in partListData.BlockList)
                                {
                                    this.partUuids.Add(block.Uuid);
                                }
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(new Exception($"\nAn error occurred when trying to load mod data", e));
                        }
                    }
                });
        }

        public string GetSteamURL()
        {
            throw new NotImplementedException();
        }
    }
}
