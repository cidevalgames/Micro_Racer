using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Networking.Netcode.Entities.Car
{
    public partial class CarInputSystem : SystemBase
    {
        InputActionAsset actions;

        protected override void OnCreate()
        {
            actions = InputSystem.actions;

            RequireForUpdate<CarInput>();
            RequireForUpdate<NetworkStreamInGame>();
        }

        protected override void OnUpdate()
        {
            InputSystem.Update();

            Vector2 movement = actions.FindAction("Move").ReadValue<Vector2>();
            Vector2 look = actions.FindAction("Look").ReadValue<Vector2>();

            bool brake = actions.FindAction("Brake").WasPerformedThisFrame();

            foreach (var carInput in SystemAPI.Query<RefRW<CarInput>>()
                .WithAll<GhostOwnerIsLocal>())
            {
                carInput.ValueRW.Movement = movement;
                carInput.ValueRW.Look = look;

                if (brake) carInput.ValueRW.Brake.Set();
            }
        }
    }
}
