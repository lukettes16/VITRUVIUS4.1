using UnityEngine;

public class EndOfDemoTrigger : MonoBehaviour
{
    [Header("Player Detection")]
    [Tooltip("Tags to recognize as players")]
    [SerializeField] private string[] playerTags = new string[] { "Player1", "Player2", "Player" };

    [Header("Trigger Behavior")]
    [Tooltip("Hide this GameObject when triggered")]
    [SerializeField] private bool hideOnTrigger = true;

    [Tooltip("Disable the trigger collider after activation")]
    [SerializeField] private bool disableColliderOnTrigger = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered)
        {
            return;
        }

        if (IsPlayer(other.gameObject))
        {
            
            hasTriggered = true;
            ActivateEndScreen();
        }
    }

    private bool IsPlayer(GameObject obj)
    {
        foreach (string tag in playerTags)
        {
            if (obj.CompareTag(tag))
            {
                
                return true;
            }
        }

        if (obj.GetComponent<MovJugador1>() != null || obj.GetComponent<MovJugador2>() != null)
        {
            
            return true;
        }

        if (obj.name.Contains("Player1") || obj.name.Contains("Player2"))
        {
            
            return true;
        }

        return false;
    }

    private void ActivateEndScreen()
    {
        
        EndOfDemoController.Instance.ShowEndScreen();

        if (hideOnTrigger)
        {
            
            gameObject.SetActive(false);
        }

        if (disableColliderOnTrigger)
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                
                col.enabled = false;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = hasTriggered ? Color.red : Color.green;
            Gizmos.matrix = transform.localToWorldMatrix;

            if (col is BoxCollider box)
            {
                Gizmos.DrawWireCube(box.center, box.size);
            }
            else if (col is SphereCollider sphere)
            {
                Gizmos.DrawWireSphere(sphere.center, sphere.radius);
            }
        }
    }
}