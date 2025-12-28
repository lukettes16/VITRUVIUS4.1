using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class LevelTransitionTrigger : MonoBehaviour
{
    [Header("Transition Settings")]
    public string nextSceneName = "DaVinciP1";
    public float fadeDuration = 1.5f;

    [Header("Item Persistence")]
    public string requiredItem = "Flashlight";

    private static Dictionary<int, List<string>> savedItems = new Dictionary<int, List<string>>();
    private static Dictionary<int, List<string>> savedKeyCards = new Dictionary<int, List<string>>();
    private bool isTransitioning = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isTransitioning) return;

        // Check for PlayerIdentifier, FixedPlayerController, or Tags
        bool isPlayer = other.GetComponent<PlayerIdentifier>() != null || 
                         other.GetComponentInParent<PlayerIdentifier>() != null ||
                         other.GetComponent<FixedPlayerController>() != null ||
                         other.GetComponentInParent<FixedPlayerController>() != null ||
                         other.CompareTag("Player") || 
                         other.CompareTag("Player1") || 
                         other.CompareTag("Player2");

        if (isPlayer)
        {
            Debug.Log("[LevelTransitionTrigger] Player detected: " + other.name);
            StartCoroutine(PerformTransition());
        }
    }

    private IEnumerator PerformTransition()
    {
        isTransitioning = true;
        Debug.Log("[LevelTransitionTrigger] Starting transition to: " + nextSceneName);

        // Save inventory before transition
        SaveAllPlayersInventory();

        if (SceneTransitionManager.Instance != null)
        {
            // Start Fade Out
            yield return StartCoroutine(SceneTransitionManager.Instance.Fade(1f));

            // Load the next scene
            SceneManager.LoadScene(nextSceneName);
            
            // Note: SceneTransitionManager is persistent, so it will handle Fade In
            // in its own Awake/Start or we can rely on it if we trigger it correctly.
            // Actually, the TransitionToScene method in SceneTransitionManager 
            // is better suited if we want it to handle everything.
            // But LevelTransitionTrigger has its own SaveAllPlayersInventory logic.
        }
        else
        {
            // Fallback if manager is missing
            SceneManager.LoadScene(nextSceneName);
        }
    }

    private void SaveAllPlayersInventory()
    {
        savedItems.Clear();
        savedKeyCards.Clear();

        PlayerIdentifier[] allPlayers = FindObjectsOfType<PlayerIdentifier>();
        foreach (var player in allPlayers)
        {
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            if (inventory != null)
            {
                savedItems[player.playerID] = inventory.GetCollectedItems();
                savedKeyCards[player.playerID] = inventory.GetCollectedKeyCards();
                
                
                if (!savedItems[player.playerID].Contains(requiredItem))
                {
                    
                    
                }
            }
        }
    }

    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void OnSceneLoaded()
    {
        if (savedItems.Count == 0 && savedKeyCards.Count == 0) return;

        
        GameObject restorer = new GameObject("InventoryRestorer");
        restorer.AddComponent<InventoryRestorerHelper>();
    }

    private class InventoryRestorerHelper : MonoBehaviour
    {
        private int attempts = 0;
        private const int maxAttempts = 10;

        private void Start()
        {
            StartCoroutine(RestoreRoutine());
        }

        private IEnumerator RestoreRoutine()
        {
            while (attempts < maxAttempts)
            {
                PlayerIdentifier[] players = FindObjectsOfType<PlayerIdentifier>();
                if (players.Length > 0)
                {
                    foreach (var player in players)
                    {
                        if (savedItems.TryGetValue(player.playerID, out List<string> items))
                        {
                            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
                            if (inventory != null)
                            {
                                List<string> keyCards = savedKeyCards.ContainsKey(player.playerID) ? 
                                    savedKeyCards[player.playerID] : new List<string>();
                                
                                inventory.RestoreInventory(keyCards, items);
                            }
                        }
                    }
                    
                    Destroy(gameObject);
                    yield break;
                }

                attempts++;
                yield return new WaitForSeconds(0.2f);
            }
            Destroy(gameObject);
        }
    }
}
