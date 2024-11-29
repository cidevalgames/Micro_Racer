using System;
using System.Linq;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Multiplayer.Fishnet.Player.Car
{
    public class CarControl : NetworkBehaviour
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

        public WheelControl[] wheels;
        private Rigidbody m_rigidbody;
        [Range(0, 1.001f)]
        public float slidingDifferenceTreshold;

        [Header("Multiplayer")]
        [SerializeField] private Transform _cameraTransform;

        private Camera _playerCamera;

        [SerializeField] private InputActionAsset _actions;

        private WheelCollider[] _wheelColliders = new WheelCollider[4];

        private Vector3 _cameraPosition;
        private Quaternion _cameraRotation;

        private bool _brake = false;
        private float _horizontal;
        private float _vertical;

        #region Types
        public struct ReplicateData : IReplicateData
        {
            public bool Brake;
            public float Horizontal;
            public float Vertical;

            public ReplicateData(bool brake, float horizontal, float vertical) : this()
            {
                Brake = brake;
                Horizontal = horizontal;
                Vertical = vertical;
            }

            private uint _tick;

            public void Dispose() { }
            public readonly uint GetTick() => _tick;
            public void SetTick(uint value) => _tick = value;
        }

        public struct ReconcileData : IReconcileData
        {
            public readonly Vector3 Position;
            public readonly Quaternion Rotation;
            public readonly Vector3 LinearVelocity;
            public readonly Vector3 AngularVelocity;

            public ReconcileData(Vector3 position, Quaternion rotation, Vector3 linearVelocity, Vector3 angularVelocity) : this()
            {
                _tick = 0;

                Position = position;
                Rotation = rotation;
                LinearVelocity = linearVelocity;
                AngularVelocity = angularVelocity;
            }

            private uint _tick;
            public void Dispose() { }
            public uint GetTick() => _tick;
            public void SetTick(uint value) => _tick = value;
        }

        //public struct ReconcileData : IReconcileData
        //{
        //    [Header("Wheel collider")]
        //    public readonly float BrakeTorque;
        //    public readonly float MotorTorque;
        //    public readonly float SteerAngle;

        //    [Header("Rigidbody")]
        //    public readonly Vector3 Position;
        //    public readonly Quaternion Rotation;
        //    public readonly Vector3 LinearVelocity;
        //    public readonly Vector3 AngularVelocity;

        //    public ReconcileData(float brakeTorque, float motorTorque, float steerAngle, 
        //        Vector3 position, Quaternion rotation, Vector3 linearVelocity, Vector3 angularVelocity) : this()
        //    {
        //        _tick = 0;

        //        BrakeTorque = brakeTorque;
        //        MotorTorque = motorTorque;
        //        SteerAngle = steerAngle;

        //        Position = position;
        //        Rotation = rotation;
        //        LinearVelocity = linearVelocity;
        //        AngularVelocity = angularVelocity;
        //    }

        //    private uint _tick;
        //    public void Dispose() { }
        //    public uint GetTick() => _tick;
        //    public void SetTick(uint value) => _tick = value;
        //}
        #endregion

        #region Network Events

        #region Network
        public override void OnStartNetwork()
        {
            _wheelColliders = GetComponentsInChildren<WheelCollider>();
            
            m_rigidbody = GetComponent<Rigidbody>();

            _playerCamera = Camera.main;

            TimeManager.OnTick += TimeManagerTickEventHandler;
            TimeManager.OnPostTick += TimeManagerPostTickEventHandler;
        }

        public override void OnStopNetwork()
        {
            TimeManager.OnTick -= TimeManagerTickEventHandler;
            TimeManager.OnPostTick -= TimeManagerPostTickEventHandler;
        }
        #endregion

        #region Client
        public override void OnStartClient()
        {
            base.OnStartClient();

            if (base.IsOwner)
            {
                _cameraPosition = _playerCamera.transform.position;
                _cameraRotation = _playerCamera.transform.rotation;

                _playerCamera.transform.SetParent(_cameraTransform);
                _playerCamera.transform.position = _cameraTransform.position;
                _playerCamera.transform.rotation = _cameraTransform.rotation;

                // Find all child gameobjects that have the wheelcontrol script attached
                wheels = GetComponentsInChildren<WheelControl>();

                //Debug.Log(wheels.Length);
            }
            else
            {
                GetComponent<CarControl>().enabled = false;
            }
        }

        public override void OnStopClient()
        {
            base.OnStopClient();

            if (base.IsOwner)
            {
                _playerCamera.transform.SetParent(null);
                _playerCamera.transform.position = _cameraPosition;
                _playerCamera.transform.rotation = _cameraRotation;
            }
        }
        #endregion

        #endregion

        private void TimeManagerTickEventHandler()
        {
            if (IsOwner)
            {
                ReplicateData replicateData = new ReplicateData(_brake, _horizontal, _vertical);

                Replicate(replicateData);
            }
            else
            {
                Replicate(default(ReplicateData));
            }
        }

        private void TimeManagerPostTickEventHandler()
        {
            if (!IsServerStarted)
                return;

            foreach (var wheelCollider in _wheelColliders)
            {
                CreateReconcile();
            }
        }

        [Replicate]
        private void Replicate(ReplicateData replicateData, 
            ReplicateState replicateState = ReplicateState.Invalid, 
            Channel channel = Channel.Unreliable)
        {
            //m_rigidbody.linearVelocity += Vector3.forward * Time.deltaTime;

            float hInput = replicateData.Horizontal;
            float vInput = replicateData.Vertical;

            // Calculate current speed in relation to the forward direction of the car
            // (this returns a negative number when traveling backwards)
            float forwardSpeed = Vector3.Dot(transform.forward, m_rigidbody.linearVelocity);

            // Calculate how close the car is to top speed
            // as a number from zero to one
            float speedFactor = Mathf.InverseLerp(0, maxSpeed, forwardSpeed);
            //print($"speedFactor : {speedFactor * 100});

            // Use that to calculate how much torque is available 
            // (zero torque at top speed)
            float currentMotorTorque = 0;

            if (vInput != 0)
            {
                currentMotorTorque = motorTorqueCurve.Evaluate(speedFactor) * 1000;
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
                wheel.CheckForSliping(Vector3.Dot(transform.forward, m_rigidbody.GetPointVelocity(wheel.transform.position)), slidingDifferenceTreshold);
                //print(m_rigidBody.GetPointVelocity(wheel.transform.position).magnitude);

                // Apply steering to Wheel colliders that have "Steerable" enabled
                if (wheel.steerable)
                {
                    wheel.wheelCollider.steerAngle = hInput * currentSteerRange;
                }

                if (replicateData.Brake)
                {
                    wheel.wheelCollider.brakeTorque = Mathf.Abs(vInput) * brakeTorque;
                    wheel.wheelCollider.motorTorque = 0;
                }
                else
                {
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

        public override void CreateReconcile()
        {
            ReconcileData reconcileData = new ReconcileData(
                    m_rigidbody.position,
                    m_rigidbody.rotation,
                    m_rigidbody.linearVelocity,
                    m_rigidbody.angularVelocity
                    );

            Reconcile(reconcileData);
        }

        [Reconcile]
        private void Reconcile(ReconcileData reconcileData, Channel channel = Channel.Unreliable)
        {
            //WheelCollider wheelCollider = _wheelColliders[_currentWheelIndex];

            //wheelCollider.brakeTorque = reconcileData.BrakeTorque;
            //wheelCollider.motorTorque = reconcileData.MotorTorque;
            //wheelCollider.steerAngle = reconcileData.SteerAngle;

            m_rigidbody.Move(reconcileData.Position, reconcileData.Rotation);
            m_rigidbody.linearVelocity = reconcileData.LinearVelocity;
            m_rigidbody.angularVelocity = reconcileData.AngularVelocity;
        }


        //void SetupCenterOfMass()
        //{
        //    // Adjust center of mass vertically, to help prevent the car from rolling
        //    m_rigidbody.centerOfMass = centreOfGravity.transform.position;
        //}

        private void Update()
        {
            if (!base.IsOwner) 
                return;

            _brake = _actions.FindAction("Brake").IsPressed();

            _horizontal = _actions.FindAction("Side Move").ReadValue<float>();
            _vertical = _actions.FindAction("Gas").ReadValue<float>();
        }
    }
}
