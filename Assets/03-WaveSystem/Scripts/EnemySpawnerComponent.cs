using Unity.Entities;
using Unity.Mathematics;

public struct EnemySpawnerComponent : IComponentData
{
    public Entity prefabToSpawn;

    public float3 spawnerPosition;

    public float spawnRate;

    public float timer;
}
