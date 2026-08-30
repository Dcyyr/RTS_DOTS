using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public class UnitSelectionManager : MonoBehaviour
{
    public static UnitSelectionManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    public event EventHandler OnMouseSelectionAreaStart;
    public event EventHandler OnMouseSelectionAreaEnd;

    private Vector2 m_MouseStartPos;
    private void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            m_MouseStartPos = Input.mousePosition;
            OnMouseSelectionAreaStart?.Invoke(this, EventArgs.Empty);
        }

        if (Input.GetMouseButtonUp(0))
        {
            Vector2 mouseEndPos = Input.mousePosition;
           

            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp)//快速构建查询（Temp = 临时分配，用完自动释放）
                .WithAll<Selected>().Build(entityManager);//// 查所有已启用Selected的实体,WithAll只匹配Selected已启用的实体

            NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
            for (int i = 0; i < entityArray.Length; i++)
            {
                entityManager.SetComponentEnabled<Selected>(entityArray[i], false);// 全部取消选中
            }


            Rect selectionAreaRect = GetSelectionAreaRect();
            float selectionAreaSize = selectionAreaRect.width * selectionAreaRect.height;
            float multipleSelectionSizeMin = 40f;
            bool isMultipleSelection = selectionAreaSize > multipleSelectionSizeMin;

            //如果是鼠标框选，则使用碰撞检测选择所有在选择区域内的单位
            if (isMultipleSelection)
            {
                entityQuery = new EntityQueryBuilder(Allocator.Temp)
                    .WithAll<LocalTransform, Unit>()
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

                int unitLayer = 6;
                RaycastInput raycastInput = new RaycastInput
                {
                    Start = cameraRay.GetPoint(0f),
                    End = cameraRay.GetPoint(9999f),

                    Filter = new CollisionFilter
                    {
                        BelongsTo = ~0u,
                        CollidesWith = 1u << unitLayer,
                        GroupIndex = 0

                    }
                };

                if (collisionWorld.CastRay(raycastInput, out Unity.Physics.RaycastHit raycastHit))
                {
                    if (entityManager.HasComponent<Unit>(raycastHit.Entity))
                    {   //选中单位
                        entityManager.SetComponentEnabled<Selected>(raycastHit.Entity, true);
                    }
                }
            }

            OnMouseSelectionAreaEnd?.Invoke(this, EventArgs.Empty);

        }


        //玩家移动到鼠标右键点击的位置
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mousePosition = MouseWorldPosition.Instance.GetPosition();//鼠标世界坐标


            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;//拿默认 ECS 世界的入口（SubScene 实体都在这）
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<UnitMover, Selected>().Build(entityManager);//只查"被选中"的单位

            NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
            NativeArray<UnitMover> unitMoverArray = entityQuery.ToComponentDataArray<UnitMover>(Allocator.Temp);
            NativeArray<float3> movePositionArray = GenerateMovePositionArray(mousePosition, entityArray.Length);
            for (int i = 0; i < unitMoverArray.Length; i++)
            {
                UnitMover unitMover = unitMoverArray[i];
                unitMover.m_TargetPosition = movePositionArray[i];//改副本
                unitMoverArray[i] = unitMover;//写回数组
            }

            entityQuery.CopyFromComponentDataArray(unitMoverArray);//数组写回实体
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
