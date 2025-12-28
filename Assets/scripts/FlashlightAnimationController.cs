using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlashlightAnimationController : MonoBehaviour
{
    [Header("Flashlight State Management")]
    [SerializeField] private FlashlightController_Enhanced flashlightController;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private Animator playerAnimator;

    [Header("Animation Parameters")]
    [SerializeField] private string flashlightEquippedParam = "FlashlightEquipped";
    [SerializeField] private string flashlightOnParam = "FlashlightOn";
    [SerializeField] private string armRaisedParam = "ArmRaised";
    [SerializeField] private string armTransitionParam = "ArmTransition";

    [Header("Transition Settings")]
    [SerializeField] private float armRaiseDuration = 0.5f;
    [SerializeField] private float armLowerDuration = 0.4f;

    [Header("Collision Detection")]
    [SerializeField] private bool enableCollisionDetection = true;
    [SerializeField] private LayerMask collisionLayers = -1;
    [SerializeField] private float collisionCheckRadius = 0.3f;
    [SerializeField] private float collisionCheckDistance = 0.5f;
    [SerializeField] private Transform armTransform;

    [Header("State Validation")]
    [SerializeField] public bool debugMode = true;

    private FlashlightState currentState = FlashlightState.None;
    private FlashlightState previousState = FlashlightState.None;
    private bool isTransitioning = false;
    private Coroutine currentTransitionCoroutine;
    private float currentArmPosition = 0f;

    private bool hasFlashlightEquipped = false;
    private bool isFlashlightOn = false;
    private bool shouldShowArmAnimation = false;

    public enum FlashlightState
    {
        None,
        Unequipped,
        EquippedOff,
        EquippedOn,
        RaisingArm,
        LoweringArm,
        Blocked
    }

    void Start()
    {
        InitializeComponents();
        ValidateInitialState();
    }

    void Update()
    {
        UpdateFlashlightState();
        HandleStateTransitions();
        UpdateArmPosition();
        CheckCollisions();
    }

    private void InitializeComponents()
    {
        if (flashlightController == null)
            flashlightController = GetComponentInChildren<FlashlightController_Enhanced>();

        if (playerInventory == null)
            playerInventory = GetComponent<PlayerInventory>();

        if (playerAnimator == null)
            playerAnimator = GetComponent<Animator>();

        if (armTransform == null && flashlightController != null)
            armTransform = flashlightController.armTransform;
    }

    private void ValidateInitialState()
    {
        if (debugMode)
        {
            
        }

        UpdateFlashlightState();
        SetInitialAnimatorParameters();
    }

    public void InitializeFromFlashlight(FlashlightController_Enhanced controller, bool initialFlashlightState)
    {
        flashlightController = controller;
        isFlashlightOn = initialFlashlightState;

        if (armTransform == null && flashlightController != null)
        {
            armTransform = flashlightController.armTransform;
        }

        if (debugMode)
        {
            
        }

        UpdateFlashlightState();
        SetInitialAnimatorParameters();
    }

    private void SetInitialAnimatorParameters()
    {
        if (playerAnimator != null)
        {
            playerAnimator.SetBool(flashlightEquippedParam, hasFlashlightEquipped);
            playerAnimator.SetBool(flashlightOnParam, isFlashlightOn);
            playerAnimator.SetFloat(armTransitionParam, currentArmPosition);
        }
    }

    private void UpdateFlashlightState()
    {
        previousState = currentState;

        bool hasFlashlightInInventory = playerInventory != null && playerInventory.HasItem("Flashlight");

        bool flashlightControllerActive = flashlightController != null && flashlightController.gameObject.activeInHierarchy;

        hasFlashlightEquipped = hasFlashlightInInventory && flashlightControllerActive;
        isFlashlightOn = hasFlashlightEquipped && flashlightController != null && flashlightController.isFlashlightOn;

        if (!hasFlashlightEquipped)
        {
            currentState = FlashlightState.None;
            shouldShowArmAnimation = false;
        }
        else if (!isFlashlightOn)
        {
            currentState = FlashlightState.EquippedOff;
            shouldShowArmAnimation = false;
        }
        else
        {
            currentState = FlashlightState.EquippedOn;
            shouldShowArmAnimation = true;
        }

        if (shouldShowArmAnimation && IsArmBlockedByCollision())
        {
            currentState = FlashlightState.Blocked;
        }

        if (isTransitioning)
        {
            if (shouldShowArmAnimation && previousState != FlashlightState.EquippedOn)
            {
                currentState = FlashlightState.RaisingArm;
            }
            else if (!shouldShowArmAnimation && previousState == FlashlightState.EquippedOn)
            {
                currentState = FlashlightState.LoweringArm;
            }
        }
    }

    private void HandleStateTransitions()
    {
        if (currentState != previousState)
        {
            if (debugMode)
            {
                
            }

            switch (currentState)
            {
                case FlashlightState.EquippedOn:
                    if (previousState != FlashlightState.EquippedOn &&
                        previousState != FlashlightState.RaisingArm)
                    {
                        StartArmRaiseAnimation();
                    }
                    break;

                case FlashlightState.EquippedOff:
                case FlashlightState.None:
                    if (previousState == FlashlightState.EquippedOn ||
                        previousState == FlashlightState.Blocked)
                    {
                        StartArmLowerAnimation();
                    }
                    break;

                case FlashlightState.Blocked:

                    if (previousState == FlashlightState.EquippedOn)
                    {
                        StartArmLowerAnimation();
                    }
                    break;
            }

            UpdateAnimatorParameters();
        }
    }

    private void UpdateAnimatorParameters()
    {
        if (playerAnimator == null) return;

        playerAnimator.SetBool(flashlightEquippedParam, hasFlashlightEquipped);
        playerAnimator.SetBool(flashlightOnParam, isFlashlightOn);
    }

    private void StartArmRaiseAnimation()
    {
        if (isTransitioning && currentTransitionCoroutine != null)
        {
            StopCoroutine(currentTransitionCoroutine);
        }

        isTransitioning = true;
        currentTransitionCoroutine = StartCoroutine(ArmRaiseCoroutine());
    }

    private void StartArmLowerAnimation()
    {
        if (isTransitioning && currentTransitionCoroutine != null)
        {
            StopCoroutine(currentTransitionCoroutine);
        }

        isTransitioning = true;
        currentTransitionCoroutine = StartCoroutine(ArmLowerCoroutine());
    }

    private IEnumerator ArmRaiseCoroutine()
    {
        float startPosition = currentArmPosition;
        float elapsedTime = 0f;

        while (elapsedTime < armRaiseDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / armRaiseDuration;

            t = t * t * (3f - 2f * t);

            currentArmPosition = Mathf.Lerp(startPosition, 1f, t);

            yield return null;
        }

        currentArmPosition = 1f;
        isTransitioning = false;

        if (debugMode)
        {
            
        }
    }

    private IEnumerator ArmLowerCoroutine()
    {
        float startPosition = currentArmPosition;
        float elapsedTime = 0f;

        while (elapsedTime < armLowerDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / armLowerDuration;

            t = t * t * (3f - 2f * t);

            currentArmPosition = Mathf.Lerp(startPosition, 0f, t);

            yield return null;
        }

        currentArmPosition = 0f;
        isTransitioning = false;

        if (debugMode)
        {
            
        }
    }

    private void UpdateArmPosition()
    {
        if (playerAnimator != null)
        {
            playerAnimator.SetFloat(armTransitionParam, currentArmPosition);
            playerAnimator.SetBool(armRaisedParam, currentArmPosition > 0.5f);
        }
    }

    private void CheckCollisions()
    {
        if (!enableCollisionDetection || armTransform == null) return;

        bool wasBlocked = currentState == FlashlightState.Blocked;
        bool isBlocked = IsArmBlockedByCollision();

        if (isBlocked && !wasBlocked)
        {
            if (debugMode)
            {
                
            }

            if (isFlashlightOn)
            {
                StartArmLowerAnimation();
            }
        }
        else if (!isBlocked && wasBlocked)
        {
            if (debugMode)
            {
                
            }

            if (isFlashlightOn && !isTransitioning)
            {
                StartArmRaiseAnimation();
            }
        }
    }

    private bool IsArmBlockedByCollision()
    {
        if (armTransform == null) return false;

        Vector3 armPosition = armTransform.position;
        Vector3 armForward = armTransform.forward;
        Vector3 armUp = armTransform.up;

        bool forwardBlocked = Physics.SphereCast(armPosition, collisionCheckRadius, armForward,
                                                  out RaycastHit forwardHit, collisionCheckDistance, collisionLayers);

        bool upBlocked = Physics.SphereCast(armPosition, collisionCheckRadius * 0.7f, armUp,
                                           out RaycastHit upHit, collisionCheckDistance * 0.5f, collisionLayers);

        if (debugMode)
        {
            Color debugColor = (forwardBlocked || upBlocked) ? Color.red : Color.green;
            Debug.DrawRay(armPosition, armForward * collisionCheckDistance, debugColor, 0.1f);
            Debug.DrawRay(armPosition, armUp * collisionCheckDistance * 0.5f, debugColor, 0.1f);
        }

        return forwardBlocked || upBlocked;
    }

    public FlashlightState GetCurrentState()
    {
        return currentState;
    }

    public bool IsFlashlightEquipped()
    {
        return hasFlashlightEquipped;
    }

    public bool IsFlashlightOn()
    {
        return isFlashlightOn;
    }

    public bool IsArmRaised()
    {
        return currentArmPosition > 0.5f;
    }

    public bool IsTransitioning()
    {
        return isTransitioning;
    }

    public void ForceStateUpdate()
    {
        UpdateFlashlightState();
        HandleStateTransitions();
    }

    public void ForceUpdateState(bool flashlightState)
    {
        isFlashlightOn = flashlightState;
        UpdateFlashlightState();
        HandleStateTransitions();

        if (debugMode)
        {
            
        }
    }

    public void OnFlashlightTurnedOn()
    {
        if (debugMode)
        {
            
        }

        isFlashlightOn = true;
        UpdateFlashlightState();
        HandleStateTransitions();
    }

    public void OnFlashlightTurnedOff()
    {
        if (debugMode)
        {
            
        }

        isFlashlightOn = false;
        UpdateFlashlightState();
        HandleStateTransitions();
    }

    #if UNITY_EDITOR
    [ContextMenu("Debug Current State")]
    public void DebugCurrentState()
    {
        
    }
    #endif
}