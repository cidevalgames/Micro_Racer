using Unity.Netcode;
using UnityEngine;

namespace Networking
{
    public class NetworkTransformTest : NetworkBehaviour
    {
        private void Update()
        {
            if (IsServer)
            {
                float theta = Time.frameCount / 10.0f;
                transform.position = new Vector3((float)Mathf.Cos(theta), .0f, (float)Mathf.Sin(theta));
            }
        }
    }
}
