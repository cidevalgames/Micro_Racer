using FishNet.Object;
using Car.Multiplayer.Common;
using UnityEngine;

namespace Car.Multiplayer.Hunter
{
    public class HunterHealth : CarHealth
    {
        [ObserversRpc]
        public override void UpdateHealthBar(CarHealth script)
        {
            base.UpdateHealthBar(script);
        }
    }
}
