using UnityEngine;

public class PositionClamp : MonoBehaviour
{
    [SerializeField] Transform clampedTransform;
    [SerializeField] Vector3 minWorldPos, maxWorldPos;
    [SerializeField] bool lockWorldPosX,lockWorldPosY,lockWorldPosZ; //pas utile pour l'instant mais go faire un module multiClamp qui s'occupe de la rotation et de la Scale
    void Start()
    {
        if (clampedTransform == null)
            clampedTransform = GetComponent<Transform>();
    }

    void FixedUpdate()
    {
        ClampPos();
    }
    void ClampPos()
    {
        Vector3 _newWorldPos;
        _newWorldPos.x = lockWorldPosX ? Mathf.Clamp(clampedTransform.position.x, minWorldPos.x, maxWorldPos.x) : clampedTransform.position.x;
        _newWorldPos.y = lockWorldPosY ? Mathf.Clamp(clampedTransform.position.y, minWorldPos.y, maxWorldPos.y) : clampedTransform.position.y;
        _newWorldPos.z = lockWorldPosX ? Mathf.Clamp(clampedTransform.position.z, minWorldPos.z, maxWorldPos.z) : clampedTransform.position.z;

        clampedTransform.position = new Vector3(_newWorldPos.x,_newWorldPos.y,_newWorldPos.z);
    }
}
