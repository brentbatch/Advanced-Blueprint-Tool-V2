using Assets.Scripts.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Operations
{
    public class MoveGameObjectsOperation : MutableOperation
    {
        private readonly GameObject[] gameObjects;
        private Vector3Int offset;

        public MoveGameObjectsOperation(IEnumerable<GameObject> gameObjects, Vector3Int offset)
        {
            this.gameObjects = gameObjects.ToArray();
            this.offset = offset;
        }

        public override void Execute()
        {
            MoveSelectedObjects(offset);
        }

        public override void Undo()
        {
            MoveSelectedObjects(offset * -1);
        }

        public void UpdateOffset(Vector3Int extraOffset)
        {
            CheckFinished();
            offset += extraOffset;
            MoveSelectedObjects(offset);
        }

        private void MoveSelectedObjects(Vector3Int offset)
        {
            foreach(var selection in gameObjects)
            {
                selection.transform.position += offset;
            }
        }
    }
}
