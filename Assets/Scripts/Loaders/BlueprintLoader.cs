using Assets.Scripts.Context;
using Assets.Scripts.Model;
using Assets.Scripts.Model.BlueprintObject;
using Assets.Scripts.Model.Data;
using Assets.Scripts.Model.Game;
using Assets.Scripts.Model.Unity;
using Assets.Scripts.Unity;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Loaders
{
    class BlueprintLoader : MonoBehaviour
    {
        public Transform contentPanel;
        public SimpleObjectPool buttonObjectPool;

        public static BlueprintButton SelectedBlueprintButton;
        public static BlueprintObject blueprintObject;

        public Camera Camera;

        public static readonly ConcurrentBag<string> KnownBlueprintPaths = new ConcurrentBag<string>();
        public static readonly BlockingCollection<BlueprintContext> LoadedBlueprints = new BlockingCollection<BlueprintContext>();

        private static readonly Dictionary<string, Coroutine> loadBlueprintsCoroutine = new Dictionary<string, Coroutine>();

        private void Start()
        {
            StartLoadBlueprints();
            DoAutoRefreshBlueprints();
            //StartCoroutine(nameof(DelayedLoadTextures));
        }

        private void StartLoadBlueprints()
        {
            string[] paths = new string[] { Path.Combine(PathResolver.ScrapMechanicAppdataUserPath, "blueprints"), PathResolver.WorkShopPath };

            foreach (string path in paths)
            {
                if (loadBlueprintsCoroutine.ContainsKey(path))
                    StopCoroutine(loadBlueprintsCoroutine[path]);

                loadBlueprintsCoroutine.Add(path, StartCoroutine(nameof(LoadBlueprints), path));
            }
        }

        private void DoAutoRefreshBlueprints()
        {
            StartCoroutine(nameof(AutoRefreshBlueprints));
        }

        IEnumerator LoadBlueprints(string blueprintDirectory)
        {
            if (string.IsNullOrWhiteSpace(blueprintDirectory))
            {
                throw new ArgumentException($"'{nameof(blueprintDirectory)}' cannot be null or whitespace", nameof(blueprintDirectory));
            }
            var knownWriteDateTime = DateTime.MinValue;

            while (true)
            {
                var lastWriteDateTime = Directory.GetLastWriteTime(blueprintDirectory);

                if (knownWriteDateTime < lastWriteDateTime)
                {
                    Debug.Log($"start loading bps {DateTime.Now:O}");
                    foreach (string bpDir in Directory.GetDirectories(blueprintDirectory))
                    {
                        if (!KnownBlueprintPaths.Contains(bpDir) && Directory.Exists(bpDir) && File.Exists($"{bpDir}/blueprint.json"))
                        {
                            BlueprintContext bp = BlueprintContext.FromFolderPath(bpDir);
                            LoadedBlueprints.Add(bp);
                            KnownBlueprintPaths.Add(bpDir);

                            GameObject newButton = buttonObjectPool.GetObject();
                            newButton.transform.SetParent(contentPanel);
                            BlueprintButton blueprintButton = newButton.GetComponent<BlueprintButton>();
                            blueprintButton.Setup(bp);
                        }
                    }
                    Debug.Log($"blueprints loaded {DateTime.Now:O}");
                    knownWriteDateTime = lastWriteDateTime;
                }
                yield return new WaitForSeconds(2f); // check for new blueprint every 2 sec
            }
        }

        IEnumerator AutoRefreshBlueprints()
        {
            while(true)
            {
                try
                {
                    foreach (var bp in LoadedBlueprints)
                    {
                        bp.Refresh();
                    }
                }
                catch(Exception e)
                {
                    Debug.LogError($"Error while refreshing blueprints: {e}");
                }
                yield return new WaitForSeconds(10f);
            }
        }

        /// <summary>
        /// click event that should trigger when clicking the 'load' button
        /// </summary>
        public void LoadSelectedBlueprint()
        {
            GameObject rootGameObject = null;
            try
            {
                if (BlueprintLoader.blueprintObject != null)
                {
                    //clean up existing blueprintobject
                    Destroy(BlueprintLoader.blueprintObject.gameObject);
                }

                Debug.Log("trying to load bp");

                BlueprintData blueprintData = SelectedBlueprintButton.BlueprintContextReference.LoadBlueprint();


                rootGameObject = Instantiate(Constants.Instance.Blueprint);
                BlueprintObject newblueprintObject = rootGameObject.GetComponent<BlueprintObject>();

                foreach (var body in blueprintData.Bodies)
                {
                    GameObject bodyGameObject = Instantiate(Constants.Instance.Body, rootGameObject.transform);
                    BodyObject bodyObject = bodyGameObject.GetComponent<BodyObject>();

                    newblueprintObject.Bodies.Add(bodyObject);
                    foreach (var child in body.Childs)
                    {
                        // todo: split CreateChildObject into smaller functions and use here
                        bodyObject.Childs.Add(CreateChildObject(child, bodyGameObject));
                    }
                }
                if (blueprintData.Joints != null)
                {
                    foreach (var joint in blueprintData.Joints)
                    {
                        // todo
                    }
                }

                var destination = newblueprintObject.Bodies[0].Childs[0].gameObject.transform.position - Camera.transform.forward * 10;

                var cameraState = Camera.GetComponent<UnityTemplateProjects.SimpleCameraController>().m_TargetCameraState;

                cameraState.x = destination.x;
                cameraState.y = destination.y;
                cameraState.z = destination.z;

                BlueprintLoader.blueprintObject = newblueprintObject;
                Debug.Log("loaded bp");
            }
            catch (Exception e)
            {
                Debug.LogError($"an error occurred while loading this blueprint.\nError: {e}\n\nStackTrace:{StackTraceUtility.ExtractStringFromException(e)}");
                if(rootGameObject != null)
                {
                    Debug.Log("destroying broken build blueprint");
                    Destroy(rootGameObject);
                }

            }
        }

        /// <summary>
        /// triggers when clicking 'delete'
        /// </summary>
        public void DeleteSelectedBlueprint()
        {
            try
            {
                Debug.Log("trying to delete bp");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            throw new NotImplementedException();
        }

        private ChildObject CreateChildObject(Child child, GameObject parent)
        {
            Shape shape = PartLoader.GetShape(child);

            GameObject childGameObject = shape.Instantiate(parent.transform);

            ChildObject childObject = childGameObject.AddComponent<ChildObject>();

            childObject.SetColor(child.Color);

            if(!Constants.Instance.potatoMode)
                shape.ApplyTextures(childGameObject);

            childObject.SetBlueprintPosition(child.Pos);
            childObject.SetBlueprintRotation(child.Xaxis, child.Zaxis);

            if (shape is Block && child.Bounds != null)
            {
                childObject.SetBlueprintBounds(child.Bounds);
            }

            return childObject;
        }



    }
}
