using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using Assets.Scripts.Model.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleFileBrowser;
using System.Collections;
using UnityEditor;
using SFB;
using Assets.Scripts.Loaders;

public class FileManager : MonoBehaviour
{
    public PartLoader partloader;

    private void Start()
    {

    }
    public void Open()
    {
        StartCoroutine(nameof(OpenExplorer));
    }


    public void OpenExplorer() // test code
    {
        /*
        int loaded = 0;
        Debug.Log($"loading all resources");
        foreach (var shape in PartLoader.loadedShapes.Values)
        {
            Debug.Log($"{DateTime.Now:O} loading: {++loaded}\npart {shape.translation?.Title} in {shape.mod?.Description?.Name}");
            shape.loadData();
            yield return null;
        }
        _ = PartLoader.CreateShapeGameObject(new Child()
        {
            ShapeId = "685ccdd5-8ee9-480d-a64a-2696c31e1db0"
        });
        //*/
        /*
        string modelPath = "";
        StandaloneFileBrowser.OpenFilePanelAsync("Select a folder", "C:", "", false, (string[] paths) => 
        {
            modelPath = paths.First();
        });


        //modelPath = EditorUtility.OpenFilePanel("open a blueprint", "", "");
        yield return null;

        //Debug.Log($"{Directory.GetParent(modelPath).FullName}");
        //BlueprintData blueprint = JsonConvert.DeserializeObject<BlueprintData>(File.ReadAllText(modelPath));
        //Debug.Log($"{JObject.FromObject(blueprint)}");
    
        List<PartTextureInfo> textures = new List<PartTextureInfo>();
        string path;
        while (true)
        {
            path = EditorUtility.OpenFilePanel("open a DIFFUSE texture", "", "");
            string nor = EditorUtility.OpenFilePanel("open a NORMAL texture", "", "");

            textures.Add(new PartTextureInfo()
            {
                diffusePath = path,
                normalPath = nor,
                material = TextureMaterial.Regular
            });
            if (!File.Exists(path) && !File.Exists(nor))
                break;
        }
        partloader.InitPart(modelPath, new PartTextureInfo[] { } );// textures.ToArray());
        //*/
    }

}
