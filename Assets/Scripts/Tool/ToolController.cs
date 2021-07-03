using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Tool
{
    public class ToolController : MonoBehaviour
    {

        public static ToolController Instance { get; private set; }

        private List<AbstractTool> toolList;
        public List<AbstractTool> ToolList { get => toolList ?? (toolList = new List<AbstractTool>()
            {
                new LiftTool(),
                new MutatorTool(),
                new MutatorTool(),
            });
        }


        private void Awake()
        {
            if (Instance == null) // stupid singleton to make class variables available anywhere
                Instance = this;
            else
                throw new Exception("more than one ToolController");
            
        }

        private void OnEnable()
        {
        }

        private void Start()
        {

        }

    }
}
