using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace Assets.Scripts.Operations
{
    public class NewPositionsOperation : MutableOperation
    {
        private readonly GameObject[] gameObjects;
        private Vector3[] originalPositions;
        private Vector3[] newPositions;

        public NewPositionsOperation(IEnumerable<GameObject> gameObjects, IEnumerable<Vector3> newPositions)
        {
            this.gameObjects = gameObjects.ToArray();
            this.originalPositions = gameObjects.Select(gameObject => gameObject.transform.position).ToArray();
            this.newPositions = newPositions.ToArray();
            Assert.IsTrue(this.gameObjects.Length == this.newPositions.Length, "NewPositionOperation: parameter Lengths do not match");
        }

        public override void Execute()
        {
            foreach(var gameObject in gameObjects)
            {

            }
        }

        public override void Undo()
        {

        }
    }
}
