using System;
using FishNet.Component.Prediction;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using FishNet;
using static Multiplayer.Fishnet.Player.Car.CarControl;

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
        [SerializeField] private float antiRoll = 1000f;

        [SerializeField] private Transform visual;

        private Rigidbody m_rigidbody;
        [Range(0, 1.001f)]
        public float slidingDifferenceTreshold;

        private Camera _playerCamera;

        [Header("Wheels")]
        [SerializeField] private Wheel[] wheels;

        private Vector3 _cameraPosition;
        private Quaternion _cameraRotation;

        // Input
        private float horizontalInput;
        private float verticalInput;

        // Actions
        private InputAction _sideMoveAction;
        private InputAction _gasAction;
        private InputAction _brakeAction;

        private MoveData _lastCreatedInput = default;

        [Header("DEBUG")]
        [SerializeField] private bool printInput = false;
        [SerializeField] private bool printSteerAngle = false;
        [SerializeField] private bool printWheelMeshesTransform = false;

        #region Types

        public struct MoveData : IReplicateData
        {
            public float Horizontal;
            public float Vertical;
            public bool Brake;

            public MoveData(float horizontal, float vertical, bool brake) : this()
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
            public readonly float Fm_rigidbodyrakeTorque;
            public readonly float BlBrakeTorque;
            public readonly float Bm_rigidbodyrakeTorque;

            public ReconcileData(Vector3 position, Quaternion rotation, Vector3 linearVelocity, Vector3 angularVelocity,
                float flSteerAngle, float frSteerAngle,
                float flMotorTorque, float frMotorTorque,
                float flBrakeTorque, float fm_rigidbodyrakeTorque, float blBrakeTorque, float bm_rigidbodyrakeTorque) : this()
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
                Fm_rigidbodyrakeTorque = fm_rigidbodyrakeTorque;
                BlBrakeTorque = blBrakeTorque;
                Bm_rigidbodyrakeTorque = bm_rigidbodyrakeTorque;
            }

            private uint _tick;
            public void Dispose() { }
            public uint GetTick() => _tick;
            public void SetTick(uint value) => _tick = value;
        }

        #endregion

        #region Network Events

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

        #region TimeManager Events

        private void TimeManager_OnTick()
        {
            if (base.IsOwner)
            {
                Reconciliation(default);
                MoveData md = CheckInput(printInput);
                Move(md);
            }
            else
            {
                Move(default(MoveData));
            }
        }

        private void TimeManager_OnPostTick()
        {
            if (base.IsServerInitialized)
            {
                CreateReconcile();
            }
        }

        #endregion

        #region Prediction

        [Replicate]
        private void Move(MoveData md,
            ReplicateState state = ReplicateState.Invalid, 
            Channel channel = Channel.Unreliable)
        {
            if (state.IsFuture())
            {
                uint lastCreatedTick = _lastCreatedInput.GetTick();
                uint thisTick = md.GetTick();

                if ((md.GetTick() - lastCreatedTick) <= 2)
                {
                    md.Horizontal = _lastCreatedInput.Horizontal;
                    md.Vertical = _lastCreatedInput.Vertical;
                    md.Brake = _lastCreatedInput.Brake;
                }
            }
            else if (state == ReplicateState.ReplayedCreated)
            {
                _lastCreatedInput.Dispose();
                _lastCreatedInput = md;
            }

            horizontalInput = md.Horizontal;
            verticalInput = md.Vertical;

            HandleWheelTransform(printWheelMeshesTransform);
            AntiRoll();

            // Calculate current speed in relation to the forward direction of the car
            // (this returns a negative number when traveling backwards)
            float forwardSpeed = Vector3.Dot(transform.forward, m_rigidbody.linearVelocity);

            // Calculate how close the car is to top speed
            // as a number from zero to one
            float speedFactor = Mathf.InverseLerp(0, maxSpeed, forwardSpeed);

            // Use that to calculate how much torque is available 
            // (zero torque at top speed)
            float currentMotorTorque = 0;

            if (verticalInput != 0)
            {
                currentMotorTorque = motorTorqueCurve.Evaluate(speedFactor) * 1000;
            }

            ///float currentMotorTorque = Mathf.Lerp(motorTorque, 0, speedFactor);

            // �and to calculate how much to steer 
            // (the car steers more gently at top speed)
            float currentSteerRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, speedFactor);

            // Check whether the user input is in the same direction 
            // as the car's velocity
            bool isAccelerating = Mathf.Sign(verticalInput) == Mathf.Sign(forwardSpeed);

            CheckForSlipping();

            HandleSteering(currentSteerRange, printSteerAngle);

            if (md.Brake)
            {
                ApplyBrakes();
            }
            else
            {
                if (isAccelerating)
                {
                    ApplyMotorTorques(currentMotorTorque);
                }
                else
                {
                    ApplyReverse();
                }
            }
        }

        /// <summary>
        /// Applies brake to all wheel if the user is trying to go in the opposite direction
        /// </summary>
        private void ApplyReverse()
        {
            foreach (Wheel w in wheels)
            {
                w.wheelCollider.brakeTorque = Mathf.Abs(verticalInput) * brakeTorque;
                w.wheelCollider.motorTorque = 0;
            }
        }

        /// <summary>
        /// Avoid the car to roll when turning
        /// </summary>
        private void AntiRoll()
        {
            // Front axle
            ApplyAntiRoll(wheels[0], wheels[1]);
            // Back axle
            ApplyAntiRoll(wheels[2], wheels[3]);
        }

        /// <summary>
        /// Apply anti roll on two wheels
        /// </summary>
        /// <param name="left">Left wheel</param>
        /// <param name="right">Right wheel</param>
        private void ApplyAntiRoll(Wheel leftWheel, Wheel rightWheel)
        {
            // Credits: http://projects.edy.es/trac/edy_vehicle-physics/wiki/TheStabilizem_rigidbodyars

            WheelCollider left = leftWheel.wheelCollider;
            WheelCollider right = rightWheel.wheelCollider;

            WheelHit hit;
            float travelLeft = 1f;
            float travelRight = 1f;

            bool isGroundedLeft = left.GetGroundHit(out hit);
            if (isGroundedLeft)
            {
                travelLeft = (-left.transform.InverseTransformPoint(hit.point).y - left.radius) / left.suspensionDistance;
            }
            bool isGroundedRight = right.GetGroundHit(out hit);
            if (isGroundedRight)
            {
                travelRight = (-right.transform.InverseTransformPoint(hit.point).y - right.radius) / right.suspensionDistance;
            }

            float antirollForce = (travelLeft - travelRight) * antiRoll;

            if (isGroundedLeft)
            {
                m_rigidbody.AddForceAtPosition(left.transform.up * -antirollForce, left.transform.position);
            }
            if (isGroundedRight)
            {
                m_rigidbody.AddForceAtPosition(right.transform.up * antirollForce, right.transform.position);
            }
        }

        /// <summary>
        /// Apply torque on motorized Wheels
        /// </summary>
        /// <param name="currentMotorTorque"></param>
        private void ApplyMotorTorques(float currentMotorTorque)
        {
            foreach (Wheel w  in wheels)
            {
                if (w.motorized)
                {
                    w.wheelCollider.motorTorque = verticalInput * currentMotorTorque;
                }

                w.wheelCollider.brakeTorque = 0;
            }
        }

        private void ApplyBrakes()
        {
            foreach (Wheel w in wheels)
            {
                w.wheelCollider.brakeTorque = Mathf.Abs(verticalInput) * brakeTorque;
                w.wheelCollider.motorTorque = 0;
            }
        }

        /// <summary>
        /// Apply steering to the Wheels that are steerable
        /// </summary>
        /// <param name="currentSteerRange"></param>
        /// <param name="wheel">The targeted Wheel</param>
        private void HandleSteering(float currentSteerRange, bool print = false)
        {
            Wheel[] steerableWheels = new Wheel[2];

            for (int i = 0; i < wheels.Length; i++)
            {
                Wheel w = wheels[i];

                if (w.steerable)
                {
                    w.wheelCollider.steerAngle = horizontalInput * currentSteerRange;

                    steerableWheels[i] = w;
                }
            }

            if (print)
                Debug.Log($"{steerableWheels[0].mesh.name}'s steer angle: {steerableWheels[0].wheelCollider.steerAngle}" +
                    $"\n{steerableWheels[1].mesh.name}'s steer angle: {steerableWheels[1].wheelCollider.steerAngle}");
        }

        public override void CreateReconcile()
        {
            ReconcileData rd = new ReconcileData(transform.position, transform.rotation, m_rigidbody.linearVelocity, m_rigidbody.angularVelocity,
                    wheels[0].wheelCollider.steerAngle, wheels[1].wheelCollider.steerAngle,
                    wheels[0].wheelCollider.motorTorque, wheels[1].wheelCollider.motorTorque,
                    wheels[0].wheelCollider.brakeTorque, wheels[1].wheelCollider.brakeTorque, wheels[2].wheelCollider.brakeTorque, wheels[3].wheelCollider.brakeTorque);

            Reconciliation(rd);
        }

        [Reconcile]
        private void Reconciliation(ReconcileData rd, Channel channel = Channel.Unreliable)
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
            wheels[1].wheelCollider.brakeTorque = rd.Fm_rigidbodyrakeTorque;
            wheels[2].wheelCollider.brakeTorque = rd.BlBrakeTorque;
            wheels[3].wheelCollider.brakeTorque = rd.Bm_rigidbodyrakeTorque;
        }

        #endregion

        private MoveData CheckInput(bool print = false)
        {
            if (!base.IsOwner)
                return default;

            var horizontal = _sideMoveAction.ReadValue<float>();
            var vertical = _gasAction.ReadValue<float>();
            var brake = _brakeAction.IsPressed();

            MoveData md = new MoveData(horizontal, vertical, brake);

            if (print)
                Debug.Log($"Horizontal: {horizontal}, Vertical: {vertical}, Brake: {brake}");

            return md;
        }

        private void HandleWheelTransform(bool print = false)
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

                if (print)
                    Debug.Log($"{w.mesh.name}'s position: {w.mesh.position}" +
                        $"\n{w.mesh.name}'s rotation: {w.mesh.rotation}");
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

        #region Primary Functions

        private void Awake()
        {
            m_rigidbody = GetComponent<Rigidbody>();

            _playerCamera = Camera.main;

            InstanceFinder.TimeManager.OnTick += TimeManager_OnTick;
            InstanceFinder.TimeManager.OnPostTick += TimeManager_OnPostTick;
        }

        private void OnDestroy()
        {
            if (InstanceFinder.TimeManager != null)
            {
                InstanceFinder.TimeManager.OnTick -= TimeManager_OnTick;
                InstanceFinder.TimeManager.OnPostTick -= TimeManager_OnPostTick;
            }
        }

        #endregion
    }
}
