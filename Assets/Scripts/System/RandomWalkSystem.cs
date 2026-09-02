using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct RandomWalkSystem : ISystem
{
   
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach((RefRW<RandomWalk> RandomWalk,RefRW<UnitMover> unitMover,RefRO<LocalTransform> localTransform)in
            SystemAPI.Query<RefRW<RandomWalk>,RefRW<UnitMover>,RefRO<LocalTransform>>())
        {
            if(math.distancesq(localTransform.ValueRO.Position,RandomWalk.ValueRO.m_TargetPosition) < UnitMoverSystem.REACHED_TARGET_DISTANCE)
            {
                Random random = RandomWalk.ValueRO.m_Random;

                float3 randomDirection = new float3(random.NextFloat(-1, 1), 0, random.NextFloat(-1, 1));
                randomDirection = math.normalize(randomDirection);

                RandomWalk.ValueRW.m_TargetPosition = RandomWalk.ValueRO.m_OriginPosition 
                    + randomDirection
                    * random.NextFloat(RandomWalk.ValueRO.m_DistanceMin, RandomWalk.ValueRO.m_DistanceMax);


                RandomWalk.ValueRW.m_Random = random;
            }
            else
            {
                unitMover.ValueRW.m_TargetPosition = RandomWalk.ValueRO.m_TargetPosition;
            }
        }

    }

}
