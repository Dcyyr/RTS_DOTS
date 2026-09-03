using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

partial struct LoseTargetSystem : ISystem
{
    

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach((RefRO<LocalTransform> LocalTransform,RefRW<Target> target, RefRW<LoseTarget> loseTarget) in 
            SystemAPI.Query<RefRO<LocalTransform>,RefRW<Target>,RefRW<LoseTarget>>())
        {
            if (target.ValueRO.m_TargetEntity == Entity.Null)
                continue;

            LocalTransform targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRW.m_TargetEntity);
            float targetDistance = math.distance(LocalTransform.ValueRO.Position, targetLocalTransform.Position);

            if(targetDistance > loseTarget.ValueRO.m_LoseTargetDistance)
            {
                //目标距离太远，失去目标，reset
                target.ValueRW.m_TargetEntity = Entity.Null;
            }
        }
    }

}
