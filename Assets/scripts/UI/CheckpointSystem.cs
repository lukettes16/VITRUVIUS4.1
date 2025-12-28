using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class CheckpointSystem : MonoBehaviour
{
    [System.Serializable]
    public class CheckpointData
    {
        public string sceneName;
        public Vector3 playerPosition;
        public Quaternion playerRotation;
        public int playerHealth;
        public int playerScore;
        public float gameTime;
        public Dictionary<string, object> customData;

        public CheckpointData()
        {
            customData = new Dictionary<string, object>();
        }
    }

    private static CheckpointSystem instance;
    public static CheckpointSystem Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<CheckpointSystem>();
                if (instance == null)
                {
                    GameObject go = new GameObject("CheckpointSystem");
                    instance = go.AddComponent<CheckpointSystem>();
                }
            }
            return instance;
        }
    }

    private CheckpointData lastCheckpoint;
    private List<CheckpointData> checkpointHistory;
    private const int MAX_CHECKPOINTS = 10;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        checkpointHistory = new List<CheckpointData>();
    }

    public void SaveCheckpoint()
    {
        CheckpointData checkpoint = new CheckpointData();

        checkpoint.sceneName = SceneManager.GetActiveScene().name;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            checkpoint.playerPosition = player.transform.position;
            checkpoint.playerRotation = player.transform.rotation;

        }

        checkpoint.gameTime = Time.time;

        lastCheckpoint = checkpoint;
        checkpointHistory.Add(checkpoint);

        if (checkpointHistory.Count > MAX_CHECKPOINTS)
        {
            checkpointHistory.RemoveAt(0);
        }

    }

    public void LoadLastCheckpoint()
    {
        if (lastCheckpoint == null)
        {
            
            return;
        }

        StartCoroutine(LoadCheckpointCoroutine(lastCheckpoint));
    }

    private System.Collections.IEnumerator LoadCheckpointCoroutine(CheckpointData checkpoint)
    {
        
        if (SceneManager.GetActiveScene().name != checkpoint.sceneName)
        {
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(checkpoint.sceneName);
            yield return new WaitUntil(() => loadOperation.isDone);
        }

        RestorePlayerState(checkpoint);

    }

    private void RestorePlayerState(CheckpointData checkpoint)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {

            player.transform.position = checkpoint.playerPosition;
            player.transform.rotation = checkpoint.playerRotation;

        }
        else
        {
            
        }
    }

    public bool HasCheckpoint()
    {
        return lastCheckpoint != null;
    }

    public CheckpointData GetLastCheckpoint()
    {
        return lastCheckpoint;
    }

    public void ClearCheckpoints()
    {
        lastCheckpoint = null;
        checkpointHistory.Clear();
        
    }

    public void EnableAutoSave(float intervalSeconds = 60f)
    {
        CancelInvoke(nameof(SaveCheckpoint));
        InvokeRepeating(nameof(SaveCheckpoint), intervalSeconds, intervalSeconds);
    }

    public void DisableAutoSave()
    {
        CancelInvoke(nameof(SaveCheckpoint));
    }
}