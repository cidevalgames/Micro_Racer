using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bunny"))
        {
            CheckpointsManager.Instance.CheckpointsNumber++;

            Destroy(gameObject);
        }
    }
}
