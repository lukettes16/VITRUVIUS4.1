using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

public class MovJugador1 : PlayerControllerBase
{
    #region Serialized Fields

    [Header("Fisica - Materiales")]
    [SerializeField] private PhysicMaterial standPhysicsMaterial;
    [SerializeField] private PhysicMaterial crouchPhysicsMaterial;
    [SerializeField] private PhysicMaterial stealthPhysicsMaterial;

    [Header("Pickup Animation")]
    [SerializeField] private ItemPickupReach pickupReach;

    [Header("Audio Extra")]
    [SerializeField] private AudioClip cooldownStartClip;
    [Range(0f, 3f)]
    [SerializeField] private float cooldownVolume = 1.0f;

    #endregion

    #region Private Fields

    private Rigidbody playerRigidbody;
    private bool isTransitioning = false;
    private float transitionTimer = 0f;
    private float previousHeight;
    private Vector3 previousCenter;
    private PhysicMaterial currentPhysicsMaterial;
    private Collider playerCollider;

    #endregion

    #region Unity Lifecycle

    protected override void Awake()
    {
        base.Awake();
        InitializeStamina();
        InitializeCollider();
        InitializeInputActions();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        EnableInputActions();
        ResetGamepadVibration();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        DisableInputActions();
        ResetGamepadVibration();
    }

    protected override void Update()
    {
        base.Update();
        UpdateVisualEffects();

        if (isInUI || !enabled || controller == null)
        {
            UpdateAnimatorToIdle();
            return;
        }

        UpdateStamina();
        UpdateMovement();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        HandleTriggerEnter(other);
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        HandleTriggerExit(other);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, collectionRange);
    }

    #endregion

    #region Initialization

    protected override void InitializeComponents()
    {
        base.InitializeComponents();
        playerRigidbody = GetComponent<Rigidbody>();
        playerCollider = GetComponent<Collider>();

        if (pickupReach == null)
        {
            pickupReach = GetComponent<ItemPickupReach>();
        }
    }

    private void InitializeStamina()
    {
        currentStamina = maxStamina;

        if (staminaUI != null)
        {
            staminaUI.InitializeMaxStamina(maxStamina);
        }
    }

    private void InitializeCollider()
    {
        controller.height = standHeight;
        controller.center = standCenter;
    }

    private void InitializeInputActions()
    {
        if (collectAction != null)
        {
            collectAction.action.performed += context => TryInteract();
        }

        if (liftDoorAction != null)
        {
            liftDoorAction.action.performed += context => OnLiftDoorPressed();
            liftDoorAction.action.canceled += context => OnLiftDoorReleased();
        }

        if (inventoryAction != null)
        {
            inventoryAction.action.performed += context => CheckForInventory();
        }
    }

    private void InitializeCamera()
    {
        if (cameraTransform == null) cameraTransform = Camera.main?.transform;
        if (cameraTransform != null)
        {
            originalCameraPosition = cameraTransform.localPosition;
        }
    }

    private void EnableInputActions()
    {
        collectAction?.action.Enable();
        liftDoorAction?.action.Enable();
        inventoryAction?.action.Enable();
    }

    private void DisableInputActions()
    {
        collectAction?.action.Disable();
        liftDoorAction?.action.Disable();
        inventoryAction?.action.Disable();
    }

    private void ResetGamepadVibration()
    {
        gamepad?.SetMotorSpeeds(0f, 0f);
    }

    #endregion

    #region Trigger Handling

    private void HandleTriggerEnter(Collider other)
    {
        FallenDoor doorScript = other.GetComponent<FallenDoor>();
        if (doorScript != null)
        {
            currentDoorToLift = doorScript;
        }

        ElectricBox box = other.GetComponentInParent<ElectricBox>() ?? other.GetComponent<ElectricBox>();
        if (box != null)
        {
            currentElectricBox = box;
        }

        PuertaDobleConLlave keyDoor = other.GetComponentInParent<PuertaDobleConLlave>() ?? other.GetComponent<PuertaDobleConLlave>();
        if (keyDoor != null)
        {
            currentKeyDoor = keyDoor;
        }

        PuertaDobleAccion door = other.GetComponentInParent<PuertaDobleAccion>() ?? other.GetComponent<PuertaDobleAccion>();
        if (door != null)
        {
            currentDoor = door;
            door.AddPlayer(this.gameObject);
        }
    }

    private void HandleTriggerExit(Collider other)
    {
        FallenDoor doorScript = other.GetComponent<FallenDoor>();
        if (doorScript != null && doorScript == currentDoorToLift)
        {
            currentDoorToLift = null;
            if (isHoldingDoor || isAnimationInLiftState)
            {
                isHoldingDoor = false;
                animator.SetBool("ShouldCancelLift", true);
                StopDoorLiftEvent();
                StartCoroutine(ResetCancelLiftFlag());
            }
        }

        ElectricBox box = other.GetComponentInParent<ElectricBox>() ?? other.GetComponent<ElectricBox>();
        if (box != null && box == currentElectricBox)
        {
            currentElectricBox = null;
        }

        PuertaDobleConLlave keyDoor = other.GetComponentInParent<PuertaDobleConLlave>() ?? other.GetComponent<PuertaDobleConLlave>();
        if (keyDoor != null && keyDoor == currentKeyDoor)
        {
            currentKeyDoor = null;
        }

        PuertaDobleAccion door = other.GetComponentInParent<PuertaDobleAccion>() ?? other.GetComponent<PuertaDobleAccion>();
        if (door != null && door == currentDoor)
        {
            door.RemovePlayer(this.gameObject);
            currentDoor = null;
        }
    }

    #endregion

    #region Input Callbacks

    public override void OnMove(InputValue value)
    {
        if (isInUI)
        {
            moveInput = Vector2.zero;
            isMoving = false;
            return;
        }

        moveInput = value.Get<Vector2>();
        isMoving = moveInput.magnitude > 0.1f;
    }

    public override void OnRun(InputValue value)
    {
        if (isInUI) return;

        float triggerValue = value.Get<float>();
        isRunningInput = value.isPressed && triggerValue > 0.1f;
    }

    public override void OnCrouch(InputValue value)
    {
        if (isInUI) return;
        if (value.isPressed && !isTransitioning)
        {
            isCrouching = !isCrouching;
            StartCrouchTransition();
        }
    }

    #endregion

    #region Interaction System

    protected override void TryInteract()
    {
        if (isInUI) return;

        if (TryInteractWithDoor()) return;
        if (TryInteractWithKeyDoor()) return;
        if (TryInteractWithElectricBox()) return;

        TryCollectItems();
    }

    private bool TryInteractWithDoor()
    {
        if (currentDoor != null)
        {
            if (!currentDoor.enabled || currentDoor == null)
            {
                currentDoor = null;
                return false;
            }

            currentDoor.IntentoDeAccion(this.gameObject);
            return true;
        }

        return false;
    }

    private bool TryInteractWithKeyDoor()
    {
        if (currentKeyDoor != null)
        {
            currentKeyDoor.IntentoAbrirPuerta(this);
            return true;
        }

        return false;
    }

    private bool TryInteractWithElectricBox()
    {
        if (currentElectricBox != null)
        {
            currentElectricBox.TryDeactivatePower(this);
            return true;
        }

        return false;
    }

    protected override void TryCollectItems()
    {
        if (isInUI) return;

        Collider[] items = Physics.OverlapSphere(transform.position, collectionRange, collectableLayer);

        GameObject closestItem = FindClosestCollectableItem(items);

        if (closestItem != null)
        {
            if (pickupReach != null)
            {

                pickupReach.ReachForItem(closestItem, OnHandReachedItem);
            }
            else
            {

                CollectItem(closestItem);
            }
        }
    }

    private GameObject FindClosestCollectableItem(Collider[] items)
    {
        GameObject closestItem = null;
        float closestDistance = float.MaxValue;

        foreach (Collider itemCollider in items)
        {
            GameObject item = itemCollider.gameObject;

            if (IsInteractiveObject(item))
            {
                continue;
            }

            if (IsCollectableItem(item))
            {
                float distance = Vector3.Distance(transform.position, item.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestItem = item;
                }
            }
        }

        return closestItem;
    }

    private bool IsInteractiveObject(GameObject obj)
    {
        return obj.GetComponent<PuertaDobleAccion>() != null ||
               obj.GetComponent<ElectricBox>() != null ||
               obj.GetComponent<PuertaDobleConLlave>() != null;
    }

    private bool IsCollectableItem(GameObject obj)
    {
        return obj.GetComponent<PickableItem>() != null ||
               obj.GetComponent<KeyCard>() != null ||
               obj.GetComponent<CollectableItem>() != null;
    }

    private void OnHandReachedItem(GameObject item)
    {
        if (item == null) return;
        CollectItem(item);
    }

    private void CollectItem(GameObject item)
    {
        PickableItem pickable = item.GetComponent<PickableItem>();
        if (pickable != null)
        {
            pickable.Collect(gameObject);
            popupBillboard?.ShowMessage($"I found the {pickable.DisplayName}!", 2f);

            if (pickupReach != null)
            {
                pickupReach.OnItemCollected();
            }
            return;
        }

        KeyCard keyCard = item.GetComponent<KeyCard>();
        if (keyCard != null)
        {
            keyCard.Collect(gameObject);
            popupBillboard?.ShowMessage($"I found the {keyCard.name}!", 2f);

            if (pickupReach != null)
            {
                pickupReach.OnItemCollected();
            }
            return;
        }

        CollectableItem collectable = item.GetComponent<CollectableItem>();
        if (collectable != null)
        {
            collectable.Collect(gameObject);
            popupBillboard?.ShowMessage($"I found the {collectable.ItemID}!", 2f);

            if (pickupReach != null)
            {
                pickupReach.OnItemCollected();
            }
            return;
        }
    }

    private IEnumerator CollectAfterReach(GameObject item)
    {
        yield return new WaitForSeconds(0.2f);

        if (item == null) yield break;

        PickableItem pickable = item.GetComponent<PickableItem>();
        if (pickable != null)
        {
            pickable.Collect(gameObject);
            popupBillboard?.ShowMessage($"I found the {pickable.DisplayName}!", 2f);

            if (pickupReach != null)
            {
                pickupReach.OnItemCollected();
            }
            yield break;
        }

        KeyCard keyCard = item.GetComponent<KeyCard>();
        if (keyCard != null)
        {
            keyCard.Collect(gameObject);
            popupBillboard?.ShowMessage($"I found the {keyCard.name}!", 2f);

            if (pickupReach != null)
            {
                pickupReach.OnItemCollected();
            }
            yield break;
        }

        CollectableItem collectable = item.GetComponent<CollectableItem>();
        if (collectable != null)
        {
            collectable.Collect(gameObject);
            popupBillboard?.ShowMessage($"I found the {collectable.ItemID}!", 2f);

            if (pickupReach != null)
            {
                pickupReach.OnItemCollected();
            }
            yield break;
        }
    }

    #endregion

    #region Door Lift System

    public override void OnLiftDoorPressed()
    {
        if (currentDoorToLift == null || isInUI) return;

        liftButtonPressed = true;
        liftButtonHoldTime = 0f;

        StartCoroutine(CheckLiftHold());
    }

    public override void OnLiftDoorReleased()
    {
        liftButtonPressed = false;
        liftButtonHoldTime = 0f;

        if (isAnimationInLiftState || animator.GetBool("IsStartingLift"))
        {
            animator.SetBool("ShouldCancelLift", true);
            StopDoorLiftEvent();
            StartCoroutine(ResetCancelLiftFlag());
        }
    }

    private IEnumerator CheckLiftHold()
    {
        while (liftButtonPressed)
        {
            liftButtonHoldTime += Time.deltaTime;

            if (liftButtonHoldTime >= minHoldTimeToStartLift && !animator.GetBool("IsStartingLift"))
            {
                isHoldingDoor = true;
                animator.SetBool("IsStartingLift", true);
                yield break;
            }

            yield return null;
        }
    }

    public override void OnDoorLiftAnimationStart()
    {
        if (currentDoorToLift == null) return;

        if (!isHoldingDoor)
        {
            animator.SetBool("ShouldCancelLift", true);
            animator.SetBool("IsStartingLift", false);
            StartCoroutine(ResetCancelLiftFlag());
            StopDoorLiftEvent();
            return;
        }

        isAnimationInLiftState = true;
    }

    public void StartDoorLiftEvent()
    {
        if (currentDoorToLift == null) return;
        isAnimationInLiftState = true;

        if (isHoldingDoor)
        {
            animator.SetBool("IsLifting", true);
            animator.SetBool("IsStartingLift", false);
            currentDoorToLift.StartLifting(this);
        }
        else
        {
            StopDoorLiftEvent();
        }
    }

    public override void OnDoorLiftAnimationComplete()
    {
        if (currentDoorToLift == null) return;

        if (isHoldingDoor)
        {
            animator.SetBool("IsLifting", true);
            animator.SetBool("IsStartingLift", false);
            isAnimationInLiftState = true;
        }
        else
        {
            StopDoorLiftEvent();
            animator.SetBool("ShouldCancelLift", true);
            StartCoroutine(ResetCancelLiftFlag());
        }
    }

    public void StopDoorLiftEvent()
    {
        animator.SetBool("IsLifting", false);
        animator.SetBool("IsStartingLift", false);
        isAnimationInLiftState = false;
        isHoldingDoor = false;

        if (currentDoorToLift != null)
        {
            currentDoorToLift.StopLifting();
        }
    }

    private IEnumerator ResetCancelLiftFlag()
    {
        float waitTime = 0.15f;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("StartLift"))
        {
            waitTime = 0.25f;
        }

        yield return new WaitForSeconds(waitTime);
        animator.SetBool("ShouldCancelLift", false);
    }

    #endregion

    #region Cooperative Effects

    public void StartCooperativeEffects(float shakeDuration, float shakeMagnitude, float lowFrequency, float highFrequency, float rumbleDuration)
    {
        if (cameraTransform != null)
        {
            StopCoroutine("ShakeCoroutine");
            StartCoroutine(ShakeCoroutine(shakeDuration, shakeMagnitude));
        }

        if (gamepad != null)
        {
            StopCoroutine("RumbleCoroutine");
            StartCoroutine(RumbleCoroutine(lowFrequency, highFrequency, rumbleDuration));
        }
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        if (isShaking) yield break;

        isShaking = true;
        float elapsed = 0f;

        if (originalCameraPosition == Vector3.zero && cameraTransform.parent == null)
        {
            originalCameraPosition = cameraTransform.localPosition;
        }

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            cameraTransform.localPosition = originalCameraPosition + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cameraTransform.localPosition = originalCameraPosition;
        isShaking = false;
    }

    private IEnumerator RumbleCoroutine(float low, float high, float duration)
    {
        if (gamepad != null)
        {
            gamepad.SetMotorSpeeds(low, high);
            yield return new WaitForSeconds(duration);
            gamepad.SetMotorSpeeds(0f, 0f);
        }
    }

    #endregion

    #region UI Management

    public void EnterLockMode(KeypadUIManager uiManager)
    {
        if (isInUI) return;

        currentLockUI = uiManager;
        isInUI = true;

        playerInput ??= GetComponent<PlayerInput>();
        playerInput?.SwitchCurrentActionMap("UI");
    }

    public void ExitLockMode()
    {
        if (!isInUI) return;

        isInUI = false;
        currentLockUI = null;
        moveInput = Vector2.zero;
        isMoving = false;

        playerInput ??= GetComponent<PlayerInput>();
        playerInput?.SwitchCurrentActionMap("Player");
    }

    private void CheckForInventory()
    {
        if (playerInventory != null && playerInventory.HasKeyCard("Tarjeta") && !isInUI)
        {
            bool currentState = inventoryCanvas.activeSelf;
            inventoryCanvas.SetActive(!currentState);
        }
    }

    #endregion

    #region Visual Effects

    private void UpdateVisualEffects()
    {
        if (fogSphereVFX != null)
        {
            Vector3 spherePosition = transform.position + sphereOffset;
            fogSphereVFX.SetVector3(vfxCenterParameterName, spherePosition);
        }
    }

    #endregion

    #region Stamina System

    private void UpdateStamina()
    {
        bool moving = moveInput.magnitude > 0.1f;

        RechargeStamina(moving);
        CheckStaminaDepletion();
        CheckStaminaCooldown();

        if (!canRun)
        {
            return;
        }

        if (isRunningInput && moving && canRun && !isCrouching)
        {
            DepleteStamina();
        }

        if (!moving && isRunningInput)
        {
            isRunningInput = false;
        }
    }

    private void RechargeStamina(bool moving)
    {
        if (currentStamina < maxStamina && !isRunningInput)
        {
            float previousStamina = currentStamina;
            currentStamina = Mathf.Clamp(currentStamina + staminaRechargeRate * Time.deltaTime, 0, maxStamina);

            if (staminaUI != null)
            {
                staminaUI.UpdateStaminaValue(currentStamina, maxStamina);
            }

            if (previousStamina < maxStamina && currentStamina >= maxStamina && staminaWasEmpty)
            {
                if (staminaUI != null)
                {
                    staminaUI.OnStaminaFullyRecharged();
                }
                staminaWasEmpty = false;
            }
        }
    }

    private void DepleteStamina()
    {
        currentStamina = Mathf.Clamp(currentStamina - staminaDepletionRate * Time.deltaTime, 0, maxStamina);

        if (staminaUI != null)
        {
            staminaUI.UpdateStaminaValue(currentStamina, maxStamina);
        }

        if (!wasRunning && staminaUI != null)
        {
            staminaUI.ShowStaminaBar();
            wasRunning = true;
        }
    }

    private void CheckStaminaDepletion()
    {
        if (currentStamina <= 0 && canRun)
        {
            canRun = false;
            cooldownTimer = runCooldown;
            staminaWasEmpty = true;
            isRunningInput = false;

            if (staminaUI != null)
            {
                staminaUI.HideStaminaBar();
            }

            if (animator != null)
            {
                animator.SetBool(tiredAnimationBool, true);
            }

            if (fatigueFeedback != null)
            {
                fatigueFeedback.SetExhausted(true);
            }

            if (audioSource != null)
            {
                if (cooldownStartClip != null)
                {
                    audioSource.PlayOneShot(cooldownStartClip, cooldownVolume);
                }

                if (pantingSound != null)
                {
                    audioSource.clip = pantingSound;
                    audioSource.loop = true;
                    audioSource.Play();
                }
            }
        }
    }

    private void CheckStaminaCooldown()
    {
        if (!canRun)
        {
            cooldownTimer -= Time.deltaTime;

            if (cooldownTimer <= 0)
            {
                RecoverFromExhaustion();
            }
        }
    }

    private void RecoverFromExhaustion()
    {
        canRun = true;
        currentStamina = maxStamina;

        if (animator != null)
        {
            animator.SetBool(tiredAnimationBool, false);
        }

        if (fatigueFeedback != null)
        {
            fatigueFeedback.SetExhausted(false);
        }

        if (audioSource != null && audioSource.isPlaying && audioSource.clip == pantingSound)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        if (staminaUI != null)
        {
            staminaUI.UpdateStaminaValue(currentStamina, maxStamina);
        }

        if (staminaWasEmpty && staminaUI != null)
        {
            staminaUI.OnStaminaFullyRecharged();
            staminaWasEmpty = false;
        }
    }

    #endregion

    #region Movement System

    private void UpdateMovement()
    {
        float desiredSpeed = CalculateDesiredSpeed();
        UpdateSpeedScalar(desiredSpeed);

        Vector3 movement = CalculateMovementDirection();
        ApplyGravity();
        MoveCharacter(movement);
        RotateCharacter(movement);

        UpdateColliderAndAnimator();
    }

    private float CalculateDesiredSpeed()
    {
        bool moving = moveInput.magnitude > 0.1f;

        if (!canRun)
        {
            return 0f;
        }
        else if (isCrouching)
        {
            if (wasRunning && staminaUI != null)
            {
                staminaUI.HideStaminaBar();
                wasRunning = false;
            }

            bool isStealthMode = Input.GetKey(KeyCode.LeftShift);

            if (playerCollider != null && !isTransitioning)
            {
                if (isStealthMode && stealthPhysicsMaterial != null)
                {
                    playerCollider.material = stealthPhysicsMaterial;
                }
                else if (crouchPhysicsMaterial != null)
                {
                    playerCollider.material = crouchPhysicsMaterial;
                }
            }

            return isStealthMode ? crouchStealthSpeed : crouchSpeed;
        }
        else if (isRunningInput && moving && canRun)
        {
            return runSpeed;
        }
        else
        {
            if (wasRunning && staminaUI != null)
            {
                staminaUI.HideStaminaBar();
                wasRunning = false;
            }
            return moveSpeed;
        }
    }

    private void UpdateSpeedScalar(float desiredSpeed)
    {
        float accel = currentSpeedScalar < desiredSpeed ? acceleration : deceleration;
        currentSpeedScalar = Mathf.MoveTowards(currentSpeedScalar, desiredSpeed, accel * Time.deltaTime);
    }

    protected override Vector3 CalculateMovementDirection()
    {
        Vector3 movement;

        if (cameraTransform != null)
        {
            Vector3 camForward = cameraTransform.forward;
            camForward.y = 0;
            camForward.Normalize();

            Vector3 camRight = cameraTransform.right;
            camRight.y = 0;
            camRight.Normalize();

            movement = (camForward * moveInput.y + camRight * moveInput.x).normalized;
        }
        else
        {
            movement = new Vector3(moveInput.x, 0, moveInput.y);
        }

        return movement;
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded)
        {
            verticalVelocity.y = -2f;
        }
        else
        {
            verticalVelocity.y += gravity * Time.deltaTime;
        }
    }

    private void MoveCharacter(Vector3 movement)
    {
        Vector3 finalMovement = (movement * currentSpeedScalar) + new Vector3(0, verticalVelocity.y, 0);
        controller.Move(finalMovement * Time.deltaTime);
    }

    private void RotateCharacter(Vector3 movement)
    {
        if (movement != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void UpdateColliderAndAnimator()
    {
        bool moving = moveInput.magnitude > 0.1f;

        animator.SetBool("IsCrouching", isCrouching);
        animator.SetBool("IsRunning", isRunningInput && moving && canRun && !isCrouching);

        if (!isTransitioning)
        {
            controller.height = isCrouching ? crouchHeight : standHeight;
            controller.center = isCrouching ? crouchCenter : standCenter;
        }

        animator.SetFloat("Speed", moving ? (isRunningInput && canRun && !isCrouching ? 2f : (isCrouching ? 1.0f : 1f)) : 0f);
    }

    private void UpdateAnimatorToIdle()
    {
        animator.SetFloat("Speed", 0f);
        animator.SetBool("IsRunning", false);
    }

    #endregion

    #region Audio (Animation Events)

    public void PlayFootstepSound(int playerID)
    {
        if (controller != null && controller.isGrounded)
        {
            AudioClip clipToPlay;

            if (isRunningInput && canRun && !isCrouching)
            {
                clipToPlay = runFootstepClip;
            }
            else if (isCrouching)
            {
                clipToPlay = crouchFootstepClip;
            }
            else
            {
                clipToPlay = walkFootstepClip;
            }

            if (clipToPlay != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(
                    clipToPlay,
                    transform.position,
                    0.3f,
                    (isRunningInput && canRun && !isCrouching) ? 1.6f : 1.4f,
                    Random.Range(0.95f, 1.05f)
                );
            }
        }
    }

    #endregion

    #region Public Control Methods

    public void StopMovement()
    {
        this.enabled = false;
        verticalVelocity = Vector3.zero;
        moveInput = Vector2.zero;
        isMoving = false;
        isRunningInput = false;

        if (staminaUI != null)
        {
            staminaUI.HideImmediate();
        }
        wasRunning = false;

        if (fatigueFeedback != null)
        {
            fatigueFeedback.SetExhausted(false);
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsCrouching", false);
        }
    }

    public void AllowMovement()
    {
        this.enabled = true;
    }

    public void ResetMovementState()
    {
        this.enabled = true;
        isInUI = false;
        moveInput = Vector2.zero;
        isRunningInput = false;
        isCrouching = false;
        verticalVelocity.y = -5f;
        wasRunning = false;

        if (fatigueFeedback != null)
        {
            fatigueFeedback.SetExhausted(false);
        }

        if (staminaUI != null)
        {
            staminaUI.HideImmediate();
        }
    }

    public void ClearCurrentDoor(PuertaDobleAccion door)
    {
        if (currentDoor == door)
        {
            currentDoor = null;
        }
    }

    #endregion

    #region Public Properties

    public bool IsCrouchingState => isCrouching;
    public bool IsRunningState => isRunningInput && isMoving && canRun && !isCrouching;
    public Transform GetTransform() => this.transform;

    #endregion

    #region Crouch Transition Methods

    private void StartCrouchTransition()
    {
        if (isTransitioning) return;

        isTransitioning = true;
        transitionTimer = 0f;
        previousHeight = controller.height;
        previousCenter = controller.center;

        StartCoroutine(SmoothCrouchTransition());
    }

    private System.Collections.IEnumerator SmoothCrouchTransition()
    {
        float targetHeight = isCrouching ? crouchHeight : standHeight;
        Vector3 targetCenter = isCrouching ? crouchCenter : standCenter;
        float targetMass = isCrouching ? crouchMass : standMass;
        float targetDrag = isCrouching ? crouchDrag : standDrag;
        PhysicMaterial targetPhysicsMaterial = isCrouching ? crouchPhysicsMaterial : standPhysicsMaterial;

        while (transitionTimer < transitionDuration)
        {
            transitionTimer += Time.deltaTime;
            float t = transitionTimer / transitionDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            controller.height = Mathf.Lerp(previousHeight, targetHeight, t);
            controller.center = Vector3.Lerp(previousCenter, targetCenter, t);

            if (playerRigidbody != null)
            {
                playerRigidbody.mass = Mathf.Lerp(playerRigidbody.mass, targetMass, t);
                playerRigidbody.drag = Mathf.Lerp(playerRigidbody.drag, targetDrag, t);
            }

            yield return null;
        }

        controller.height = targetHeight;
        controller.center = targetCenter;

        if (playerRigidbody != null)
        {
            playerRigidbody.mass = targetMass;
            playerRigidbody.drag = targetDrag;
        }

        if (playerCollider != null && targetPhysicsMaterial != null)
        {
            playerCollider.material = targetPhysicsMaterial;
            currentPhysicsMaterial = targetPhysicsMaterial;
        }

        isTransitioning = false;
    }

    #endregion
}