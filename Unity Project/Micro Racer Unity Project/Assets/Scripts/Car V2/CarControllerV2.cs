using UnityEngine;

public class CarControllerV2 : MonoBehaviour
{
    public float motorTorque = 2000;
    public AnimationCurve motorTorqueCurve;
    public float brakeTorque = 2000;
    public AnimationCurve brakeTorqueCurve;
    public float maxSpeed = 50;
    public float steeringRange = 30;
    public float steeringRangeAtMaxSpeed = 10;
    public AnimationCurve steeringHelpCurve;//Not implemented yet
    public GameObject centreOfGravity;

    public WheelControlV2[] wheels;
    public Rigidbody rigidBody;
    [Range(0, 1.001f)]
    public float slidingDifferenceTreshold;


    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();

        //find all child gameobjects that have the wheelcontrol script attached
        wheels = GetComponentsInChildren<WheelControlV2>();
    }

    void SetupCenterOfMass()
    {
        // Adjust center of mass vertically, to help prevent the car from rolling
        rigidBody.centerOfMass = centreOfGravity.transform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        float vInput = Input.GetAxis("Vertical");
        float hInput = Input.GetAxis("Horizontal");

        // Calculate current speed in relation to the forward direction of the car
        // (this returns a negative number when traveling backwards)
        float forwardSpeed = Vector3.Dot(transform.forward, rigidBody.linearVelocity);


        // Calculate how close the car is to top speed
        // as a number from zero to one
        float speedFactor = Mathf.InverseLerp(0, maxSpeed, forwardSpeed);
        print(speedFactor);

        // Use that to calculate how much torque is available 
        // (zero torque at top speed)
        float currentMotorTorque = 0;
        if (vInput != 0)
        {
            currentMotorTorque = motorTorqueCurve.Evaluate(speedFactor) * 10000;
        }
        ///float currentMotorTorque = Mathf.Lerp(motorTorque, 0, speedFactor);

        // �and to calculate how much to steer 
        // (the car steers more gently at top speed)
        float currentSteerRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, speedFactor);

        // Check whether the user input is in the same direction 
        // as the car's velocity
        bool isAccelerating = Mathf.Sign(vInput) == Mathf.Sign(forwardSpeed);

        foreach (var wheel in wheels)
        {
            // Check for Wheelspin
            wheel.CheckForSliping(rigidBody.GetPointVelocity(wheel.transform.position).magnitude, slidingDifferenceTreshold);
            // Apply steering to Wheel colliders that have "Steerable" enabled
            if (wheel.steerable)
            {
                wheel.wheelCollider.steerAngle = hInput * currentSteerRange;
            }

            if (isAccelerating)
            {
                // Apply torque to Wheel colliders that have "Motorized" enabled
                if (wheel.motorized)
                {
                    wheel.wheelCollider.motorTorque = vInput * currentMotorTorque;
                }
                wheel.wheelCollider.brakeTorque = 0;
            }
            else
            {
                // If the user is trying to go in the opposite direction
                // apply brakes to all wheels
                wheel.wheelCollider.brakeTorque = Mathf.Abs(vInput) * brakeTorque;
                wheel.wheelCollider.motorTorque = 0;
            }
        }
    }
}
