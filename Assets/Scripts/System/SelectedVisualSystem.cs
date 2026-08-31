using System.Diagnostics;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;


[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[UpdateBefore(typeof(ResetEventsSystem))]
partial struct SelectedVisualSystem : ISystem
{

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {


        foreach (RefRO<Selected> selected in SystemAPI.Query<RefRO<Selected>>().WithPresent<Selected>())
        {

            if (selected.ValueRO.m_OnSelected)
            {
                UnityEngine.Debug.Log("OnSelected");
                RefRW<LocalTransform> visualLocalTransform = SystemAPI.GetComponentRW<LocalTransform>(selected.ValueRO.m_SelectedEntity);
                visualLocalTransform.ValueRW.Scale = selected.ValueRO.m_Scale;

            }
            // 用 else if：同帧同时有 OnSelected 和 OnDeselected 时，选中优先（显示光圈）
            else if (selected.ValueRO.m_OnDeselected)
            {
                UnityEngine.Debug.Log("OnDeSelected");

                RefRW<LocalTransform> visualLocalTransform = SystemAPI.GetComponentRW<LocalTransform>(selected.ValueRO.m_SelectedEntity);
                visualLocalTransform.ValueRW.Scale = 0;
            }

        }



    }


}
