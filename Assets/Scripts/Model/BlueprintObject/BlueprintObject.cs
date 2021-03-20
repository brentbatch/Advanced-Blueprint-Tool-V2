using Assets.Scripts.Model.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Model.BlueprintObject
{
    public class BlueprintObject : MonoBehaviour
    {
        public List<BodyObject> Bodies = new List<BodyObject>();
        public List<JointObject> Joints = new List<JointObject>();

        public void ReturnObject() // return to collection aka cleanup
        {

        }

        public BlueprintData ToBlueprint()
        {
            throw new NotImplementedException();
        }
    }
}
