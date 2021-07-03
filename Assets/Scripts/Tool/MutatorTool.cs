using Assets.Scripts.CustomGizmos;
using Assets.Scripts.Model.BlueprintObject;
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
                new ToolFunction { title = "select shapes", description = "", sprite = Resources.Load<Sprite>( "Textures/mutate" ),
                    OnInteract = ToggleSelectionMenu, OnLeftClick = Selection, OnUnEquip = StopSelection, OnRightClick = DisableSelection},
                new ToolFunction { title = "move shapes", description = "", sprite = Resources.Load<Sprite>( "Textures/move" ),
                    OnEquip = ShowMove, OnInteract = ToggleMoveMenu, OnLeftClick = Move, OnUnEquip = DisableMove, OnRightClick = StopMove },
                new ToolFunction { title = "rotate shapes", description = "", sprite = Resources.Load<Sprite>( "Textures/mutate" ) },
                new ToolFunction { title = "scale blocks", description = "", sprite = Resources.Load<Sprite>( "Textures/mutate" ) },
            };
            selectedToolFunction = functions[0];

        }

        public override void OnStart()
        {
            selectionGizmo = SelectionGizmo.Instance;
            moveGizmo = MoveGizmo.Instance;
        }
        // // don't do anything on clicking the function in the hotbar:
        // public override void OnHotBarFunctionClick() {}

        private void Selection(bool keyDown) => selectionGizmo.Selection(keyDown);
        private void StopSelection() => selectionGizmo.StopSelection();
        private void DisableSelection(bool keyDown) => selectionGizmo.DisableSelection();
        private void ToggleSelectionMenu(bool keyDown)
        {
            // todo
        }


        private void ShowMove()
        {
            // open & set movegizmo active
            moveGizmo.SetActive(true, OnMove);
            moveGizmo.SetPosition(selectionGizmo.selectionFilter.GetCenter());
        }
        private void DisableMove()
        {
            // remove movegizmo
            moveGizmo.SetActive(false);
        }
        private void Move(bool keyDown)
        {
            // start moving block (selection gizmo)
            moveGizmo.Move(keyDown);
        }
        private void StopMove(bool keyDown) // RMB
        {
            // stop current move action in movegizmo
            moveGizmo.CancelMove();
        }
        private void ToggleMoveMenu(bool keyDown)
        {

        }
        private void OnMove(Vector3Int offset)
        {
            List<GameObject> gameObjects = selectionGizmo.selectionFilter.GetSelectedGameObjects();
            selectionGizmo.selectionFilter = SelectionFilter.FromGameObjects(
                gameObjects,
                selectionGizmo.selectionFilter.startSelection + offset,
                selectionGizmo.selectionFilter.endSelection + offset);
            foreach (GameObject gameObject in gameObjects)
            {
                gameObject.transform.position += offset;
            }
        }

    }
}