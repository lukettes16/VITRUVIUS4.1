using UnityEngine;
using UnityEngine.InputSystem;

public class FixedPlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    public int playerId = 1;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 720f;

    private CharacterController characterController;
    private Camera playerCamera;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private Gamepad gamepad;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            characterController = gameObject.AddComponent<CharacterController>();
        }

        if (FixedSplitScreenBootstrap.Instance != null)
        {
            playerCamera = FixedSplitScreenBootstrap.Instance.GetCameraForPlayer(playerId);
        }

        AssignToSplitScreenSystem();
    }

    void Update()
    {
        HandleInput();
        HandleMovement();
    }

    private void HandleInput()
    {

        if (JoystickManager.Instance != null)
        {
            gamepad = JoystickManager.Instance.GetGamepadForPlayer(playerId);
        }

        if (gamepad == null) return;

        Vector2 rawMove = gamepad.leftStick.ReadValue();
        if (JoystickManager.Instance != null)
        {
            moveInput = JoystickManager.Instance.ProcessLeftStickInput(rawMove);
        }
        else
        {
            moveInput = rawMove;
        }

        Vector2 rawLook = gamepad.rightStick.ReadValue();
        if (JoystickManager.Instance != null)
        {
            lookInput = JoystickManager.Instance.ProcessRightStickInput(rawLook);
        }
        else
        {
            lookInput = rawLook;
        }
    }

    private void HandleMovement()
    {
        if (characterController == null) return;

        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y);

        if (playerCamera != null)
        {
            movement = playerCamera.transform.TransformDirection(movement);
            movement.y = 0f;
        }

        if (movement.magnitude > 0.1f)
        {
            characterController.Move(movement * moveSpeed * Time.deltaTime);

            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        characterController.Move(Vector3.down * 9.81f * Time.deltaTime);
    }

    private void AssignToSplitScreenSystem()
    {
        if (FixedSplitScreenBootstrap.Instance != null)
        {

            if (playerId == 1)
            {
                FixedSplitScreenBootstrap.Instance.SetCameraTargets(gameObject, null);
            }
            else if (playerId == 2)
            {

                GameObject player1 = GameObject.FindGameObjectWithTag("Player1");
                if (player1 != null)
                {
                    FixedSplitScreenBootstrap.Instance.SetCameraTargets(player1, gameObject);
                }
                else
                {
                    FixedSplitScreenBootstrap.Instance.SetCameraTargets(null, gameObject);
                }
            }
        }
    }

    public Gamepad GetCurrentGamepad()
    {
        return gamepad;
    }

    public bool HasGamepad()
    {
        return gamepad != null;
    }
}