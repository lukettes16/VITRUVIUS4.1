using UnityEngine;

public class GameOverSystemActivator : MonoBehaviour
{
    [Header("System Configuration")]
    public bool autoDisableOldSystem = true;
    public bool autoEnableNewSystem = true;
    public GameObject newGameOverPrefab;

    private void Start()
    {
        HandleSystemTransition();
    }

    private void HandleSystemTransition()
    {

        if (autoDisableOldSystem)
        {
            DisableOldGameOverSystem();
        }

        if (autoEnableNewSystem)
        {
            EnableNewGameOverSystem();
        }
    }

    private void DisableOldGameOverSystem()
    {

        GameOverManager oldManager = FindObjectOfType<GameOverManager>();

        if (oldManager != null)
        {

            oldManager.enabled = false;

            GameObject fadeScreen = GameObject.Find("FadeScreen");
            if (fadeScreen != null)
            {
                fadeScreen.SetActive(false);
            }

            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("GameOver") || obj.name.Contains("FadeScreen"))
                {
                    if (obj.transform.parent == null)
                    {
                        obj.SetActive(false);
                    }
                }
            }

        }
        else
        {
            
        }

        GameOverManager[] allOldManagers = FindObjectsOfType<GameOverManager>();
        foreach (GameOverManager manager in allOldManagers)
        {
            if (manager != null && manager.enabled)
            {
                manager.enabled = false;
                
            }
        }
    }

    private void EnableNewGameOverSystem()
    {

        NewGameOverManager existingManager = FindObjectOfType<NewGameOverManager>();

        if (existingManager == null)
        {

            if (newGameOverPrefab != null)
            {

                GameObject newSystem = Instantiate(newGameOverPrefab);
                newSystem.name = "NewGameOverSystem";
                
            }
            else
            {

                GameObject go = new GameObject("NewGameOverSystem");
                go.AddComponent<NewGameOverManager>();
                go.AddComponent<CheckpointSystem>();
                
            }
        }
        else
        {
            
        }

        NewGameOverManager newManager = FindObjectOfType<NewGameOverManager>();
        if (newManager != null)
        {

            newManager.gameObject.SetActive(true);

            newManager.HideGameOver();

        }
    }

    [ContextMenu("Trigger Transition")]
    public void TriggerTransition()
    {
        HandleSystemTransition();
    }

    [ContextMenu("Test New System")]
    public void TestNewSystem()
    {
        NewGameOverManager newManager = FindObjectOfType<NewGameOverManager>();
        if (newManager != null)
        {
            newManager.ShowGameOver();
            
        }
        else
        {
            
        }
    }

    [ContextMenu("Check System Status")]

    [ContextMenu("Check System Status")]

    [ContextMenu("Check System Status")]
    public void CheckSystemStatus()
    {
        GameOverManager oldManager = FindObjectOfType<GameOverManager>();
        NewGameOverManager newManager = FindObjectOfType<NewGameOverManager>();

        string oldStatus = (oldManager != null && oldManager.enabled) ? "ACTIVE" : "INACTIVE";
        
        string newStatus = (newManager != null && newManager.gameObject.activeInHierarchy) ? "ACTIVE" : "INACTIVE";
        
        if (oldManager != null)
        {
            
        }

        if (newManager != null)
        {
            
        }
    }
}