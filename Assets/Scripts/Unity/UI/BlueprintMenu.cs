using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Assets.Scripts.Context;
using Assets.Scripts.Loaders;
using Assets.Scripts.Model.BlueprintObject;
using Assets.Scripts.Model.Data;
using Assets.Scripts.Model.Game;
using Assets.Scripts.Model.Unity;
using Assets.Scripts.Resolver;
using TMPro;
using UnityEngine;
using Button = UnityEngine.UI.Button;
using Debug = UnityEngine.Debug;
using Joint = Assets.Scripts.Model.Data.Joint;

namespace Assets.Scripts.Unity.UI
{
    public class BlueprintMenu : ThreadedMonoBehaviour
    {
        [SerializeField] private Transform contentPanel;
        [SerializeField] private SimpleObjectPool buttonObjectPool;
        [SerializeField] private TMP_InputField titleField;
        [SerializeField] private TMP_InputField descriptionField;
        [SerializeField] private Button applyButton;

        public static BlueprintButton SelectedBlueprintButton;
        public static BlueprintScript blueprintObject;
        

        public static readonly Dictionary<string, BlueprintContext> LoadedBlueprints = new Dictionary<string, BlueprintContext>();

        private void Start()
        {
            string[] paths = new string[] { Path.Combine(PathResolver.ScrapMechanicAppdataUserPath, "Blueprints"), PathResolver.WorkShopPath };
            foreach (string path in paths)
            {
                _ = LoadBlueprints(path);
            }
            
            // UI button events:
            titleField.onValueChanged.AddListener(_ => applyButton.gameObject.SetActive(true));
            descriptionField.onValueChanged.AddListener(_ => applyButton.gameObject.SetActive(true));

            applyButton.onClick.AddListener(() =>
            {
                BlueprintContext blueprintContextReference = SelectedBlueprintButton.BlueprintContextReference;
                DescriptionData descriptionData = blueprintContextReference.Description;
                try
                {
                    descriptionData.Name = titleField.text;
                    descriptionData.Description = descriptionField.text;
                    blueprintContextReference.SaveDescription();

                    BlueprintButton[] blueprintbuttons = Resources.FindObjectsOfTypeAll<Model.Unity.BlueprintButton>();
                    blueprintbuttons.First(button => button.BlueprintContextReference == blueprintContextReference).Initialize();

                    applyButton.gameObject.SetActive(false);
                    GameController.Instance.messageController.WarningMessage(
                        "Title and description changes have been saved.");
                }
                catch (Exception e)
                {
                    GameController.Instance.messageController.OkMessage(
                        $"Could not save {blueprintContextReference.BlueprintFolderPath}/description.json\n{e.Message}");
                }
            });

            // file watcher:
            foreach (string path in paths)
            {
                using var watcher = new FileSystemWatcher(path);
                watcher.NotifyFilter = NotifyFilters.Size;
                watcher.IncludeSubdirectories = true;
                watcher.EnableRaisingEvents = true;

                watcher.Changed += OnChanged;
            }
        }
        
        private void OnChanged(object sender, FileSystemEventArgs e)// different thread
        {
            try
            {
                string path = e.FullPath;
                bool exists = Directory.Exists(path) || File.Exists(path);

                if (exists)
                {
                    FileAttributes fileAttributes = File.GetAttributes(path);
                    if ((fileAttributes & FileAttributes.Directory) != FileAttributes.Directory)
                        path = Directory.GetParent(path)?.FullName;
                    if (LoadedBlueprints.TryGetValue(path, out var blueprintCtx))
                    {
                        _ = RunMain(() =>
                        {
                            blueprintCtx.LoadDescription();
                            blueprintCtx.LoadIcon();
                            blueprintCtx.btn.Initialize();
                            if (SelectedBlueprintButton == blueprintCtx.btn)
                                SelectBlueprintButton(SelectedBlueprintButton);
                        }).LogErrors();
                    }
                    else if (File.Exists($"{path}/blueprint.json"))
                    {
                        _ = RunMain(() =>
                        {
                            BlueprintContext bp = BlueprintContext.FromFolderPath(path);
                            LoadedBlueprints.Add(path, bp);

                            GameObject newButton = buttonObjectPool.GetObject();
                            newButton.transform.SetParent(contentPanel);
                            BlueprintButton blueprintButton = newButton.GetComponent<BlueprintButton>();
                            blueprintButton.Setup(bp);
                        }).LogErrors();
                    }
                }
                else
                {
                    if (path.Contains('.')) // assume it's a file
                        path = path.Substring(0, path.LastIndexOf(Path.DirectorySeparatorChar));
                    if (LoadedBlueprints.TryGetValue(path, out var blueprintCtx))
                    {
                        _ = RunMain(() =>
                        {
                            if (SelectedBlueprintButton == blueprintCtx.btn)
                            {
                                titleField.text = string.Empty;
                                descriptionField.text = string.Empty;
                                applyButton.gameObject.SetActive(false);
                            }
                            Destroy(blueprintCtx.btn.gameObject);
                            LoadedBlueprints.Remove(path);
                        }).LogErrors();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
        

        private async Task LoadBlueprints(string blueprintDirectory)
        {
            if (string.IsNullOrWhiteSpace(blueprintDirectory))
            {
                throw new ArgumentException($"'{nameof(blueprintDirectory)}' cannot be null or whitespace", nameof(blueprintDirectory));
            }
            var knownWriteDateTime = DateTime.MinValue;

            int exceptions = 0;
            while (true)
            {
                try
                {
                    var lastWriteDateTime = Directory.GetLastWriteTime(blueprintDirectory);

                    if (knownWriteDateTime < lastWriteDateTime)
                    {
                        knownWriteDateTime = lastWriteDateTime;

                        foreach (string bpDir in Directory.GetDirectories(blueprintDirectory))
                        {
                            if (!LoadedBlueprints.ContainsKey(bpDir) && Directory.Exists(bpDir) &&
                                File.Exists($"{bpDir}/blueprint.json"))
                            {
                                BlueprintContext bp = BlueprintContext.FromFolderPath(bpDir);
                                LoadedBlueprints.Add(bpDir, bp);

                                GameObject newButton = buttonObjectPool.GetObject();
                                newButton.transform.SetParent(contentPanel);
                                BlueprintButton blueprintButton = newButton.GetComponent<BlueprintButton>();
                                blueprintButton.Setup(bp);
                            }
                        }
                    }
                }
                catch(Exception e)
                {
                    if (exceptions++ < 10)
                        Debug.LogException(e);
                }
                finally
                {
                    await Task.Delay(2000);
                }
            }
        }


        


        public void SelectBlueprintButton(BlueprintButton blueprintButton)
        { 
            Color color;
            if (SelectedBlueprintButton != null)
            {
                color = SelectedBlueprintButton.button.image.color;
                color.a = 40f / 255f;
                SelectedBlueprintButton.button.image.color = color;
            }
            SelectedBlueprintButton = blueprintButton;

            color = blueprintButton.button.image.color;
            color.a = 100f / 255f;
            blueprintButton.button.image.color = color;
            DescriptionData descriptionData = blueprintButton.BlueprintContextReference.Description;

            titleField.SetTextWithoutNotify(descriptionData.Name);
            descriptionField.SetTextWithoutNotify(descriptionData.Description);
            applyButton.gameObject.SetActive(false);
        }

        public void OpenBlueprintFilePath()
        {
            if (SelectedBlueprintButton?.BlueprintContextReference?.BlueprintFolderPath == null)
            {
                GameController.Instance.messageController.WarningMessage($"Please select a blueprint before clicking 'open blueprint folder'");
                return;
            }

            Process.Start(SelectedBlueprintButton.BlueprintContextReference.BlueprintFolderPath);
        }
        public void OpenBlueprintSteam()
        {
            if (SelectedBlueprintButton?.BlueprintContextReference?.BlueprintFolderPath == null)
            {
                GameController.Instance.messageController.WarningMessage($"Please select a blueprint before clicking 'open steam page'");
                return;
            }

            long id = SelectedBlueprintButton.BlueprintContextReference.Description.FileId;
            if (id == 0)
            {
                GameController.Instance.messageController.WarningMessage($"Current selected blueprint doesn't have a steam page.");
                return;
            }
            System.Diagnostics.Process.Start("steam://openurl/https://steamcommunity.com/sharedfiles/filedetails/?id="+id);
        }
        /// <summary>
        /// click event that should trigger when clicking the 'load' button
        /// </summary>
        public void LoadSelectedBlueprint()
        {
            GameObject rootGameObject = null;
            try
            {
                if (BlueprintMenu.blueprintObject != null)
                {
                    //clean up existing blueprintobject
                    Destroy(BlueprintMenu.blueprintObject.gameObject);
                }

                Debug.Log("trying to load bp");

                BlueprintData blueprintData = SelectedBlueprintButton.BlueprintContextReference.LoadBlueprint();


                rootGameObject = Instantiate(GameController.Instance.Blueprint);
                BlueprintScript blueprintScript = rootGameObject.GetComponent<BlueprintScript>();

                int shapeIdx = 0;

                foreach (var body in blueprintData.Bodies)
                {
                    GameObject bodyGameObject = Instantiate(GameController.Instance.Body, rootGameObject.transform);
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
                PlayerController playerController = GameController.Instance.playerController;
                if (playerController.snapToCreation)
                {
                    Camera camera = playerController.gameObject.GetComponent<Camera>();
                    var center = blueprintScript.CalculateCenter();
                    var destination = center - camera.transform.forward * playerController.distanceToSnap;

                    var cameraState = playerController.m_TargetCameraState;

                    cameraState.x = destination.x;
                    cameraState.y = destination.y;
                    cameraState.z = destination.z;
                    //GameObject.Find("Sphere").transform.position = center;
                }
                #endregion

                BlueprintMenu.blueprintObject = blueprintScript;
                Debug.Log("successfully loaded bp");
            }
            catch (Exception e)
            {
                Debug.LogException(new Exception($"\nan error occurred while loading this blueprint.", e));
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
            MessageController messageController = GameController.Instance.messageController;
            if (BlueprintMenu.blueprintObject == null)
            {
                messageController.WarningMessage("Cannot save a creation that doesn't exist.\nLoad one first.",3);
                return;
            }

            //string blueprintDataStr = JsonConvert.SerializeObject(BlueprintLoader.blueprintObject.ToBlueprintData());
            messageController.YesNoMessage("Are you sure you want to save your creation to this blueprint?", 
                yesAction: () =>
                {
                    try
                    {
                        SelectedBlueprintButton.BlueprintContextReference.Blueprint = BlueprintMenu.blueprintObject.ToBlueprintData();
                        SelectedBlueprintButton.BlueprintContextReference.SaveBlueprint();
                        messageController.WarningMessage("Creation successfully saved!");
                    }
                    catch(Exception e)
                    {
                        messageController.WarningMessage("Something went wrong while saving your creation to this blueprint!\nCheck the log for more information", 5);
                        Debug.LogException(e, this);
                    }
                }, 
                noAction: () =>
                {
                    messageController.WarningMessage("Creation not saved.",2);
                });
        }

        /// <summary>
        /// triggers when clicking 'delete'
        /// </summary>
        public void DeleteSelectedBlueprint()
        {
            try
            {
                MessageController messageController = GameController.Instance.messageController;
                GameObject.Destroy(SelectedBlueprintButton.gameObject);
                LoadedBlueprints.Remove(SelectedBlueprintButton.BlueprintContextReference.BlueprintFolderPath);
                SelectedBlueprintButton.BlueprintContextReference.Delete();

                messageController.WarningMessage("Blueprint has been deleted.", 2f);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }


        // todo: move methods below to separate class
        private ChildScript CreateChildObject(Child child, GameObject parent)
        {
            Shape shape = PartLoader.GetShape(child);

            GameObject GameObject = shape.Instantiate(parent.transform);

            ChildScript childScript = GameObject.AddComponent<ChildScript>();
            childScript.shape = shape;

            childScript.Controller = child.Controller;
            childScript.SetColor(child.Color);

            if (!GameController.Instance.potatoMode)
                shape.ApplyTextures(GameObject);

            if (shape is Block && child.Bounds != null)
            {
                childScript.SetBlueprintBounds(child.Bounds);
            }

            childScript.SetBlueprintPosition(child.Pos);
            childScript.SetBlueprintRotation(child.Xaxis, child.Zaxis);


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

            if (!GameController.Instance.potatoMode)
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
                        Debug.LogError($"Invalid Joints! no joint.childA information. Attempting fix");

                        // todo: attempt fix: find child(s) that is connected to this jointscript, 1 child: childA, 2 childs: check childB || error
                    }
                    else
                    {
                        Debug.LogWarning($"joint.childA had invalid information! resolved via child.joints!");
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
                    if (jointScript.childB == null)
                    {
                        // this is allowed. a joint doesn't have to have a childB
                        // todo: attempt fix: find child(s) that is connected to this jointscript, 1 child: childA, 2 childs: check childB || ok
                    }
                    else
                    {
                        Debug.LogWarning($"joint.childB had invalid information! resolved via child.joints!"); // 'recovered' in prev step
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
            
        }

    }
}
