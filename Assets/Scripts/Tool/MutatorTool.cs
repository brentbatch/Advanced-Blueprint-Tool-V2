using Assets.Scripts.CustomGizmos;
using Assets.Scripts.Extensions;
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
        private RotateGizmo rotateGizmo;

        public MutatorTool() : base(title: "MutatorTool", description: "")
        {
            MessageController messageController = GameController.Instance.messageController;
            sprite = Resources.Load<Sprite>("Textures/mutate");
            functions = new List<ToolFunction>()
            {
                // todo: select shapes function, 'e' adds extra filters such as color, type etc, ctr+a selects all
                //new ToolFunction { title = "select shapes", description = "", sprite = Resources.Load<Sprite>( "Textures/mutate" ),
                //    OnEquip = ShowSelection, OnInteract = ToggleSelectionMenu, OnLeftClick = Selection, OnUnEquip = StopSelection, OnRightClick = b => CancelSelection()},
                new ToolFunction { title = "move shapes", description = "", sprite = Resources.Load<Sprite>( "Textures/move" ),
                    OnEquip = EquipMoveFunction, OnUnEquip = UnEquipMoveFunction, OnLeftClick = SelectionMoveLMB, OnRightClick = SelectionMoveRMB, OnMove2 = MoveSelectionByArrows },
                new ToolFunction { title = "rotate shapes", description = "", sprite = Resources.Load<Sprite>( "Textures/mutate" ),
                    OnEquip = EquipRotateFunction, OnUnEquip = UnEquipRotateFunction, OnLeftClick = SelectionRotateLMB, OnRightClick = SelectionRotateRMB, OnNextRotation = ToggleGridMode},
                new ToolFunction { title = "scale blocks", description = "", sprite = Resources.Load<Sprite>( "Textures/mutate" ) ,OnEquip = () => {
                    messageController.WarningMessage("Feature not yet implemented."); 
                }},
            };
            selectedToolFunction = functions[0];

        }

        public override void OnStart()
        {
            selectionGizmo = SelectionGizmo.Instance;
            moveGizmo = MoveGizmo.Instance;
            rotateGizmo = RotateGizmo.Instance;
        }

        public override void OnUnEquip()
        {
            moveGizmo.SetActive(false);
            selectionGizmo.SetActive(false);
            rotateGizmo.SetActive(false);
            base.OnUnEquip();
        }

        // // don't do anything on clicking the function in the hotbar:
        // public override void OnHotBarFunctionClick() {}

        private void ShowMoveGizmo(SelectionFilter selectionFilter)
        {
            moveGizmo.SetActive(true, MoveObjects);
            moveGizmo.SetPosition(selectionFilter.GetCenter());
        }

        private void MoveObjects(Vector3Int offset)
        {
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

        // ---------------------

        private void EquipMoveFunction()
        {
            selectionGizmo.SetActive(true, ShowMoveGizmo);
            bool hasSelection = selectionGizmo.HasSelection();
            moveGizmo.SetActive(hasSelection, MoveObjects);
            if (hasSelection)
            {
                moveGizmo.SetPosition(selectionGizmo.selectionFilter.GetCenter());
            }
        }

        private void UnEquipMoveFunction()
        {
            moveGizmo.SetActive(false);
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

        private void MoveSelectionByArrows(Vector3 vector3)
        {
            Vector3 angles = PlayerController.gameObject.transform.eulerAngles;
            var rotation90 = Mathf.RoundToInt(angles.y / 90f) * 90;
            vector3 = vector3.Rotate(0, rotation90, 0);
            
            Vector3Int offset = Vector3Int.RoundToInt(vector3);

            MoveObjects(offset);
            moveGizmo.SetPosition(selectionGizmo.selectionFilter.GetCenter());
        }

        private void ToggleMoveMenu(bool keyDown)
        {
            selectionGizmo.SetActive(true, ShowMoveGizmo);
        }

        // ----------------------------


        private void RotateSelected(SelectionFilter selectionFilter)
        {
            Vector3Int position = Vector3Int.FloorToInt(selectionGizmo.selectionFilter.GetCenter());
            moveGizmo.SetPosition(position);
            rotateGizmo.SetPosition(position);
            moveGizmo.SetActive(true, MoveCenterRotationPoint);
            rotateGizmo.SetActive(true, RotateObject);
        }

        private void MoveCenterRotationPoint(Vector3Int offset) // position should also work on 0.5 0.5 0.5 offset
        {
            rotateGizmo.transform.position += offset;
        }


        private Vector3Int lastRotation = Vector3Int.zero;
        private void RotateObject(Vector3Int rotation)
        {
            var pivot = moveGizmo.transform.position;
            var rotation90 = Vector3Int.RoundToInt((Vector3)rotation / 90f) * 90;

            var rotationDelta = rotation90 - lastRotation;

            if (rotationDelta != Vector3Int.zero)
            {
                SelectionFilter selectionFilter = selectionGizmo.selectionFilter;
                List<GameObject> gameObjects = selectionFilter.GetSelectedGameObjects();
                selectionGizmo.selectionFilter = SelectionFilter.FromGameObjects( gameObjects,
                    Vector3Int.RoundToInt(((Vector3)selectionFilter.min).Rotate(rotationDelta, pivot)),
                    Vector3Int.RoundToInt(((Vector3)selectionFilter.max).Rotate(rotationDelta, pivot)));

                foreach (GameObject gameObject in gameObjects)
                {
                    gameObject.transform.RotateAround(pivot, ((Vector3)rotationDelta).normalized, rotationDelta.magnitude);
                }
            }
            lastRotation = rotation90;
        }



        private void EquipRotateFunction() // movegizmo needs to be able to go in intervals of 0.5- ish - some button to switch between the 2 (add 0.50.50.5)
        {
            selectionGizmo.SetActive(true, RotateSelected);
            bool hasSelection = selectionGizmo.HasSelection();
            moveGizmo.SetActive(hasSelection, MoveCenterRotationPoint);
            rotateGizmo.SetActive(hasSelection, RotateObject);
            if (hasSelection)
            {
                Vector3Int position = Vector3Int.FloorToInt(selectionGizmo.selectionFilter.GetCenter());
                moveGizmo.SetPosition(position);
                rotateGizmo.SetPosition(position);
            }
        }

        private void UnEquipRotateFunction()
        {
            moveGizmo.SetActive(false);
            rotateGizmo.SetActive(false);
        }

        private void SelectionRotateLMB(bool keyDown)
        {
            if (selectionGizmo.IsSelecting || selectionGizmo.IsScaling) // end selection
            {
                selectionGizmo.Selection(keyDown);
                return;
            }
            if (rotateGizmo.IsRotating) // ending rotation
            {
                rotateGizmo.Rotate(keyDown);
                moveGizmo.SetActive(true, MoveCenterRotationPoint);
                return;
            }
            if (moveGizmo.IsMoving) // ending movement
            {
                moveGizmo.Move(keyDown);
                rotateGizmo.SetActive(true, RotateObject);
                return;
            }
            if (rotateGizmo.Rotate(keyDown)) // begin either rotation or move (if possible)
            {
                lastRotation = Vector3Int.zero;
                moveGizmo.SetActive(false);
                return;
            }
            else if (moveGizmo.Move(keyDown))
            {
                rotateGizmo.SetActive(false);
                return;
            }

            selectionGizmo.Selection(keyDown); // change selection
        }

        private void SelectionRotateRMB(bool keyDown)
        {
            moveGizmo.CancelMove();
            rotateGizmo.CancelRotate();
            if (!selectionGizmo.Selection(false))
            {
                selectionGizmo.CancelSelection();
                moveGizmo.SetActive(false);
                rotateGizmo.SetActive(false);
            }
        }

        private void ToggleGridMode(bool keyDown) // 'Q'
        {
            if (!keyDown) return;
            bool hasSelection = selectionGizmo.HasSelection();
            if (hasSelection)
            {
                bool isCenter = Mathf.RoundToInt(moveGizmo.transform.position.x * 2f).mod(2) == 1;
                Vector3 position = Vector3Int.FloorToInt(selectionGizmo.selectionFilter.GetCenter()) + (isCenter ? Vector3.zero : Vector3.one / 2);
                moveGizmo.SetPosition(position);
                rotateGizmo.SetPosition(position);
            }
        }

    }
}