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
    public class RotateGizmo : MonoBehaviour
    {
        public static RotateGizmo Instance { get; private set; }
        public PlayerController PlayerController { get; private set; }

        [SerializeField] private GameObject XTorus;
        [SerializeField] private GameObject YTorus;
        [SerializeField] private GameObject ZTorus;
        [SerializeField] private GameObject SelectionDot;


        public bool IsRotating { get; private set; }
        private GameObject selectedGizmoGameObject;
        private Action<Vector3Int> OnRotate;
        [SerializeField] private Vector3 startSelectionDirection;

        private void Awake()
        {
            if (Instance == null) // stupid singleton to make class variables available anywhere
                Instance = this;
            else
                throw new Exception("more than one RotateGizmo");
        }

        private void Start()
        {
            PlayerController = GameController.Instance.playerController;

            XTorus.GetComponent<MeshRenderer>().material.color = new Color(1, 0, 0, 0.7f);
            YTorus.GetComponent<MeshRenderer>().material.color = new Color(0, 1, 0, 0.7f);
            ZTorus.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 1, 0.7f);

            SetActive(false);
        }

        public void SetPosition(Vector3 position) => transform.position = position;

        public void SetActive(bool active, Action<Vector3Int> onRotate = null)
        {
            gameObject.SetActive(active);
            OnRotate = onRotate ?? (vec => { });

            if (active)
            {
                SelectionDot.SetActive(false);
                if (onRotate == null) Debug.LogWarning("RotateGizmo.SetActive 'onRotate' parameter is null!");
            }
            else
            {
                CancelRotate();
            }
        }

        public void CancelRotate()
        {
            IsRotating = false;
            SelectionDot.SetActive(false);
            selectedGizmoGameObject = null;
        }

        public bool Rotate(bool keyDown)
        {
            if (!gameObject.activeInHierarchy)
            {
                Debug.LogWarning("'MoveGizmo.Move' available AFTER doing SetActive!");
                return false;
            }
            bool wasRotating = IsRotating;
            IsRotating = keyDown && selectedGizmoGameObject != null;
            return selectedGizmoGameObject != null || wasRotating; // has something selected , it will rotate something
        }

        private bool ClosestRotateGizmoInSight(out RaycastHit raycastHit)
        {
            Transform playerTransform = PlayerController.gameObject.transform;
            IOrderedEnumerable<RaycastHit> validHits = 
                Physics.RaycastAll(playerTransform.position, playerTransform.forward, 200f, ~0 /*1 << LayerMask.NameToLayer("Gizmo")*/)
                    .Where(hit => hit.transform.CompareTag("RotateGizmo"))
                    .OrderBy(hit => (hit.point - playerTransform.position).magnitude);
            raycastHit = validHits.FirstOrDefault();
            return validHits.Any();
        }

        private void FixedUpdate()
        {
            if (!IsRotating)
            {
                if (ClosestRotateGizmoInSight(out RaycastHit raycastHit))
                {
                    selectedGizmoGameObject = raycastHit.transform.gameObject;
                    startSelectionDirection = (raycastHit.point - transform.position);
                    var offset = selectedGizmoGameObject.transform.up * Vector3.Dot(startSelectionDirection, selectedGizmoGameObject.transform.up);
                    startSelectionDirection = (startSelectionDirection - offset).normalized;
                    SelectionDot.SetActive(true);
                    SelectionDot.transform.position = transform.position + startSelectionDirection * 2 * transform.localScale.x;
                }
                else
                {
                    selectedGizmoGameObject = null;
                    SelectionDot.SetActive(false);
                }
            }
            else
            {
                Transform playerTransform = PlayerController.gameObject.transform;
                Transform selectedTransform = selectedGizmoGameObject.transform;

                if (Math3d.LinePlaneIntersection(out Vector3 intersection, playerTransform.position, playerTransform.forward, selectedTransform.up, selectedTransform.position))
                {
                    Vector3 localDirection = (intersection - transform.position).normalized;
                    SelectionDot.transform.position = transform.position + localDirection * 2 * transform.localScale.x;

                    int signedAngle = Mathf.RoundToInt((float)Vector3.SignedAngle(startSelectionDirection, localDirection, selectedTransform.up));

                    Vector3Int rotation = Vector3Int.RoundToInt(selectedTransform.up * signedAngle);

                    OnRotate(rotation);
                }

            }
        }

        private void Update()
        {
            Transform playerTransform = PlayerController.gameObject.transform;
            float scale = Mathf.Max(0.9f, Mathf.Sqrt((playerTransform.position - transform.position).magnitude) / 1.5f - 0.5f);
            transform.localScale = Vector3.one * scale;
        }
    }
}
