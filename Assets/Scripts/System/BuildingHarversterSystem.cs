using Unity.Burst;
using Unity.Entities;

partial struct BuildingHarversterSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {

    }

   
    public void OnUpdate(ref SystemState state)
    {
        foreach (RefRW<BuildingHarverster> buildingHarverster in SystemAPI.Query<RefRW<BuildingHarverster>>())
        {
            buildingHarverster.ValueRW.m_HarversterTimer -= SystemAPI.Time.DeltaTime;
            if (buildingHarverster.ValueRO.m_HarversterTimer < 0)
            {
                buildingHarverster.ValueRW.m_HarversterTimer = buildingHarverster.ValueRO.m_HarversterMaxTimer;

                ResourceManager.Instance.AddResourceAmount(buildingHarverster.ValueRO.m_ResourceType,1);
            }
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }
}
