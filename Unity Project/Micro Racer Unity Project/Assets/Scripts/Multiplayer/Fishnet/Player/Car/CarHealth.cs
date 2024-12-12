using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using UnityEngine;

namespace Multiplayer.Fishnet.Player.Car
{
    public class CarHealth : NetworkBehaviour
    {
        [SerializeField, Min(500)] private float impactThreshold = 1600f;
        [SerializeField] private float impactMaxForce = 150000f;

        [SerializeField] private bool setMaxHealth = true;
        
        [AllowMutableSyncType]
        private SyncVar<int> _health = new SyncVar<int>();

        [SerializeField] private int maxHealth = 100;

        private Rigidbody m_rigidbody;

        #region Primary Functions

        private void Awake()
        {
            m_rigidbody = GetComponent<Rigidbody>();

            impactThreshold = m_rigidbody.mass;

            if (setMaxHealth)
                _health.Value = maxHealth;
        }

        private void Update()
        {
            //if (Input.GetKeyDown(KeyCode.R))
            //    UpdateHealth(this, -1);
        }

        #endregion

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (!IsOwner)
                GetComponent<CarHealth>().enabled = false;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.GetType() == typeof(MeshCollider))
            {
                //Debug.Log($"Rigidbody velocity: {GetComponent<Rigidbody>().linearVelocity.magnitude}");

                float impactForce = GetImpactForce(GetComponent<Rigidbody>());

                Debug.Log(impactForce);

                if (impactForce > impactThreshold)
                {
                    float value = Mathf.InverseLerp(impactThreshold, impactMaxForce, impactForce);

                    //Debug.Log(value);

                    UpdateHealth(this, -(int)(maxHealth * value));
                }
            }
        }

        [ServerRpc]
        public void UpdateHealth(CarHealth script, int amountToChange)
        {
            script._health.Value += amountToChange;

            Debug.Log($"Player {base.Owner.ClientId}'s health value is {script._health.Value}");
        }

        #region Calculations

        /// <summary>
        /// Get the impact force in Newton (N) from a given Rigidbody
        /// </summary>
        /// <param name="rigidbody"></param>
        /// <returns></returns>
        private float GetImpactForce(Rigidbody rigidbody)
        {
            float energy = GetKineticEnergy(rigidbody);
            float distanceTraveled = rigidbody.linearVelocity.magnitude;

            return energy / distanceTraveled;
        }

        /// <summary>
        /// Get the kinetic energy in Joules (J) from a given Rigidbody
        /// </summary>
        /// <param name="rigidbody"></param>
        /// <returns></returns>
        private float GetKineticEnergy(Rigidbody rigidbody)
        {
            return 1f/2f * rigidbody.mass * Mathf.Pow(rigidbody.linearVelocity.magnitude, 2);
        }

        #endregion
    }
}