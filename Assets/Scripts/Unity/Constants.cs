using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Unity
{
    public class Constants : MonoBehaviour
    {
        public static Constants Instance;

        public GameObject Cube;

        public GameObject Blueprint;
        public GameObject Body;
        public GameObject Block;
        public GameObject Part;
        public GameObject Joint;
        public GameObject SubMesh;


        public void Awake()
        {
            if (Instance == null) // stupid singleton to make class variables available anywhere
                Instance = this;
        }


    }

}