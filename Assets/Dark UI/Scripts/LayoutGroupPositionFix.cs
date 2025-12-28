using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Michsky.UI.Dark
{
    public class LayoutGroupPositionFix : MonoBehaviour
    {
        LayoutGroup lg;

        void Start()
        {

            lg = gameObject.GetComponent<LayoutGroup>();
            StartCoroutine(ExecuteAfterTime(0.01f));
        }

        IEnumerator ExecuteAfterTime(float time)
        {
            yield return new WaitForSeconds(time);
            lg.enabled = false;
            lg.enabled = true;
            StopCoroutine(ExecuteAfterTime(0.01f));
            Destroy(this);
        }
    }
}