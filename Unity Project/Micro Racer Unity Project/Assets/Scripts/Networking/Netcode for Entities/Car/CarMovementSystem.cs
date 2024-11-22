using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

namespace Networking.Netcode.Entities.Car
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [BurstCompile]
    public partial struct CarMovementSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var speed = SystemAPI.Time.DeltaTime * 4;
            foreach (var (input, trans) in SystemAPI.Query<RefRO<CarInput>, RefRW<LocalTransform>>().WithAll<Simulate>())
            {
                var moveInput = new float2(input.ValueRO.Horizontal, input.ValueRO.Vertical);
                moveInput = math.normalizesafe(moveInput) * speed;
                trans.ValueRW.Position += new float3(moveInput.x, 0, moveInput.y);
            }
        }
    }
}
