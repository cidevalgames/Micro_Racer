using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Networking.Netcode.Entities.Car
{

    public struct CarInput : IInputComponentData
    {
        public float2 Movement;
        public float2 Look;
        public InputEvent Brake;
    }

    [DisallowMultipleComponent]
    public class CarInputAuthoring : MonoBehaviour
    {
        class CarInputBaking : Baker<CarInputAuthoring>
        {
            public override void Bake(CarInputAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<CarInput>(entity);
            }
        }
    }

    [UpdateInGroup(typeof(GhostInputSystemGroup))]
    public partial struct SampleCarInput : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkStreamInGame>();
            state.RequireForUpdate<CarSpawner>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var playerInput in SystemAPI.Query<RefRW<CarInput>>().WithAll<GhostOwnerIsLocal>())
            {
                playerInput.ValueRW = default;

                Vector2 moveValue = InputSystem.actions.FindAction("Move").ReadValue<Vector2>();

                playerInput.ValueRW.Movement = moveValue;

                //if (Input.GetKey("left"))
                //    playerInput.ValueRW.Horizontal -= 1;
                //if (Input.GetKey("right"))
                //    playerInput.ValueRW.Horizontal += 1;
                //if (Input.GetKey("down"))
                //    playerInput.ValueRW.Vertical -= 1;
                //if (Input.GetKey("up"))
                //    playerInput.ValueRW.Vertical += 1;
            }
        }
    }
}
