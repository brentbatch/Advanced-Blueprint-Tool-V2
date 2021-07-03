using Assets.Scripts.Extensions;
using Assets.Scripts.Tool;
using Assets.Scripts.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public AbstractTool selectedTool;
    [SerializeField] private GameObject projectile;

    #region camera variables
    [Header("Movement Settings")]
    [Tooltip("Speed on translation")]
    public float movementSpeed = 5f;

    [Tooltip("Speed modifier")]
    public float sprintModifier = 3.5f;

    [Tooltip("Time it takes to interpolate camera position 99% of the way to the target."), Range(0.001f, 1f)]
    public float positionLerpTime = 0.2f;

    [Tooltip("Time it takes to interpolate camera position 99% of the way to the target when SPRINTING."), Range(0.001f, 1f)]
    public float sprintPositionLerpTime;

    [Header("Rotation Settings")]
    [Tooltip("X = Change in mouse position.\nY = Multiplicative factor for camera rotation.")]
    public AnimationCurve mouseSensitivityCurve = new AnimationCurve(new Keyframe(0f, 0.5f, 0f, 5f), new Keyframe(1f, 2.5f, 0f, 0f));

    [Tooltip("Time it takes to interpolate camera rotation 99% of the way to the target."), Range(0.001f, 1f)]
    public float rotationLerpTime = 0.01f;

    [Tooltip("Multiplied by the mouse speed")]
    public float mouseSensitivity = 1f;

    [Tooltip("Whether or not to invert our Y axis for mouse input to rotation.")]
    public bool invertY = false;

    public CameraState m_TargetCameraState = new CameraState();
    CameraState m_InterpolatingCameraState = new CameraState();
    #endregion

    [Header("Camera extras")]
    [SerializeField] public bool snapToCreation = true;
    [SerializeField] public float distanceToSnap = 30f;


    // inputs from inputactions:
    public Vector2 inputMovement;
    public Vector2 inputView;
    public bool isSprinting;
    public bool isCrouching;
    public bool isJumping;

    // script-set variables:
    private bool isHovering = true;
    private Vector3 gravityVelocity;


    public static bool isCursorVisible => GameController.IsCursorVisible;

    private void OnEnable()
    {
        m_TargetCameraState.SetFromTransform(transform);
        m_InterpolatingCameraState.SetFromTransform(transform);
    }

    private void Awake()
    {

    }

    void Start()
    {
        InputActions inputActions = GameController.Instance.inputActions;

        inputActions.Game.Move.performed += ctx => inputMovement = ctx.ReadValue<Vector2>();
        inputActions.Game.Move.canceled += ctx => inputMovement = ctx.ReadValue<Vector2>();
        inputActions.Game.Sprint.performed += ctx => this.isSprinting = true;
        inputActions.Game.Sprint.canceled += ctx => this.isSprinting = false;
        inputActions.Game.Look.performed += ctx => inputView = ctx.ReadValue<Vector2>();
        inputActions.Game.Look.canceled += ctx => inputView = ctx.ReadValue<Vector2>();
        inputActions.Game.Jump.performed += ctx =>
        {
            this.isJumping = ctx.ReadValueAsButton();
            if (ctx.ReadValueAsButton())
            {
                OnJump();
            }
        };
        inputActions.Game.Crouch.performed += ctx =>
        {
            this.isCrouching = ctx.ReadValueAsButton();
            if (ctx.ReadValueAsButton())
            {
                OnCrouch();
            }
        };
        inputActions.Game.Camera.performed += ctx =>
        {
            if (ctx.ReadValueAsButton())
            {
                GameController.OnCursorToggle();
            }
        };

        //inputActions.Game.DoubleJump.performed += ctx => OnDoubleJump();
        //inputActions.Game.LeftClick.performed += ctx => OnLeftClick();

        //inputs for the tool:
        inputActions.Game.LeftClick.performed += ctx => { if (!isCursorVisible) this.selectedTool?.OnLeftClick(ctx.ReadValueAsButton()); };
        inputActions.Game.RightClick.performed += ctx => { if (!isCursorVisible) this.selectedTool?.OnRightClick(ctx.ReadValueAsButton()); };
        inputActions.Game.Interact.performed += ctx => { if (!isCursorVisible) this.selectedTool?.OnInteract(ctx.ReadValueAsButton()); };
        inputActions.Game.NextRotation.performed += ctx => { if (!isCursorVisible) this.selectedTool?.OnNextRotation(ctx.ReadValueAsButton()); };
        inputActions.Game.PreviousRotation.performed += ctx => { if (!isCursorVisible) this.selectedTool?.OnPreviousRotation(ctx.ReadValueAsButton()); };
        inputActions.Game.R.performed += ctx => { if (!isCursorVisible) this.selectedTool?.OnR(ctx.ReadValueAsButton()); };
        inputActions.Game.F.performed += ctx => { if (!isCursorVisible) this.selectedTool?.OnF(ctx.ReadValueAsButton()); };
        inputActions.Game.Tab.performed += ctx => { if (!isCursorVisible) this.selectedTool?.OnTab(ctx.ReadValueAsButton()); };
        inputActions.Game.Help.performed += ctx => { if (!isCursorVisible) this.selectedTool?.OnToolInfo(ctx.ReadValueAsButton()); };

    }


    Color color;
    public void OpenColorPicker()
    {
        ColorPicker.Create(this.color, "colortool", OnColorChanged, OnColorSelected);
    }

    private void OnColorSelected(Color c)
    {
        Debug.Log(c);
    }

    private void OnColorChanged(Color c)
    {
        this.color = c;
        Debug.Log(c);
    }

    void Update()
    {
        if (!isCursorVisible)
        {
            Move(inputMovement);
            Look(inputView);
        }
        DoInterPolate();
    }

    #region translation, rotation & interpolation
    void Move(Vector2 direction)
    {
        Vector3 translation = new Vector3(direction.x, 0, direction.y) * Time.deltaTime;

        if (isHovering)
        {
            if (isCrouching)
            {
                translation.y -= Time.deltaTime;
            }
            if (isJumping)
            {
                translation.y += Time.deltaTime;
            }
        }
        else
        {
            gravityVelocity += Physics.gravity * Time.deltaTime;
            translation += gravityVelocity * Time.deltaTime;

            // todo: add groundcheck, toggle player collider
        }

        if (isSprinting)
        {
            translation *= sprintModifier;
        }
        translation *= movementSpeed;

        m_TargetCameraState.Translate(translation);
    }

    void Look(Vector2 inputView)
    {
        var mouseSensitivityFactor = mouseSensitivityCurve.Evaluate(inputView.magnitude) * mouseSensitivity / 100f;

        m_TargetCameraState.yaw += inputView.x * mouseSensitivityFactor;
        m_TargetCameraState.pitch += inputView.y * mouseSensitivityFactor * (invertY ? 1 : -1);
        m_TargetCameraState.pitch = Mathf.Clamp(m_TargetCameraState.pitch, -90, 90);
    }

    private void DoInterPolate()
    {
        // Framerate-independent interpolation
        // Calculate the lerp amount, such that we get 99% of the way to our target in the specified time
        var positionLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / (isSprinting? sprintPositionLerpTime : positionLerpTime)) * Time.deltaTime);
        var rotationLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / rotationLerpTime) * Time.deltaTime);
        m_InterpolatingCameraState.LerpTowards(m_TargetCameraState, positionLerpPct, rotationLerpPct);
        m_InterpolatingCameraState.UpdateTransform(transform);
    }
    #endregion


    private void OnLeftClick()
    {
        if (isCursorVisible)
            return;

        GameObject newProjectile = Instantiate(projectile, transform.position + transform.forward * 0.5f, Quaternion.identity);
        newProjectile.GetComponent<Rigidbody>().AddForce(100f * transform.forward, ForceMode.Impulse);
    }

    private void OnJump()
    {

    }

    private void OnDoubleJump()
    {
        //isHovering = !isHovering;
    }

    private void OnCrouch()
    {

    }


    public class CameraState
    {
        public float yaw;
        public float pitch;
        public float roll;
        public float x;
        public float y;
        public float z;

        public void SetFromTransform(Transform t)
        {
            pitch = t.eulerAngles.x;
            yaw = t.eulerAngles.y;
            roll = t.eulerAngles.z;
            x = t.position.x;
            y = t.position.y;
            z = t.position.z;
        }

        public void Translate(Vector3 translation)
        {
            Vector3 rotatedTranslation = Quaternion.Euler(0, yaw, roll) * translation;

            x += rotatedTranslation.x;
            y += rotatedTranslation.y;
            z += rotatedTranslation.z;
        }

        internal void LerpTowards(CameraState target, float positionLerpPct, float rotationLerpPct)
        {
            yaw = Mathf.Lerp(yaw, target.yaw, rotationLerpPct);
            pitch = Mathf.Lerp(pitch, target.pitch, rotationLerpPct);
            roll = Mathf.Lerp(roll, target.roll, rotationLerpPct);

            x = Mathf.Lerp(x, target.x, positionLerpPct);
            y = Mathf.Lerp(y, target.y, positionLerpPct);
            z = Mathf.Lerp(z, target.z, positionLerpPct);
        }

        internal void UpdateTransform(Transform t)
        {
            t.eulerAngles = new Vector3(pitch, yaw, roll);
            t.position = new Vector3(x, y, z);
        }
    }
}
