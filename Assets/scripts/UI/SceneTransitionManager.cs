using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SceneTransitionManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("SceneTransitionManager");
                    _instance = go.AddComponent<SceneTransitionManager>();
                    Debug.Log("[SceneTransitionManager] Automatic Runtime Instance Created");
                }
            }
            return _instance;
        }
    }
    private static SceneTransitionManager _instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoInitialize()
    {
        // Simply accessing the Instance property will trigger the creation if it doesn't exist
        var inst = Instance;
    }

    [Header("UI References")]
    public CanvasGroup faderCanvasGroup;
    public float fadeDuration = 1.0f;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        SetupUI();

        // Ensure we start with a clear screen
        if (faderCanvasGroup != null)
        {
            faderCanvasGroup.alpha = 0f;
            faderCanvasGroup.blocksRaycasts = false;
        }
    }

    private void SetupUI()
    {
        if (faderCanvasGroup == null)
        {
            faderCanvasGroup = GetComponentInChildren<CanvasGroup>();
        }

        if (faderCanvasGroup == null)
        {
            // Create a basic UI if missing
            Debug.Log("[SceneTransitionManager] No CanvasGroup found, creating automatic UI...");
            GameObject canvasGo = new GameObject("FadeCanvas");
            canvasGo.transform.SetParent(transform);
            
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            
            canvasGo.AddComponent<GraphicRaycaster>();
            faderCanvasGroup = canvasGo.AddComponent<CanvasGroup>();
            
            GameObject imageGo = new GameObject("FadeImage");
            imageGo.transform.SetParent(canvasGo.transform);
            Image image = imageGo.AddComponent<Image>();
            image.color = Color.black;
            
            RectTransform rect = image.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Automatically fade in when a new scene is loaded
        StartCoroutine(Fade(0f));
    }

    public void TransitionToScene(string sceneName)
    {
        StartCoroutine(PerformTransition(sceneName));
    }

    private IEnumerator PerformTransition(string sceneName)
    {
        // Fade Out
        yield return StartCoroutine(Fade(1f));

        // Load Scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Fade In
        yield return StartCoroutine(Fade(0f));
    }

    public IEnumerator Fade(float targetAlpha)
    {
        if (faderCanvasGroup == null) yield break;

        faderCanvasGroup.blocksRaycasts = true;
        float startAlpha = faderCanvasGroup.alpha;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            faderCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            yield return null;
        }

        faderCanvasGroup.alpha = targetAlpha;
        if (targetAlpha == 0f)
        {
            faderCanvasGroup.blocksRaycasts = false;
        }
    }
}
