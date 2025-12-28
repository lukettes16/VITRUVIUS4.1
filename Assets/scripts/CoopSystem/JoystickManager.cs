using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using System.Collections.Generic;
using System.Linq;

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

    public Gamepad GetGamepadForPlayer(int playerId)
    {
        return playerId == 1 ? player1Gamepad : player2Gamepad;
    }

    public bool PlayerHasJoystick(int playerId)
    {
        return GetGamepadForPlayer(playerId) != null;
    }

    public Gamepad GetPlayerGamepad(int playerId)
    {
        return GetGamepadForPlayer(playerId);
    }

    public void InitializeJoystickAssignment()
    {
        var gamepads = Gamepad.all;

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

    public void RegisterPlayer(PlayerInput pInput, int playerIndex)
    {
        if (pInput == null) return;

        Gamepad targetGamepad = playerIndex == 1 ? player1Gamepad : player2Gamepad;

        if (pInput.user.valid)
        {
            pInput.user.UnpairDevices();
        }

        if (targetGamepad != null)
        {
            try
            {

                if (!pInput.user.valid)
                {
                    pInput.gameObject.SetActive(false);
                    pInput.gameObject.SetActive(true);
                }

                InputUser.PerformPairingWithDevice(targetGamepad, pInput.user);

                pInput.user.ActivateControlScheme("Gamepad");
                pInput.SwitchCurrentControlScheme("Gamepad", targetGamepad);

                pInput.neverAutoSwitchControlSchemes = true;

                pInput.SwitchCurrentActionMap("Player");

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

    public void ForceReassignment()
    {
        InitializeJoystickAssignment();

        var handlers = FindObjectsOfType<PlayerInput>();
        foreach (var pInput in handlers)
        {
            int pId = 0;
            if (pInput.gameObject.name.Contains("1") || pInput.gameObject.CompareTag("Player1")) pId = 1;
            else if (pInput.gameObject.name.Contains("2") || pInput.gameObject.CompareTag("Player2")) pId = 2;

            if (pId > 0) RegisterPlayer(pInput, pId);
        }
    }

    public string GetStatusInfo()
    {
        string p1 = player1Gamepad != null ? player1Gamepad.displayName : "No asignado";
        string p2 = player2Gamepad != null ? player2Gamepad.displayName : "No asignado";
        return $"P1: {p1} | P2: {p2}";
    }

    public Vector2 ProcessLeftStickInput(Vector2 rawInput)
    {
        return ApplyDeadzone(rawInput, leftStickDeadzone) * moveSensitivity;
    }

    public Vector2 ProcessRightStickInput(Vector2 rawInput)
    {
        return ApplyDeadzone(rawInput, rightStickDeadzone) * lookSensitivity;
    }

    public float ProcessTriggerInput(float rawInput)
    {
        return ApplyTriggerDeadzone(rawInput, triggerDeadzone);
    }

    private Vector2 ApplyDeadzone(Vector2 input, float deadzone)
    {
        float magnitude = input.magnitude;

        if (magnitude < deadzone)
        {
            return Vector2.zero;
        }

        float normalizedMagnitude = Mathf.Clamp01((magnitude - deadzone) / (1f - deadzone));
        return input.normalized * normalizedMagnitude;
    }

    private float ApplyTriggerDeadzone(float input, float deadzone)
    {
        if (input < deadzone)
        {
            return 0f;
        }

        return Mathf.Clamp01((input - deadzone) / (1f - deadzone));
    }
}