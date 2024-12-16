using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using UnityEngine;

namespace Multiplayer.Fishnet.Player.Capsule
{
    public class PlayerHealth : NetworkBehaviour
    {
        [AllowMutableSyncType]
        private SyncVar<int> _health = new SyncVar<int>();

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (!IsOwner)
                GetComponent<PlayerHealth>().enabled = false;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
                UpdateHealth(this, -1);
        }

        [ServerRpc]
        public void UpdateHealth(PlayerHealth script, int amountToChange)
        {
            script._health.Value += amountToChange;

            Debug.Log($"Player {base.Owner.ClientId}'s health value is {script._health.Value}");
        }
    }
}