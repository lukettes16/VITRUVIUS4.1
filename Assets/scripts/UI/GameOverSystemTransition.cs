using UnityEngine;

public class GameOverSystemTransition : MonoBehaviour
{
    [Header("System Management")]
    public bool disableOldSystemOnStart = true;
    public bool enableNewSystemOnStart = true;

    private NewGameOverManager newGameOverManager;
    private GameOverManager oldGameOverManager;
    private bool oldSystemActive = false;

    private void Awake()
    {

        newGameOverManager = FindObjectOfType<NewGameOverManager>();
        oldGameOverManager = FindObjectOfType<GameOverManager>();

        HandleSystemTransition();
    }

    private void HandleSystemTransition()
    {

        if (disableOldSystemOnStart && oldGameOverManager != null)
        {
            DisableOldSystem();
        }

        if (enableNewSystemOnStart)
        {
            if (newGameOverManager == null)
            {

                GameObject go = new GameObject("NewGameOverManager");
                newGameOverManager = go.AddComponent<NewGameOverManager>();
            }

            newGameOverManager.gameObject.SetActive(true);
        }

        string oldStatus = oldSystemActive ? "active" : "disabled";
        string newStatus = (newGameOverManager != null) ? "active" : "missing";
        
    }

    private void DisableOldSystem()
    {
        if (oldGameOverManager != null)
        {

            oldGameOverManager.enabled = false;

            var fadeScreen = GameObject.Find("FadeScreen");
            if (fadeScreen != null)
            {
                fadeScreen.SetActive(false);
            }

            Transform[] children = oldGameOverManager.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                if (child != oldGameOverManager.transform)
                {
                    child.gameObject.SetActive(false);
                }
            }

            oldSystemActive = false;
            
        }
    }

    public void EnableOldSystem()
    {
        if (oldGameOverManager != null)
        {
            oldGameOverManager.enabled = true;
            oldSystemActive = true;
            
        }
    }

    public void DisableNewSystem()
    {
        if (newGameOverManager != null)
        {
            newGameOverManager.gameObject.SetActive(false);
            
        }
    }

    public void EnableNewSystem()
    {
        if (newGameOverManager != null)
        {
            newGameOverManager.gameObject.SetActive(true);
            
        }
    }

    public string GetActiveSystem()
    {
        if (newGameOverManager != null && newGameOverManager.gameObject.activeInHierarchy)
            return "New System";
        else if (oldGameOverManager != null && oldGameOverManager.enabled)
            return "Old System";
        else
            return "No Active System";
    }

    public void ShowGameOverNewSystem()
    {
        if (newGameOverManager != null)
        {
            newGameOverManager.ShowGameOver();
        }
        else
        {
            
        }
    }

    public void ShowGameOverOldSystem()
    {
        if (oldGameOverManager != null && oldGameOverManager.enabled)
        {

            oldGameOverManager.TriggerGameOver();
        }
        else
        {
            
        }
    }
}