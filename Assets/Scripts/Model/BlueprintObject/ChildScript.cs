using Assets.Scripts.Loaders;
using Assets.Scripts.Model.Data;
using Assets.Scripts.Model.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Model.BlueprintObject
{
    // todo:
    // ShapeScript needs to be made that applies for blocks, parts & joints
    // blueprintPosition {get; set;} property
    public class ChildScript : MonoBehaviour 
    {
        public List<JointScript> connectedJoints;
        public Shape shape;

        public Vector3Int RotatedBounds { get; set; }
        public Controller Controller { get; set; }

        public int xaxis; // debug
        public int zaxis;


        [ContextMenu("rotation")]
        void SetRotationDebug()
        {
            this.SetBlueprintRotation(this.xaxis, this.zaxis);
        }

        public void SetColor(string strColor)
        {
            strColor = strColor.First() == '#' ? strColor : '#' + strColor;
            if (ColorUtility.TryParseHtmlString(strColor, out Color color))
            {
                foreach (var meshRend in gameObject.GetComponentsInChildren<MeshRenderer>())
                {
                    meshRend.material.color = color;
                }
            }
        }

        public void SetBlueprintPosition(Pos pos) // y and z reversed
        {
            transform.position = new Vector3Int(pos.X, pos.Z, pos.Y);
        }

        public void SetBlueprintBounds(Data.Bounds bounds) // y and z reversed
        {
            if (shape is Part) // only blocks have bounds.
            {
                Debug.LogWarning($"Trying to set bounds on a child that is a part!");
                return;
            }
            var childMesh = transform.GetChild(0);
            var scale = childMesh.transform.localScale;
            (scale.x, scale.y, scale.z) = (bounds.X, bounds.Z, bounds.Y);

            childMesh.transform.localScale = scale;
            childMesh.transform.localPosition = scale / 2;

            var collider = GetComponent<BoxCollider>();
            collider.size = scale;
            collider.center = scale / 2;
        }

        public void SetBlueprintRotation(int xaxis, int zaxis)
        {
            this.xaxis = xaxis;
            this.zaxis = zaxis;

            var xAbs = Math.Abs(xaxis);
            var zAbs = Math.Abs(zaxis);
            int xSign = xaxis > 0 ? 1 : -1;
            int zSign = zaxis > 0 ? 1 : -1;

            Vector3 right = new Vector3(xAbs == 1 ? xSign : 0, xAbs == 3 ? xSign : 0, xAbs == 2 ? xSign : 0);
            Vector3 up = new Vector3(zAbs == 1 ? zSign : 0, zAbs == 3 ? zSign : 0, zAbs == 2 ? zSign : 0);
            Vector3 forward = Vector3.Cross(right, up);
            
            gameObject.transform.rotation = Quaternion.LookRotation(forward, up);
            CalculateRotatedBounds();
        }

        public virtual (int, int, int) GetBlueprintPosition() //fkd in joints
        {
            return (Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z), Mathf.RoundToInt(transform.position.y));
        }
        public (int, int) GetBlueprintRotation()
        {
            int xaxis = Mathf.RoundToInt(transform.right.x + transform.right.z * -2 + transform.right.y * 3);
            int zaxis = Mathf.RoundToInt(transform.up.x + transform.up.z * -2 + transform.up.y * 3);
            return (xaxis, zaxis);
        }

        /// <summary>
        /// use rotated bounds for features.
        /// </summary>
        protected void CalculateRotatedBounds()
        {
            //throw new NotImplementedException();
        }

        public void Rotate()
        {

        }
    }
}
