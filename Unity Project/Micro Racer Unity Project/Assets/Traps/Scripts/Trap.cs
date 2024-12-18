using System.Collections;
using UnityEngine;
using Car.Multiplayer.Common;

namespace Traps
{
    public abstract class Trap : MonoBehaviour
    {
        [SerializeField] private protected bool isEnabled = true;

        private protected bool isTriggered;

        public virtual void Start()
        {
            isTriggered = false;
        }

        public virtual void Update()
        {
            if (!isEnabled)
                return;
        }

        public virtual void OnTriggerEnter(Collider other)
        {
            if (!isEnabled)
                return;

            // Checks if the entered collider is from a player
            bool isPlayer = (other.CompareTag("Hunter") || other.CompareTag("Bunny"));

            if (!isPlayer)
                return;

            if (isTriggered)
                return;

            isTriggered = true;

            Debug.Log($"{other.name} has entered trap {this.name}.");

            TriggerEnter(other);

            //Debug.Log("Heuuuuu......... j'espère que c'était pas la voiture parce que là je viens de la laisser passer");
        }

        public virtual void TriggerEnter(Collider other) { }

        //public virtual IEnumerator OnTriggerCoroutine(CarControl target = null)
        //{
            

        //    yield return null;
        //}
    }
}