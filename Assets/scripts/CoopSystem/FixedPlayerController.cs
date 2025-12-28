using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controlador de jugador.
/// </summary>
public class FixedPlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    public int playerId = 1; // 1 o 2
    
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

        // Obtener cámara del sistema fijo
        if (FixedSplitScreenBootstrap.Instance != null)
        {
            playerCamera = FixedSplitScreenBootstrap.Instance.GetCameraForPlayer(playerId);
        }

        // Asignar a sistema de split-screen
        AssignToSplitScreenSystem();
    }

    void Update()
    {
        HandleInput();
        HandleMovement();
    }

    /// <summary>
    /// Maneja el input del jugador
    /// </summary>
    private void HandleInput()
    {
        // Obtener gamepad para este jugador
        if (JoystickManager.Instance != null)
        {
            gamepad = JoystickManager.Instance.GetGamepadForPlayer(playerId);
        }

        if (gamepad == null) return;

        // Input de movimiento (stick izquierdo)
        Vector2 rawMove = gamepad.leftStick.ReadValue();
        if (JoystickManager.Instance != null)
        {
            moveInput = JoystickManager.Instance.ProcessLeftStickInput(rawMove);
        }
        else
        {
            moveInput = rawMove;
        }

        // Input de cámara (stick derecho)
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

    /// <summary>
    /// Maneja el movimiento del jugador
    /// </summary>
    private void HandleMovement()
    {
        if (characterController == null) return;

        // Movimiento en el plano horizontal
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y);
        
        // Transformar movimiento relativo a la cámara
        if (playerCamera != null)
        {
            movement = playerCamera.transform.TransformDirection(movement);
            movement.y = 0f; // Mantener en el plano horizontal
        }

        // Aplicar movimiento
        if (movement.magnitude > 0.1f)
        {
            characterController.Move(movement * moveSpeed * Time.deltaTime);
            
            // Rotar hacia la dirección de movimiento
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Aplicar gravedad
        characterController.Move(Vector3.down * 9.81f * Time.deltaTime);
    }


    /// <summary>
    /// Asigna este jugador al sistema de split-screen fijo
    /// </summary>
    private void AssignToSplitScreenSystem()
    {
        if (FixedSplitScreenBootstrap.Instance != null)
        {
            // Configurar targets según el playerId
            if (playerId == 1)
            {
                FixedSplitScreenBootstrap.Instance.SetCameraTargets(gameObject, null);
            }
            else if (playerId == 2)
            {
                // Buscar el player 1 para asignar ambos
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

    /// <summary>
    /// Obtiene el gamepad actual para este jugador
    /// </summary>
    public Gamepad GetCurrentGamepad()
    {
        return gamepad;
    }

    /// <summary>
    /// Verifica si este jugador tiene un gamepad asignado
    /// </summary>
    public bool HasGamepad()
    {
        return gamepad != null;
    }
}