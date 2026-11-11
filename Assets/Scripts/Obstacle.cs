using UnityEngine;

// Attach to every red obstacle. Collider must have "Is Trigger" checked.
// Obstacles also need a Rigidbody with "Is Kinematic" checked so triggers fire reliably, even for moving ones.
public class Obstacle : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            GameManager.Instance.HitObstacle();
        }
    }

    // Fires every frame while overlapping, so standing inside an obstacle keeps costing lives
    // (after the invulnerability window in GameManager).
    void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            GameManager.Instance.HitObstacle();
        }
    }
}
