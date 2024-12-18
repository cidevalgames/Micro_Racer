using System.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Car.Controller.V1
{
    public class WheelControl : MonoBehaviour
    {
        public Transform wheelModel;
        public TrailRenderer slideTrailRenderer;
        [Tooltip("X = Sideways slip & Y = Forward slip")]
        private Vector2 savedParameters;

        [HideInInspector] public WheelCollider wheelCollider;
        // Create properties for the CarControl script
        // (You should enable/disable these via the 
        // Editor Inspector window)
        public bool steerable;
        public bool motorized;
        public bool oiled;

        public Vector3 position;
        public Quaternion rotation;

        // Start is called before the first frame update
        private void Start()
        {

            wheelCollider = GetComponent<WheelCollider>();
            savedParameters.x = wheelCollider.sidewaysFriction.stiffness;
            savedParameters.y = wheelCollider.forwardFriction.stiffness;

        }

        // Update is called once per frame
        void Update()
        {
            // Get the Wheel collider's world pose values and
            // use them to set the wheel model's position and rotation
            wheelCollider.GetWorldPose(out position, out rotation);
            wheelModel.transform.position = position;
            wheelModel.transform.rotation = rotation;
        }

        //Checking if the wheel is spinning at a diferent speed than the car is moving
        public void CheckForSliping(float carVelocity, float x)
        {
            float distanceTraveledByTheWheel = math.abs((wheelCollider.radius * 2 * math.PI) * (wheelCollider.rpm / 60));

            if (wheelCollider.isGrounded && distanceTraveledByTheWheel > carVelocity + x || distanceTraveledByTheWheel < carVelocity - x)
            {
                //is slidin
                slideTrailRenderer.emitting = true;
            }
            else
            {
                //is not slidin
                slideTrailRenderer.emitting = false;
            }
        }


    }
}