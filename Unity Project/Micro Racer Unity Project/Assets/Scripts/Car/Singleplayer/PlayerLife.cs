using Car.Multiplayer.Common;
using UnityEngine;

public class PlayerLife : MonoBehaviour
{
    public Rigidbody m_rigidbody;
    public CarControl m_carControl;
    float maxPV;
    float currentPV;

    void Start()
    {
        if (m_rigidbody == null)
            m_rigidbody = GetComponent<Rigidbody>();

        currentPV = maxPV;
    }

    public void MineAttack(float strength, Vector3 pointOfImpact)
    {
        m_rigidbody.AddForce(strength * Vector3.up, ForceMode.Impulse);
        //transform.InverseTransformDirection(pointOfImpact);
    }
}

