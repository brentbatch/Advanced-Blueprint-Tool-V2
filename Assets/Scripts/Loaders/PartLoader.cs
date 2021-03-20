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
using Assets.Scripts.Model.Shapes;
using Assets.Scripts.Context;
using Joint = Assets.Scripts.Model.Data.Joint;
using System.Threading;

namespace Assets.Scripts.Loaders
{
    public enum TextureMaterial
    {
        DifAsgNor,
        Regular,
        Glass
    }
    public class PartTextureInfo
    {
        public TextureMaterial material;
        public string diffusePath;
        public string normalPath;
        public string asgPath;
    }

    public class PartLoader : MonoBehaviour
    {
        public static AssimpContext importer = new AssimpContext();
        public static PartLoader Instance;

        public UnityEngine.Material material;
        public UnityEngine.Material glassMaterial;

        public static readonly ConcurrentDictionary<string, Shape> loadedShapes = new ConcurrentDictionary<string, Shape>();
        public static readonly ConcurrentDictionary<string, ModContext> mods = new ConcurrentDictionary<string, ModContext>();
        public static readonly ConcurrentDictionary<string, TranslationData> translations = new ConcurrentDictionary<string, TranslationData>();

        public void Awake()
        {
            if (Instance == null) // stupid singleton to make materials available anywhere
                Instance = this;
            importer = new AssimpContext();
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



        // todo: get rid of this:

        public Task<GameObject> InitPart(string meshPath, PartTextureInfo[] textureList)
        {
            return InitPart(meshPath, textureList, new Vector3(0, 0, 0), new Color(1,1,1));
        }

        public Task<GameObject> InitPart(string meshPath, PartTextureInfo[] textureInfoArray, Vector3 position, Color color)
        {
            // create 'parent' gameobject:
            GameObject Go = new GameObject();
            Go.transform.position = position;

            List<GameObject> subObjects = new List<GameObject>();
            try
            {
                if (File.Exists(textureInfoArray[1]?.diffusePath))
                {
                    Go.name = Path.GetFileName(textureInfoArray[1].diffusePath);
                }
                else if (File.Exists(textureInfoArray[1]?.normalPath))
                {
                    Go.name = Path.GetFileName(textureInfoArray[1].normalPath);
                }

                // load scene using assimp:
                Scene scene = LoadScene(meshPath);

                for (int i = 0; i < scene.MeshCount; i++)
                {
                    var unityMesh = ConvertMesh(scene.Meshes[i]);
                    var unityMaterial = CreateMaterial(textureInfoArray[i]);

                    unityMaterial.SetColor("_Color", color);

                    GameObject childObject = new GameObject();
                    subObjects.Add(childObject);
                    if (textureInfoArray[i]?.diffusePath != null)
                    {
                        childObject.name = Path.GetFileName(textureInfoArray[i]?.diffusePath);
                    }
                    else if (textureInfoArray[i]?.normalPath != null)
                    {
                        childObject.name = Path.GetFileName(textureInfoArray[i]?.normalPath);
                    }
                    childObject.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
                    childObject.transform.SetParent(Go.transform);
                    childObject.AddComponent<MeshFilter>();
                    childObject.AddComponent<MeshRenderer>();
                    childObject.GetComponent<MeshFilter>().mesh = unityMesh;
                    childObject.GetComponent<MeshRenderer>().material = unityMaterial;
                }
            }
            catch (Exception e)
            {
                // clean up created childobjects:
                subObjects.ForEach(obj => Destroy(obj));
                Destroy(Go);
                Debug.LogError($"An error occurred: {e.Message}{e.StackTrace}");
                return null;
            }
            return Task.FromResult(Go);
        }

        private Scene LoadScene(string meshPath)
        {
            //importer.SetConfig(new Assimp.Configs.MeshVertexLimitConfig(60000));
            //importer.SetConfig(new Assimp.Configs.MeshTriangleLimitConfig(60000));
            //importer.SetConfig(new Assimp.Configs.RemoveDegeneratePrimitivesConfig(true));
            //importer.SetConfig(new Assimp.Configs.SortByPrimitiveTypeConfig(Assimp.PrimitiveType.Line | Assimp.PrimitiveType.Point));

            Assimp.PostProcessSteps postProcessSteps = Assimp.PostProcessPreset.TargetRealTimeMaximumQuality | Assimp.PostProcessSteps.MakeLeftHanded | Assimp.PostProcessSteps.FlipWindingOrder;

            Scene scene = importer.ImportFile(meshPath);
            //importer.Dispose();
            return scene;
        }

        private UnityEngine.Mesh ConvertMesh(Assimp.Mesh mesh)
        {
            UnityEngine.Mesh unityMesh = new UnityEngine.Mesh
            {
                vertices = mesh.Vertices.Select(v => new Vector3(-v.X, v.Y, v.Z)).ToArray(),
                triangles = GetTriangles(mesh.Faces).ToArray(),
                normals = mesh.Normals?.Select(n => new Vector3(-n.X, n.Y, n.Z)).ToArray()
            };
            for (int i = 0; i < mesh.TextureCoordinateChannelCount && i < 8; i++)
            {
                unityMesh.SetUVs(i, mesh.TextureCoordinateChannels[i]?.Select(coord => new Vector2(coord.X, coord.Y)).ToArray());
            }
            return unityMesh;
        }

        IEnumerable<int> GetTriangles(List<Face> faces)
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
                        Debug.LogError("not implemented: more than 5 vertices in one face!");
                        break;
                }
            }
        }

        private UnityEngine.Material CreateMaterial(PartTextureInfo info)
        {
            UnityEngine.Material newMaterial;

            switch (info?.material)
            {
                case TextureMaterial.Glass:
                    newMaterial = new UnityEngine.Material(glassMaterial);
                    break;
                default:
                    newMaterial = new UnityEngine.Material(material);
                    break;
            }

            if (info?.diffusePath != null)
            {
                
                newMaterial.SetTexture("_MainTex", /*TGALoader.LoadTGA(info.diffusePath) );//*/ TgaDecoder.TgaDecoder.FromFile(info.diffusePath));
            }
            if (info?.normalPath != null)
            {
                newMaterial.SetTexture("_NorTex", /*TGALoader.LoadTGA(info.normalPath)); //*/TgaDecoder.TgaDecoder.FromFile(info.normalPath));
            }
            if (info?.asgPath != null)
            {
                newMaterial.SetTexture("_AsgTex", TgaDecoder.TgaDecoder.FromFile(info.asgPath));
            }
            return newMaterial;
        }

    }
}
