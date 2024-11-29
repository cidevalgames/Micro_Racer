//using FishNet.Object;
//using FishNet.Object.Prediction;
//using FishNet.Transporting;
//using GameKit.Dependencies.Utilities;
//using System;
//using UnityEngine;
//using UnityEngine.InputSystem;

//public class CarPrediction : NetworkBehaviour
//{
//        // How much force to add to the Rigidbody for brake.
//    [SerializeField]
//    private float _brakeForce = 8f;

//    // How much force to add to the Rigidbody for normal movements.
//    [SerializeField]
//    private float _moveForce = 15f;

//    /* PredictionRigidbody is set within OnStart / StopNetwork to use our
//     * caching system. You could simply initialize a new instance in the field
//     * but for increased performance using the cache is demonstrated */
//    public PredictionRigidbody m_PredictionRigidbody;

//    private InputActionAsset m_actions;

//    // True if to brake next replicate.
//    private bool _brake;

//    private void Awake()
//    {
//        m_PredictionRigidbody = ObjectCaches<PredictionRigidbody>.Retrieve();
//        m_PredictionRigidbody.Initialize(GetComponent<Rigidbody>());

//        m_actions = InputSystem.actions;
//    }

//    private void OnDestroy()
//    {
//        ObjectCaches<PredictionRigidbody>.StoreAndDefault(ref m_PredictionRigidbody);
//    }
    
//    public override void OnStartNetwork()
//    {
//        base.TimeManager.OnTick += TimeManager_OnTick;
//        base.TimeManager.OnPostTick += TimeManager_OnPostTick;
//    }

//    public override void OnStopNetwork()
//    {
//        base.TimeManager.OnTick -= TimeManager_OnTick;
//        base.TimeManager.OnPostTick -= TimeManager_OnPostTick;
//    }

//    private void Update()
//    {
//        if (base.IsOwner)
//        {
//            if (m_actions.FindAction("Brake").WasPerformedThisFrame())
//                _brake = true;
//        }
//    }

//    private void TimeManager_OnTick()
//    {
//        RunInputs(CreateReplicateData());
//    }

//    private ReplicateData CreateReplicateData()
//    {
//        if (!base.IsOwner)
//            return default;

//        // Build the replicate data with all inputs which affect the prediction.
//        float horizontal = m_actions.FindAction("Side Move").ReadValue<float>();
//        float vertical = m_actions.FindAction("Gas").ReadValue<float>();

//        ReplicateData md = new ReplicateData(_brake, horizontal, vertical);

//        _brake = false;

//        return md;
//    }

//    [Replicate]
//    private void RunInputs(ReplicateData data, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable)
//    {
//        Debug.Log(m_PredictionRigidbody.Rigidbody.linearVelocity);

//        /* ReplicateState is set based on if the data is new, being replayed, etc.
//         * Visit the ReplicationState enum for more information on what each value
//         * indicates. At the end of this guide a more advanced use of state will
//         * be demonstrated. */

//        /* Be sure to always apply and set velocities using PredictionRigidbody
//         * and never on the Rigidbody itself; this includes if also accessing from
//         * another script. */
//        Vector3 forces = new Vector3(data.Horizontal, 0f, data.Vertical) * _moveForce;
//        m_PredictionRigidbody.AddForce(forces);

//        if (data.Brake)
//        {
//            //wheel.wheelCollider.brakeTorque = Mathf.Abs(vInput) * brakeTorque;
//            //wheel.wheelCollider.motorTorque = 0;
//        }

//        /* Add gravity to make the object fall faster. This is of course
//         * entirely optionnal. */
//        m_PredictionRigidbody.AddForce(Physics.gravity * 3f);

//        /* Simulate the added forces.
//         * Typically you call this at the end of your replicate. Calling
//         * Simulate is ultimately telling the PredictionRigidbody to iterate
//         * the forces we added above. */
//        m_PredictionRigidbody.Simulate();
//    }

//    private void TimeManager_OnPostTick()
//    {
//        CreateReconcile();
//    }

//    // Create the reconcile data here and call your reconcile method.
//    public override void CreateReconcile()
//    {
//        /* We must send back the state of the Rigidbody. Using your
//         * PredictionRigidbody field in the reconcile data is an easy
//         * way to accomplish this. More advanced states may require other
//         * values to be sent; this will be covered later on. */
//        ReconcileData rd = new ReconcileData(m_PredictionRigidbody);

//        /* Like with the replicate you could specify a channel here, though
//         * it's unlikely you ever would with a reconcile. */
//        ReconcileState(rd);
//    }

//    [Reconcile]
//    private void ReconcileState(ReconcileData data, Channel channel = Channel.Unreliable)
//    {
//        /* Call reconcile on your PredictionRigidbody field passing in
//         * values from data. */
//        m_PredictionRigidbody.Reconcile(data.PredictionRigidbody);
//    }
//}
