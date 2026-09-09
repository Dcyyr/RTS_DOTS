using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class AnimationDataAuthoring : MonoBehaviour
{

    public AnimationDataSO m_SoldierIdle;
    public class Baker : Baker<AnimationDataAuthoring>
    {
        public override void Bake(AnimationDataAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            EntitiesGraphicsSystem entitiesGraphicsSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<EntitiesGraphicsSystem>();

            //blob asset
            AnimationData animationData = new AnimationData();

            BlobBuilder blobBuilder = new BlobBuilder(Allocator.Temp);
            ref AnimationInfo animationInfo = ref blobBuilder.ConstructRoot<AnimationInfo>();

            animationInfo.m_FrameTimerMax = authoring.m_SoldierIdle.m_FrameTimerMax;
            animationInfo.m_FrameMax = authoring.m_SoldierIdle.m_MeshArray.Length;

            BlobBuilderArray<BatchMeshID> blobBuilderArray =
                blobBuilder.Allocate<BatchMeshID>(ref animationInfo.m_BatchMeshIdBlobArray, authoring.m_SoldierIdle.m_MeshArray.Length);

            for (int i = 0; i < authoring.m_SoldierIdle.m_MeshArray.Length; i++)
            {
                Mesh mesh = authoring.m_SoldierIdle.m_MeshArray[i];
                blobBuilderArray[i] = entitiesGraphicsSystem.RegisterMesh(mesh);
            }

            animationData.m_SoldierIdle = blobBuilder.CreateBlobAssetReference<AnimationInfo>(Allocator.Persistent);

            blobBuilder.Dispose();

            AddBlobAsset(ref animationData.m_SoldierIdle, out Unity.Entities.Hash128 hash);

            //
            AddComponent(entity, animationData);



        }
    }

}

public struct AnimationData : IComponentData
{
    public BlobAssetReference<AnimationInfo> m_SoldierIdle;

}

public struct AnimationInfo
{
    public float m_FrameTimerMax;
    public float m_FrameMax;
    public BlobArray<BatchMeshID> m_BatchMeshIdBlobArray;
}


