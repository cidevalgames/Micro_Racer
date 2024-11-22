using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Unity.Physics.Systems;
using UnityEngine;

namespace Networking.Netcode.Entities.Car
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [BurstCompile]
    public partial struct CarMovementSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CarInput>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var speed = SystemAPI.Time.DeltaTime * 4;
            foreach (var (input, trans) in SystemAPI.Query<RefRO<CarInput>, RefRW<LocalTransform>>().WithAll<Simulate>())
            {
                float2 moveInput = input.ValueRO.Movement;
                moveInput = math.normalizesafe(moveInput) * speed;
                trans.ValueRW.Position += new float3(moveInput.x, 0, moveInput.y);

                if (input.ValueRO.Brake.IsSet)
                {
                    // TODO Implement brake
                }
            }
        }
    }
}
