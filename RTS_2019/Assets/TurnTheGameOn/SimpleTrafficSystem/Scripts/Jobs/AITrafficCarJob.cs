namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine;
    using Unity.Mathematics;
    using Unity.Collections;
    using Unity.Burst;
    using UnityEngine.Jobs;

    [BurstCompile]
    public struct AITrafficCarJob : IJobParallelForTransform
    {
        public NativeArray<int> currentRoutePointIndexArray;
        public NativeArray<int> waypointDataListCountArray;
        public NativeArray<bool> isDrivingArray;
        public NativeArray<bool> isActiveArray;
        public NativeArray<bool> overrideInputArray;
        public NativeArray<bool> isBrakingArray;
        public NativeArray<bool> frontHit;
        public NativeArray<bool> leftHit;
        public NativeArray<bool> rightHit;
        public NativeArray<bool> stopForTrafficLight;
        public NativeArray<bool> routeIsActive;
        public NativeArray<float> speedArray;
        public NativeArray<float> routeProgressArray;
        public NativeArray<float> targetSpeedArray;
        public NativeArray<float> speedLimitArray;
        public NativeArray<float> accelArray;
        public NativeArray<float> targetAngleArray;
        public NativeArray<float> moveSteerArray;
        public NativeArray<float> moveAccelArray;
        public NativeArray<float> moveFootBrakeArray;
        public NativeArray<float> movehandBrakeArray;
        public NativeArray<float> overrideAccelerationPowerArray;
        public NativeArray<float> overrideBrakePowerArray;
        public NativeArray<float> frontHitDistance;
        public NativeArray<float> leftHitDistance;
        public NativeArray<float> rightHitDistance;
        public NativeArray<float3> routePointPositionArray;
        public NativeArray<Vector3> carTransformPreviousPositionArray;
        public NativeArray<Vector3> carTransformPositionArray;
        public NativeArray<Vector3> localTargetArray;
        public NativeArray<float> topSpeedArray;
        public float deltaTime;
        public float maxSteerAngle;
        public float accelerationPower;
        public float speedMultiplier;
        public float steerSensitivity;
        public float stopThreshold;
        public float frontSensorLength;

        public void Execute(int index, TransformAccess driveTargetTransformAccessArray)
        {
            if (isActiveArray[index])
            {
                if (currentRoutePointIndexArray[index] < waypointDataListCountArray[index]) driveTargetTransformAccessArray.position = routePointPositionArray[index];

                #region StopThreshold
                if (stopForTrafficLight[index] && routeProgressArray[index] > 0 && currentRoutePointIndexArray[index] >= waypointDataListCountArray[index] - 1)
                {
                    if (!overrideInputArray[index])
                    {
                        overrideInputArray[index] = true;
                        overrideBrakePowerArray[index] = 1f;
                        overrideAccelerationPowerArray[index] = 0f;
                    }
                }
                else if (frontHit[index] && frontHitDistance[index] < stopThreshold)
                {
                    if (!overrideInputArray[index])
                    {
                        overrideBrakePowerArray[index] = 1f;
                        overrideAccelerationPowerArray[index] = 0f;
                        overrideInputArray[index] = true;
                    }
                }
                else if (overrideInputArray[index])
                {
                    overrideBrakePowerArray[index] = 0f;
                    overrideAccelerationPowerArray[index] = 0f;
                    overrideInputArray[index] = false;
                }
                #endregion

                #region move
                if (isDrivingArray[index])
                {
                    targetSpeedArray[index] = topSpeedArray[index];
                    if (targetSpeedArray[index] > speedLimitArray[index]) targetSpeedArray[index] = speedLimitArray[index];
                    if (frontHit[index]) targetSpeedArray[index] = Mathf.InverseLerp(0, frontSensorLength, frontHitDistance[index]) * targetSpeedArray[index];
                    accelArray[index] = targetSpeedArray[index] - speedArray[index];
                    localTargetArray[index] = driveTargetTransformAccessArray.localPosition;
                    targetAngleArray[index] = math.atan2(localTargetArray[index].x, localTargetArray[index].z) * 52.29578f;
                    moveSteerArray[index] = math.clamp(targetAngleArray[index] * steerSensitivity, -1, 1) * math.sign(speedArray[index]);
                    moveSteerArray[index] *= maxSteerAngle;
                    if (speedArray[index] > topSpeedArray[index]) moveAccelArray[index] = 0;
                    else moveAccelArray[index] = (math.clamp(accelArray[index], 0, 1)) * accelerationPower;
                    moveFootBrakeArray[index] = (-1 * math.clamp(accelArray[index], -1, 0));
                    movehandBrakeArray[index] = 0;
                }
                else
                {
                    if (speedArray[index] > 2)
                    {
                        localTargetArray[index] = driveTargetTransformAccessArray.localPosition;
                        targetAngleArray[index] = math.atan2(localTargetArray[index].x, localTargetArray[index].z) * 52.29578f;
                        moveSteerArray[index] = math.clamp(targetAngleArray[index] * steerSensitivity, -1, 1) * math.sign(speedArray[index]);
                        moveSteerArray[index] *= maxSteerAngle;
                        moveAccelArray[index] = 0;
                        moveFootBrakeArray[index] = -1;
                        movehandBrakeArray[index] = 1;
                    }
                    else
                    {
                        moveSteerArray[index] = 0;
                        moveAccelArray[index] = 0;
                        moveFootBrakeArray[index] = -1;
                        movehandBrakeArray[index] = 1;
                    }
                }

                if (overrideInputArray[index])
                {
                    moveAccelArray[index] = overrideAccelerationPowerArray[index] * accelerationPower;
                    moveFootBrakeArray[index] = overrideBrakePowerArray[index];
                }
                if (moveFootBrakeArray[index] > 0.0f) isBrakingArray[index] = true;
                else if (moveFootBrakeArray[index] == 0.0f) isBrakingArray[index] = false;

                speedArray[index] = ((carTransformPositionArray[index] - carTransformPreviousPositionArray[index]).magnitude / deltaTime) * speedMultiplier;
                #endregion
            }
        }
    }
}