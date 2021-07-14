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
#if UNITY_EDITOR
using UnityEditor.ShortcutManagement;
#endif

namespace Assets.Scripts.Unity
{
    public class GameController : MonoBehaviour
    {
        public static GameController Instance;

        [Header("References")]
        public PlayerController playerController;
        public HotbarController hotbarController;
        public ToolController toolController;
        public MessageController messageController;

        // unity player input system : https://github.com/Unity-Technologies/InputSystem/blob/develop/Packages/com.unity.inputsystem/InputSystem/Plugins/PlayerInput/PlayerInput.cs
        public PlayerInput playerInput;
        public EscapeMenu escapeMenu;

        public TMP_Text hotbarTitleText;
        public TMP_Text hotbarDescriptionText;

        [Header("Instances")]
        public InputActions inputActions;

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


        [Header("Paths")]
        public string upgradeResourcesPath = "$Game_data/upgrade_resources.json";

        public void Awake()
        {
            if (Instance == null) // stupid singleton to make class variables available anywhere
                Instance = this;
            else
                throw new Exception("more than one GameController");

            inputActions = new InputActions();
            IsCursorVisible = true;
#if UNITY_EDITOR
            ShortcutManager.instance.activeProfileId = "playmode";
#endif
        }
        private void OnApplicationQuit()
        {
#if UNITY_EDITOR
            ShortcutManager.instance.activeProfileId = "Default";
#endif

        }

        private void OnEnable()
        {
            playerInput.currentActionMap = inputActions.Game;
            
            inputActions.Game.Camera.performed += ctx => { if (ctx.ReadValueAsButton()) OnCursorToggle(); };
            inputActions.Game.Escape.performed += ctx => { 
                if (toolController.selectedTool?.OnEsc(ctx.ReadValueAsButton()) != false && ctx.ReadValueAsButton())
                {
                    escapeMenu.Toggle();
                }
            };
            inputActions.UI.Escape.performed += ctx => {
                if (toolController.selectedTool?.OnEsc(ctx.ReadValueAsButton()) != false && ctx.ReadValueAsButton())
                {
                    escapeMenu.Toggle();
                }
            };

            Button focusBtn = GameObject.Find("PullFocusButton").GetComponent<Button>();
            focusBtn.onClick.AddListener(() =>
            {
                if (toolController.selectedTool?.OnFocus() != false)
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
            var newmap = cursorVisible ? Instance.inputActions.UI.Get() : Instance.inputActions.Game.Get();
            if (Instance.playerInput.currentActionMap != newmap) Instance.playerInput.currentActionMap = newmap;

            IsCursorVisible = cursorVisible;
            Cursor.lockState = IsCursorVisible ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = IsCursorVisible;
            //Time.timeScale = cursorVisible ? 0 : 1;
            EventSystem.current.SetSelectedGameObject(null);
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