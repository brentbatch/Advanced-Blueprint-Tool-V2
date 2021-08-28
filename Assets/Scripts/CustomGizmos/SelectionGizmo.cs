using Assets.Scripts.Unity;
using Assets.Scripts.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.CustomGizmos
{
    public class SelectionGizmo : MonoBehaviour
    {
        public static SelectionGizmo Instance { get; private set; }
        private PlayerController PlayerController;

        [SerializeField] private MeshRenderer Cube;
        [SerializeField] private MeshRenderer XFace;
        [SerializeField] private MeshRenderer XNegFace;
        [SerializeField] private MeshRenderer YFace;
        [SerializeField] private MeshRenderer YNegFace;
        [SerializeField] private MeshRenderer ZFace;
        [SerializeField] private MeshRenderer ZNegFace;

        private LayerMask gizmoLayerMask;
        private Action<SelectionFilter> OnSelect;
        public SelectionFilter selectionFilter;

        public bool IsSelecting { get; private set; }
        public bool IsScaling { get; private set; }

        private Vector3Int scaleDirection;
        private MeshRenderer selectedFace;


        private void Awake()
        {
            if (Instance == null) // stupid singleton to make class variables available anywhere
                Instance = this;
            else
                throw new Exception("more than one SelectionGizmo");

            gizmoLayerMask = 1 << LayerMask.NameToLayer("Gizmo");
        }

        private void Start()
        {
            PlayerController = GameController.Instance.playerController;
            Cube.enabled = false;
            HideFaces();
            SetActive(false);
        }

        private void HideFaces()
        {
            foreach (var renderer in new List<MeshRenderer> { XFace, XNegFace, YFace, YNegFace, ZFace, ZNegFace })
            {
                renderer.enabled = false;
            }
        }

        public void SetSelection(SelectionFilter selectionFilter) => this.selectionFilter = selectionFilter;

        public void SetActive(bool active, Action<SelectionFilter> onSelect = null)
        {
            gameObject.SetActive(active);
            Cube.enabled = active;
            OnSelect = onSelect ?? (vec => { });

            if (active)
            {
                if (OnSelect == null) Debug.LogWarning("SelectionGizmo.SetActive 'OnSelect' parameter is null!");
            }
            else
            {
                CancelSelection();
            }
        }

        public void CancelSelection()
        {
            IsScaling = false;
            IsSelecting = false;
            selectionFilter = default;
            gameObject.transform.position = Vector3.zero;
            gameObject.transform.localScale = Vector3.zero;
        }

        public bool HasSelection() => !selectionFilter.Equals(default(SelectionFilter));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyDown"></param>
        /// <param name="stretchModifier">do selection in plane(s) between 2 selected points</param>
        /// <returns>Started/ended selection or scaling of selection</returns>
        public bool Selection(bool keyDown, bool stretchModifier = false)
        {
            if (!gameObject.activeInHierarchy)
            {
                Debug.LogWarning("'SelectionGizmo.Selection' available AFTER doing SetActive!");
                return false;
            }

            var playerTransform = PlayerController.gameObject.transform;
            if (keyDown && Physics.Raycast(playerTransform.position, playerTransform.forward, out RaycastHit hit, 500, gizmoLayerMask))
            {
                MeshRenderer hitMeshRenderer = hit.collider.GetComponent<MeshRenderer>();
                if (new List<MeshRenderer> { XFace, XNegFace, YFace, YNegFace, ZFace, ZNegFace }.Contains(hitMeshRenderer))
                {
                    IsScaling = true;
                    scaleDirection = Vector3Int.RoundToInt(hit.normal);
                    selectedFace = hitMeshRenderer;
                    return true;
                }
                return false; // i hit something on the gizmo layer but it's not any of what i need, something else needs to handle it
            }
            if (!keyDown && (IsScaling || IsSelecting))
            {
                IsScaling = false;
                IsSelecting = false;
                OnSelect(selectionFilter);
                return true;
            }
            IsScaling = false;

            if (keyDown && Physics.Raycast(playerTransform.position, playerTransform.forward, out RaycastHit raycastHit, 500, ~gizmoLayerMask))
            {
                if (stretchModifier && HasSelection())
                {
                    SelectionFilter newSelection = SelectionFilter.NewSelection(raycastHit);
                    Vector3Int newMin = newSelection.min;
                    Vector3Int newMax = newSelection.max;
                    Vector3Int currentMin = selectionFilter.min;
                    Vector3Int currentMax = selectionFilter.max;
                    Vector3Int finalMin = currentMin;
                    Vector3Int finalMax = currentMax;

                    if (Math.Min(newMin.x, newMax.x) >= Math.Max(currentMin.x, currentMax.x) || Math.Max(newMin.x, newMax.x) <= Math.Min(currentMin.x, currentMax.x))
                    {
                        finalMin.x = -100000;
                        finalMax.x = 100000;
                    }
                    if (Math.Min(newMin.y, newMax.y) >= Math.Max(currentMin.y, currentMax.y) || Math.Max(newMin.y, newMax.y) <= Math.Min(currentMin.y, currentMax.y))
                    {
                        finalMin.y = -100000;
                        finalMax.y = 100000;
                    }
                    if (Math.Min(newMin.z, newMax.z) >= Math.Max(currentMin.z, currentMax.z) || Math.Max(newMin.z, newMax.z) <= Math.Min(currentMin.z, currentMax.z))
                    {
                        finalMin.z = -100000;
                        finalMax.z = 100000;
                    }
                    var bigSelection = SelectionFilter.FromSelection(finalMin, finalMax, ~gizmoLayerMask);
                    Collider[] colliders = Physics.OverlapBox(bigSelection.GetCenter(), (Vector3)bigSelection.GetSize() * 0.4999f, Quaternion.identity, ~gizmoLayerMask);
                    if (finalMax.x == 100000)
                    {
                        finalMin.x = (int)colliders.Min(collider => collider.bounds.min.x);
                        finalMax.x = (int)colliders.Max(collider => collider.bounds.max.x);
                    }
                    if (finalMax.y == 100000)
                    {
                        finalMin.y = (int)colliders.Min(collider => collider.bounds.min.y);
                        finalMax.y = (int)colliders.Max(collider => collider.bounds.max.y);
                    }
                    if (finalMax.z == 100000)
                    {
                        finalMin.z = (int)colliders.Min(collider => collider.bounds.min.z);
                        finalMax.z = (int)colliders.Max(collider => collider.bounds.max.z);
                    }
                    selectionFilter = SelectionFilter.FromSelection(finalMin, finalMax, ~gizmoLayerMask);
                    OnSelect(selectionFilter);
                    HideFaces();
                }
                else
                {
                    IsSelecting = true;
                    selectionFilter = SelectionFilter.NewSelection(raycastHit);
                }
                return true;
            }
            return false;
        }


        private void FixedUpdate()
        {
            if (IsScaling)
            {
                var playerTransform = PlayerController.gameObject.transform;
                var center = selectionFilter.GetCenter();
                if (Math3d.ClosestPointsOnTwoLines(out _, out Vector3 point, playerTransform.position, playerTransform.forward, center, scaleDirection))
                {
                    Vector3Int min = selectionFilter.min;
                    Vector3Int max = selectionFilter.max;

                    float desiredLength = Vector3.Dot(point - center, scaleDirection);
                    if (desiredLength > 0)
                    {
                        Vector3 offsetCenter = center + scaleDirection;

                        Vector3Int size = selectionFilter.GetSize();
                        float currentLength = Mathf.Abs(Vector3.Dot(size, scaleDirection)) / 2;
                        // determine closest point to scaling direction:
                        if ((min - offsetCenter).sqrMagnitude < (max - offsetCenter).sqrMagnitude)
                            min += scaleDirection * Mathf.RoundToInt(desiredLength - currentLength);
                        else
                            max += scaleDirection * Mathf.RoundToInt(desiredLength - currentLength);
                        selectionFilter = SelectionFilter.FromSelection(min, max, ~gizmoLayerMask);
                        OnSelect(selectionFilter);
                    }
                }
            }
            if (IsSelecting)
            {
                var playerTransform = PlayerController.gameObject.transform;
                if (Physics.Raycast(playerTransform.position, playerTransform.forward, out RaycastHit hit, 500, ~gizmoLayerMask))
                {
                    selectionFilter.DoSelection(hit);
                }
            }
        }

        private void Update()
        {
            var ms = DateTime.Now.Ticks / 10000;
            Cube.material.color = ms % 1000 < 200 ? new Color(0.8f, 0.5f, 0f, 0.2f * Mathf.Abs(ms % 200 + (ms % 200 > 100 ? -200 : 0)) / 100f) : Color.clear;

            if (selectedFace != null)
            {
                selectedFace.enabled = IsScaling;
                selectedFace.material.color = new Color(1f, 0f, 0f, 0.35f * (Mathf.Abs(((DateTime.Now.Millisecond + 500) % 2000) - 1000) / 100f) + 0.05f);
            }

            gameObject.transform.position = selectionFilter.GetCenter();
            gameObject.transform.localScale = selectionFilter.GetSize();
        }
    }
}
