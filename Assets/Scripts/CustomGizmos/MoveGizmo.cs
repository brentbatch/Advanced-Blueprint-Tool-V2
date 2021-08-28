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
    public class MoveGizmo : MonoBehaviour
    {
        public static MoveGizmo Instance { get; private set; }
        private PlayerController PlayerController;
        private List<GameObject> faces;
        private List<GameObject> arrows;

        [SerializeField] private GameObject XArrow;
        [SerializeField] private GameObject YArrow;
        [SerializeField] private GameObject ZArrow;
        [SerializeField] private GameObject XYFace;
        [SerializeField] private GameObject YZFace;
        [SerializeField] private GameObject ZXFace;

        public bool IsMoving { get; private set; }
        private GameObject selectedGizmoGameObject;
        private Action<Vector3Int> OnMove;

        private void Awake()
        {
            if (Instance == null) // stupid singleton to make class variables available anywhere
                Instance = this;
            else
                throw new Exception("more than one MoveGizmo");
        }

        private void Start()
        {
            PlayerController = GameController.Instance.playerController;
            faces = new List<GameObject> { XYFace, YZFace, ZXFace };
            arrows = new List<GameObject> { XArrow, YArrow, ZArrow };
            foreach (var renderer in XArrow.GetComponentsInChildren<MeshRenderer>()) renderer.material.color = new Color(1, 0, 0);
            foreach (var renderer in YArrow.GetComponentsInChildren<MeshRenderer>()) renderer.material.color = new Color(0, 1, 0);
            foreach (var renderer in ZArrow.GetComponentsInChildren<MeshRenderer>()) renderer.material.color = new Color(0, 0, 1);
            YZFace.GetComponent<MeshRenderer>().material.color = new Color(1, 0, 0, 0.7f);
            ZXFace.GetComponent<MeshRenderer>().material.color = new Color(0, 1, 0, 0.7f);
            XYFace.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 1, 0.7f);
            foreach (var face in faces)
                face.GetComponent<MeshRenderer>().enabled = false;

            SetActive(false);
        }

        public void SetPosition(Vector3 position) => transform.position = position;

        public void SetActive(bool active, Action<Vector3Int> onMove = null)
        {
            gameObject.SetActive(active);
            OnMove = onMove ?? (vec => { });

            if (active)
            {
                if (onMove == null) Debug.LogWarning("MoveGizmo.SetActive 'OnMove' parameter is null!");
            }
            else
            {
                CancelMove();
            }
        }

        public void CancelMove()
        {
            IsMoving = false;
            if (selectedGizmoGameObject != null)
            {
                SetHighlight(selectedGizmoGameObject, false);
                selectedGizmoGameObject = null;
            }
        }

        public bool Move(bool keyDown)
        {
            if (!gameObject.activeInHierarchy)
            {
                Debug.LogWarning("'MoveGizmo.Move' available AFTER doing SetActive!");
                return false;
            }
            bool wasMoving = IsMoving;
            IsMoving = keyDown && selectedGizmoGameObject != null;
            return selectedGizmoGameObject != null || wasMoving; // has something selected , it will move something
        }

        private bool ClosestMoveGizmoInSight(out RaycastHit raycastHit)
        {
            Transform playerTransform = PlayerController.gameObject.transform;
            IOrderedEnumerable<RaycastHit> raycastHits = Physics.RaycastAll(playerTransform.position, playerTransform.forward, 200f, 1 << LayerMask.NameToLayer("Gizmo"))
                .Where(hit => hit.transform.CompareTag("MoveGizmo"))
                .OrderBy(hit => (hit.point - playerTransform.position).magnitude); 
            raycastHit = raycastHits.FirstOrDefault();
            return raycastHits.Any();
        }

        private void FixedUpdate()
        {
            if (!IsMoving)
            {
                if (ClosestMoveGizmoInSight(out RaycastHit raycastHit))
                {
                    GameObject closestGameObject = raycastHit.transform.gameObject;

                    if (!closestGameObject.Equals(selectedGizmoGameObject))
                    {
                        if (selectedGizmoGameObject != null)
                        {
                            SetHighlight(selectedGizmoGameObject, false);
                        }
                        SetHighlight(selectedGizmoGameObject = closestGameObject, true);
                    }
                }
                else
                {
                    if (selectedGizmoGameObject != null)
                    {
                        SetHighlight(selectedGizmoGameObject, false);
                        selectedGizmoGameObject = null;
                    }
                }
            }
            else if (selectedGizmoGameObject != null)
            {
                Transform playerTransform = PlayerController.gameObject.transform;
                Transform selectedTransform = selectedGizmoGameObject.transform;
                if (selectedGizmoGameObject.name.Contains("Face"))
                {
                    if (Math3d.LinePlaneIntersection(out Vector3 newPosition, playerTransform.position, playerTransform.forward, selectedTransform.up, selectedTransform.position))
                    {
                        Vector3Int offset = Vector3Int.RoundToInt(newPosition - transform.position);
                        transform.position += offset;
                        OnMove(offset);
                    }
                }
                else
                {
                    if (Math3d.ClosestPointsOnTwoLines(out _, out Vector3 point, playerTransform.position, playerTransform.forward, selectedTransform.position, selectedTransform.up))
                    {
                        Vector3 newPosition = (point - selectedTransform.up);
                        Vector3Int offset = Vector3Int.RoundToInt(newPosition - transform.position);
                        transform.position += offset;
                        OnMove(offset);
                    }
                }
            }
        }

        private void Update()
        {
            Transform playerTransform = PlayerController.gameObject.transform;
            float scale = Mathf.Max(0.3f, Mathf.Sqrt((playerTransform.position - transform.position).magnitude) / 3f - 0.5f);
            transform.localScale = Vector3.one * scale;
        }

        private void SetHighlight(GameObject hightlightObject, bool enable)
        {
            if (hightlightObject.name.Contains("Face"))
            {
                hightlightObject.GetComponent<MeshRenderer>().enabled = enable;
            }
            else
            {
                foreach(MeshRenderer renderer in hightlightObject.GetComponentsInChildren<MeshRenderer>())
                {
                    Color color = renderer.material.color;
                    Color.RGBToHSV(color, out var H, out _, out var V);
                    renderer.material.color = Color.HSVToRGB(H, enable ? 0.2f : 1f, V);
                }
            }
        }
    }
}
