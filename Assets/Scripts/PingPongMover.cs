using UnityEngine;

// Moves an object back and forth between its start position and start + offset.
public class PingPongMover : MonoBehaviour
{
    public Vector3 offset = new Vector3(8f, 0f, 0f);
    public float speed = 0.4f;   // cycles per second-ish; higher = faster

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // PingPong returns a value that bounces between 0 and 1.
        float t = Mathf.PingPong(Time.time * speed, 1f);
        transform.position = startPosition + offset * t;
    }
}
