using Assets.Scripts;
using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Car.Multiplayer.Common
{
    public abstract class CarHealth : NetworkBehaviour
    {
        [Header("Collisions")]
        [SerializeField, Min(500)] private float impactThreshold = 500f;
        [SerializeField] private float impactMaxForce = 150000f;
        [SerializeField] private LayerMask collidableLayers;

        [Header("Health")]
        [SerializeField] private bool setMaxHealth = true;
        
        [AllowMutableSyncType]
        private SyncVar<float> _health = new SyncVar<float>();

        [SerializeField] private int maxHealth = 100;

        [Header("UI")]
        [SerializeField] private Slider healthBar;
        [SerializeField] private TextMeshProUGUI healthPercentage;

        [Header("DEBUG")]
        [SerializeField] private bool printHealth = false;

        private Rigidbody m_rigidbody;

        #region Primary Functions

        private void Awake()
        {
            m_rigidbody = GetComponent<Rigidbody>();

            if (impactThreshold == 0)
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

        public void NetworkCollisionEnter(Collider other)
        {
            if (collidableLayers.ContainsMask(other.gameObject))
            {
                //Debug.Log($"Rigidbody velocity: {GetComponent<Rigidbody>().linearVelocity.magnitude}");

                float impactForce = GetImpactForce(GetComponent<CarHealth>().GetComponent<Rigidbody>());

                if (impactForce > impactThreshold)
                {
                    float value = Mathf.InverseLerp(this.impactThreshold, this.impactMaxForce, impactForce);

                    UpdateHealth(this, -(maxHealth * value));
                }
            }
        }

        [ServerRpc]
        private void UpdateHealth(CarHealth script, float amountToChange)
        {
            script._health.Value += amountToChange;

            if (printHealth)
                Debug.Log($"Player {base.Owner.ClientId}'s health value is {script._health.Value}");

            script.UpdateHealthBar(script);
        }

        public virtual void UpdateHealthBar(CarHealth script)
        {
            float amount = Mathf.InverseLerp(0, maxHealth, script._health.Value);
            amount = Mathf.Round(amount * 1000.0f) * 0.001f;

            script.healthBar.value = amount;

            script.healthPercentage.text = $"{(amount * 100).ToString("00.0")} %";
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