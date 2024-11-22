using Unity.Entities;
using UnityEngine;

namespace Networking.Netcode.Entities.Ghost
{
    public struct Cube : IComponentData
    {

    }

    [DisallowMultipleComponent]
    public class CubeAuthoring : MonoBehaviour
    {
        class Baker : Baker<CubeAuthoring>
        {
            public override void Bake(CubeAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<Cube>(entity);
            }
        }
    }
}
