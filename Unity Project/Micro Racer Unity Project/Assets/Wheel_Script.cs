using UnityEngine;

public class Wheel_Script : MonoBehaviour
{
    [SerializeField] Rigidbody body;
    [SerializeField] Rigidbody spring;

    public Vector3 rest_position;

    private Vector3 force = Vector3.zero;
    public float offset;
    public float strength;
    private float velocity;
    public float damping;

    void Start()
    {
        rest_position = transform.position;
    }

    private void Update()
    {
        velocity = spring.velocity.y;
        offset = rest_position.y - transform.localPosition.y;
        force.y = (offset * strength) - (velocity * damping);
        spring.AddForce(force);
        body.AddForceAtPosition(-force, transform.position);
    }
}
