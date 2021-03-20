using Assets.Scripts.Model.Data;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Model.BlueprintObject
{
    public class BodyObject : MonoBehaviour
    {
        public List<ChildObject> Childs = new List<ChildObject>();

    }
}