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
        public BodyScript Body { get; set; }
        public List<JointScript> connectedJoints;
        public Shape shape;
        public int shapeIdx;

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

        public string GetColor()
        {
            return ColorUtility.ToHtmlStringRGB(
                gameObject.GetComponentsInChildren<MeshRenderer>()[0].material.color).ToLower();
        }

        public virtual (int, int, int) GetBlueprintPosition() //fkd in joints
        {
            return (Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z), Mathf.RoundToInt(transform.position.y));
        }

        public (int, int, int) GetBounds()
        {
            var childMesh = transform.GetChild(0);
            var scale = childMesh.transform.localScale;
            return (Mathf.RoundToInt(scale.x), Mathf.RoundToInt(scale.z), Mathf.RoundToInt(scale.y));
        }

        public (int, int) GetBlueprintRotation()
        {
            int xaxis = Mathf.RoundToInt(transform.right.x + transform.right.z * 2 + transform.right.y * 3);
            int zaxis = Mathf.RoundToInt(transform.up.x + transform.up.z * 2 + transform.up.y * 3);
            return (xaxis, zaxis);
        }

        public virtual void Destroy()
        {
            if (connectedJoints != null)
            {
                var joints = connectedJoints.ToArray();
                connectedJoints = null;
                foreach (JointScript joint in joints)
                {
                    if (joint.childA == this)
                    {
                        joint.Destroy();
                    }
                    if (joint.childB == this)
                    {
                        joint.childB = null;
                    }
                }
            }
            UnityEngine.GameObject.Destroy(gameObject);
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


        internal Data.Child ToBlueprintData()
        {
            bool isPart = shape is Part;

            Data.Child child = new Data.Child()
            {
                Color = this.GetColor(),
                ShapeId = isPart ? (shape as Part).partData.Uuid : (shape as Block).blockData.Uuid,
                Controller = this.Controller,
                Joints = connectedJoints?.Select(
                    joint => new JointReference()
                    {
                        Id = joint.Id
                    }).ToList()
            };

            {
                (int x, int y, int z) = this.GetBlueprintPosition();
                child.Pos = new Data.Pos() { X = x, Y = y, Z = z };

                (int xaxis, int zaxis) = this.GetBlueprintRotation();
                child.Xaxis = xaxis;
                child.Zaxis = zaxis;
            }

            if (!isPart)
            {
                (int x, int y, int z) = this.GetBounds();
                child.Bounds = new Data.Bounds() { X = x, Y = y, Z = z };
            }

            return child;
        }
    }
}
