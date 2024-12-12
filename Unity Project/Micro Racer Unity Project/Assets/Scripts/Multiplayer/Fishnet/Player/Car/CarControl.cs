using System;
using System.Linq;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using Unity.Cinemachine;
using Unity.Mathematics;
using Unity.VisualScripting;
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

        [SerializeField] private Transform visual;

        private Rigidbody m_rigidbody;
        [Range(0, 1.001f)]
        public float slidingDifferenceTreshold;

        private Camera _playerCamera;

        [Header("Wheels")]
        [SerializeField] private Wheel[] wheels;

        private Vector3 _cameraPosition;
        private Quaternion _cameraRotation;

        // Actions
        private InputAction _sideMoveAction;
        private InputAction _gasAction;
        private InputAction _brakeAction;

        // Input values
        private bool _brake = false;
        private float _horizontal;
        private float _vertical;

        #region Types

        public struct ReplicateData : IReplicateData
        {
            public float Horizontal;
            public float Vertical;
            public bool Brake;

            public ReplicateData(float horizontal, float vertical, bool brake) : this()
            {
                Horizontal = horizontal;
                Vertical = vertical;
                Brake = brake;
            }

            private uint _tick;

            public void Dispose() { }
            public readonly uint GetTick() => _tick;
            public void SetTick(uint value) => _tick = value;
        }

        public struct ReconcileData : IReconcileData
        {
            // Rigidbody
            public readonly Vector3 Position;
            public readonly Quaternion Rotation;
            public readonly Vector3 LinearVelocity;
            public readonly Vector3 AngularVelocity;

            // Steer angles
            public readonly float FlSteerAngle;
            public readonly float FrSteerAngle;

            // Motor torques
            public readonly float FlMotorTorque;
            public readonly float FrMotorTorque;

            // Brake torques
            public readonly float FlBrakeTorque;
            public readonly float FrBrakeTorque;
            public readonly float BlBrakeTorque;
            public readonly float BrBrakeTorque;

            public ReconcileData(Vector3 position, Quaternion rotation, Vector3 linearVelocity, Vector3 angularVelocity,
                float flSteerAngle, float frSteerAngle,
                float flMotorTorque, float frMotorTorque,
                float flBrakeTorque, float frBrakeTorque, float blBrakeTorque, float brBrakeTorque) : this()
            {
                _tick = 0;

                // Rigidbody
                Position = position;
                Rotation = rotation;
                LinearVelocity = linearVelocity;
                AngularVelocity = angularVelocity;

                // Steer angles
                FlSteerAngle = flSteerAngle;
                FrSteerAngle = frSteerAngle;

                // Motor torques
                FlMotorTorque = flMotorTorque;
                FrMotorTorque = frMotorTorque;

                // Brake torques
                FlBrakeTorque = flBrakeTorque;
                FrBrakeTorque = frBrakeTorque;
                BlBrakeTorque = blBrakeTorque;
                BrBrakeTorque = brBrakeTorque;
            }

            private uint _tick;
            public void Dispose() { }
            public uint GetTick() => _tick;
            public void SetTick(uint value) => _tick = value;
        }

        #endregion

        #region Network Events

        #region Network

        public override void OnStartNetwork()
        {
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
                // Assign the car to the camera follow

                var cameraFollow = GameObject.FindGameObjectWithTag("PlayerFollowCamera").GetComponent<CinemachineCamera>();

                CameraTarget newTarget = new CameraTarget();
                newTarget.TrackingTarget = visual;
                newTarget.LookAtTarget = visual;
                newTarget.CustomLookAtTarget = true;

                cameraFollow.Target = newTarget;

                // Get all actions

                InputActionMap playerActionMap = InputSystem.actions.FindActionMap("Player", true);

                _sideMoveAction = playerActionMap.FindAction("Side Move", true);
                _gasAction = playerActionMap.FindAction("Gas", true);
                _brakeAction = playerActionMap.FindAction("Brake", true);
            }
        }

        public override void OnStopClient()
        {
            base.OnStopClient();

            if (base.IsOwner)
            {
                var cameraFollow = GameObject.FindGameObjectWithTag("PlayerFollowCamera").GetComponent<CinemachineCamera>();

                cameraFollow.Target.TrackingTarget = null;
                cameraFollow.Target.LookAtTarget = null;

                _playerCamera.transform.position = _cameraPosition;
                _playerCamera.transform.rotation = _cameraRotation;
            }
        }

        #endregion

        #endregion

        private void TimeManagerTickEventHandler()
        {
            if (base.IsOwner)
            {
                //Reconcile(default);

                ReplicateData replicateData = new ReplicateData(_horizontal, _vertical, _brake);

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

            CreateReconcile();
        }

        [Replicate]
        private void Replicate(ReplicateData rd, 
            ReplicateState replicateState = ReplicateState.Invalid, 
            Channel channel = Channel.Unreliable)
        {
            //m_rigidbody.linearVelocity += Vector3.forward * Time.deltaTime;

            float hInput = rd.Horizontal;
            float vInput = rd.Vertical;

            //Debug.Log($"Horizontal: {hInput}, Vertical: {vInput}");

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

            CheckForSlipping();

            foreach (var w in wheels)
            {
                // Check for Wheelspin
                //print(m_rigidBody.GetPointVelocity(wheel.transform.position).magnitude);

                // Apply steering to Wheel colliders that have "Steerable" enabled
                if (w.steerable)
                {
                    w.wheelCollider.steerAngle = hInput * currentSteerRange;

                    Debug.Log($"{w.mesh.name} Steer angle: {w.wheelCollider.steerAngle}");
                }

                if (rd.Brake)
                {
                    w.wheelCollider.brakeTorque = Mathf.Abs(vInput) * brakeTorque;
                    w.wheelCollider.motorTorque = 0;
                }
                else
                {
                    if (isAccelerating)
                    {
                        // Apply torque to Wheel colliders that have "Motorized" enabled
                        if (w.motorized)
                        {
                            w.wheelCollider.motorTorque = vInput * currentMotorTorque;
                        }

                        w.wheelCollider.brakeTorque = 0;
                    }
                    else
                    {
                        // If the user is trying to go in the opposite direction
                        // apply brakes to all wheels
                        w.wheelCollider.brakeTorque = Mathf.Abs(vInput) * brakeTorque;
                        w.wheelCollider.motorTorque = 0;
                    }
                }
            }

            HandleWheelTransform();
        }

        public override void CreateReconcile()
        {
            ReconcileData rd = new ReconcileData(
                    m_rigidbody.position,
                    m_rigidbody.rotation,
                    m_rigidbody.linearVelocity,
                    m_rigidbody.angularVelocity,

                    wheels[0].wheelCollider.steerAngle, wheels[1].wheelCollider.steerAngle,
                    wheels[0].wheelCollider.motorTorque, wheels[1].wheelCollider.motorTorque,
                    wheels[0].wheelCollider.brakeTorque, wheels[1].wheelCollider.brakeTorque, wheels[2].wheelCollider.brakeTorque, wheels[3].wheelCollider.brakeTorque
                    );

            Reconcile(rd);
        }

        [Reconcile]
        private void Reconcile(ReconcileData rd, Channel channel = Channel.Unreliable)
        {
            // Set Rigidbody
            transform.position = rd.Position;
            transform.rotation = rd.Rotation;
            m_rigidbody.linearVelocity = rd.LinearVelocity;
            m_rigidbody.angularVelocity = rd.AngularVelocity;

            // Set WheelColliders
            wheels[0].wheelCollider.steerAngle = rd.FlSteerAngle;
            wheels[1].wheelCollider.steerAngle = rd.FrSteerAngle;
            wheels[0].wheelCollider.motorTorque = rd.FlMotorTorque;
            wheels[1].wheelCollider.motorTorque = rd.FrMotorTorque;
            wheels[0].wheelCollider.brakeTorque = rd.FlBrakeTorque;
            wheels[1].wheelCollider.brakeTorque = rd.FrBrakeTorque;
            wheels[2].wheelCollider.brakeTorque = rd.BlBrakeTorque;
            wheels[3].wheelCollider.brakeTorque = rd.BrBrakeTorque;
        }

        private void HandleWheelTransform()
        {
            foreach (Wheel w in wheels)
            {
                Vector3 pos = w.mesh.position;
                Quaternion rot = w.mesh.rotation;

                w.wheelCollider.GetWorldPose(out pos, out rot);

                //pos = w.wheelCollider.transform.position;
                pos = pos - w.wheelCollider.transform.parent.position + w.mesh.parent.position;

                w.mesh.position = pos;
                w.mesh.rotation = rot;
            }
        }

        private void CheckForSlipping()
        {
            float x = slidingDifferenceTreshold;

            foreach (Wheel w in wheels)
            {
                WheelCollider wheelCollider = w.wheelCollider;

                float carVelocity = Vector3.Dot(transform.forward, m_rigidbody.GetPointVelocity(wheelCollider.transform.position));

                float distanceTraveledByTheWheel = math.abs((wheelCollider.radius * 2 * math.PI) * (wheelCollider.rpm / 60));

                bool isSliding = distanceTraveledByTheWheel > carVelocity + x || distanceTraveledByTheWheel < carVelocity - x;

                if (isSliding)
                {
                    w.trailRenderer.emitting = true;
                }
                else
                {
                    w.trailRenderer.emitting = false;
                }
            }            
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

            _brake = _brakeAction.IsPressed();

            _horizontal = _sideMoveAction.ReadValue<float>();
            _vertical = _gasAction.ReadValue<float>();
        }
    }
}
