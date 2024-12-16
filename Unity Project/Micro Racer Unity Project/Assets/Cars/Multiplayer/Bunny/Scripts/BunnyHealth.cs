using FishNet.Object;
using Car.Multiplayer.Common;
using UnityEngine;

namespace Car.Multiplayer.Bunny
{
    public class BunnyHealth : CarHealth
    {
        [ObserversRpc]
        public override void UpdateHealthBar(CarHealth script)
        {
            base.UpdateHealthBar(script);
        }
    }
}
