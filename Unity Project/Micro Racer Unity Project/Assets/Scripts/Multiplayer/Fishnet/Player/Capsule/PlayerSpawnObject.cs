using FishNet.Object;
using UnityEngine;

namespace Multiplayer.Fishnet.Player.Capsule
{
    public class PlayerSpawnObject : NetworkBehaviour
    {
        [SerializeField] private GameObject objectToSpawn;

        [HideInInspector]
        public GameObject spawnedObject;

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (base.IsOwner)
            {

            }
            else
            {
                this.enabled = false;
            }
        }

        private void Update()
        {
            if (!IsOwner) return;

            if (!spawnedObject && Input.GetKeyDown(KeyCode.Keypad1))
            {
                SpawnObjectServer(this, transform, objectToSpawn);
            }

            if (spawnedObject && Input.GetKeyDown(KeyCode.Keypad2))
            {
                DespawnObjectServer(spawnedObject);
            }
        }

        [ServerRpc]
        public void SpawnObjectServer(PlayerSpawnObject script, Transform playerTransform, GameObject objectToSpawn)
        {
            Debug.Log($"Spawning an object from {script.name} player");

            GameObject spawned = Instantiate(objectToSpawn, playerTransform.position + playerTransform.forward * 2 + Vector3.up, Quaternion.identity);
            ServerManager.Spawn(spawned);
            SetSpawnedObject(script, spawned);
        }

        [ObserversRpc]
        public void SetSpawnedObject(PlayerSpawnObject script, GameObject spawnedObject)
        {
            script.spawnedObject = spawnedObject;
        }

        [ServerRpc(RequireOwnership = false)]
        public void DespawnObjectServer(GameObject obj)
        {
            ServerManager.Despawn(obj);
        }
    }
}
