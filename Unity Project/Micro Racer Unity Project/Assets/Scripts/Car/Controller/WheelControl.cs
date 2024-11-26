using Unity.Mathematics;
using UnityEngine;

namespace Car.Controller.V1
{
    public class WheelControl : MonoBehaviour
    {
        public Transform wheelModel;
        public TrailRenderer slideTrailRenderer;

        [HideInInspector] public WheelCollider wheelCollider;

        // Create properties for the CarControl script
        // (You should enable/disable these via the 
        // Editor Inspector window)
        public bool steerable;
        public bool motorized;

        public Vector3 position;
        public Quaternion rotation;

        // Start is called before the first frame update
        private void Start()
        {
            wheelCollider = GetComponent<WheelCollider>();
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
            print("distanceTraveledByTheWheel = "+ distanceTraveledByTheWheel);
            if (distanceTraveledByTheWheel > carVelocity + x||distanceTraveledByTheWheel< carVelocity-x)
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