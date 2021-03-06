using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Tool;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.InputSystem.InputAction;


namespace Assets.Scripts.Unity
{
    public class HotbarController : MonoBehaviour
    {
        [Header("References")]
        private Sprite transparentSprite;
        private ToolController toolController;
        private List<AbstractTool> tools;

        [Header("Config")]
        public int indexToolHotbarButton;
        public int indexFunctionHotbarButton;
        public int offsetTool;
        public int offsetFunction;


        [Header("UI")]
        [SerializeField] private GameObject toolPanel;
        [SerializeField] private GameObject functionPanel;

        [SerializeField] private List<GameObject> toolButtons;
        [SerializeField] private List<GameObject> functionButtons;

        [SerializeField] private GameObject toolPanelButtonLeft;
        [SerializeField] private GameObject toolPanelButtonRight;
        [SerializeField] private GameObject functionPanelButtonLeft;
        [SerializeField] private GameObject functionPanelButtonRight;

        [SerializeField] private GameObject titlePanel;
        [SerializeField] private GameObject descriptionPanel;

        private readonly List<HotbarButton> toolHotbarButtons = new List<HotbarButton>();
        private readonly List<HotbarButton> functionHotbarButtons = new List<HotbarButton>();

        public static bool IsToolBarSelected { get; private set; }


        private void Awake()
        {
            IsToolBarSelected = true;
            transparentSprite = Resources.Load<Sprite>("Textures/transparent");
        }

        private void OnEnable()
        {
        }

        private void Start()
        {
            InputActions inputActions = GameController.Instance.inputActions;
            inputActions.Enable();
            inputActions.Game.MouseScroll.performed += ctx => OnScroll(ctx);
            inputActions.Game.MiddleClick.performed += ctx => OnMiddleMouseClick(ctx);

            toolController = GameController.Instance.toolController;
            tools = toolController.ToolList;

            tools.ForEach(tool => tool.OnStart());

            for (int i = 0; i < toolButtons.Count; i++)
            {
                GameObject mainButton = toolButtons[i];

                HotbarButton hotbarButton = new HotbarButton(mainButton);
                toolHotbarButtons.Add(hotbarButton);

                int btnIndex = i;
                hotbarButton.onSelected += () =>
                {
                    IsToolBarSelected = true;
                    // update selection outline:
                    toolHotbarButtons.ForEach(btn => { if (btn != hotbarButton) btn.UnSelected(); });

                    indexToolHotbarButton = btnIndex;
                    //int index = (btnIndex + offsetTool).mod(tools.Count);
                    int index = (btnIndex + offsetTool);
                    if (index >= tools.Count)
                        return;
                    AbstractTool tool = tools[index];

                    toolController.selectedTool?.OnUnEquip();
                    toolController.selectedTool = tool;
                    tool.OnEquip();
                    functionHotbarButtons[indexFunctionHotbarButton].Selected();
                    IsToolBarSelected = true;

                    ShowToolBarHighlight();
                    FillFunctionIcons();
                };
            }
            for (int i = 0; i < functionButtons.Count; i++)
            {
                GameObject featureButton = functionButtons[i];

                HotbarButton hotbarButton = new HotbarButton(featureButton);
                functionHotbarButtons.Add(hotbarButton);

                int btnIndex = i;
                hotbarButton.onSelected += () =>
                {
                    IsToolBarSelected = false;
                    // unselect other buttons:
                    functionHotbarButtons.ForEach(btn => { if (btn != hotbarButton) btn.UnSelected(); });

                    indexFunctionHotbarButton = btnIndex;
                    int index = (btnIndex + offsetFunction);
                    toolController.selectedTool.SelectToolFunction(index);
                    ShowToolBarHighlight();
                };
                // btnclick from UI:
                hotbarButton.onClick += () => {
                    IsToolBarSelected = false;
                    toolController.selectedTool.OnHotBarFunctionClick();
                    ShowToolBarHighlight();
                };

            }

            toolPanelButtonRight.GetComponent<Button>().onClick.AddListener(NextTool);
            toolPanelButtonLeft.GetComponent<Button>().onClick.AddListener(PreviousTool);
            functionPanelButtonRight.GetComponent<Button>().onClick.AddListener(NextFunction);
            functionPanelButtonLeft.GetComponent<Button>().onClick.AddListener(PreviousFunction);

            FillToolIcons();
            toolHotbarButtons[0].Selected();
            functionHotbarButtons[0].Selected();
        }

#pragma warning disable UNT0008 // Null propagation on Unity objects
        private void Update() => toolController.selectedTool?.OnUpdate();
        private void FixedUpdate() => toolController.selectedTool?.OnFixedUpdate();
#pragma warning restore UNT0008 // Null propagation on Unity objects

        private void FillToolIcons()
        {
            // todo: mind the 'offset'
            for (int i = 0; i < toolHotbarButtons.Count; i++)
            {
                var btn = toolHotbarButtons[i];
                int offsetindex = (i + offsetTool);//.mod(tools.Count);
                btn.image.sprite = 0 <= offsetindex && offsetindex < tools.Count ? tools[offsetindex].sprite : transparentSprite;
            }
        }

        private void FillFunctionIcons()
        {
            // todo: mind the 'offset'
            List<ToolFunction> toolFunctions = toolController.selectedTool.functions;
            for (int i = 0; i< functionHotbarButtons.Count; i++)
            {
                var btn = functionHotbarButtons[i];
                int offsetindex = Mathf.Max(i + offsetFunction, 0);
                btn.image.sprite = toolFunctions.Count > offsetindex ? toolFunctions[offsetindex].sprite : transparentSprite;
            }
        }

        private void OnMiddleMouseClick(CallbackContext ctx)
        {
            if (ctx.ReadValueAsButton())
            {
                IsToolBarSelected = !IsToolBarSelected;
                toolController.selectedTool?.ShowToolInfo();
                ShowToolBarHighlight();
                //StartCoroutine(ResetColor(img));
            }
        }

        private void ShowToolBarHighlight()
        {
            Image toolPanelImg = toolPanel.GetComponent<Image>();
            Image functionPanelImg = functionPanel.GetComponent<Image>();
            if (IsToolBarSelected)
            {
                toolPanelImg.color = new Color(1f, 1f, 1f, 100 / 255f);
                functionPanelImg.color = new Color(1f, 1f, 1f, 0f);
            }
            else
            {
                toolPanelImg.color = new Color(1f, 1f, 1f, 0);
                functionPanelImg.color = new Color(1f, 1f, 1f, 100 / 255f);
            }
        }

        IEnumerator ResetColor(Image image)
        {
            yield return new WaitForSeconds(0.2f);
            image.color = Color.clear;
        }

        private void OnScroll(CallbackContext ctx)
        {
            // scroll through hotbar
            float scroll = ctx.ReadValue<Vector2>().y;

            if (IsToolBarSelected)
            {
                if (scroll > 0)
                {
                    PreviousTool();
                }
                else
                {
                    NextTool();
                }
            }
            else
            {
                if (scroll > 0)
                {
                    PreviousFunction();
                }
                else
                {
                    NextFunction();
                }
            }
        }

        private void NextFunction()
        {
            if (indexFunctionHotbarButton + 1 >= 9)
            {
                int functionAmount = toolController.selectedTool.functions.Count;
                //if (functionAmount > 9)
                //    offsetFunction = (offsetFunction + 1).mod(functionAmount); // loops around
                if (offsetFunction + indexFunctionHotbarButton + 1 < toolController.selectedTool.functions.Count) // stops at max
                    offsetFunction++;
            }
            else
            {
                indexFunctionHotbarButton++;
            }

            FillFunctionIcons();
            functionHotbarButtons[indexFunctionHotbarButton].Selected();
        }

        private void PreviousFunction()
        {
            if (indexFunctionHotbarButton - 1 < 0)
            {
                int functionAmount = toolController.selectedTool.functions.Count;
                //if (functionAmount > 9)
                //    offsetFunction = (offsetFunction - 1).mod(functionAmount); // loops around
                if (offsetFunction - 1 >= 0) // stops at max
                    offsetFunction--;
            }
            else
            {
                indexFunctionHotbarButton--;
            }

            FillFunctionIcons();
            functionHotbarButtons[indexFunctionHotbarButton].Selected();
        }

        private void NextTool()
        {
            if(indexToolHotbarButton + 1 >= 9)
            {
                //offsetTool = (offsetTool + 1).mod(tools.Count); // loops around
                //if (offsetTool + 1 < tools.Count) // stops at max
                //    offsetTool++;
            }
            else
            {
                indexToolHotbarButton++;
            }

            FillToolIcons();
            toolHotbarButtons[indexToolHotbarButton].Selected();
        }

        private void PreviousTool()
        {
            if (indexToolHotbarButton - 1 < 0)
            {
                //offsetTool = (offsetTool - 1).mod(tools.Count);
            }
            else
            {
                indexToolHotbarButton--;
            }

            FillToolIcons();
            toolHotbarButtons[indexToolHotbarButton].Selected();
        }



    }
}