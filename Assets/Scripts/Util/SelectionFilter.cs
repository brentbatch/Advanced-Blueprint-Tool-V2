using Assets.Scripts.Model.BlueprintObject;
using Assets.Scripts.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Util
{
    [Serializable]
    public struct SelectionFilter // struct because i want a copy when i pass it to the OperationInvoker (for undo purposes)
    {
        private static readonly PlayerController PlayerController = GameController.Instance.playerController;

        private int layerMask;

        public bool IsSelecting { get; private set; }
        public bool IsMultiSelection { get; private set; }

        public Vector3Int startSelection;
        public Vector3Int endSelection;
        private GameObject selectedObject;
        private Bounds selectedObjectBounds;
        private List<GameObject> selectedObjects;


        public static SelectionFilter StartSelection(int layerMask = ~(1 << 8) /* gizmo */)
        {
            var playerTransform = PlayerController.gameObject.transform;
            return StartSelection(playerTransform.position, playerTransform.forward, layerMask);
        }
        public void ContinueSelection()
        {
            var playerTransform = PlayerController.gameObject.transform;
            ContinueSelection(playerTransform.position, playerTransform.forward);
        }
        public void EndSelection()
        {
            var playerTransform = PlayerController.gameObject.transform;
            EndSelection(playerTransform.position, playerTransform.forward);
        }


        public static SelectionFilter StartSelection(Vector3 origin, Vector3 direction, int layerMask = ~(1 << 8) /* gizmo */)
        {
            var selectionFilter = new SelectionFilter
            {
                IsSelecting = true,
                IsMultiSelection = false,
                layerMask = layerMask
            };

            if (Physics.Raycast(origin, direction, out RaycastHit hit, 500, layerMask))
            {
                selectionFilter.selectedObject = hit.transform.gameObject;
                selectionFilter.selectedObjectBounds = hit.collider.bounds;
                selectionFilter.startSelection = Vector3Int.RoundToInt(hit.point);
                selectionFilter.endSelection = Vector3Int.RoundToInt(hit.point);
            }
            else
            {
                selectionFilter.startSelection = Vector3Int.zero;
                selectionFilter.endSelection = Vector3Int.zero;
            }
            return selectionFilter;
        }

        public void ContinueSelection(Vector3 origin, Vector3 direction)
        {
            if (!IsSelecting) return;

            if (IsMultiSelection)
            {
                // multiselection
                if (Physics.Raycast(origin, direction, out RaycastHit hit, 500, layerMask))
                {
                    endSelection = Vector3Int.RoundToInt(hit.point);
                }
            }
            else if (Physics.Raycast(origin, direction, out RaycastHit hit, 500, layerMask))
            {
                if (selectedObject == null) // initial startSelection failed
                {
                    selectedObject = hit.transform.gameObject;
                    selectedObjectBounds = hit.collider.bounds;
                    startSelection = Vector3Int.RoundToInt(hit.point);
                    endSelection = Vector3Int.RoundToInt(hit.point);
                }
                else if (selectedObject != hit.transform.gameObject) // start multiselection
                {
                    IsMultiSelection = true;
                }
            }
        }

        public void EndSelection(Vector3 origin, Vector3 direction)
        {
            if (!IsSelecting) return;
            ContinueSelection(origin, direction);

            if (!IsMultiSelection)
            {
                if (selectedObjectBounds != null) // single selection & something was selected
                {
                    startSelection = Vector3Int.RoundToInt(selectedObjectBounds.min);
                    endSelection = Vector3Int.RoundToInt(selectedObjectBounds.max);
                }
                else
                {
                    startSelection = Vector3Int.zero;
                    endSelection = Vector3Int.zero;
                }
            }

            IsSelecting = false;
        }

        public static SelectionFilter FromSelection(Vector3Int startSelection, Vector3Int endSelection)
        {
            var selection = new SelectionFilter
            {
                startSelection = startSelection,
                endSelection = endSelection,
                IsMultiSelection = true
            };
            return selection;
        }
        public static SelectionFilter FromGameObjects(List<GameObject> gameObjects, Vector3Int startSelection, Vector3Int endSelection) =>
            new SelectionFilter
            {
                startSelection = startSelection,
                endSelection = endSelection,
                selectedObjects = gameObjects,
                IsMultiSelection = true
            };
        public static SelectionFilter FromGameObject(GameObject gameObject)
        {
            var selectedObjectBounds = gameObject.GetComponent<BoxCollider>().bounds;
            var selection = new SelectionFilter
            {
                selectedObject = gameObject,
                selectedObjectBounds = selectedObjectBounds,
                startSelection = Vector3Int.RoundToInt(selectedObjectBounds.min),
                endSelection = Vector3Int.RoundToInt(selectedObjectBounds.max),
            };
            return selection;
        }


        public Vector3 GetCenter()
        {
            if (!IsMultiSelection && selectedObjectBounds != null)
                return selectedObjectBounds.center;
            var center = ((Vector3)(startSelection + endSelection)) / 2;
            if (IsMultiSelection && selectedObjectBounds != null)
            {
                Vector3Int size = (startSelection - endSelection);
                if (size.x == 0) center.x += (center.x == selectedObjectBounds.min.x) ? 0.5f : -0.5f;
                if (size.y == 0) center.y += (center.y == selectedObjectBounds.min.y) ? 0.5f : -0.5f;
                if (size.z == 0) center.z += (center.z == selectedObjectBounds.min.z) ? 0.5f : -0.5f;
            }
            return center;
        }

        public Vector3Int GetSize()
        {
            if (!IsMultiSelection && selectedObjectBounds != null)
                return Vector3Int.RoundToInt(selectedObjectBounds.size);
            Vector3Int size = (startSelection - endSelection);
            if (size.x < 0) size.x = -size.x;
            if (size.y < 0) size.y = -size.y;
            if (size.z < 0) size.z = -size.z;
            if (IsMultiSelection && selectedObjectBounds != null)
            {
                if (size.x == 0) size.x = 1;
                if (size.y == 0) size.y = 1;
                if (size.z == 0) size.z = 1;
            }
            return size;
        }

        public Vector3Int min => Vector3Int.RoundToInt(GetCenter() - ((Vector3)GetSize()) / 2);
        public Vector3Int max => Vector3Int.RoundToInt(GetCenter() + ((Vector3)GetSize()) / 2);

        public List<GameObject> GetSelectedGameObjects()
        {
            if (this.IsMultiSelection && selectedObjects?.Count() == default) // null or 0
                CalculateSelectedGameObjectsFromSelection();
            return this.selectedObjects ?? new List<GameObject> { selectedObject };
        }

        private void CalculateSelectedGameObjectsFromSelection()
        {
            selectedObjects = Physics.OverlapBox(GetCenter(), GetSize() / 2, Quaternion.identity, layerMask)
                .Select(collider => collider.gameObject)
                .ToList();
        }

    }
}
