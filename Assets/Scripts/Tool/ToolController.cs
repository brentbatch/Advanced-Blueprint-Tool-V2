using Assets.Scripts.Unity;
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
        [SerializeField] public AbstractTool selectedTool;


        private List<AbstractTool> toolList;
        public List<AbstractTool> ToolList { get => toolList ?? (toolList = new List<AbstractTool>()
            {
                new LiftTool(),
                new MutatorTool(),
            });
        }


        private void Awake()
        {
            
        }

        private void OnEnable()
        {

        }

        private void Start()
        {
            InputActions inputActions = GameController.Instance.inputActions;

            inputActions.Game.LeftClick.performed += ctx => selectedTool?.OnLeftClick(ctx.ReadValueAsButton());
            inputActions.Game.RightClick.performed += ctx => selectedTool?.OnRightClick(ctx.ReadValueAsButton());
            inputActions.Game.Interact.performed += ctx => selectedTool?.OnInteract(ctx.ReadValueAsButton());
            inputActions.Game.NextRotation.performed += ctx => selectedTool?.OnNextRotation(ctx.ReadValueAsButton());
            inputActions.Game.PreviousRotation.performed += ctx => selectedTool?.OnPreviousRotation(ctx.ReadValueAsButton());
            inputActions.Game.R.performed += ctx => selectedTool?.OnR(ctx.ReadValueAsButton());
            inputActions.Game.F.performed += ctx => selectedTool?.OnF(ctx.ReadValueAsButton());
            inputActions.Game.Tab.performed += ctx => selectedTool?.OnTab(ctx.ReadValueAsButton());
            inputActions.Game.Help.performed += ctx => selectedTool?.OnToolInfo(ctx.ReadValueAsButton());
            inputActions.Game.Move2.performed += ctx => selectedTool?.OnMove2(ctx.ReadValue<Vector3>());


        }



    }
}
