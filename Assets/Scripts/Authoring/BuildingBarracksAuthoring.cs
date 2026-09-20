using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class BuildingBarracksAuthoring : MonoBehaviour
{
    public float m_CreateMaxTimer;


    public class Baker : Baker<BuildingBarracksAuthoring>
    {
        public override void Bake(BuildingBarracksAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new BuildingBarracks
            {
                m_CreateMaxTimer = authoring.m_CreateMaxTimer,
                m_RallyPositionOffset = new float3(10, 0, 0),
            });

            DynamicBuffer<SpawnUnitTypeBuffer> spawnUnitTypeBuffer = AddBuffer<SpawnUnitTypeBuffer>(entity);
            spawnUnitTypeBuffer.Add(new SpawnUnitTypeBuffer
            {
                m_UnitType = UnitTypeSO.UnitType.Soldier,
            });
            spawnUnitTypeBuffer.Add(new SpawnUnitTypeBuffer
            {
                m_UnitType = UnitTypeSO.UnitType.Soldier,
            });
            spawnUnitTypeBuffer.Add(new SpawnUnitTypeBuffer
            {
                m_UnitType = UnitTypeSO.UnitType.Scout,
            });
        }
    }

}

public struct BuildingBarracks : IComponentData
{
    public float m_CreateTimer;
    public float m_CreateMaxTimer;

    public float3 m_RallyPositionOffset;
    public UnitTypeSO.UnitType m_ActiveUnitType;


}
[InternalBufferCapacity(10)]
public struct SpawnUnitTypeBuffer :IBufferElementData
{
    public UnitTypeSO.UnitType m_UnitType;
}