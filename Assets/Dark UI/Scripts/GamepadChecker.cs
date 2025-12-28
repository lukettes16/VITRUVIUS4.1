using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Michsky.UI.Dark
{
    public class GamepadChecker : MonoBehaviour
    {
        [Header("RESOURCES")]
        public GameObject virtualCursor;
        public GameObject eventSystem;

        [Header("OBJECTS")]
        [Tooltip("Objects in this list will be active when gamepad is un-plugged.")]
        public List<GameObject> keyboardObjects = new List<GameObject>();
        [Tooltip("Objects in this list will be active when gamepad is plugged.")]
        public List<GameObject> gamepadObjects = new List<GameObject>();

        [Header("SETTINGS")]
        [Tooltip("Always update input device. If you turn off this feature, you won't able to change the input device after start, but it might increase the performance.")]
        public bool alwaysSearch = true;

        private GamepadChecker checkerScript;
        private int GamepadConnected = 0;
        private Vector3 startMousePos;
        private Vector3 startPos;

        bool gamepadEnabled;

        void Start()
        {
            checkerScript = gameObject.GetComponent<GamepadChecker>();

            if (alwaysSearch == false)
            {
                checkerScript.enabled = false;
            }

            else
            {
                checkerScript.enabled = true;
                
            }

            for (int i = 0; i < gamepadObjects.Count; i++)
                gamepadObjects[i].SetActive(true);

            for (int i = 0; i < gamepadObjects.Count; i++)
                gamepadObjects[i].SetActive(false);

            for (int i = 0; i < keyboardObjects.Count; i++)
                keyboardObjects[i].SetActive(true);

            for (int i = 0; i < keyboardObjects.Count; i++)
                keyboardObjects[i].SetActive(false);

            SwitchToKeyboard();
        }

        void Update()
        {
            string[] names = Input.GetJoystickNames();

            for (int x = 0; x < names.Length; x++)
            {

                if (names[x].Length >= 1)
                    GamepadConnected = 1;

                else if (names[x].Length == 0)
                    GamepadConnected = 0;
            }

            if (GamepadConnected == 1 && gamepadEnabled == false)
                SwitchToController();

            else if (GamepadConnected == 0 && gamepadEnabled == true)
                SwitchToKeyboard();
        }

        public void SwitchToController()
        {
            for (int i = 0; i < keyboardObjects.Count; i++)
                keyboardObjects[i].SetActive(false);

            for (int i = 0; i < gamepadObjects.Count; i++)
                gamepadObjects[i].SetActive(true);

            gamepadEnabled = true;
            eventSystem.SetActive(false);
            virtualCursor.SetActive(true);
            
        }

        public void SwitchToKeyboard()
        {
            for (int i = 0; i < keyboardObjects.Count; i++)
                keyboardObjects[i].SetActive(true);

            for (int i = 0; i < gamepadObjects.Count; i++)
                gamepadObjects[i].SetActive(false);

            gamepadEnabled = false;
            virtualCursor.SetActive(false);
            eventSystem.SetActive(true);
            
        }
    }
}