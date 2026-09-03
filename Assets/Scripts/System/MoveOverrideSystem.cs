using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct MoveOverrideSystem : ISystem
{
    

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach((RefRO<LocalTransform> LocalTransform,RefRO<MoveOverride> moveOverride,RefRW<UnitMover> unitMover,EnabledRefRW<MoveOverride> moveOverrideEnabled) in 
            SystemAPI.Query<RefRO<LocalTransform>,RefRO<MoveOverride>,RefRW<UnitMover>, EnabledRefRW<MoveOverride>>())
        {
            if(math.distancesq(LocalTransform.ValueRO.Position,moveOverride.ValueRO.m_TargetPosition) > UnitMoverSystem.REACHED_TARGET_DISTANCE)
            {
                unitMover.ValueRW.m_TargetPosition = moveOverride.ValueRO.m_TargetPosition;
            }
            else
            {
                moveOverrideEnabled.ValueRW = false;

            }
        }
    }

   
}
