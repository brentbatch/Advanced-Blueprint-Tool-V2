using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Operations
{
    public class NewPositionOperation : MutableOperation
    {
        private readonly GameObject gameObject;
        private Vector3 originalPosition;
        private Vector3 newPosition;

        public NewPositionOperation(GameObject gameObject, Vector3 newPosition)
        {
            this.gameObject = gameObject;
            this.originalPosition = gameObject.transform.position;
            this.newPosition = newPosition;
        }

        public override void Execute()
        {
            gameObject.transform.position = newPosition;
        }

        public override void Undo()
        {
            gameObject.transform.position = originalPosition;
        }

    }
}
