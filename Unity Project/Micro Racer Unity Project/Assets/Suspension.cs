using UnityEngine;

public class Suspension : MonoBehaviour
{
    [SerializeField] Rigidbody body;
    [SerializeField] Rigidbody spring;

    public Vector3 rest_position;

    private Vector3 force = Vector3.zero;
    public float offset;
    public Vector2 minMaxOffset;
    [Space]
    public float strength;
    public float damping;
    [Space]
    public float velocity;

    void Start()
    {

        rest_position = transform.localPosition;
    }
    private void update()
    {
        
    }
    private void FixedUpdate()
    {
        
        offset = rest_position.y - transform.localPosition.y;
        if (minMaxOffset.x > offset && minMaxOffset.y < offset)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, Mathf.Clamp(transform.localPosition.y, minMaxOffset.x, minMaxOffset.y), transform.localPosition.z);
            body.velocity = new Vector3(0,0,0);
        }
        velocity = spring.velocity.y;
        force.y = (offset * strength) - (velocity * damping);
        spring.AddForce(force);
        if (force.y<0)
        body.AddForceAtPosition(-force, transform.position);
    }
}
