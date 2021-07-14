using Assets.Scripts.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Assets.Scripts.Loaders;

namespace Assets.Scripts.Tool
{
    public class LiftTool : AbstractTool
    {
        /*
         * protected static PlayerController PlayerController { get; private set; }
         * protected static TMP_Text TitleText { get; private set; }
         * protected static TMP_Text DescriptionText { get; private set; }
         * 
         * protected readonly string title;
         * protected readonly string description;
         * public Sprite sprite;
         * 
         * public List<ToolFunction> functions;
         * public ToolFunction selectedToolFunction;
         * public int SelectedToolIndex { get; private set; }
         * 
         * protected bool leftClickButtonState;
         * protected bool rightClickButtonState;
         * protected bool interactButtonState;
         * protected bool nextRotationButtonState;
         * protected bool previousRotationButtonState;
         * protected bool rButtonState;
         * protected bool fButtonState;
         * protected bool tabButtonState;
         * protected bool escButtonState; 
         */
        readonly Canvas blueprintLoadCanvas;

        public LiftTool() : base(title: "LiftTool", description: "")
        {
            sprite = Resources.Load<Sprite>("Textures/lift");
            functions = new List<ToolFunction>()
            {
                new ToolFunction { title = "load/save", description = "load/save blueprint", OnInteract = ToggleLoadBlueprintMenu, sprite = Resources.Load<Sprite>( "Textures/blueprint_download" )},
                //new ToolFunction { title = "save", description = "save blueprint", OnInteract = ToggleSaveCreationMenu, sprite = Resources.Load<Sprite>( "Textures/blueprint_upload" )},
                //new ToolFunction { title = "info", description = "save blueprint", OnInteract = ToggleSaveCreationMenu, sprite = Resources.Load<Sprite>( "Textures/blueprint_upload" )}, // part count, mods?, center, (wires), new image ...
                new ToolFunction { title = "clear", description = "clear scene", OnInteract = ClearScene, sprite = Resources.Load<Sprite>( "Textures/clear" )},
                //new ToolFunction { title = "delete", description = "delete blueprint", OnInteract = null, sprite = Resources.Load<Sprite>( "Textures/delete" )},
            };
            selectedToolFunction = functions[0];

            // find the UI components & add listeners to buttons:
            blueprintLoadCanvas = GameObject.Find("BlueprintLoadCanvas").GetComponent<Canvas>();
            blueprintLoadCanvas.enabled = false;

            GameObject menu = blueprintLoadCanvas.gameObject.transform.Find("Menu").gameObject;
            GameObject actionPanel = menu.transform.Find("ActionPanel").gameObject;

            Button exitBtn = menu.transform.Find("Exit").GetComponent<Button>();
            exitBtn.onClick.AddListener(() => CloseLoadBlueprintMenu());

            Button loadBtn = actionPanel.transform.Find("Load").GetComponent<Button>();
            loadBtn.onClick.AddListener(() => CloseLoadBlueprintMenu());
        }

        public override void SelectToolFunction(int index)
        {
            CloseAllUI();
            base.SelectToolFunction(index);
        }
        public override void OnUnEquip()
        {
            CloseAllUI();
            base.OnUnEquip();
        }
        public override bool OnFocus()
        {
            if (CloseAllUI())
                return false;
            return base.OnFocus();
        }
        public override bool OnEsc(bool isKeyDown)
        {
            return !CloseAllUI() && base.OnEsc(isKeyDown);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns>true = closed a ui element</returns>
        public bool CloseAllUI()
        {
            return CloseLoadBlueprintMenu()
                || CloseSaveBlueprintMenu();
        }

        // --------------------------------------

        #region Load creation methods
        public void ToggleLoadBlueprintMenu(bool state)
        {
            if (!state) return;
            GameController.CursorFocusUI(!blueprintLoadCanvas.enabled);
            blueprintLoadCanvas.enabled = !blueprintLoadCanvas.enabled;
        }

        public bool CloseLoadBlueprintMenu()
        {
            if (blueprintLoadCanvas.enabled)
            {
                GameController.CursorFocusUI(false);
                blueprintLoadCanvas.enabled = false;
                return true;
            }
            return false;
        }
        #endregion


        #region Save creation methods
        public void ToggleSaveCreationMenu(bool state)
        {
            if (!state) return;

        }

        public bool CloseSaveBlueprintMenu()
        {
            if (blueprintLoadCanvas.enabled)
            {
                GameController.CursorFocusUI(false);
                blueprintLoadCanvas.enabled = false;
                return true;
            }
            return false;
        }
        #endregion


        #region clear scene
        private void ClearScene(bool state)
        {
            if (!state) return;
            if (BlueprintLoader.blueprintObject != null)
            {
                GameObject.Destroy(BlueprintLoader.blueprintObject?.gameObject);
                BlueprintLoader.blueprintObject = null;
            }
        }

        #endregion

        
    }
}