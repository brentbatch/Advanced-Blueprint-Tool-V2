using System;
using UnityEngine;

namespace Assets.Scripts.Model.BlueprintObject
{
    public class JointObject : ChildObject
    {

        [ContextMenu("rotation")]
        void SetRotationDebug()
        {
            this.SetBlueprintRotation(this.xaxis, this.zaxis);
        }

        public override void SetBlueprintRotation(int xaxis, int zaxis)
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
            this.CalculateRotatedBounds();
        }


        public override (int,int,int) GetBlueprintPosition() //fkd in joints   // TODO: use class instead of (int,int,int) // this new class should have override add operators
        {
            (int xaxis, int zaxis) = this.GetBlueprintRotation();
            (int x, int y, int z) = GetRotationPositionOffset(xaxis, zaxis);
            // re-hack position offset:
            
            // get and offset position property

            return default;
        }

        public void DoRotationPositionOffset(int xaxis, int zaxis) 
        {
            // un-hack position:
            (int x, int y, int z) = GetRotationPositionOffset(xaxis, zaxis);
            // todo: shift position property
            gameObject.transform.position += new Vector3(x,y,z); // unity xyz
        }

        private (int, int, int) GetRotationPositionOffset(int xaxis, int zaxis) // unity xyz
        {
            int x = 0, y = 0, z = 0;
            var xAbs = Math.Abs(xaxis);
            //var zAbs = Math.Abs(zaxis);

            if (xaxis == -1 || (zaxis == -1 && xAbs != 1) || ((xaxis == 2 && zaxis == 3) || (xaxis == 3 && zaxis == -2) || (xaxis == -2 && zaxis == -3) || (xaxis == -3 && zaxis == 2)))
            {
                x = 1;
            }
            if (xaxis == -3 || (zaxis == -3 && xAbs != 3) || ((xaxis == 1 && zaxis == 2) || (xaxis == 2 && zaxis == -1) || (xaxis == -1 && zaxis == -2) || (xaxis == -2 && zaxis == 1)))
            {
                y = 1;
            }
            if (xaxis == -2 || (zaxis == -2 && xAbs != 2) || (xaxis == 3 && zaxis == 1) || ((xaxis == 1 && zaxis == -3) || (xaxis == -3 && zaxis == -1) || (xaxis == -1 && zaxis == 3)))
            {
                z = 1;
            }
            return (x,y,z);
        }

    }
}