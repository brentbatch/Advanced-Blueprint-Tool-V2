using System;
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

    }
}