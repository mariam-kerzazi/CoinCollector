using UnityEngine;

// Spins an object continuously. Used on coins (spin around Y) and on the sweeping red bar obstacle.
public class Rotator : MonoBehaviour
{
    public Vector3 axis = Vector3.up;
    public float degreesPerSecond = 90f;

    void Update()
    {
        // Space.World spins around the world's vertical axis, so a coin standing on its edge
        // turns like a real coin.
        transform.Rotate(axis * degreesPerSecond * Time.deltaTime, Space.World);
    }
}
