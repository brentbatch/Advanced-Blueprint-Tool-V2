using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

using Data = Assets.Scripts.Model.Data;

namespace Assets.Scripts.Model.BlueprintObject
{
    public class BodyScript : MonoBehaviour
    {
        public List<ChildScript> Childs = new List<ChildScript>();

        internal Data.Body ToBlueprintData()
        {
            return new Data.Body()
            {
                Childs = Childs.Select(child => child.ToBlueprintData()).ToList()
            };
        }
    }
}