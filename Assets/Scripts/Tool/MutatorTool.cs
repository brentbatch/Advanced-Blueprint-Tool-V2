using Assets.Scripts.CustomGizmos;
using Assets.Scripts.Model.BlueprintObject;
using Assets.Scripts.Unity;
using Assets.Scripts.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Tool
{
    public class MutatorTool : AbstractTool
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
        private SelectionGizmo selectionGizmo;
        private MoveGizmo moveGizmo;

        public MutatorTool() : base(title: "MutatorTool", description: "")
        {
            sprite = Resources.Load<Sprite>("Textures/mutate");
            functions = new List<ToolFunction>()
            {
                // todo: select shapes function, 'e' adds extra filters such as color, type etc, ctr+a selects all
                //new ToolFunction { title = "select shapes", description = "", sprite = Resources.Load<Sprite>( "Textures/mutate" ),
                //    OnEquip = ShowSelection, OnInteract = ToggleSelectionMenu, OnLeftClick = Selection, OnUnEquip = StopSelection, OnRightClick = b => CancelSelection()},
                new ToolFunction { title = "move shapes", description = "", sprite = Resources.Load<Sprite>( "Textures/move" ),
                    OnEquip = EquipMoveFunction, OnInteract = ToggleMoveMenu, OnLeftClick = SelectionMoveLMB, OnUnEquip = UnEquipMoveFunction, OnRightClick = SelectionMoveRMB },
                new ToolFunction { title = "rotate shapes", description = "", sprite = Resources.Load<Sprite>( "Textures/mutate" ),OnEquip = () => {
                    MessageController messageController = GameController.Instance.messageController;
                    messageController.WarningMessage("Feature not yet implemented.");
                }},
                new ToolFunction { title = "scale blocks", description = "", sprite = Resources.Load<Sprite>( "Textures/mutate" ) ,OnEquip = () => {
                    MessageController messageController = GameController.Instance.messageController;
                    messageController.WarningMessage("Feature not yet implemented."); 
                }},
            };
            selectedToolFunction = functions[0];

        }


        public override void OnStart()
        {
            selectionGizmo = SelectionGizmo.Instance;
            moveGizmo = MoveGizmo.Instance;
        }

        public override void OnUnEquip()
        {
            moveGizmo.SetActive(false);
            selectionGizmo.SetActive(false);
            base.OnUnEquip();
        }

        // // don't do anything on clicking the function in the hotbar:
        // public override void OnHotBarFunctionClick() {}

        private void EquipMoveFunction()
        {
            selectionGizmo.SetActive(true, OnSelect);

        }

        private void SelectionMoveLMB(bool keyDown)
        {
            if (selectionGizmo.IsSelecting || selectionGizmo.IsScaling || !moveGizmo.Move(keyDown))
                selectionGizmo.Selection(keyDown);
        }
        private void SelectionMoveRMB(bool keyDown)
        {
            moveGizmo.CancelMove();
            if (!selectionGizmo.Selection(false))
            {
                selectionGizmo.CancelSelection();
                moveGizmo.SetActive(false);
            }
        }

        private void UnEquipMoveFunction()
        {
            moveGizmo.SetActive(false);
            selectionGizmo.SetActive(false);
        }

        private void OnSelect(SelectionFilter selectionFilter)
        {
            moveGizmo.SetActive(true, OnMove);
            moveGizmo.SetPosition(selectionFilter.GetCenter());
        }

        private void OnMove(Vector3Int offset)
        {
            //todo
            List<GameObject> gameObjects = selectionGizmo.selectionFilter.GetSelectedGameObjects();
            selectionGizmo.selectionFilter = SelectionFilter.FromGameObjects(
                gameObjects,
                selectionGizmo.selectionFilter.min + offset,
                selectionGizmo.selectionFilter.max + offset);
            foreach (GameObject gameObject in gameObjects)
            {
                gameObject.transform.position += offset;
            }
        }
        private void ToggleMoveMenu(bool keyDown)
        {

        }



    }
}