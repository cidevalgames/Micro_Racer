using System;
using System.Collections;
using UnityEngine;

namespace Car.Multiplayer.Common.Spawn
{
    [Serializable]
    public class SpawnPoint
    {
        public Transform transform;
        public bool wasUsed = false;

        public SpawnPoint()
        {
            this.transform = null;
            this.wasUsed = false;
        }

        public SpawnPoint(Transform transform, bool wasUsed)
        {
            this.transform = transform;
            this.wasUsed = wasUsed;
        }
    }
}