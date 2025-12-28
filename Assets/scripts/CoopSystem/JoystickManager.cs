using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Gestión de joysticks.
/// </summary>
public class JoystickManager : MonoBehaviour
{
    public static JoystickManager Instance;
    
    [Header("Deadzone Configuration")]
    [Range(0f, 0.5f)]
    public float leftStickDeadzone = 0.15f;
    [Range(0f, 0.5f)]
    public float rightStickDeadzone = 0.15f;
    [Range(0f, 0.5f)]
    public float triggerDeadzone = 0.1f;
    
    [Header("Sensitivity")]
    [Range(0.1f, 5f)]
    public float moveSensitivity = 1.0f;
    [Range(0.1f, 5f)]
    public float lookSensitivity = 1.0f;

    [Header("Player Assignment")]
    [SerializeField] private Gamepad player1Gamepad;
    [SerializeField] private Gamepad player2Gamepad;

    // Eventos para notificar cambios
    public System.Action<int, Gamepad> OnJoystickAssigned;
    public System.Action<int> OnJoystickDisconnected;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeJoystickAssignment();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Obtiene el gamepad asignado a un jugador
    /// </summary>
    public Gamepad GetGamepadForPlayer(int playerId)
    {
        return playerId == 1 ? player1Gamepad : player2Gamepad;
    }

    /// <summary>
    /// Verifica si un jugador tiene un joystick asignado
    /// </summary>
    public bool PlayerHasJoystick(int playerId)
    {
        return GetGamepadForPlayer(playerId) != null;
    }

    /// <summary>
    /// Obtiene el gamepad asignado a un jugador (alias para compatibilidad)
    /// </summary>
    public Gamepad GetPlayerGamepad(int playerId)
    {
        return GetGamepadForPlayer(playerId);
    }
    /// <summary>
    /// Inicializa la asignación de joysticks de forma estática y bloqueada.
    /// </summary>
    public void InitializeJoystickAssignment()
    {
        var gamepads = Gamepad.all;

        // Limpiar asignaciones anteriores si es necesario
        player1Gamepad = null;
        player2Gamepad = null;

        if (gamepads.Count >= 1)
        {
            player1Gamepad = gamepads[0];
            OnJoystickAssigned?.Invoke(1, player1Gamepad);
        }
        
        if (gamepads.Count >= 2)
        {
            player2Gamepad = gamepads[1];
            OnJoystickAssigned?.Invoke(2, player2Gamepad);
        }
    }

    /// <summary>
    /// Registra un jugador específico con su input y dispositivo asignado de forma ESTRICTA.
    /// </summary>
    public void RegisterPlayer(PlayerInput pInput, int playerIndex)
    {
        if (pInput == null) return;

        Gamepad targetGamepad = playerIndex == 1 ? player1Gamepad : player2Gamepad;
        
        // 1. Limpieza total de dispositivos previos
        if (pInput.user.valid)
        {
            pInput.user.UnpairDevices();
        }

        if (targetGamepad != null)
        {
            try
            {
                // 2. Asegurar que el usuario de input sea válido
                if (!pInput.user.valid)
                {
                    pInput.gameObject.SetActive(false); // Reinicio rápido para asegurar validez
                    pInput.gameObject.SetActive(true);
                }

                // 3. Vincular EXCLUSIVAMENTE el gamepad asignado
                InputUser.PerformPairingWithDevice(targetGamepad, pInput.user);
                
                // 4. Forzar el uso del dispositivo y esquema
                pInput.user.ActivateControlScheme("Gamepad");
                pInput.SwitchCurrentControlScheme("Gamepad", targetGamepad);
                
                // Deshabilitar el auto-switch para que no detecte otros mandos
                pInput.neverAutoSwitchControlSchemes = true;
                
                pInput.SwitchCurrentActionMap("Player");
                
                
                // Verificar la asignación después de un frame
                StartCoroutine(VerifyAssignment(pInput, playerIndex, targetGamepad));
            }
            catch (System.Exception)
            {
            }
        }
        else
        {
        }
    }
    
    /// <summary>
    /// Verifica que la asignación se haya realizado correctamente y no haya otros dispositivos interfiriendo.
    /// </summary>
    private System.Collections.IEnumerator VerifyAssignment(PlayerInput pInput, int playerIndex, Gamepad gamepad)
    {
        yield return new WaitForSeconds(0.1f);
        
        if (pInput.user.pairedDevices.Contains(gamepad))
        {
            int deviceCount = pInput.user.pairedDevices.Count;
            if (deviceCount > 1)
            {
                foreach (var device in pInput.user.pairedDevices.ToList())
                {
                    if (device != gamepad) pInput.user.UnpairDevice(device);
                }
            }
        }
    }

    /// <summary>
    /// Fuerza la reasignación de joysticks (útil si se conectó uno nuevo)
    /// </summary>
    public void ForceReassignment()
    {
        InitializeJoystickAssignment();
        
        // Re-registrar jugadores activos
        var handlers = FindObjectsOfType<PlayerInput>();
        foreach (var pInput in handlers)
        {
            int pId = 0;
            if (pInput.gameObject.name.Contains("1") || pInput.gameObject.CompareTag("Player1")) pId = 1;
            else if (pInput.gameObject.name.Contains("2") || pInput.gameObject.CompareTag("Player2")) pId = 2;
            
            if (pId > 0) RegisterPlayer(pInput, pId);
        }
    }

    /// <summary>
    /// Obtiene información de estado para debug
    /// </summary>
    public string GetStatusInfo()
    {
        string p1 = player1Gamepad != null ? player1Gamepad.displayName : "No asignado";
        string p2 = player2Gamepad != null ? player2Gamepad.displayName : "No asignado";
        return $"P1: {p1} | P2: {p2}";
    }

    /// <summary>
    /// Procesa input del stick izquierdo con deadzone y sensibilidad
    /// </summary>
    public Vector2 ProcessLeftStickInput(Vector2 rawInput)
    {
        return ApplyDeadzone(rawInput, leftStickDeadzone) * moveSensitivity;
    }

    /// <summary>
    /// Procesa input del stick derecho con deadzone y sensibilidad
    /// </summary>
    public Vector2 ProcessRightStickInput(Vector2 rawInput)
    {
        return ApplyDeadzone(rawInput, rightStickDeadzone) * lookSensitivity;
    }

    /// <summary>
    /// Procesa input de gatillos con deadzone
    /// </summary>
    public float ProcessTriggerInput(float rawInput)
    {
        return ApplyTriggerDeadzone(rawInput, triggerDeadzone);
    }

    /// <summary>
    /// Aplica deadzone al input del stick
    /// </summary>
    private Vector2 ApplyDeadzone(Vector2 input, float deadzone)
    {
        float magnitude = input.magnitude;
        
        if (magnitude < deadzone)
        {
            return Vector2.zero;
        }
        
        // Remapear el input para tener respuesta suave
        float normalizedMagnitude = Mathf.Clamp01((magnitude - deadzone) / (1f - deadzone));
        return input.normalized * normalizedMagnitude;
    }

    /// <summary>
    /// Aplica deadzone al input del gatillo
    /// </summary>
    private float ApplyTriggerDeadzone(float input, float deadzone)
    {
        if (input < deadzone)
        {
            return 0f;
        }
        
        return Mathf.Clamp01((input - deadzone) / (1f - deadzone));
    }
}