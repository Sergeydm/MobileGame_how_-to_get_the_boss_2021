namespace TurnTheGameOn.SimpleTrafficSystem
{
    using Unity.Mathematics;
    using Unity.Collections;
    using Unity.Burst;
    using UnityEngine.Jobs;

    [BurstCompile]
    public struct AITrafficDistanceJob : IJobParallelForTransform
    {
        public float3 playerPosition;
        public NativeArray<float> distanceToPlayerArray;
        public float cullDistance;
        public float actizeZone;
        public float spawnZone;
        public NativeArray<bool> withinLimitArray;
        public NativeArray<bool> isVisibleArray;
        public NativeArray<bool> lightIsActiveArray;
        public NativeArray<bool> outOfBoundsArray;
        public NativeArray<bool> isDisabledArray;

        public void Execute(int index, TransformAccess carTransformAccessArray)
        {
            if (isDisabledArray[index] == false)
            {
                distanceToPlayerArray[index] = math.distance(carTransformAccessArray.position, playerPosition);
                withinLimitArray[index] = distanceToPlayerArray[index] < cullDistance;
                outOfBoundsArray[index] = distanceToPlayerArray[index] > spawnZone;

                if (isVisibleArray[index] || withinLimitArray[index])
                {
                    lightIsActiveArray[index] = true;
                }
                else
                {
                    lightIsActiveArray[index] = false;
                }
            }
        }
    }
}