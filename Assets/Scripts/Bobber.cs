using UnityEngine;

// Makes an object float gently up and down. Used on coins (purely cosmetic).
public class Bobber : MonoBehaviour
{
    public float height = 0.2f;   // how far up/down it moves
    public float speed = 2f;      // how fast it bobs

    private float baseY;
    private float phase;

    void Start()
    {
        baseY = transform.position.y;
        // Offset the phase by position so the coins don't all bob in perfect sync.
        phase = transform.position.x + transform.position.z;
    }

    void Update()
    {
        Vector3 p = transform.position;
        p.y = baseY + Mathf.Sin(Time.time * speed + phase) * height;
        transform.position = p;
    }
}