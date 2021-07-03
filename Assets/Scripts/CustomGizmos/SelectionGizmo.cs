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

        private bool isScaling;
        private Vector3Int scaleDirection;
        private MeshRenderer selectedFace;
        public SelectionFilter selectionFilter;

        private void Awake()
        {
            if (Instance == null) // stupid singleton to make class variables available anywhere
                Instance = this;
            else
                throw new Exception("more than one SelectionGizmo");
        }

        private void Start()
        {
            PlayerController = GameController.Instance.playerController;
            DisableMeshRenderers();
        }

        public void Selection(bool keyDown)
        {
            gameObject.SetActive(true);
            Cube.enabled = true;

            var playerTransform = PlayerController.gameObject.transform;
            if (keyDown && Physics.Raycast(playerTransform.position, playerTransform.forward, out RaycastHit hit, 500, 1 << LayerMask.NameToLayer("Gizmo")))
            {
                MeshRenderer hitMeshRenderer = hit.collider.GetComponent<MeshRenderer>();
                if (new List<MeshRenderer> { XFace, XNegFace, YFace, YNegFace, ZFace, ZNegFace }.Contains(hitMeshRenderer))
                {
                    isScaling = true;
                    scaleDirection = Vector3Int.RoundToInt(hit.normal);
                    selectedFace = hitMeshRenderer;
                }
                return;
            }
            if (isScaling && !keyDown)
            {
                isScaling = false;
                return;
            }

            isScaling = false;
            if (keyDown)
                selectionFilter = SelectionFilter.StartSelection();
            else
                selectionFilter.EndSelection();
        }

        public void StopSelection()
        {
            // if selection?.isselecting, endselection
            // if scaling, end scaling
            isScaling = false;
            if (selectionFilter.IsSelecting)
            {
                selectionFilter.EndSelection();
            }
        }

        public void DisableSelection()
        {
            DisableMeshRenderers();
            selectionFilter = new SelectionFilter();
            gameObject.transform.position = Vector3.zero;
            gameObject.transform.localScale = Vector3.zero;
            gameObject.SetActive(false);
        }

        private void DisableMeshRenderers()
        {
            foreach (var renderer in new List<MeshRenderer> { Cube, XFace, XNegFace, YFace, YNegFace, ZFace, ZNegFace })
            {
                renderer.enabled = false;
            }
        }


        private void FixedUpdate()
        {
            if (isScaling)
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
                        selectionFilter = SelectionFilter.FromSelection(min, max);
                    }
                }
            }
            if (selectionFilter.IsSelecting)
            {
                selectionFilter.ContinueSelection();
            }

            Cube.material.color = new Color(0.8f, 0.5f, 0f, 0.15f * (Mathf.Abs((DateTime.Now.Millisecond % 2000) - 1000) / 1000f) + 0.05f);
            
            if (selectedFace != null)
            {
                selectedFace.enabled = isScaling;
                selectedFace.material.color = new Color(1f, 0f, 0f, 0.35f * (Mathf.Abs(((DateTime.Now.Millisecond + 500) % 2000) - 1000) / 100f) + 0.05f);
            }

            gameObject.transform.position = selectionFilter.GetCenter();
            gameObject.transform.localScale = selectionFilter.GetSize();
        }
    }
}
