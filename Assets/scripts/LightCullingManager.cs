using UnityEngine;
using System.Collections.Generic;

public class LightCullingManager : MonoBehaviour
{
    [Header("Culling Settings")]
    [Tooltip("Maximum distance from any camera where lights remain enabled")]
    [SerializeField] private float cullingDistance = 40f;

    [Tooltip("How often to update light culling (in seconds)")]
    [SerializeField] private float updateInterval = 0.3f;

    [Tooltip("Lights that should never be culled (e.g., main directional light)")]
    [SerializeField] private List<Light> alwaysOnLights = new List<Light>();

    [Header("Player Cameras")]
    [Tooltip("Cameras to check distance from (leave empty to auto-find)")]
    [SerializeField] private Camera[] playerCameras;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;

    private Light[] allLights;
    private float updateTimer;
    private int activeLightCount;
    private int totalLightCount;

    private void Start()
    {

        if (playerCameras == null || playerCameras.Length == 0)
        {
            playerCameras = FindObjectsOfType<Camera>();
            
        }

        RefreshLightList();

    }

    private void Update()
    {
        updateTimer += Time.deltaTime;

        if (updateTimer >= updateInterval)
        {
            updateTimer = 0f;
            UpdateLightCulling();
        }
    }

    public void RefreshLightList()
    {
        allLights = FindObjectsOfType<Light>();
        totalLightCount = allLights.Length;
    }

    private void UpdateLightCulling()
    {
        if (playerCameras == null || playerCameras.Length == 0)
        {
            
            return;
        }

        activeLightCount = 0;

        foreach (Light light in allLights)
        {
            if (light == null) continue;

            if (alwaysOnLights.Contains(light))
            {
                light.enabled = true;
                activeLightCount++;
                continue;
            }

            if (IsPlayerLight(light))
            {
                light.enabled = true;
                activeLightCount++;
                continue;
            }

            if (light.type == LightType.Directional)
            {
                light.enabled = true;
                activeLightCount++;
                continue;
            }

            bool shouldBeEnabled = false;
            Vector3 lightPosition = light.transform.position;

            foreach (Camera cam in playerCameras)
            {
                if (cam == null || !cam.enabled) continue;

                float distance = Vector3.Distance(cam.transform.position, lightPosition);

                if (distance <= cullingDistance)
                {
                    shouldBeEnabled = true;
                    break;
                }
            }

            light.enabled = shouldBeEnabled;
            if (shouldBeEnabled) activeLightCount++;
        }

        if (showDebugInfo)
        {
            
        }
    }

    private bool IsPlayerLight(Light light)
    {
        Transform current = light.transform;
        while (current != null)
        {
            if (current.name == "Player1" || current.name == "Player2")
            {
                return true;
            }
            current = current.parent;
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (playerCameras == null) return;

        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        foreach (Camera cam in playerCameras)
        {
            if (cam != null)
            {
                Gizmos.DrawWireSphere(cam.transform.position, cullingDistance);
            }
        }
    }

    public void SetCullingDistance(float distance)
    {
        cullingDistance = distance;
        UpdateLightCulling();
    }

    public void AddAlwaysOnLight(Light light)
    {
        if (!alwaysOnLights.Contains(light))
        {
            alwaysOnLights.Add(light);
        }
    }

    public int GetActiveLightCount() => activeLightCount;
    public int GetTotalLightCount() => totalLightCount;
}