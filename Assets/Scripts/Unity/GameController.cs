using Assets.Scripts.Tool;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Assets.Scripts.Unity
{
    public class GameController : MonoBehaviour
    {
        public static GameController Instance;

        [Header("References")]
        public PlayerController playerController;
        public HotbarController hotbarController;
        public ToolController ToolController;

        // unity player input system : https://github.com/Unity-Technologies/InputSystem/blob/develop/Packages/com.unity.inputsystem/InputSystem/Plugins/PlayerInput/PlayerInput.cs
        public PlayerInput playerInput;
        public InputActions inputActions;
        public EscapeMenu escapeMenu;

        public TMP_Text hotbarTitleText;
        public TMP_Text hotbarDescriptionText;


        [Header("Prefabs")]
        public GameObject Cube;

        public GameObject Blueprint;
        public GameObject Body;
        public GameObject Block;
        public GameObject Part;
        public GameObject Joint;
        public GameObject SubMesh;

        [Header("Materials")]
        public UnityEngine.Material partMaterial;
        public UnityEngine.Material glassPartMaterial;

        public UnityEngine.Material blockMaterial;
        public UnityEngine.Material glassBlockMaterial;

        [Header("Settings/properties")]
        public bool potatoMode = false;
        public static bool IsCursorVisible { get; private set; }


        // paths:
        public string upgradeResourcesPath = "$Game_data/upgrade_resources.json";

        public void Awake()
        {
            if (Instance == null) // stupid singleton to make class variables available anywhere
                Instance = this;
            else
                throw new Exception("more than one GameController");

            //escapeMenu = GameObject.Find("EscapeCanvas").GetComponent<EscapeMenu>();
            //playerInput = GetComponent<PlayerInput>();
            inputActions = new InputActions();
            IsCursorVisible = true;
        }

        private void OnEnable()
        {
            playerInput.currentActionMap = inputActions.Game;
            
            inputActions.Game.Escape.performed += ctx => { 
                if (playerController.selectedTool?.OnEsc(ctx.ReadValueAsButton()) != false
                && ctx.ReadValueAsButton())
                {
                    escapeMenu.Toggle();
                }
            };

            Button focusBtn = GameObject.Find("PullFocusButton").GetComponent<Button>();
            focusBtn.onClick.AddListener(() =>
            {
                if (playerController.selectedTool?.OnFocus() != false)
                {
                    SetCursorState(false);
                }
            });
        }

        private void OnDisable()
        {
            inputActions.Disable();
            inputActions.Dispose();
        }

        public void SetPotatoMode(Toggle toggle)
        {
            potatoMode = toggle.isOn;
        }

        public static void OnCursorToggle()
        {
            SetCursorState(!IsCursorVisible);
        }

        public static void SetCursorState(bool cursorVisible)
        {
            Instance.playerInput.currentActionMap = cursorVisible ? Instance.inputActions.UI.Get() : Instance.inputActions.Game.Get();

            IsCursorVisible = cursorVisible;
            Cursor.lockState = IsCursorVisible ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = IsCursorVisible;
            if (!cursorVisible)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }


        private static bool wasCursorVisible = true;
        /// <summary>
        /// calling this with 'true' will make cursor visible, 'false' will reset cursor state to what it was before called with 'true'
        /// </summary>
        /// <param name="UiOpens"></param>
        public static void CursorFocusUI(bool UiOpens)
        {
            if (UiOpens)
            {
                wasCursorVisible = IsCursorVisible;
                SetCursorState(true);
            }
            else
            {
                SetCursorState(wasCursorVisible);
            }
        }
    }

}