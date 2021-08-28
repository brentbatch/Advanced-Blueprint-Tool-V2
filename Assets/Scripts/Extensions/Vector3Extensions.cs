using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Extensions
{
    public static class Vector3Extensions
    {
        public static Vector3 Rotate(this Vector3 vector, Quaternion rotation, Vector3 pivot = default(Vector3))
        {
            return rotation * (vector - pivot) + pivot;
        }

        public static Vector3 Rotate(this Vector3 vector, Vector3 rotation, Vector3 pivot = default(Vector3))
        {
            return Rotate(vector, Quaternion.Euler(rotation), pivot);
        }

        public static Vector3 Rotate(this Vector3 vector, float x, float y, float z, Vector3 pivot = default(Vector3))
        {
            return Rotate(vector, Quaternion.Euler(x, y, z), pivot);
        }
    }
}
