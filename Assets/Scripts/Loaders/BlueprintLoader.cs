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

                int shapeIdx = 0;

                foreach (var body in blueprintData.Bodies)
                {
                    GameObject bodyGameObject = Instantiate(Constants.Instance.Body, rootGameObject.transform);
                    BodyScript bodyScript = bodyGameObject.GetComponent<BodyScript>();

                    blueprintScript.Bodies.Add(bodyScript);
                    foreach (var child in body.Childs)
                    {
                        ChildScript childScript = CreateChildObject(child, bodyGameObject);
                        childScript.shapeIdx = shapeIdx;
                        childScript.Body = bodyScript;

                        bodyScript.Childs.Add(childScript);
                        shapeIdx++;
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
                Debug.LogError($"an error occurred while loading this blueprint.\nError: {e}\n\nStackTrace:{e.StackTrace}");
                if(rootGameObject != null)
                {
                    Debug.Log("failed loading blueprint");
                    Destroy(rootGameObject);
                }
            }
        }

        /// <summary>
        /// click event that saves the blueprint
        /// </summary>
        public void SaveBlueprint()
        {
            // todo




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

        /// <summary>
        /// create child - joint references so both know what they are connected to
        /// </summary>
        /// <param name="blueprintScript"></param>
        /// <param name="blueprintData"></param>
        private void ArrangeJointReferencesUsingData(BlueprintScript blueprintScript, BlueprintData blueprintData)
        {
            Debug.Log($"Arranging child-joint connection references");
            var dtstart = DateTime.Now;

            // child -> joint is a bit of a pain because scriptchild knows nothing of joints[]
            int childIndex = 0;
            IEnumerable<Child> flatDataChildList = blueprintData.Bodies.SelectMany(body => body.Childs).Select(child => child);
            List<ChildScript> flatLiveChildList = blueprintScript.Bodies.SelectMany(body => body.Childs).Select(child => child).ToList();

            IEnumerable<Joint> flatDataJointList = blueprintData.Joints;
            List<JointScript> flatLiveJointsList = blueprintScript.Joints;

            if (blueprintData.Joints == null || blueprintData.Joints.Count == 0)
            {
                Debug.Log($"no joints found, skipping reference step");
                return;
            }

            foreach (Child childData in flatDataChildList)
            {
                if (childData.Joints?.Count > 0)
                {
                    ChildScript childScript = flatLiveChildList[childIndex];
                    childScript.connectedJoints = new List<JointScript>();
                    foreach(JointReference jointRef in childData.Joints)
                    {
                        JointScript jointScript = flatLiveJointsList.FirstOrDefault(joint => joint.Id == jointRef.Id);
                        if (jointScript == default)
                        {
                            Debug.LogWarning($"child.joints: reference to missing joint!");
                            continue;
                        }

                        childScript.connectedJoints.Add(jointScript);

                        // connect that joint to this child if it has the data to connect to this child:
                        Joint jointData = flatDataJointList.FirstOrDefault(joint => joint.Id == jointRef.Id);
                        if (jointData.ChildA == childScript.shapeIdx)
                        {
                            jointScript.childA = childScript;
                        }
                        if (jointData.ChildB == childScript.shapeIdx)
                        {
                            jointScript.childB = childScript;
                        }
                    }
                }
                childIndex++;
            }

            //Debug.Log($"Verifying joint-child connection references");
            // verify connections, perspective: joint
            int jointIndex = 0;
            foreach (Joint jointData in flatDataJointList)
            {
                JointScript jointScript = flatLiveJointsList[jointIndex];

                if (jointData.ChildA == -1 || jointData.ChildA >= flatLiveChildList.Count)
                {
                    if (jointScript.childA == null)
                    {
                        Debug.LogError($"Invalid joint.childA information. Attempting fix");

                        // todo: attempt fix: find child(s) that is connected to this jointscript, 1 child: childA, 2 childs: check childB || error
                    }
                    else
                    {
                        Debug.LogWarning($"Invalid joint.childA information. resolved with joint.childA info!");
                    }
                }
                else
                {
                    ChildScript childScriptA = flatLiveChildList[jointData.ChildA];
                    if (childScriptA.connectedJoints == null)
                        childScriptA.connectedJoints = new List<JointScript>();

                    if (!childScriptA.connectedJoints.Contains(jointScript))
                    {
                        Debug.LogWarning($"child.joints had missing information. resolving with joint.childA info!");
                        childScriptA.connectedJoints.Add(jointScript);
                    }
                }

                if (jointData.ChildB == -1 || jointData.ChildB >= flatLiveChildList.Count)
                {
                    if (jointScript.childB != null)
                    {
                        Debug.LogWarning($"joint.childB had invalid information! resolved via child.joints!"); // 'recovered' in prev step
                    }
                    else
                    {
                        // todo: attempt fix: find child(s) that is connected to this jointscript, 1 child: childA, 2 childs: check childB || error 
                    }
                }
                else
                {
                    ChildScript childScriptB = flatLiveChildList[jointData.ChildB];
                    if (childScriptB.connectedJoints == null)
                        childScriptB.connectedJoints = new List<JointScript>();

                    if (!childScriptB.connectedJoints.Contains(jointScript))
                    {
                        Debug.LogWarning($"child.joints had missing information. resolving with joint.childA info!");
                        childScriptB.connectedJoints.Add(jointScript);
                    }
                }
                jointIndex++;
            }

            // todo: to be complete there should be another verify run on all childs to check if connected joints actually reference them back.


            var dt = DateTime.Now - dtstart;
            Debug.Log($"arrangejoints took {dt.TotalMilliseconds} ms");
        }

    }
}
