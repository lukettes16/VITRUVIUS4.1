using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manejador de input para split-screen.
/// </summary>
public class FixedSplitScreenInputHandler : MonoBehaviour
{
    private Gamepad player1Gamepad;
    private Gamepad player2Gamepad;

    void Start()
    {
    }

    void Update()
    {
        // Actualizar referencias a gamepads
        UpdateGamepadReferences();
        
        // Procesar otros inputs estáticos si es necesario
        ProcessStaticInputs();
    }

    /// <summary>
    /// Actualiza las referencias a los gamepads
    /// </summary>
    private void UpdateGamepadReferences()
    {
        if (JoystickManager.Instance != null)
        {
            player1Gamepad = JoystickManager.Instance.GetGamepadForPlayer(1);
            player2Gamepad = JoystickManager.Instance.GetGamepadForPlayer(2);
        }
        else
        {
            // Fallback directo al sistema de input
            var gamepads = Gamepad.all;
            if (gamepads.Count > 0) player1Gamepad = gamepads[0];
            if (gamepads.Count > 1) player2Gamepad = gamepads[1];
        }
    }

    /// <summary>
    /// Procesa inputs estáticos adicionales
    /// </summary>
    private void ProcessStaticInputs()
    {
        // Ejemplo: Salir con ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    /// <summary>
    /// Verifica si un jugador tiene gamepad conectado
    /// </summary>
    public bool HasGamepad(int playerId)
    {
        if (playerId == 1) return player1Gamepad != null;
        if (playerId == 2) return player2Gamepad != null;
        return false;
    }

    /// <summary>
    /// Obtiene el gamepad para un jugador específico
    /// </summary>
    public Gamepad GetGamepad(int playerId)
    {
        if (playerId == 1) return player1Gamepad;
        if (playerId == 2) return player2Gamepad;
        return null;
    }

    /// <summary>
    /// Obtiene el estado actual del sistema
    /// </summary>
    public string GetSystemStatus()
    {
        return "";
    }
}