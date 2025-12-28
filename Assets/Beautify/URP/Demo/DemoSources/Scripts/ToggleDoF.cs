using UnityEngine;
using Beautify.Universal;

namespace Beautify.Demos {

    public class ToggleDoF : MonoBehaviour {

        void Update() {
            if (Input.GetMouseButtonDown(0)) {

                bool state = BeautifySettings.settings.depthOfField.value;
                BeautifySettings.settings.depthOfField.Override(!state);
            }
        }

    }
}