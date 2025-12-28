using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Sistema de split-screen para 2 jugadores.
/// </summary>
[DefaultExecutionOrder(-300)]
public class FixedSplitScreenBootstrap : MonoBehaviour
{
    [Header("Fixed Split Screen Configuration")]
    [Tooltip("Orientación del split-screen (Vertical = Izq/Der, Horizontal = Arr/Aba)")]
    public SplitOrientation splitOrientation = SplitOrientation.Vertical;
    
    [Tooltip("Color del borde visual entre secciones")]
    public Color borderColor = Color.white;
    
    [Tooltip("Ancho del borde visual en píxeles")]
    public float borderWidth = 2f;

    [Header("Camera Settings")]
    [Tooltip("Si es true, busca cámaras en los hijos de los jugadores en lugar de crear nuevas")]
    public bool useExistingCameras = false;
    
    [Tooltip("FOV para ambas cámaras")]
    public float fieldOfView = 60f;
    
    [Tooltip("Near clipping plane")]
    public float nearClip = 0.3f;
    
    [Tooltip("Far clipping plane")]
    public float farClip = 1000f;

    [Header("Cameras")]
    [Tooltip("Asigna la cámara permanente para el Jugador 1")]
    public Camera player1Camera;
    
    [Tooltip("Asigna la cámara permanente para el Jugador 2")]
    public Camera player2Camera;

    [Header("Third Person Settings")]
    [Tooltip("Distancia de la cámara al jugador")]
    public float thirdPersonDistance = 8f;
    
    [Tooltip("Altura de la cámara sobre el jugador")]
    public float thirdPersonHeight = 3f;
    
    [Tooltip("Velocidad de seguimiento")]
    public float followSpeed = 5f;

    public enum SplitOrientation
    {
        Vertical,
        Horizontal
    }

    public static FixedSplitScreenBootstrap Instance { get; private set; }
    
    public Camera Player1Camera => player1Camera;
    public Camera Player2Camera => player2Camera;
    
    private GameObject player1Target;
    private GameObject player2Target;
    private GameObject borderVisual;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFixedSplitScreen();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeFixedSplitScreen()
    {
        var oldCamController = FindObjectOfType<CameraController>();
        if (oldCamController != null)
        {
            oldCamController.enabled = false;
        }

        ConfigureViewports();
        CreateVisualBorder();
        SetupCameraTracking();
        AssignCamerasToPlayers();
    }

    private void ConfigureViewports()
    {
        if (player1Camera == null || player2Camera == null) return;

        if (splitOrientation == SplitOrientation.Vertical)
        {
            player1Camera.rect = new Rect(0f, 0f, 0.5f, 1f);
            player2Camera.rect = new Rect(0.5f, 0f, 0.5f, 1f);
        }
        else
        {
            player1Camera.rect = new Rect(0f, 0.5f, 1f, 0.5f);
            player2Camera.rect = new Rect(0f, 0f, 1f, 0.5f);
        }

        player1Camera.depth = 0;
        player2Camera.depth = 1;

        if (!player1Camera.CompareTag("MainCamera"))
        {
            player1Camera.tag = "MainCamera";
        }

        foreach (var cam in Camera.allCameras)
        {
            if (cam != player1Camera && cam != player2Camera && cam.CompareTag("MainCamera"))
            {
                cam.enabled = false;
            }
        }

        EnsureSingleAudioListener();
    }

    private void CreateVisualBorder()
    {
        if (borderVisual != null) Destroy(borderVisual);
        
        borderVisual = new GameObject("SplitScreenBorder");
        borderVisual.transform.SetParent(transform);
        
        LineRenderer lineRenderer = borderVisual.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = borderColor;
        lineRenderer.endColor = borderColor;
        lineRenderer.startWidth = borderWidth;
        lineRenderer.endWidth = borderWidth;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = false;
        lineRenderer.sortingOrder = 1000;

        if (splitOrientation == SplitOrientation.Vertical)
        {
            lineRenderer.SetPosition(0, new Vector3(0.5f, 0f, 0f));
            lineRenderer.SetPosition(1, new Vector3(0.5f, 1f, 0f));
        }
        else
        {
            lineRenderer.SetPosition(0, new Vector3(0f, 0.5f, 0f));
            lineRenderer.SetPosition(1, new Vector3(1f, 0.5f, 0f));
        }
    }

    private void SetupCameraTracking()
    {
        if (player1Camera != null)
        {
            var follower1 = player1Camera.gameObject.GetComponent<FixedCameraFollower>();
            if (follower1 == null) follower1 = player1Camera.gameObject.AddComponent<FixedCameraFollower>();
            follower1.Initialize(this, 1);
        }

        if (player2Camera != null)
        {
            var follower2 = player2Camera.gameObject.GetComponent<FixedCameraFollower>();
            if (follower2 == null) follower2 = player2Camera.gameObject.AddComponent<FixedCameraFollower>();
            follower2.Initialize(this, 2);
        }
    }

    public void SetCameraTargets(GameObject player1, GameObject player2)
    {
        player1Target = player1;
        player2Target = player2;

        if (player1 != null)
        {
            var p1Controller = player1.GetComponent<PlayerControllerBase>();
            if (p1Controller != null && player1Camera != null)
                p1Controller.AssignCamera(player1Camera);
        }

        if (player2 != null)
        {
            var p2Controller = player2.GetComponent<PlayerControllerBase>();
            if (p2Controller != null && player2Camera != null)
                p2Controller.AssignCamera(player2Camera);
        }

        if (player1Camera != null)
        {
            var follower = player1Camera.GetComponent<FixedCameraFollower>();
            if (follower != null) follower.SetTarget(player1Target);
        }

        if (player2Camera != null)
        {
            var follower = player2Camera.GetComponent<FixedCameraFollower>();
            if (follower != null) follower.SetTarget(player2Target);
        }

        EnsureSingleAudioListener();
    }

    void Update()
    {
        if (player1Camera != null && player2Camera != null)
        {
            if (!player1Camera.enabled) player1Camera.enabled = true;
            if (!player2Camera.enabled) player2Camera.enabled = true;
        }
    }

    private void EnsureSingleAudioListener()
    {
        AudioListener[] allListeners = FindObjectsOfType<AudioListener>(true);
        if (allListeners.Length <= 1) return;

        AudioListener targetListener = player1Camera != null ? player1Camera.GetComponent<AudioListener>() : null;

        if (targetListener == null)
        {
            foreach (var l in allListeners)
            {
                if (l.enabled)
                {
                    targetListener = l;
                    break;
                }
            }
            if (targetListener == null && allListeners.Length > 0) targetListener = allListeners[0];
        }

        foreach (var listener in allListeners)
        {
            listener.enabled = (listener == targetListener);
        }
    }

    private void AssignCamerasToPlayers()
    {
        GameObject p1 = GameObject.FindGameObjectWithTag("Player1");
        GameObject p2 = GameObject.FindGameObjectWithTag("Player2");
        
        if (p1 == null || p2 == null)
        {
            GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
            foreach (var p in allPlayers)
            {
                if (p1 == null && (p.name.Contains("1") || p.name.ToLower().Contains("player1"))) p1 = p;
                else if (p2 == null && (p.name.Contains("2") || p.name.ToLower().Contains("player2"))) p2 = p;
            }
        }

        if (p1 != null && p2 != null)
        {
            SetCameraTargets(p1, p2);
            UpdateExternalSystems();
        }
    }

    private void UpdateExternalSystems()
    {
#if VLB_URP || true 
        try
        {
            var vlbConfigType = System.Type.GetType("VLB.Config, VolumetricLightBeam");
            if (vlbConfigType != null)
            {
                var instanceProp = vlbConfigType.GetProperty("Instance");
                if (instanceProp != null)
                {
                    var instance = instanceProp.GetValue(null);
                    var forceUpdateMethod = vlbConfigType.GetMethod("ForceUpdateFadeOutCamera");
                    if (forceUpdateMethod != null && instance != null)
                    {
                        forceUpdateMethod.Invoke(instance, null);
                    }
                }
            }
        }
        catch (System.Exception) {}
#endif
    }

    public Camera GetCameraForPlayer(int playerId)
    {
        return playerId == 1 ? player1Camera : player2Camera;
    }


    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}

public class FixedCameraFollower : MonoBehaviour
{
    private FixedSplitScreenBootstrap bootstrap;
    private Transform target;
    private int playerId;

    private float followSmoothTime = 0.1f;
    private Vector3 velocity = Vector3.zero;

    private float yaw;
    private float pitch;
    private InputAction lookAction;
    private float lookSensitivity = 1.95f;

    public void Initialize(FixedSplitScreenBootstrap bootstrap, int playerId)
    {
        this.bootstrap = bootstrap;
        this.playerId = playerId;
    }

    public void SetTarget(GameObject targetObject)
    {
        target = targetObject?.transform;
        if (target != null)
        {
            yaw = target.eulerAngles.y;
            pitch = 0f;

            PlayerInput pInput = targetObject.GetComponent<PlayerInput>();
            if (pInput == null) pInput = targetObject.GetComponentInParent<PlayerInput>();
            
            if (pInput != null)
            {
                lookAction = pInput.actions.FindAction("Look") ?? pInput.actions.FindAction("look");
                if (lookAction != null) lookAction.Enable();
            }
        }
    }

    void LateUpdate()
    {
        if (target == null || bootstrap == null) return;

        if (lookAction != null)
        {
            Vector2 lookInput = lookAction.ReadValue<Vector2>();
            float sens = 100f;
            if (JoystickManager.Instance != null)
            {
                lookInput = JoystickManager.Instance.ProcessRightStickInput(lookInput);
                sens *= JoystickManager.Instance.lookSensitivity;
            }

            yaw += lookInput.x * lookSensitivity * sens * Time.deltaTime;
            pitch -= lookInput.y * lookSensitivity * sens * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, -40f, 70f); 
        }

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 offset = rotation * new Vector3(0, 0, -bootstrap.thirdPersonDistance);
        Vector3 targetPosition = target.position + Vector3.up * bootstrap.thirdPersonHeight + offset;
        
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, followSmoothTime);
        transform.LookAt(target.position + Vector3.up * bootstrap.thirdPersonHeight);
    }
}
