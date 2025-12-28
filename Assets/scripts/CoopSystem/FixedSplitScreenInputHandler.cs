using UnityEngine;
using UnityEngine.InputSystem;

public class FixedSplitScreenInputHandler : MonoBehaviour
{
    private Gamepad player1Gamepad;
    private Gamepad player2Gamepad;

    void Start()
    {
    }

    void Update()
    {

        UpdateGamepadReferences();

        ProcessStaticInputs();
    }

    private void UpdateGamepadReferences()
    {
        if (JoystickManager.Instance != null)
        {
            player1Gamepad = JoystickManager.Instance.GetGamepadForPlayer(1);
            player2Gamepad = JoystickManager.Instance.GetGamepadForPlayer(2);
        }
        else
        {

            var gamepads = Gamepad.all;
            if (gamepads.Count > 0) player1Gamepad = gamepads[0];
            if (gamepads.Count > 1) player2Gamepad = gamepads[1];
        }
    }

    private void ProcessStaticInputs()
    {

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    public bool HasGamepad(int playerId)
    {
        if (playerId == 1) return player1Gamepad != null;
        if (playerId == 2) return player2Gamepad != null;
        return false;
    }

    public Gamepad GetGamepad(int playerId)
    {
        if (playerId == 1) return player1Gamepad;
        if (playerId == 2) return player2Gamepad;
        return null;
    }

    public string GetSystemStatus()
    {
        return "";
    }
}