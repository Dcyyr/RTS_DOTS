using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitSelectionManager : MonoBehaviour
{
    public static UnitSelectionManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    public event EventHandler OnMouseSelectionAreaStart;
    public event EventHandler OnMouseSelectionAreaEnd;

    public event EventHandler OnSelectedEntitiesChanged;

    private Vector2 m_MouseStartPos;
    private void Update()
    {
        //UI在上层时不会在点击UI误认为是别的层
        if(EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if(!BuildingPlacementManager.Instance.GetActiveBuildingTypeSO().IsNone())
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            m_MouseStartPos = Input.mousePosition;
            OnMouseSelectionAreaStart?.Invoke(this, EventArgs.Empty);
        }

        if (Input.GetMouseButtonUp(0))
        {
            Vector2 mouseEndPos = Input.mousePosition;


            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;//拿默认ECS世界的入口（SubScene 实体都在这）
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp)//快速构建查询（Temp = 临时分配，用完自动释放）
                .WithAll<Selected>().Build(entityManager);//// 查所有已启用Selected的实体,WithAll只匹配Selected已启用的实体

            NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
            NativeArray<Selected> selectedArray = entityQuery.ToComponentDataArray<Selected>(Allocator.Temp);
            for (int i = 0; i < entityArray.Length; i++)
            {
                entityManager.SetComponentEnabled<Selected>(entityArray[i], false);// 全部取消选中

                Selected selected = selectedArray[i];
                selected.m_OnDeselected = true;
                entityManager.SetComponentData(entityArray[i], selected);

            }

            Rect selectionAreaRect = GetSelectionAreaRect();
            float selectionAreaSize = selectionAreaRect.width * selectionAreaRect.height;
            float multipleSelectionSizeMin = 40f;
            bool isMultipleSelection = selectionAreaSize > multipleSelectionSizeMin;

            //如果是鼠标框选，则使用碰撞检测选择所有在选择区域内的单位
            if (isMultipleSelection)
            {
                entityQuery = new EntityQueryBuilder(Allocator.Temp)
                    .WithAll<LocalTransform, Faction>()
                    .WithPresent<Selected>()//匹配有Selected组件的实体,不管启用还是禁用
                    .Build(entityManager);

                entityArray = entityQuery.ToEntityArray(Allocator.Temp);
                NativeArray<LocalTransform> LocalTransformArray = entityQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);

                for (int i = 0; i < LocalTransformArray.Length; i++)
                {
                    LocalTransform unitLocalTransform = LocalTransformArray[i];
                    Vector2 unitScreenPosition = Camera.main.WorldToScreenPoint(unitLocalTransform.Position);
                    if (selectionAreaRect.Contains(unitScreenPosition))
                    {
                        //单位在选择的区域内
                        entityManager.SetComponentEnabled<Selected>(entityArray[i], true);

                        Selected selected = entityManager.GetComponentData<Selected>(entityArray[i]);

                        selected.m_OnSelected = true;
                        entityManager.SetComponentData(entityArray[i], selected);
                    }
                }
            }
            else
            {
                //鼠标点击选择单个单位
                entityQuery = entityManager.CreateEntityQuery(typeof(PhysicsWorldSingleton));

                PhysicsWorldSingleton physicsWorldSingleton = entityQuery.GetSingleton<PhysicsWorldSingleton>();
                CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
                UnityEngine.Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                RaycastInput raycastInput = new RaycastInput
                {
                    Start = cameraRay.GetPoint(0f),
                    End = cameraRay.GetPoint(9999f),

                    Filter = new CollisionFilter
                    {
                        BelongsTo = ~0u,
                        CollidesWith = 1u << GameAssets.UNITS_LAYER | 1u << GameAssets.BUILDINGS_LAYER,
                        GroupIndex = 0

                    }
                };

                if (collisionWorld.CastRay(raycastInput, out Unity.Physics.RaycastHit raycastHit))
                {
                    if (entityManager.HasComponent<Selected>(raycastHit.Entity))
                    {   //选中单位
                        entityManager.SetComponentEnabled<Selected>(raycastHit.Entity, true);


                        Selected selected = entityManager.GetComponentData<Selected>(raycastHit.Entity);
                        selected.m_OnSelected = true;
                        entityManager.SetComponentData(raycastHit.Entity, selected);
                    }
                }
            }

            OnMouseSelectionAreaEnd?.Invoke(this, EventArgs.Empty);
            OnSelectedEntitiesChanged?.Invoke(this, EventArgs.Empty);

        }


        //玩家移动到鼠标右键点击的位置
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mousePosition = MouseWorldPosition.Instance.GetPosition();//鼠标世界坐标


            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            EntityQuery entityQuery = entityManager.CreateEntityQuery(typeof(PhysicsWorldSingleton));

            PhysicsWorldSingleton physicsWorldSingleton = entityQuery.GetSingleton<PhysicsWorldSingleton>();
            CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
            UnityEngine.Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastInput raycastInput = new RaycastInput
            {
                Start = cameraRay.GetPoint(0f),
                End = cameraRay.GetPoint(9999f),

                Filter = new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = 1u << GameAssets.UNITS_LAYER | 1u << GameAssets.BUILDINGS_LAYER,
                    GroupIndex = 0

                }
            };

            bool isAttackingSingleTarget = false;

            if (collisionWorld.CastRay(raycastInput, out Unity.Physics.RaycastHit raycastHit))
            {
                if (entityManager.HasComponent<Faction>(raycastHit.Entity))
                {   //选中单位

                    Faction targetFaction = entityManager.GetComponentData<Faction>(raycastHit.Entity);
                    if (targetFaction.m_FactionType == FactionType.Zombie)
                    {
                        //鼠标点击僵尸,设置僵尸为点击实体
                        isAttackingSingleTarget = true;

                        entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected>().WithPresent<TargetOverride>().Build(entityManager);//只查"被选中"的单位

                        NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
                        NativeArray<TargetOverride> targetOverrideArray = entityQuery.ToComponentDataArray<TargetOverride>(Allocator.Temp);
                        for (int i = 0; i < targetOverrideArray.Length; i++)
                        {
                            TargetOverride targetOverride = targetOverrideArray[i];
                            targetOverride.m_TargetEntity = raycastHit.Entity;//改副本
                            targetOverrideArray[i] = targetOverride;//写回数组
                            entityManager.SetComponentEnabled<MoveOverride>(entityArray[i], false);//禁用Override组件
                        }

                        entityQuery.CopyFromComponentDataArray(targetOverrideArray);//数组写回实体

                    }

                }
            }


            if (!isAttackingSingleTarget)
            {
                entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected>().WithPresent<MoveOverride, TargetOverride>().Build(entityManager);//只查"被选中"的单位

                NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
                NativeArray<MoveOverride> unitMoveOverrideArray = entityQuery.ToComponentDataArray<MoveOverride>(Allocator.Temp);
                NativeArray<TargetOverride> targetOverrideArray = entityQuery.ToComponentDataArray<TargetOverride>(Allocator.Temp);

                NativeArray<float3> movePositionArray = GenerateMovePositionArray(mousePosition, entityArray.Length);
                for (int i = 0; i < unitMoveOverrideArray.Length; i++)
                {
                    MoveOverride unitMoveOverride = unitMoveOverrideArray[i];
                    unitMoveOverride.m_TargetPosition = movePositionArray[i];//改副本
                    unitMoveOverrideArray[i] = unitMoveOverride;//写回数组
                    entityManager.SetComponentEnabled<MoveOverride>(entityArray[i], true);//启用MoveOverride组件

                    //不会超出范围还继续追着打
                    TargetOverride targetOverride = targetOverrideArray[i];
                    targetOverride.m_TargetEntity = Entity.Null;//改副本（右键地面/自己人时没有攻击目标，写成命中的友军会导致单位攻击自己 → NaN → 消失）
                    targetOverrideArray[i] = targetOverride;//写回数组
                }


                entityQuery.CopyFromComponentDataArray(unitMoveOverrideArray);
                entityQuery.CopyFromComponentDataArray(targetOverrideArray);//数组写回实体
            }

            //处理兵营集中位置
            entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected,BuildingBarracks,LocalTransform>().Build(entityManager);//只查"被选中"的单位


            NativeArray<BuildingBarracks> buildingBarracksArray = entityQuery.ToComponentDataArray<BuildingBarracks>(Allocator.Temp);
            NativeArray<LocalTransform> localTransformArray = entityQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);

            for (int i = 0; i < buildingBarracksArray.Length; i++)
            {
                BuildingBarracks buildingBarracks = buildingBarracksArray[i];

                buildingBarracks.m_RallyPositionOffset = (float3)mousePosition - localTransformArray[i].Position;
                buildingBarracksArray[i] = buildingBarracks;

            }


            entityQuery.CopyFromComponentDataArray(buildingBarracksArray);
        }
    }


    public Rect GetSelectionAreaRect()
    {

        Vector2 mouseEndPos = Input.mousePosition;
        //(起点.x, 终点.x),(起点.y, 终点.y)
        Vector2 LowerLeftCorner = new Vector2(Mathf.Min(m_MouseStartPos.x, mouseEndPos.x), Mathf.Min(m_MouseStartPos.y, mouseEndPos.y));
        ////(起点.x, 终点.x),(起点.y, 终点.y)
        Vector2 UpperRighttCorner = new Vector2(Mathf.Max(m_MouseStartPos.x, mouseEndPos.x), Mathf.Max(m_MouseStartPos.y, mouseEndPos.y));

        return new Rect(LowerLeftCorner.x, LowerLeftCorner.y, UpperRighttCorner.x - LowerLeftCorner.x, UpperRighttCorner.y - LowerLeftCorner.y);//(左下角，宽，高)
    }

    private NativeArray<float3> GenerateMovePositionArray(float3 targetPos, int positionCount)
    {
        NativeArray<float3> positionArray = new NativeArray<float3>(positionCount, Allocator.Temp);

        if (positionCount == 0) return positionArray;
        positionArray[0] = targetPos;
        if (positionCount == 1)
        {
            return positionArray;
        }

        float ringSize = 2.2f;
        int ring = 0;
        int PositionIndex = 1;

        while (PositionIndex < positionCount)
        {
            int ringPositionCount = 3 + ring * 2;

            for (int i = 0; i < ringPositionCount; i++)
            {
                float angle = i * (math.PI2 / ringPositionCount);
                float3 ringVector = math.rotate(quaternion.RotateY(angle), new float3(ringSize * (ring + 1), 0, 0));
                float3 ringPosition = targetPos + ringVector;

                positionArray[PositionIndex] = ringPosition;
                PositionIndex++;

                if (PositionIndex >= positionCount)
                {
                    break;
                }
            }
            ring++;
        }

        return positionArray;
    }

}
