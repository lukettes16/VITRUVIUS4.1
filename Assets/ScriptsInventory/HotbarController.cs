using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class HotbarController : MonoBehaviour
{
    [Header("References")]
    public PlayerInventory playerInventory;
    public PlayerInput playerInput;
    public CanvasGroup hotBarCanvasGroup;

    [Header("UI Slots")]
    public Image[] slots;
    public Color selectedColor = Color.yellow;
    public Color normalColor = Color.white;

    [Header("Fade Settings")]
    public float fadeDuration = 0.5f;
    public float fadedAlpha = 0.1f;
    public float normalAlpha = 1f;
    public float fadeDelay = 3f;

    [Header("Input Actions")]
    public InputActionReference moveRightAction;
    public InputActionReference moveLeftAction;
    public InputActionReference analyzeAction;

    [Header("World Space Canvas")]
    [Tooltip("Card analysis canvas.")]
    public GameObject cardCanvas;

    [Tooltip("Optional player dialogue text.")]
    public TextMeshProUGUI playerDialogueText;

    private int selectedIndex = 0;
    private bool isCardCanvasOpen = false;
    private float lastInteractionTime;

    private void Awake()
    {
        if (playerInput == null)
            playerInput = GetComponentInParent<PlayerInput>();

        if (playerDialogueText == null)
            playerDialogueText = GetComponentInChildren<TextMeshProUGUI>(true);

        lastInteractionTime = Time.time;
        SetAlpha(fadedAlpha);
    }

    private void OnEnable()
    {
        if (moveRightAction != null)
            moveRightAction.action.performed += OnMoveRight;
        if (moveLeftAction != null)
            moveLeftAction.action.performed += OnMoveLeft;
        if (analyzeAction != null)
            analyzeAction.action.performed += OnAnalyzeItem;

        if (moveRightAction != null) moveRightAction.action.Enable();
        if (moveLeftAction != null) moveLeftAction.action.Enable();
        if (analyzeAction != null) analyzeAction.action.Enable();

        UpdateSlotSelection();
    }

    private void OnDisable()
    {
        if (moveRightAction != null)
            moveRightAction.action.performed -= OnMoveRight;
        if (moveLeftAction != null)
            moveLeftAction.action.performed -= OnMoveLeft;
        if (analyzeAction != null)
            analyzeAction.action.performed -= OnAnalyzeItem;

        if (moveRightAction != null) moveRightAction.action.Disable();
        if (moveLeftAction != null) moveLeftAction.action.Disable();
        if (analyzeAction != null) analyzeAction.action.Disable();
    }

    private void Update()
    {

        if (hotBarCanvasGroup != null && hotBarCanvasGroup.alpha == normalAlpha && Time.time > lastInteractionTime + fadeDelay)
        {
            StartFade(fadedAlpha);
        }
    }

    public void RegisterInteraction()
    {
        StartFade(normalAlpha);
        lastInteractionTime = Time.time;
    }

    private void StartFade(float targetAlpha)
    {
        if (hotBarCanvasGroup == null) return;

        StopAllCoroutines();
        StartCoroutine(FadeCoroutine(targetAlpha));
    }

    private IEnumerator FadeCoroutine(float targetAlpha)
    {
        float startAlpha = hotBarCanvasGroup.alpha;
        float elapsedTime = 0f;

        if (targetAlpha == normalAlpha)
        {
            hotBarCanvasGroup.interactable = true;
            hotBarCanvasGroup.blocksRaycasts = true;
        }

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            hotBarCanvasGroup.alpha = newAlpha;
            yield return null;
        }

        hotBarCanvasGroup.alpha = targetAlpha;

        if (targetAlpha == fadedAlpha)
        {
            hotBarCanvasGroup.interactable = false;
            hotBarCanvasGroup.blocksRaycasts = false;
        }
    }

    private void SetAlpha(float alpha)
    {
        if (hotBarCanvasGroup == null) return;
        hotBarCanvasGroup.alpha = alpha;
        hotBarCanvasGroup.interactable = alpha >= normalAlpha;
        hotBarCanvasGroup.blocksRaycasts = alpha >= normalAlpha;
    }

    private void OnMoveRight(InputAction.CallbackContext ctx)
    {
        RegisterInteraction();

        selectedIndex = (selectedIndex + 1) % slots.Length;
        UpdateSlotSelection();
        CloseCardCanvas();
    }

    private void OnMoveLeft(InputAction.CallbackContext ctx)
    {
        RegisterInteraction();

        selectedIndex--;
        if (selectedIndex < 0)
            selectedIndex = slots.Length - 1;
        UpdateSlotSelection();
        CloseCardCanvas();
    }

    private void OnAnalyzeItem(InputAction.CallbackContext ctx)
    {
        RegisterInteraction();

        if (playerInventory == null) return;

        var allItems = new List<string>();
        allItems.AddRange(playerInventory.GetCollectedItems());
        allItems.AddRange(playerInventory.GetCollectedKeyCards());

        if (selectedIndex >= allItems.Count)
        {
            ShowPlayerNotification("Nothing to analyze here.");
            return;
        }

        string selectedItem = allItems[selectedIndex];

        switch (selectedItem)
        {
            case "Card":
                ToggleCardCanvas();
                break;

            case "Lever":
                ShowPlayerNotification("This might help me cut the electricity.");
                break;

            case "Key":
                ShowPlayerNotification("A key... I wonder what it opens.");
                break;

            default:

                ShowPlayerNotification("Cannot analyze this item right now.");
                break;
        }
    }

    private void ToggleCardCanvas()
    {
        if (cardCanvas == null)
        {
            
            return;
        }

        isCardCanvasOpen = !isCardCanvasOpen;
        cardCanvas.SetActive(isCardCanvasOpen);

    }

    private void CloseCardCanvas()
    {
        if (cardCanvas != null && isCardCanvasOpen)
        {
            cardCanvas.SetActive(false);
            isCardCanvasOpen = false;
        }
    }

    private void ShowPlayerNotification(string message)
    {
        if (playerDialogueText != null)
        {
            playerDialogueText.text = message;
            return;
        }

        PlayerUIController uiController = GetComponentInParent<PlayerUIController>();
        if (uiController != null)
        {
            uiController.ShowNotification(message);
        }
        else
        {
            
        }
    }

    private void UpdateSlotSelection()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].color = (i == selectedIndex) ? selectedColor : normalColor;
        }
    }

    public int GetSelectedIndex() => selectedIndex;

}