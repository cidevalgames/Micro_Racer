using Unity.Entities;
using UnityEngine;

namespace Networking.Netcode.Entities.Car
{
    public struct Car : IComponentData
    {

    }

    [DisallowMultipleComponent]
    public class CarAuthoring : MonoBehaviour
    {
        class Baker : Baker<CarAuthoring>
        {
            public override void Bake(CarAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<Car>(entity);
            }
        }
    }
}
