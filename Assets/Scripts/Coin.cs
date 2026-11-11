using UnityEngine;

// Attach to every coin. The coin's collider must have "Is Trigger" checked.
public class Coin : MonoBehaviour
{
    // Called by Unity when another collider enters this coin's trigger.
    void OnTriggerEnter(Collider other)
    {
        // Only react to the player (not obstacles or anything else).
        if (other.GetComponent<PlayerController>() != null)
        {
            GameManager.Instance.CollectCoin();
            Destroy(gameObject);   // the coin disappears
        }
    }
}
