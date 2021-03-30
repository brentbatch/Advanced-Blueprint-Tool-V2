using Assimp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using Newtonsoft.Json;
using Assets.Scripts.Model.Data;
using System.Collections.Concurrent;
using Assets.Scripts.Model.Game;
using Assets.Scripts.Context;
using Joint = Assets.Scripts.Model.Data.Joint;
using System.Threading;

namespace Assets.Scripts.Loaders
{
    public class PartLoader : MonoBehaviour
    {
        public static PartLoader Instance;

        public static readonly ConcurrentDictionary<string, Shape> loadedShapes = new ConcurrentDictionary<string, Shape>();
        public static readonly ConcurrentDictionary<string, ModContext> mods = new ConcurrentDictionary<string, ModContext>();
        public static readonly ConcurrentDictionary<string, TranslationData> translations = new ConcurrentDictionary<string, TranslationData>();

        public void Awake()
        {
            if (Instance == null) // stupid singleton to make materials available anywhere
                Instance = this;
        }

        public void Start()
        {
            DoLoadParts();
            var t = new Thread(() =>
            {
                //PreLoadPartMeshes();
            });
            t.Start();
            //StartCoroutine(nameof(PreLoadPartTextures));
        }

        public void DoLoadParts()
        {
            // vanilla shapes/blocks:
            Debug.Log($"Start Loading parts - Vanilla Game {DateTime.Now:O}");
            string basePath = PathResolver.ResolvePath("$Game_data");
            LoadVanilla(basePath);
            Debug.Log($"Start Loading parts - Vanilla Survival {DateTime.Now:O}");
            basePath = PathResolver.ResolvePath("$survival_data");
            LoadVanilla(basePath);
            Debug.Log($"Start Loading parts - Vanilla Challenge {DateTime.Now:O}");
            basePath = PathResolver.ResolvePath("$challenge_data");
            LoadVanilla(basePath);

            // workshop mods:
            Debug.Log($"Start Loading mods - Workshop {DateTime.Now:O}");
            string appdataModsPath = Path.Combine(PathResolver.WorkShopPath);
            LoadMods(appdataModsPath);

            // appdata mods:
            Debug.Log($"Start Loading mods - Appdata {DateTime.Now:O}");
            string localModsPath = Path.Combine(PathResolver.ScrapMechanicAppdataUserPath, "Mods");
            LoadMods(localModsPath);

            Debug.Log($"Finished Loading parts - {DateTime.Now:O}");

        }

        IEnumerator PreLoadPartTextures() // needs to run threaded!!!!!
        {
            Debug.Log($"start preloading part meshes");
            int i = 0;
            int length = loadedShapes.Count;
            var enumerator = loadedShapes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                enumerator.Current.Value.LoadTextures();
                yield return null;
                Debug.Log($"preloaded {i++}/{length} shapes");
            }
        }

        
        public void LoadVanilla(string basePath)
        {
            string shapeSetFilePath = Path.Combine(basePath, "Objects/Database/shapesets.json");

            var shapeSets = JsonConvert.DeserializeObject<ShapeSetListData>(File.ReadAllText(shapeSetFilePath));

            Parallel.ForEach(shapeSets.ShapeSetList,
                partListPath =>
                {
                    var partListFilePath = PathResolver.ResolvePath(partListPath);
                    try
                    {
                        var partListData = JsonConvert.DeserializeObject<PartListData>(File.ReadAllText(partListFilePath));
                        LoadShapes(partListData);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Could not load vanilla partlist/blocklist: {e}");
                    }
                });
            LoadLanguageFiles(Path.Combine(basePath, "Gui/Language"));
        }

        public void LoadMods(string modsPath)
        {
            if (Directory.Exists(modsPath))
            {
                foreach (string localModPath in Directory.GetDirectories(modsPath))
                {
                    if (!ModContext.IsValidModPath(localModPath))
                    {
                        // Debug.LogWarning($"Invalid mod path, skipping {localModPath}"); // there are also blueprints in this folder
                        continue;
                    }
                    var mod = ModContext.FromFolderPath(localModPath);
                    if (mods.ContainsKey(mod.Description.LocalId))
                    {
                        Debug.LogWarning($"This mod is already loaded, skipping {localModPath}");
                    }
                    else
                    {
                        mods.TryAdd(mod.Description.LocalId, mod);
                        mod.LoadModShapes();
                        LoadLanguageFiles(Path.Combine(mod.ModFolderPath, "Gui", "Language"));
                    }
                }
            }
        }

        public static void LoadShapes(PartListData partListData, ModContext mod = null) // modUuid can be "vanilla"
        {
            if (partListData.PartList != null)
                Parallel.ForEach(partListData.PartList, partData =>
                {
                    var part = new Part(partData, mod);
                    loadedShapes.TryAdd(partData.Uuid, part);
                });

            if (partListData.BlockList != null)
                Parallel.ForEach(partListData.BlockList, blockData =>
                {
                    var block = new Block(blockData, mod);
                    loadedShapes.TryAdd(blockData.Uuid, block);
                });
        }

        public static void LoadLanguageFiles(string languageFolderPath)
        {
            try
            {
                // Debug.Log($"Loading Language files {DateTime.Now:O}"); too much logging
                languageFolderPath = Path.Combine(languageFolderPath, "English");
                if (!Directory.Exists(languageFolderPath))
                    return;

                foreach(string languageFile in Directory.GetFiles(languageFolderPath))
                {
                    if (Path.GetExtension(languageFile).ToLower() != ".json" || !languageFile.ToLower().Contains("inventory"))
                        continue;

                    string text = File.ReadAllText(languageFile);
                    LanguageData languageData = JsonConvert.DeserializeObject<LanguageData>(text);

                    if (languageData == null)
                        continue;
                    foreach(var languageKV in languageData)
                    {
                        string key = languageKV.Key;
                        var translation = languageKV.Value;
                        translations.TryAdd(key, translation);
                        Shape shape = loadedShapes.FirstOrDefault(shapeKV => shapeKV.Key == key).Value;
                        if (shape != null)
                        {
                            shape.translation = translation;
                        }
                    }
                }
            } 
            catch (Exception e)
            {
                Debug.LogError($"Could not load translation data in {languageFolderPath}\nError: {e}");
            }
        }


        public static Shape GetShape(Child child)
        {
            var shape = loadedShapes.GetOrAdd(child.ShapeId, 
                (key) => Shape.CreateBlank(child));
            return shape;
        }

        public static Shape GetJoint(Joint joint)
        {
            var shape = loadedShapes.GetOrAdd(joint.ShapeId, 
                (key) => Shape.CreateBlank(joint));
            return shape;
        }

    }
}
