using Car.Multiplayer.Common;
using Car.Multiplayer.Common.Spawn;
using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using UnityEngine;

namespace Network
{
    public class RoleSelection : NetworkBehaviour
    {
        public PlayerRole m_playerRole = PlayerRole.None;

        [Header("Prefabs")]
        [SerializeField] private GameObject hunterPrefab;
        [SerializeField] private GameObject bunnyPrefab;

        #region Client

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (!base.IsOwner)
            {
                GetComponent<RoleSelection>().enabled = false;
                GetComponent<RoleSelection>().gameObject.SetActive(false);
            }
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
        }

        #endregion

        #region Spawn

        [ContextMenu("Spawn player")]
        public void SpawnPlayer()
        {
            switch (m_playerRole)
            {
                case PlayerRole.Hunter:
                    SpawnHunter(); 
                    break;
                case PlayerRole.Bunny:
                    SpawnBunny();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Use this function to spawn a hunter.
        /// </summary>
        public void SpawnHunter()
        {
            SpawnPoint spawnPoint = new SpawnPoint();

            foreach (var sp in SpawnPoints.Instance.hunterSpawnPoints)
            {
                if (!sp.wasUsed)
                    spawnPoint = sp;
            }

            if (!spawnPoint.transform)
                return;

            Spawn(hunterPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation, LocalConnection);
            spawnPoint.wasUsed = true;
        }

        /// <summary>
        /// Use this function to spawn a bunny.
        /// </summary>
        public void SpawnBunny()
        {
            SpawnPoint spawnPoint = new SpawnPoint(null, false);

            foreach (var sp in SpawnPoints.Instance.bunnySpawnPoints)
            {
                if (!sp.wasUsed)
                    spawnPoint = sp;
            }

            if (!spawnPoint.transform)
                return;

            Spawn(bunnyPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation, LocalConnection);
            spawnPoint.wasUsed = true;
        }

        /// <summary>
        /// Call Server to spawn a player.
        /// </summary>
        /// <param name="playerPrefab">The player prefab you want to instantiate.</param>
        /// <param name="position">The spawn point position.</param>
        /// <param name="rotation">The spawn point rotation.</param>
        /// <param name="conn">The network connection (LocalConnection).</param>
        [ServerRpc(RequireOwnership = false)]
        private void Spawn(GameObject playerPrefab, Vector3 position, Quaternion rotation, NetworkConnection conn)
        {
            GameObject player = Instantiate(playerPrefab, position, rotation);
            Spawn(player, conn);
        }

        #endregion
    }
}