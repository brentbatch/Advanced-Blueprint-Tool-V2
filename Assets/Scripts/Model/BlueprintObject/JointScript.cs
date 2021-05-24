using Assets.Scripts.Model.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Model.BlueprintObject
{
    public class JointScript : ChildScript
    {
        public int Id { get; set; }
        public ChildScript childA { get; set; }
        public ChildScript childB { get; set; }


        [ContextMenu("rotation")]
        void SetRotationDebug()
        {
            this.SetBlueprintRotation(this.xaxis, this.zaxis);
        }

        public override (int, int, int) GetBlueprintPosition() //fkd in joints
        {
            (int xaxis, int zaxis) = this.GetBlueprintRotation();
            Vector3Int offset = GetRotationPositionOffset(xaxis, zaxis);
            // re-hack position offset:
            var position = transform.position - offset;
            return (Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z), Mathf.RoundToInt(position.y));
        }

        public void DoRotationPositionOffset(int xaxis, int zaxis) 
        {
            // un-hack position when loading from blueprint JSON to unity script.
            Vector3Int offset = GetRotationPositionOffset(xaxis, zaxis);
            gameObject.transform.position += offset;
        }

        private Vector3Int GetRotationPositionOffset(int xaxis, int zaxis) // unity xyz
        {
            Vector3Int offset = Vector3Int.zero;
            var xAbs = Math.Abs(xaxis);
            //var zAbs = Math.Abs(zaxis);

            if (xaxis == -1 || (zaxis == -1 && xAbs != 1) || ((xaxis == 2 && zaxis == 3) || (xaxis == 3 && zaxis == -2) || (xaxis == -2 && zaxis == -3) || (xaxis == -3 && zaxis == 2)))
            {
                offset.x = 1;
            }
            if (xaxis == -3 || (zaxis == -3 && xAbs != 3) || ((xaxis == 1 && zaxis == 2) || (xaxis == 2 && zaxis == -1) || (xaxis == -1 && zaxis == -2) || (xaxis == -2 && zaxis == 1)))
            {
                offset.y = 1;
            }
            if (xaxis == -2 || (zaxis == -2 && xAbs != 2) || (xaxis == 3 && zaxis == 1) || ((xaxis == 1 && zaxis == -3) || (xaxis == -3 && zaxis == -1) || (xaxis == -1 && zaxis == 3)))
            {
                offset.z = 1;
            }
            return offset;
        }

        internal Data.Joint ToBlueprintData(List<BodyScript> bodies) // z is up
        {
            Data.PartData partData = (this.shape as Part).partData;

            List<ChildScript> childScripts = bodies.SelectMany(bodyScript => bodyScript.Childs).ToList();

            (int x, int y, int z) = this.GetBlueprintPosition();

            (int xaxis, int zaxis) = this.GetBlueprintRotation();

            var zAbs = Math.Abs(zaxis);
            var zSign = Math.Sign(zaxis);
            int jointLength = partData.Bearing != null ? 0 : Mathf.RoundToInt(gameObject.GetComponent<BoxCollider>().size.y) * zSign;
            (int offsetx, int offsety, int offsetz) = (zAbs == 1 ? jointLength : 0, zAbs == 2 ? jointLength : 0, zAbs == 3 ? jointLength : 0);


            Data.Joint joint = new Data.Joint()
            {
                ChildA = childScripts.IndexOf(this.childA),
                ChildB = this.childB == null ? -1 : childScripts.IndexOf(this.childB),
                Color = this.GetColor(),
                Controller = this.Controller,
                Id = this.Id,
                ShapeId = partData.Uuid,
                PosA = new Data.Pos() { X = x, Y = y, Z = z},
                PosB = this.childB == null ? null : new Data.Pos() { X = x + offsetx, Y = y + offsety, Z = z + offsetz },
                XaxisA = xaxis,
                XaxisB = xaxis,
                ZaxisA = zaxis,
                ZaxisB = zaxis
            };

            return joint;
        }

        public override void Destroy()
        {
            UnityEngine.GameObject.Destroy(gameObject);
            // nothing else for now
        }
    }
}