using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class SceneTransitionEditorTool : EditorWindow
{
    [MenuItem("Tools/Vitruvius/Setup Scene Transition")]
    public static void SetupTransition()
    {
        // 1. Check if manager exists
        SceneTransitionManager manager = FindObjectOfType<SceneTransitionManager>();
        
        if (manager == null)
        {
            GameObject managerGo = new GameObject("SceneTransitionManager");
            manager = managerGo.AddComponent<SceneTransitionManager>();
            Undo.RegisterCreatedObjectUndo(managerGo, "Create SceneTransitionManager");
        }

        // 2. Setup UI
        SetupUI(manager);

        Selection.activeGameObject = manager.gameObject;
        Debug.Log("[SceneTransitionTool] Transition System Setup Complete!");
    }

    private static void SetupUI(SceneTransitionManager manager)
    {
        // Check if CanvasGroup exists
        CanvasGroup group = manager.GetComponentInChildren<CanvasGroup>();
        if (group != null) return;

        // Create Canvas
        GameObject canvasGo = new GameObject("FadeCanvas");
        canvasGo.transform.SetParent(manager.transform);
        
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        
        canvasGo.AddComponent<GraphicRaycaster>();
        canvasGo.AddComponent<CanvasGroup>();
        
        // Create Background Image
        GameObject imageGo = new GameObject("FadeImage");
        imageGo.transform.SetParent(canvasGo.transform);
        Image image = imageGo.AddComponent<Image>();
        image.color = Color.black;
        
        RectTransform rect = image.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        manager.faderCanvasGroup = canvasGo.GetComponent<CanvasGroup>();
        
        Undo.RegisterCreatedObjectUndo(canvasGo, "Setup Transition UI");
    }
}
