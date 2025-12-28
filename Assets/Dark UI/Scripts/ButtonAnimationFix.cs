using UnityEngine;
using UnityEngine.UI;

namespace Michsky.UI.Dark
{
    public class ButtonAnimationFix : MonoBehaviour
    {
        private Button fixButton;

        void Start()
        {
            fixButton = gameObject.GetComponent<Button>();
            fixButton.onClick.AddListener(Fix);
        }

        public void Fix()
        {

            fixButton.gameObject.SetActive(false);
            fixButton.gameObject.SetActive(true);
        }
    }
}