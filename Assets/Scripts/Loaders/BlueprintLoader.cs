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
using Joint = Assets.Scripts.Model.Data.Joint;

namespace Assets.Scripts.Loaders
{
    class BlueprintLoader : MonoBehaviour
    {
        public Transform contentPanel;
        public SimpleObjectPool buttonObjectPool;

        public static BlueprintButton SelectedBlueprintButton;
        public static BlueprintScript blueprintObject;

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
                BlueprintScript blueprintScript = rootGameObject.GetComponent<BlueprintScript>();

                foreach (var body in blueprintData.Bodies)
                {
                    GameObject bodyGameObject = Instantiate(Constants.Instance.Body, rootGameObject.transform);
                    BodyScript bodyScript = bodyGameObject.GetComponent<BodyScript>();

                    blueprintScript.Bodies.Add(bodyScript);
                    foreach (var child in body.Childs)
                    {
                        bodyScript.Childs.Add(CreateChildObject(child, bodyGameObject));
                    }
                }
                if (blueprintData.Joints != null)
                {
                    foreach (var joint in blueprintData.Joints)
                    {
                        blueprintScript.Joints.Add(CreateJointObject(joint, rootGameObject));
                    }
                }

                ArrangeJointReferencesUsingData(blueprintScript, blueprintData);

                #region handle camera position
                var center = blueprintScript.CalculateCenter();
                var destination = center - Camera.transform.forward * 15;
                var cameraState = Camera.GetComponent<UnityTemplateProjects.SimpleCameraController>().m_TargetCameraState;

                cameraState.x = destination.x;
                cameraState.y = destination.y;
                cameraState.z = destination.z;
                #endregion
                
                BlueprintLoader.blueprintObject = blueprintScript;
                Debug.Log("successfully loaded bp");
            }
            catch (Exception e)
            {
                Debug.LogError($"an error occurred while loading this blueprint.\nError: {e}\n\nStackTrace:{StackTraceUtility.ExtractStringFromException(e)}");
                if(rootGameObject != null)
                {
                    Debug.Log("failed loading blueprint");
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

        private ChildScript CreateChildObject(Child child, GameObject parent)
        {
            Shape shape = PartLoader.GetShape(child);

            GameObject GameObject = shape.Instantiate(parent.transform);

            ChildScript childScript = GameObject.AddComponent<ChildScript>();
            childScript.shape = shape;

            childScript.Controller = child.Controller;
            childScript.SetColor(child.Color);

            if (!Constants.Instance.potatoMode)
                shape.ApplyTextures(GameObject);

            childScript.SetBlueprintPosition(child.Pos);
            childScript.SetBlueprintRotation(child.Xaxis, child.Zaxis);

            if (shape is Block && child.Bounds != null)
            {
                childScript.SetBlueprintBounds(child.Bounds);
            }

            return childScript;
        }

        private JointScript CreateJointObject(Joint joint, GameObject parent)
        {
            Shape shape = PartLoader.GetJoint(joint);

            GameObject gameObject = shape.Instantiate(parent.transform);
            
            JointScript jointScript = gameObject.AddComponent<JointScript>();
            jointScript.shape = shape;

            jointScript.Id = joint.Id;
            jointScript.Controller = joint.Controller; // todo: this is not gud

            jointScript.SetColor(joint.Color);

            if (!Constants.Instance.potatoMode)
                shape.ApplyTextures(gameObject);

            jointScript.SetBlueprintPosition(joint.PosA);
            jointScript.SetBlueprintRotation(joint.XaxisA, joint.ZaxisA);

            jointScript.DoRotationPositionOffset(joint.XaxisA, joint.ZaxisA); // required for joints

            return jointScript;
        }

        private void ArrangeJointReferencesUsingData(BlueprintScript blueprintScript, BlueprintData blueprintData)
        {
            var dtstart = DateTime.Now;

            // child -> joint is a bit of a pain because scriptchild knows nothing of joints[]
            int childIndex = 0;
            IEnumerable<Child> flatDataChildList = blueprintData.Bodies.SelectMany(body => body.Childs).Select(child => child);
            var flatLiveChildList = blueprintScript.Bodies.SelectMany(body => body.Childs).Select(child => child).ToList();
            
            foreach (var child in flatDataChildList)
            {
                if (child.Joints?.Count > 0)
                {
                    var childScript = flatLiveChildList[childIndex];

                }
                childIndex++;
            }

            IEnumerable<Joint> flatDataJointList = blueprintData.Joints;
            var flatLiveJointsList = blueprintScript.Joints;

            // do the same like above

            var dt = DateTime.Now - dtstart;
            Debug.Log($"arrangejoints took {dt.TotalMilliseconds} ms");
        }

    }
}
