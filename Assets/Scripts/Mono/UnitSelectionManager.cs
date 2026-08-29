using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class UnitSelectionManager : MonoBehaviour
{
    public static UnitSelectionManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    public event EventHandler OnSelectionAreaStart;
    public event EventHandler OnSelectionAreaEnd;

    private Vector2 m_MouseStartPos;
    private void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            m_MouseStartPos = Input.mousePosition;
            Debug.Log("Start" + m_MouseStartPos);
            OnSelectionAreaStart?.Invoke(this, EventArgs.Empty);
        }

        if (Input.GetMouseButtonUp(0))
        {
            Vector2 mouseEndPos = Input.mousePosition;
            Debug.Log("End" + mouseEndPos);

            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected>().Build(entityManager);
            NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
            for (int i = 0; i < entityArray.Length; i++)
            {
                entityManager.SetComponentEnabled<Selected>(entityArray[i], false);
            }

            Rect selectionAreaRect = GetSelectionAreaRect();
            float selectionAreaSize = selectionAreaRect.width * selectionAreaRect.height;
            float multipleSelectionSizeMin = 40f;
            bool isMultipleSelection = selectionAreaSize > multipleSelectionSizeMin;

            //如果是鼠标框选，则使用碰撞检测选择所有在选择区域内的单位
            if (isMultipleSelection)
            {
                entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<LocalTransform, Unit>().WithPresent<Selected>().Build(entityManager);

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
            }else
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

                if(collisionWorld.CastRay(raycastInput,out Unity.Physics.RaycastHit raycastHit))
                {
                    if(entityManager.HasComponent<Unit>(raycastHit.Entity))
                    {   //选中单位
                        entityManager.SetComponentEnabled<Selected>(raycastHit.Entity, true);
                    }
                }
            }

            OnSelectionAreaEnd?.Invoke(this, EventArgs.Empty);

        }


        //玩家移动到鼠标右键点击的位置
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mousePosition = MouseWorldPosition.Instance.GetPosition();


            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<UnitMover, Selected>().Build(entityManager);

            NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
            NativeArray<UnitMover> unitMoverArray = entityQuery.ToComponentDataArray<UnitMover>(Allocator.Temp);

            for (int i = 0; i < unitMoverArray.Length; i++)
            {
                UnitMover unitMover = unitMoverArray[i];
                unitMover.m_TargetPosition = mousePosition;
                unitMoverArray[i] = unitMover;
            }

            entityQuery.CopyFromComponentDataArray(unitMoverArray);
        }
    }


    public Rect GetSelectionAreaRect()
    {

        Vector2 mouseEndPos = Input.mousePosition;

        Vector2 LowerLeftCorner = new Vector2(Mathf.Min(m_MouseStartPos.x, mouseEndPos.x), Mathf.Min(m_MouseStartPos.y, mouseEndPos.y));
        Vector2 UpperRighttCorner = new Vector2(Mathf.Max(m_MouseStartPos.x, mouseEndPos.x), Mathf.Max(m_MouseStartPos.y, mouseEndPos.y));

        return new Rect(LowerLeftCorner.x, LowerLeftCorner.y, UpperRighttCorner.x - LowerLeftCorner.x, UpperRighttCorner.y - LowerLeftCorner.y);
    }
}
