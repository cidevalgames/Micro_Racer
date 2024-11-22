using Unity.Entities;
using UnityEngine;

namespace Networking.Netcode.Entities.Car
{
    public struct CarSpawner : IComponentData
    {
        public Entity Car;
    }

    [DisallowMultipleComponent]
    public class CarSpawnerAuthoring : MonoBehaviour
    {
        public GameObject Car;

        class Baker : Baker<CarSpawnerAuthoring>
        {
            public override void Bake(CarSpawnerAuthoring authoring)
            {
                CarSpawner component = default(CarSpawner);
                component.Car = GetEntity(authoring.Car, TransformUsageFlags.Dynamic);
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, component);
            }
        }
    }
}
