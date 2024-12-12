using FishNet;
using FishNet.Broadcast;
using FishNet.Connection;
using FishNet.Transporting;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Multiplayer.Fishnet.Cube
{
    public class CubePositionBroadcast : MonoBehaviour
    {
        [SerializeField] private List<Transform> cubePositions = new();
        
        private int _transformIndex;

        private void OnEnable()
        {
            InstanceFinder.ClientManager.RegisterBroadcast<PositionIndex>(OnPositionBroadcast);
            InstanceFinder.ServerManager.RegisterBroadcast<PositionIndex>(OnClientPositionBroadcast);
        }

        private void OnDisable()
        {
            InstanceFinder.ClientManager.UnregisterBroadcast<PositionIndex>(OnPositionBroadcast);
            InstanceFinder.ServerManager.UnregisterBroadcast<PositionIndex>(OnClientPositionBroadcast);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                int nextIndex = _transformIndex + 1;

                if (nextIndex >= cubePositions.Count)
                    nextIndex = 0;

                if (InstanceFinder.IsServerStarted)
                {
                    InstanceFinder.ServerManager.Broadcast(new PositionIndex() { tIndex = nextIndex });
                }
                else if (InstanceFinder.IsClientStarted)
                {
                    InstanceFinder.ClientManager.Broadcast(new PositionIndex() { tIndex = nextIndex });
                }
            }

            transform.position = cubePositions[_transformIndex].position;
        }

        private void OnPositionBroadcast(PositionIndex indexStruct, Channel channel)
        {
            _transformIndex = indexStruct.tIndex;
        }

        private void OnClientPositionBroadcast(NetworkConnection networkConnection, PositionIndex indexStruct, Channel channel)
        {
            InstanceFinder.ServerManager.Broadcast(indexStruct);
        }

        public struct PositionIndex : IBroadcast
        {
            public int tIndex;
        }
    }
}
