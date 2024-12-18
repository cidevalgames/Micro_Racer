using Car.Multiplayer.Common;
using System.Collections;
using UnityEngine;

namespace Traps
{
    public class OilTrapController : Trap
    {
        [SerializeField] float timeOiled;

        [Header("Timers")]
        [SerializeField] float activeTime;
        [SerializeField] float timeUntilDeath;

        private float cooldown = 0f;

        public override void Start()
        {
            base.Start();

            timeUntilDeath = activeTime;
        }

        public override void Update()
        {
            base.Update();

            if (cooldown < timeUntilDeath)
            {
                cooldown += Time.deltaTime;
            }
            else
            {
                if (cooldown > timeUntilDeath + 2)
                    Destroy(this);

                isEnabled = false;
                transform.position += Vector3.down * Time.deltaTime;
            }
        }

        public override void TriggerEnter(Collider other)
        {
            CarControl target = other.GetComponentInParent<CarControl>();

            StartCoroutine(OnTriggerCoroutine(target));
        }

        private IEnumerator OnTriggerCoroutine(CarControl target)
        {
            target.oiled = true;

            yield return new WaitForSeconds(timeOiled);

            target.oiled = false;

            isTriggered = false;
        }
    }
}
