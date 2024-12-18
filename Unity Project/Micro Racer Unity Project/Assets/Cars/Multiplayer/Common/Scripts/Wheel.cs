using UnityEngine;

namespace Car.Multiplayer.Common
{
    [System.Serializable]
    public class Wheel
    {
        [Header("Components")]
        public WheelCollider wheelCollider;
        public Transform mesh;
        public TrailRenderer trailRenderer;

        [Header("Parameters")]
        public bool steerable;
        public bool motorized;
        
        public Vector2 savedParameters;

        public void Initialize()
        {
            savedParameters.x = wheelCollider.sidewaysFriction.stiffness;
            savedParameters.y = wheelCollider.forwardFriction.stiffness;
        }

    }
}
