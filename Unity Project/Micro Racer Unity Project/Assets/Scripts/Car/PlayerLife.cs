using UnityEngine;

public class PlayerLife : MonoBehaviour
{
    Rigidbody body;
    public float maxPV;
    public float currentPV;

    void Start()
    {
        body = GetComponent<Rigidbody>();
        currentPV = maxPV;
    }
    public void MineAttack(float strength, Vector3 pointOfImpact)
    {
        body.AddForceAtPosition(strength * Vector3.up, pointOfImpact, ForceMode.Impulse);
    }
}

