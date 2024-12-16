using System.Collections;
using UnityEngine;

namespace Car.Multiplayer.Common.Spawn
{
    public class SpawnPoints : MonoBehaviour
    {
        public static SpawnPoints Instance;

        public SpawnPoint[] hunterSpawnPoints;
        public SpawnPoint[] bunnySpawnPoints;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }
    }
}