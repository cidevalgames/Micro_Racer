using System.Collections;
using Car.Multiplayer.Common;
using UnityEngine;

namespace Traps
{
    public class MineTrapController : Trap
    {
        [SerializeField] float strength = 15000f;
    
        [Header("Timers")]
        [SerializeField] float fuseTime;
        [SerializeField] float timeBeforeAttack;
    
        [Header("Refs")]
        [SerializeField] PlayerLife playerLifeAffected;
        [SerializeField] ParticleSystem particles;

        public override void Start()
        {
            base.Start();

            if (particles == null)
                particles = gameObject.GetComponentInChildren<ParticleSystem>();
        }

        public override void OnTriggerStay(Collider other)
        {
            base.OnTriggerStay(other);

            playerLifeAffected = other.GetComponent<PlayerLife>();

            StartCoroutine(OnTriggerCoroutine());
        }

        public override IEnumerator OnTriggerCoroutine(CarControl target = null)
        {
            yield return new WaitForSeconds(fuseTime);

            Physics.Raycast(transform.position, gameObject.transform.up, out RaycastHit hit, 0.3f);
            particles.Play();

            playerLifeAffected.MineAttack(strength, hit.point);

            yield return new WaitForSeconds(timeBeforeAttack);

            base.OnTriggerCoroutine(target);
        }
    }
}
