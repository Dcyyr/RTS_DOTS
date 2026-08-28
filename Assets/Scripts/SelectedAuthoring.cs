using Unity.Entities;
using UnityEngine;

public class SelectedAuthoring : MonoBehaviour
{

    public GameObject m_SelectedGameObject;
    public float m_Scale;
    public class SelectedAuthoringBaker : Baker<SelectedAuthoring>
    {
        public override void Bake(SelectedAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new Selected
            {
                //½«gameobject×ª»»Îªentity
                m_SelectedEntity = GetEntity(authoring.m_SelectedGameObject, TransformUsageFlags.Dynamic),
                m_Scale = authoring.m_Scale

            });
        }
    }
}


public struct Selected : IComponentData,IEnableableComponent
{
    public Entity m_SelectedEntity;
    public float m_Scale;

}



