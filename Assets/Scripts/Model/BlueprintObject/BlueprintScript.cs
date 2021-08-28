using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using Data = Assets.Scripts.Model.Data;

namespace Assets.Scripts.Model.BlueprintObject
{
    public class BlueprintScript : MonoBehaviour
    {
        private Vector3? center;


        public List<BodyScript> Bodies = new List<BodyScript>();
        public List<JointScript> Joints = new List<JointScript>();

        public Vector3 Center { get => center??CalculateCenter(); private set => center = value; }


        public void ReturnObject() // return to collection aka cleanup
        {

        }

        public Data.BlueprintData ToBlueprintData()
        {
            Data.BlueprintData blueprintData = new Data.BlueprintData()
            {
                Bodies = new List<Data.Body>(),
                Joints = Joints.Count > 0 ? new List<Data.Joint>() : null,
                Version = 3
            };

            blueprintData.Bodies = Bodies.Select(body => body.ToBlueprintData()).ToList();
            blueprintData.Joints = Joints.Select(joint => joint.ToBlueprintData(Bodies)).ToList();

            return blueprintData;
        }

        public Vector3 CalculateCenter()
        {
            var positions = Bodies.SelectMany(body => body.Childs).Select(child => child.gameObject.transform.position + child.RotatedBounds/2); // todo; fix this, child center position is incorrect
            var center = positions.Aggregate((Vector3 vec1, Vector3 vec2) => vec1 + vec2) / positions.Count();
            return (Vector3)(this.center = center);
        }
    }
}
