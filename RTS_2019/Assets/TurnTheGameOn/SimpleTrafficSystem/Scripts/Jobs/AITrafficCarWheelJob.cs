namespace TurnTheGameOn.SimpleTrafficSystem
{
    using Unity.Mathematics;
    using Unity.Collections;
    using Unity.Burst;
    using UnityEngine.Jobs;

    [BurstCompile]
    public struct AITrafficCarWheelJob : IJobParallelForTransform
    {
        public NativeArray<float3> wheelPosition_CachedArray;
        public NativeArray<quaternion> wheelQuaternion_CachedArray;

        public void Execute(int index, TransformAccess carWheelTransformArray)
        {
            carWheelTransformArray.position = wheelPosition_CachedArray[index];
            carWheelTransformArray.rotation = wheelQuaternion_CachedArray[index];
        }
    }
}