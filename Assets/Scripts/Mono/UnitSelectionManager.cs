using System;
using Unity.Collections;
using Unity.Entities;
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

            

            entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<LocalTransform, Unit>().WithPresent<Selected>().Build(entityManager);

            entityArray = entityQuery.ToEntityArray(Allocator.Temp);
            NativeArray<LocalTransform> LocalTransformArray = entityQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);

            Rect selectionAreaRect = GetSelectionAreaRect();
            for (int i = 0; i < LocalTransformArray.Length; i++)
            {
                LocalTransform unitLocalTransform = LocalTransformArray[i];
                Vector2 unitScreenPosition = Camera.main.WorldToScreenPoint(unitLocalTransform.Position);
                if(selectionAreaRect.Contains(unitScreenPosition))
                {
                    //单位在选择的区域内
                    entityManager.SetComponentEnabled<Selected>(entityArray[i], true);
                }
            }

            OnSelectionAreaEnd?.Invoke(this, EventArgs.Empty);

        }


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
