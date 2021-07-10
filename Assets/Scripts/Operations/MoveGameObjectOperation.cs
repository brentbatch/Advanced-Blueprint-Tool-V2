using Assets.Scripts.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Operations
{
    public class MoveGameObjectOperation : MutableOperation
    {
        private readonly GameObject gameObject;
        private Vector3Int offset;

        public MoveGameObjectOperation(GameObject gameObject, Vector3Int offset)
        {
            this.gameObject = gameObject;
            this.offset = offset;
        }

        public override void Execute()
        {
            MoveSelectedObject(offset);
        }

        public override void Undo()
        {
            MoveSelectedObject(offset * -1);
        }

        public void UpdateOffset(Vector3Int extraOffset)
        {
            CheckFinished();
            offset += extraOffset;
            MoveSelectedObject(offset);
        }

        private void MoveSelectedObject(Vector3Int offset)
        {
            gameObject.transform.position += offset;
        }
    }
}
