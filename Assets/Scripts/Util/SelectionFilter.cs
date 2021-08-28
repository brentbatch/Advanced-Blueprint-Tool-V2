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
        private int layerMask;
        private List<GameObject> selectedObjects;
        private GameObject selectedObject;
        private Bounds selectedObjectBounds;

        public Vector3Int min;
        public Vector3Int max;
        public bool IsMultiSelection { get; private set; }


        public static SelectionFilter NewSelection(RaycastHit raycastHit, int layerMask = ~(1 << 8))
        {
            var selection = new SelectionFilter 
            {
                layerMask = layerMask
            };
            selection.DoSelection(raycastHit);
            return selection;
        }
        public void DoSelection(RaycastHit raycastHit)
        {
            if (raycastHit.Equals(default(RaycastHit)))
                return;
            if (selectedObject == null)
            {
                // new 
                selectedObject = raycastHit.transform.gameObject;
                selectedObjectBounds = raycastHit.collider.bounds;
                min = Vector3Int.RoundToInt(selectedObjectBounds.min);
                max = Vector3Int.RoundToInt(selectedObjectBounds.max);
            }
            else if (raycastHit.transform.gameObject != selectedObject)
            {
                // extend
                IsMultiSelection = true;
                var min1 = selectedObjectBounds.min;
                var max1 = selectedObjectBounds.max;
                var otherMin = raycastHit.collider.bounds.min;
                var otherMax = raycastHit.collider.bounds.max;
                Vector3Int newMin = default, newMax = default;
                newMin.x = Mathf.RoundToInt(Mathf.Min(min1.x, max1.x, otherMin.x, otherMax.x));
                newMin.y = Mathf.RoundToInt(Mathf.Min(min1.y, max1.y, otherMin.y, otherMax.y));
                newMin.z = Mathf.RoundToInt(Mathf.Min(min1.z, max1.z, otherMin.z, otherMax.z));
                newMax.x = Mathf.RoundToInt(Mathf.Max(min1.x, max1.x, otherMin.x, otherMax.x));
                newMax.y = Mathf.RoundToInt(Mathf.Max(min1.y, max1.y, otherMin.y, otherMax.y));
                newMax.z = Mathf.RoundToInt(Mathf.Max(min1.z, max1.z, otherMin.z, otherMax.z));
                if (newMin.Equals(min))
                    max = newMax;
                else
                    min = newMin;
            }
        }



        public static SelectionFilter FromSelection(Vector3Int min, Vector3Int max, int layerMask = ~(1 << 8))
        {
            var selection = new SelectionFilter
            {
                min = min,
                max = max,
                IsMultiSelection = true,
                layerMask = layerMask
            };
            return selection;
        }
        public static SelectionFilter FromGameObjects(List<GameObject> gameObjects, Vector3Int min, Vector3Int max) =>
            new SelectionFilter
            {
                min = min,
                max = max,
                selectedObjects = gameObjects,
                IsMultiSelection = true
            };
        public static SelectionFilter FromGameObject(GameObject gameObject)
        {
            var selectedObjectBounds = gameObject.GetComponent<BoxCollider>().bounds;
            var selection = new SelectionFilter
            {
                min = Vector3Int.RoundToInt(selectedObjectBounds.min),
                max = Vector3Int.RoundToInt(selectedObjectBounds.max),
                selectedObject = gameObject,
                selectedObjectBounds = selectedObjectBounds,
            };
            return selection;
        }


        public Vector3 GetCenter()
        {
            if (!IsMultiSelection && selectedObjectBounds != null)
                return selectedObjectBounds.center;
            return ((Vector3)(max + min)) / 2;
        }

        public Vector3Int GetSize()
        {
            if (!IsMultiSelection && selectedObjectBounds != null)
                return Vector3Int.RoundToInt(selectedObjectBounds.size);
            return (max - min);
        }


        public List<GameObject> GetSelectedGameObjects()
        {
            if (this.IsMultiSelection && selectedObjects?.Count() == default) // null or 0
                CalculateSelectedGameObjectsFromSelection();
            return this.selectedObjects ?? new List<GameObject> { selectedObject };
        }

        private void CalculateSelectedGameObjectsFromSelection()
        {
            selectedObjects = Physics.OverlapBox(GetCenter(), (Vector3)GetSize() * 0.4999f, Quaternion.identity, layerMask)
                .Select(collider => collider.gameObject)
                .ToList();
        }

    }
}
