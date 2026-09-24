using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class AnimationDataSetAuthoring : MonoBehaviour
{

    public AnimationDataSOList m_AnimationDataSOList;
    public class Baker : Baker<AnimationDataSetAuthoring>
    {
        public override void Bake(AnimationDataSetAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            EntitiesGraphicsSystem entitiesGraphicsSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<EntitiesGraphicsSystem>();

            //blob asset
            AnimationDataSet animationData = new AnimationDataSet();

            BlobBuilder blobBuilder = new BlobBuilder(Allocator.Temp);
            ref BlobArray<AnimationData> animationDataBlobArray = ref blobBuilder.ConstructRoot<BlobArray<AnimationData>>();

            BlobBuilderArray<AnimationData> animationDataBuilderArray = blobBuilder.Allocate<AnimationData>(ref animationDataBlobArray, System.Enum.GetValues(typeof(AnimationDataSO.AnimationType)).Length);


            int index = 0;
            foreach (AnimationDataSO.AnimationType animationType in System.Enum.GetValues(typeof(AnimationDataSO.AnimationType)))
            {
                AnimationDataSO animationDataSO = authoring.m_AnimationDataSOList.GetAnimationDataSO(animationType);
                
                animationDataBuilderArray[index].m_FrameTimerMax = animationDataSO.m_FrameTimerMax;
                animationDataBuilderArray[index].m_FrameMax = animationDataSO.m_MeshArray.Length;

                BlobBuilderArray<BatchMeshID> blobBuilderArray =
                    blobBuilder.Allocate<BatchMeshID>(ref animationDataBuilderArray[index].m_BatchMeshIdBlobArray, animationDataSO.m_MeshArray.Length);

                for (int i = 0; i < animationDataSO.m_MeshArray.Length; i++)
                {
                    Mesh mesh = animationDataSO.m_MeshArray[i];
                    blobBuilderArray[i] = entitiesGraphicsSystem.RegisterMesh(mesh);
                }

                index++;
                
            }
           

            animationData.m_AnimationDataBlobArrayAssetReference = blobBuilder.CreateBlobAssetReference<BlobArray<AnimationData>>(Allocator.Persistent);
            blobBuilder.Dispose();

            AddBlobAsset(ref animationData.m_AnimationDataBlobArrayAssetReference, out Unity.Entities.Hash128 hash);

            //
            AddComponent(entity, animationData);



        }

        
    }

}

public struct AnimationDataSet : IComponentData
{
    public BlobAssetReference<BlobArray<AnimationData>> m_AnimationDataBlobArrayAssetReference;

}

public struct AnimationData
{
    public float m_FrameTimerMax;
    public float m_FrameMax;
    public BlobArray<BatchMeshID> m_BatchMeshIdBlobArray;
}