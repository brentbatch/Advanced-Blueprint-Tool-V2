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
    // todo: put all raycast stuff from SelectionFilter in this gizmo.

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
            foreach (var renderer in new List<MeshRenderer> { Cube, XFace, XNegFace, YFace, YNegFace, ZFace, ZNegFace })
            {
                renderer.enabled = false;
            }
            SetActive(false);
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

        public bool Selection(bool keyDown)
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
                IsSelecting = true;
                selectionFilter = SelectionFilter.NewSelection(raycastHit);
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
            Cube.material.color = new Color(0.8f, 0.5f, 0f, 0.15f * (Mathf.Abs((DateTime.Now.Millisecond % 2000) - 1000) / 1000f) + 0.05f);

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
