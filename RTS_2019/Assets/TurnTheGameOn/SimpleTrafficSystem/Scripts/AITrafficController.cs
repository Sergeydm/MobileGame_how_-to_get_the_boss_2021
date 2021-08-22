namespace TurnTheGameOn.SimpleTrafficSystem
{
    using System.Collections.Generic;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.Jobs;
    using Unity.Collections;
    using Unity.Mathematics;
    using Unity.Jobs;

    public class AITrafficController : MonoBehaviour
    {
        #region Variables
        public static AITrafficController Instance;
        public AITrafficDebug m_AITrafficDebug;
        [Header("Pooling")]
        public AITrafficPool _AITrafficPool;
        public Transform centerPoint;
        public bool usePooling;
        public bool useRouteLimit;
        private bool isInitialized;
        [Header("Detection Sensor")]
        public LayerMask layerMask;
        public Vector3 frontSensorSize = new Vector3(1.01f, 1f, 0.001f);
        public float frontSensorLength = 5f;
        public Vector3 sideSensorSize = new Vector3(1.01f, 1f, 0.001f);
        public float sideSensorLength = 5f;
        [Header("Car Settings")]
        public float speedMultiplier = 2.23693629f;
        public float accelerationPower = 1500;
        public float steerSensitivity = 0.02f;
        public float maxSteerAngle = 37f;
        public float stopThreshold = 5f;
        public float minDrag = 0.3f;
        public float minAngularDrag = 0.3f;
        [Header("Lane Changing")]
        public bool enableLaneChanging;
        public float changeLaneTrigger = 3f;
        public float minSpeedToChangeLanes = 5f;
        public float changeLaneCooldown = 20f;

        public List<AITrafficCar> carAIList { get; private set; } = new List<AITrafficCar>();
        private List<float> changeLaneCooldownTimer = new List<float>();
        public List<AITrafficWaypointRoute> allWaypointRoutesList = new List<AITrafficWaypointRoute>();
        public List<AITrafficWaypointRoute> waypointRouteList { get; private set; } = new List<AITrafficWaypointRoute>();
        private List<AITrafficWaypoint> currentWaypointList = new List<AITrafficWaypoint>();
        private List<WheelCollider> frontRightWheelColliderList = new List<WheelCollider>();
        private List<WheelCollider> frontLefttWheelColliderList = new List<WheelCollider>();
        private List<WheelCollider> backRighttWheelColliderList = new List<WheelCollider>();
        private List<WheelCollider> backLeftWheelColliderList = new List<WheelCollider>();
        private List<Rigidbody> rigidbodyList = new List<Rigidbody>();

        private List<Vector3> frontOriginList = new List<Vector3>();
        private List<Vector3> frontDirectionList = new List<Vector3>();
        private List<Quaternion> frontRotationList = new List<Quaternion>();
        private List<Transform> frontTransformCached = new List<Transform>();
        private List<Transform> frontHitTransform = new List<Transform>();
        private List<Transform> frontPreviousHitTransform = new List<Transform>();

        private List<Vector3> leftOriginList = new List<Vector3>();
        private List<Vector3> leftDirectionList = new List<Vector3>();
        private List<Quaternion> leftRotationList = new List<Quaternion>();
        private List<Transform> leftTransformCached = new List<Transform>();
        private List<Transform> leftHitTransform = new List<Transform>();
        private List<Transform> leftPreviousHitTransform = new List<Transform>();

        private List<Vector3> rightOriginList = new List<Vector3>();
        private List<Vector3> rightDirectionList = new List<Vector3>();
        private List<Quaternion> rightRotationList = new List<Quaternion>();
        private List<Transform> rightTransformCached = new List<Transform>();
        private List<Transform> rightHitTransform = new List<Transform>();
        private List<Transform> rightPreviousHitTransform = new List<Transform>();

        private List<AITrafficWaypointRouteInfo> carAIWaypointRouteInfo = new List<AITrafficWaypointRouteInfo>();
        private List<Material> brakeMaterial = new List<Material>();
        private List<Light> headLight = new List<Light>();
        private List<AITrafficSpawnPoint> trafficSpawnPoints = new List<AITrafficSpawnPoint>();
        private List<AITrafficSpawnPoint> availableSpawnPoints = new List<AITrafficSpawnPoint>();
        private List<AITrafficPoolEntry> trafficPool = new List<AITrafficPoolEntry>();
        public NativeArray<bool> isChangingLanes;
        public NativeArray<bool> canChangeLanes;
        public NativeArray<bool> frontHit;
        public NativeArray<bool> leftHit;
        public NativeArray<bool> rightHit;
        public NativeArray<bool> stopForTrafficLight;
        public NativeArray<bool> routeIsActive;
        public NativeArray<float> frontHitDistance;
        public NativeArray<float> leftHitDistance;
        public NativeArray<float> rightHitDistance;
        public NativeArray<float> topSpeedArray;
        public NativeArray<Vector3> carTransformPositionArray;
        public TransformAccessArray driveTargetTransformAccessArray;
        private NativeArray<int> currentRoutePointIndexArray;
        private NativeArray<int> waypointDataListCountArray;
        private NativeArray<bool> isActiveArray;
        private NativeArray<bool> isDrivingArray;
        private NativeArray<bool> overrideDragArray;
        private NativeArray<bool> overrideInputArray;
        private NativeArray<bool> isBrakingArray;
        private NativeArray<float> speedArray;
        private NativeArray<float> routeProgressArray;
        private NativeArray<float> targetSpeedArray;
        private NativeArray<float> accelArray;
        private NativeArray<float> speedLimitArray;
        private NativeArray<float> targetAngleArray;
        private NativeArray<float> dragArray;
        private NativeArray<float> angularDragArray;
        private NativeArray<float> moveSteerArray;
        private NativeArray<float> moveAccelArray;
        private NativeArray<float> moveFootBrakeArray;
        private NativeArray<float> movehandBrakeArray;
        private NativeArray<float> overrideAccelerationPowerArray;
        private NativeArray<float> overrideBrakePowerArray;
        private NativeArray<float3> routePointPositionArray;
        private NativeArray<float3> FRwheelPosition_CachedArray;
        private NativeArray<float3> FLwheelPosition_CachedArray;
        private NativeArray<float3> BRwheelPosition_CachedArray;
        private NativeArray<float3> BLwheelPosition_CachedArray;
        private NativeArray<Vector3> carTransformPreviousPositionArray;
        private NativeArray<Vector3> localTargetArray;
        private NativeArray<quaternion> FRwheelQuaternion_CachedArray;
        private NativeArray<quaternion> FLwheelQuaternion_CachedArray;
        private NativeArray<quaternion> BRwheelQuaternion_CachedArray;
        private NativeArray<quaternion> BLwheelQuaternion_CachedArray;
        public NativeArray<bool> isDisabledArray;
        private NativeArray<float> distanceToPlayerArray;
        private NativeArray<bool> withinLimitArray;
        private NativeArray<bool> isEnabledArray;
        private NativeArray<bool> outOfBoundsArray;
        private NativeArray<bool> lightIsActiveArray;
        private NativeArray<bool> isVisibleArray;
        private TransformAccessArray carTransformAccessArray;
        private TransformAccessArray frontRightWheelTransformList;
        private TransformAccessArray frontLeftWheelTransformList;
        private TransformAccessArray backRightWheelTransformList;
        private TransformAccessArray backLeftWheelTransformList;
        private JobHandle jobHandle;
        private AITrafficCarJob carAITrafficJob;
        private AITrafficCarWheelJob frAITrafficCarWheelJob;
        private AITrafficCarWheelJob flAITrafficCarWheelJob;
        private AITrafficCarWheelJob brAITrafficCarWheelJob;
        private AITrafficCarWheelJob blAITrafficCarWheelJob;
        private AITrafficCarPositionJob carTransformpositionJob;
        private AITrafficDistanceJob _AITrafficDistanceJob;
        public float3 centerPosition { get; private set; }
        private float spawnTimer;
        private float distanceToSpawnPoint;
        private float startTime;
        private float deltaTime;
        private int carCount;
        private int currentAmountToSpawn;
        private int randomSpawnPointIndex;
        private int currentDensity;
        private WheelCollider currentWheelCollider;
        private AITrafficCar spawncar;
        private AITrafficCar loadCar;
        private Vector3 wheelPosition_Cached;
        private Quaternion wheelQuaternion_Cached;
        private RaycastHit boxHit;
        private AITrafficWaypoint nextWaypoint;
        private List<float> changeLaneTriggerTimer = new List<float>();
        private bool canTurnLeft, canTurnRight;
        private Vector3 relativePoint;
        private AITrafficPoolEntry newTrafficPoolEntry = new AITrafficPoolEntry();
        private int PossibleTargetDirection(Transform _from, Transform _to)
        {
            relativePoint = _from.InverseTransformPoint(_to.position);
            if (relativePoint.x < 0.0) return -1;
            else if (relativePoint.x > 0.0) return 1;
            else return 0;
        }

        #endregion

        #region Main Methods
        private void OnEnable()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogWarning("Multiple AITrafficController Instances found in scene, this is not allowed. Destroying this duplicate AITrafficController.");
                Destroy(this);
            }
        }

        private void Start()
        {
            if (usePooling)
            {
                StartCoroutine(SpawnStartupTraffic());
            }
            else
            {
                StartCoroutine(Initialize());
            }
        }

        IEnumerator Initialize()
        {
            yield return new WaitForSeconds(1f);
            for (int i = 0; i < carCount; i++)
            {
                routePointPositionArray[i] = waypointRouteList[i].waypointDataList[currentRoutePointIndexArray[i]]._transform.position;
                carAIList[i].StartDriving();
            }
            isInitialized = true;
        }

        private void FixedUpdate()
        {
            if (isInitialized)
            {
                if (m_AITrafficDebug.debugProcessTime) startTime = Time.realtimeSinceStartup;
                deltaTime = Time.deltaTime;
                for (int i = 0; i < carCount; i++)
                {
                    stopForTrafficLight[i] = carAIWaypointRouteInfo[i].stopForTrafficLight;
                }
                carAITrafficJob = new AITrafficCarJob
                {
                    frontSensorLength = frontSensorLength,
                    currentRoutePointIndexArray = currentRoutePointIndexArray,
                    waypointDataListCountArray = waypointDataListCountArray,
                    carTransformPreviousPositionArray = carTransformPreviousPositionArray,
                    carTransformPositionArray = carTransformPositionArray,
                    routePointPositionArray = routePointPositionArray,
                    isDrivingArray = isDrivingArray,
                    isActiveArray = isActiveArray,
                    speedArray = speedArray,
                    deltaTime = deltaTime,
                    routeProgressArray = routeProgressArray,
                    topSpeedArray = topSpeedArray,
                    targetSpeedArray = targetSpeedArray,
                    speedLimitArray = speedLimitArray,
                    accelArray = accelArray,
                    localTargetArray = localTargetArray,
                    targetAngleArray = targetAngleArray,
                    moveSteerArray = moveSteerArray,
                    moveAccelArray = moveAccelArray,
                    moveFootBrakeArray = moveFootBrakeArray,
                    movehandBrakeArray = movehandBrakeArray,
                    maxSteerAngle = maxSteerAngle,
                    overrideInputArray = overrideInputArray,
                    overrideAccelerationPowerArray = overrideAccelerationPowerArray,
                    overrideBrakePowerArray = overrideBrakePowerArray,
                    isBrakingArray = isBrakingArray,
                    speedMultiplier = speedMultiplier,
                    steerSensitivity = steerSensitivity,
                    stopThreshold = stopThreshold,
                    frontHitDistance = frontHitDistance,
                    leftHitDistance = leftHitDistance,
                    rightHitDistance = rightHitDistance,
                    frontHit = frontHit,
                    leftHit = leftHit,
                    rightHit = rightHit,
                    stopForTrafficLight = stopForTrafficLight,
                    routeIsActive = routeIsActive,
                    accelerationPower = accelerationPower,
                };
                jobHandle = carAITrafficJob.Schedule(driveTargetTransformAccessArray);
                jobHandle.Complete();

                for (int i = 0; i < carCount; i++) // operate on results
                {
                    if (isActiveArray[i])
                    {
                        #region sensor logic
                        if (isDrivingArray[i])
                        {
                            #region Front Sensor
                            /// Front Sensor
                            frontOriginList[i] = frontTransformCached[i].position;
                            frontDirectionList[i] = frontTransformCached[i].forward;
                            frontRotationList[i] = frontTransformCached[i].rotation;
                            if (Physics.BoxCast(
                                frontOriginList[i],
                                frontSensorSize,
                                frontDirectionList[i],
                                out boxHit,
                                frontRotationList[i],
                                frontSensorLength,
                                layerMask,
                                QueryTriggerInteraction.UseGlobal))
                            {
                                frontHitTransform[i] = boxHit.transform; // cache transform lookup
                                if (frontHitTransform[i] != frontPreviousHitTransform[i])
                                {
                                    frontPreviousHitTransform[i] = frontHitTransform[i];
                                }
                                frontHitDistance[i] = boxHit.distance;
                                frontHit[i] = true;
                            }
                            else if (frontHit[i] != false) //ResetHitBox
                            {
                                frontHitDistance[i] = frontSensorLength;
                                frontHit[i] = false;
                            }
                            #endregion

                            #region Lane Change
                            if (enableLaneChanging)
                            {
                                if (speedArray[i] > minSpeedToChangeLanes)
                                {
                                    if (!canChangeLanes[i])
                                    {
                                        changeLaneCooldownTimer[i] += deltaTime;
                                        if (changeLaneCooldownTimer[i] > changeLaneCooldown)
                                        {
                                            canChangeLanes[i] = true;
                                            changeLaneCooldownTimer[i] = 0f;
                                        }
                                    }
                                    if (frontHit[i] == true && canChangeLanes[i] && isChangingLanes[i] == false)
                                    {
                                        changeLaneTriggerTimer[i] += Time.deltaTime;

                                        #region Left Sensor
                                        /// Left Sensor
                                        leftOriginList[i] = leftTransformCached[i].position;
                                        leftDirectionList[i] = leftTransformCached[i].forward;
                                        leftRotationList[i] = leftTransformCached[i].rotation;
                                        if (Physics.BoxCast(
                                            leftOriginList[i],
                                            sideSensorSize,
                                            leftDirectionList[i],
                                            out boxHit,
                                            leftRotationList[i],
                                            sideSensorLength,
                                            layerMask,
                                            QueryTriggerInteraction.UseGlobal))
                                        {
                                            leftHitTransform[i] = boxHit.transform; // cache transform lookup
                                            if (leftHitTransform[i] != leftPreviousHitTransform[i])
                                            {
                                                leftPreviousHitTransform[i] = leftHitTransform[i];
                                            }
                                            leftHitDistance[i] = boxHit.distance;
                                            leftHit[i] = true;
                                        }
                                        else if (leftHit[i] != false) //ResetHitBox
                                        {
                                            leftHitDistance[i] = sideSensorLength;
                                            leftHit[i] = false;
                                        }
                                        #endregion

                                        #region Right Sensor
                                        /// Right Sensor
                                        rightOriginList[i] = rightTransformCached[i].position;
                                        rightDirectionList[i] = rightTransformCached[i].forward;
                                        rightRotationList[i] = rightTransformCached[i].rotation;
                                        if (Physics.BoxCast(
                                            rightOriginList[i],
                                            sideSensorSize,
                                            rightDirectionList[i],
                                            out boxHit,
                                            rightRotationList[i],
                                            sideSensorLength,
                                            layerMask,
                                            QueryTriggerInteraction.UseGlobal))
                                        {
                                            rightHitTransform[i] = boxHit.transform; // cache transform lookup
                                            if (rightHitTransform[i] != rightPreviousHitTransform[i])
                                            {
                                                rightPreviousHitTransform[i] = rightHitTransform[i];
                                            }
                                            rightHitDistance[i] = boxHit.distance;
                                            rightHit[i] = true;
                                        }
                                        else if (rightHit[i] != false) //ResetHitBox
                                        {
                                            rightHitDistance[i] = sideSensorLength;
                                            rightHit[i] = false;
                                        }
                                        #endregion

                                        canTurnLeft = leftHit[i] == true ? false : true;
                                        canTurnRight = rightHit[i] == true ? false : true;

                                        if (changeLaneTriggerTimer[i] >= changeLaneTrigger)
                                        {
                                            canChangeLanes[i] = false;
                                            nextWaypoint = currentWaypointList[i];

                                            if (nextWaypoint != null)
                                            {
                                                if (nextWaypoint.onReachWaypointSettings.laneChangePoints.Count > 0)  // take the first alternate route
                                                {
                                                    for (int j = 0; j < nextWaypoint.onReachWaypointSettings.laneChangePoints.Count; j++)
                                                    {
                                                        if (
                                                            PossibleTargetDirection(carTransformAccessArray[i], nextWaypoint.onReachWaypointSettings.laneChangePoints[j].transform) == -1 && canTurnLeft ||
                                                            PossibleTargetDirection(carTransformAccessArray[i], nextWaypoint.onReachWaypointSettings.laneChangePoints[j].transform) == 1 && canTurnRight
                                                            )
                                                        {
                                                            carAIList[i].ChangeToRouteWaypoint(nextWaypoint.onReachWaypointSettings.laneChangePoints[j].onReachWaypointSettings);
                                                            isChangingLanes[i] = true;
                                                            canChangeLanes[i] = false;
                                                            changeLaneTriggerTimer[i] = 0f;
                                                        }
                                                    }

                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        changeLaneTriggerTimer[i] = 0f;
                                        leftHit[i] = false;
                                        rightHit[i] = false;
                                        leftHitDistance[i] = sideSensorLength;
                                        rightHitDistance[i] = sideSensorLength;
                                    }
                                }
                            }
                            #endregion
                        }
                        if ((speedArray[i] == 0 || !overrideInputArray[i]))
                        {
                            rigidbodyList[i].drag = minDrag;
                            rigidbodyList[i].angularDrag = minAngularDrag;
                        }
                        else if (overrideInputArray[i])
                        {
                            float amountToAdd = Mathf.InverseLerp(0, frontSensorLength, frontHitDistance[i]) * targetSpeedArray[i];
                            if (frontHitDistance[i] < 1) amountToAdd = targetSpeedArray[i] * 10;
                            rigidbodyList[i].drag = minDrag + (Mathf.InverseLerp(0, frontSensorLength, frontHitDistance[i]) * amountToAdd);//Mathf.InverseLerp(_start, _end, _position)
                            rigidbodyList[i].angularDrag = minAngularDrag + Mathf.InverseLerp(0, frontSensorLength, frontHitDistance[i] * amountToAdd);
                            changeLaneTriggerTimer[i] = 0;
                        }
                        #endregion

                        if (isDrivingArray[i])
                        {
                            for (int j = 0; j < 4; j++) // move
                            {
                                if (j == 0)
                                {
                                    currentWheelCollider = frontRightWheelColliderList[i];
                                    currentWheelCollider.motorTorque = moveAccelArray[i];

                                    currentWheelCollider.brakeTorque = moveFootBrakeArray[i];
                                    currentWheelCollider.GetWorldPose(out wheelPosition_Cached, out wheelQuaternion_Cached);

                                    currentWheelCollider.steerAngle = moveSteerArray[i];
                                    FRwheelPosition_CachedArray[i] = wheelPosition_Cached;
                                    FRwheelQuaternion_CachedArray[i] = wheelQuaternion_Cached;
                                }
                                else if (j == 1)
                                {
                                    currentWheelCollider = frontLefttWheelColliderList[i];
                                    currentWheelCollider.motorTorque = moveAccelArray[i];

                                    currentWheelCollider.brakeTorque = moveFootBrakeArray[i];
                                    currentWheelCollider.GetWorldPose(out wheelPosition_Cached, out wheelQuaternion_Cached);

                                    currentWheelCollider.steerAngle = moveSteerArray[i];
                                    FLwheelPosition_CachedArray[i] = wheelPosition_Cached;
                                    FLwheelQuaternion_CachedArray[i] = wheelQuaternion_Cached;
                                }
                                else if (j == 2)
                                {
                                    currentWheelCollider = backRighttWheelColliderList[i];
                                    currentWheelCollider.motorTorque = moveAccelArray[i];

                                    currentWheelCollider.brakeTorque = moveFootBrakeArray[i];
                                    currentWheelCollider.GetWorldPose(out wheelPosition_Cached, out wheelQuaternion_Cached);

                                    BRwheelPosition_CachedArray[i] = wheelPosition_Cached;
                                    BRwheelQuaternion_CachedArray[i] = wheelQuaternion_Cached;
                                }
                                else if (j == 3)
                                {
                                    currentWheelCollider = backLeftWheelColliderList[i];
                                    currentWheelCollider.motorTorque = moveAccelArray[i];

                                    currentWheelCollider.brakeTorque = moveFootBrakeArray[i];
                                    currentWheelCollider.GetWorldPose(out wheelPosition_Cached, out wheelQuaternion_Cached);

                                    BLwheelPosition_CachedArray[i] = wheelPosition_Cached;
                                    BLwheelQuaternion_CachedArray[i] = wheelQuaternion_Cached;
                                }
                            }
                        }
                        if (isBrakingArray[i]) brakeMaterial[i].EnableKeyword("_EMISSION");
                        else brakeMaterial[i].DisableKeyword("_EMISSION");
                    }
                }

                carTransformpositionJob = new AITrafficCarPositionJob
                {
                    carTransformPreviousPositionArray = carTransformPreviousPositionArray,
                    carTransformPositionArray = carTransformPositionArray,
                };
                jobHandle = carTransformpositionJob.Schedule(carTransformAccessArray);
                jobHandle.Complete();

                frAITrafficCarWheelJob = new AITrafficCarWheelJob
                {
                    wheelPosition_CachedArray = FRwheelPosition_CachedArray,
                    wheelQuaternion_CachedArray = FRwheelQuaternion_CachedArray,
                };
                jobHandle = frAITrafficCarWheelJob.Schedule(frontRightWheelTransformList);
                jobHandle.Complete();

                flAITrafficCarWheelJob = new AITrafficCarWheelJob
                {
                    wheelPosition_CachedArray = FLwheelPosition_CachedArray,
                    wheelQuaternion_CachedArray = FLwheelQuaternion_CachedArray,
                };
                jobHandle = flAITrafficCarWheelJob.Schedule(frontLeftWheelTransformList);
                jobHandle.Complete();

                brAITrafficCarWheelJob = new AITrafficCarWheelJob
                {
                    wheelPosition_CachedArray = BRwheelPosition_CachedArray,
                    wheelQuaternion_CachedArray = BRwheelQuaternion_CachedArray,
                };
                jobHandle = brAITrafficCarWheelJob.Schedule(backRightWheelTransformList);
                jobHandle.Complete();

                blAITrafficCarWheelJob = new AITrafficCarWheelJob
                {
                    wheelPosition_CachedArray = BLwheelPosition_CachedArray,
                    wheelQuaternion_CachedArray = BLwheelQuaternion_CachedArray,
                };
                jobHandle = blAITrafficCarWheelJob.Schedule(backLeftWheelTransformList);
                jobHandle.Complete();

                if (usePooling)
                {
                    centerPosition = centerPoint.position;
                    _AITrafficDistanceJob = new AITrafficDistanceJob
                    {
                        playerPosition = centerPosition,
                        distanceToPlayerArray = distanceToPlayerArray,
                        isVisibleArray = isVisibleArray,
                        withinLimitArray = withinLimitArray,
                        cullDistance = _AITrafficPool.cullHeadLight,
                        lightIsActiveArray = lightIsActiveArray,
                        outOfBoundsArray = outOfBoundsArray,
                        actizeZone = _AITrafficPool.actizeZone,
                        spawnZone = _AITrafficPool.spawnZone,
                        isDisabledArray = isDisabledArray,
                    };
                    jobHandle = _AITrafficDistanceJob.Schedule(carTransformAccessArray);
                    jobHandle.Complete();
                    //if (useRouteLimit)
                    //{
                        for (int i = 0; i < allWaypointRoutesList.Count; i++)
                        {
                            allWaypointRoutesList[i].previousDensity = allWaypointRoutesList[i].currentDensity;
                            allWaypointRoutesList[i].currentDensity = 0;
                        }
                    //}
                    for (int i = 0; i < carCount; i++)
                    {
                        if (isDisabledArray[i] == false)
                        {
                            waypointRouteList[i].currentDensity += 1;
                            if (outOfBoundsArray[i])
                            {
                                MoveCarToPool(carAIList[i].assignedIndex);
                            }
                        }
                        else if (outOfBoundsArray[i] == false)
                        {
                            if (lightIsActiveArray[i])
                            {
                                if (isEnabledArray[i] == false)
                                {
                                    isEnabledArray[i] = true;
                                    headLight[i].enabled = true;
                                }
                            }
                            else
                            {
                                if (isEnabledArray[i])
                                {
                                    isEnabledArray[i] = false;
                                    headLight[i].enabled = false;
                                }
                            }
                        }
                    }
                    if (spawnTimer >= _AITrafficPool.spawnRate) SpawnTraffic();
                    else spawnTimer += deltaTime;
                }
                if (m_AITrafficDebug.debugProcessTime) Debug.Log((("AI Update " + (Time.realtimeSinceStartup - startTime) * 1000f)) + "ms");
            }
        }

        private void OnDestroy()
        {
            DisposeArrays();
        }

        void DisposeArrays()
        {
            currentRoutePointIndexArray.Dispose();
            waypointDataListCountArray.Dispose();
            carTransformPreviousPositionArray.Dispose();
            carTransformPositionArray.Dispose();
            routePointPositionArray.Dispose();
            driveTargetTransformAccessArray.Dispose();
            isDrivingArray.Dispose();
            isActiveArray.Dispose();
            speedArray.Dispose();
            carTransformAccessArray.Dispose();
            routeProgressArray.Dispose();
            targetSpeedArray.Dispose();
            accelArray.Dispose();
            speedLimitArray.Dispose();
            targetAngleArray.Dispose();
            dragArray.Dispose();
            angularDragArray.Dispose();
            overrideDragArray.Dispose();
            localTargetArray.Dispose();
            moveSteerArray.Dispose();
            moveAccelArray.Dispose();
            moveFootBrakeArray.Dispose();
            movehandBrakeArray.Dispose();
            overrideInputArray.Dispose();
            overrideAccelerationPowerArray.Dispose();
            overrideBrakePowerArray.Dispose();
            isBrakingArray.Dispose();
            frontRightWheelTransformList.Dispose();
            frontLeftWheelTransformList.Dispose();
            backRightWheelTransformList.Dispose();
            backLeftWheelTransformList.Dispose();
            FRwheelPosition_CachedArray.Dispose();
            FRwheelQuaternion_CachedArray.Dispose();
            FLwheelPosition_CachedArray.Dispose();
            FLwheelQuaternion_CachedArray.Dispose();
            BRwheelPosition_CachedArray.Dispose();
            BRwheelQuaternion_CachedArray.Dispose();
            BLwheelPosition_CachedArray.Dispose();
            BLwheelQuaternion_CachedArray.Dispose();
            topSpeedArray.Dispose();
            canChangeLanes.Dispose();
            isChangingLanes.Dispose();
            frontHitDistance.Dispose();
            leftHitDistance.Dispose();
            rightHitDistance.Dispose();
            frontHit.Dispose();
            leftHit.Dispose();
            rightHit.Dispose();
            stopForTrafficLight.Dispose();
            routeIsActive.Dispose();
            isVisibleArray.Dispose();
            isDisabledArray.Dispose();
            distanceToPlayerArray.Dispose();
            withinLimitArray.Dispose();
            isEnabledArray.Dispose();
            outOfBoundsArray.Dispose();
            lightIsActiveArray.Dispose();
        }
        #endregion

        #region Set Array Data
        public void Set_IsDrivingArray(int _index, bool _value)
        {
            if (isDrivingArray[_index] != _value)
            {
                isBrakingArray[_index] = _value == true ? false : true;
                isDrivingArray[_index] = _value;
                rigidbodyList[_index].isKinematic = _value == true ? false : true;
                if (_value == false)
                {
                    moveAccelArray[_index] = 0;
                    moveFootBrakeArray[_index] = -1;
                    movehandBrakeArray[_index] = 1;
                    for (int j = 0; j < 4; j++) // move
                    {
                        if (j == 0)
                        {
                            currentWheelCollider = frontRightWheelColliderList[_index];
                            currentWheelCollider.motorTorque = moveAccelArray[_index];

                            currentWheelCollider.brakeTorque = moveFootBrakeArray[_index];
                            currentWheelCollider.GetWorldPose(out wheelPosition_Cached, out wheelQuaternion_Cached);

                            currentWheelCollider.steerAngle = moveSteerArray[_index];
                            FRwheelPosition_CachedArray[_index] = wheelPosition_Cached;
                            FRwheelQuaternion_CachedArray[_index] = wheelQuaternion_Cached;
                        }
                        else if (j == 1)
                        {
                            currentWheelCollider = frontLefttWheelColliderList[_index];
                            currentWheelCollider.motorTorque = moveAccelArray[_index];

                            currentWheelCollider.brakeTorque = moveFootBrakeArray[_index];
                            currentWheelCollider.GetWorldPose(out wheelPosition_Cached, out wheelQuaternion_Cached);

                            currentWheelCollider.steerAngle = moveSteerArray[_index];
                            FLwheelPosition_CachedArray[_index] = wheelPosition_Cached;
                            FLwheelQuaternion_CachedArray[_index] = wheelQuaternion_Cached;
                        }
                        else if (j == 2)
                        {
                            currentWheelCollider = backRighttWheelColliderList[_index];
                            currentWheelCollider.motorTorque = moveAccelArray[_index];

                            currentWheelCollider.brakeTorque = moveFootBrakeArray[_index];
                            currentWheelCollider.GetWorldPose(out wheelPosition_Cached, out wheelQuaternion_Cached);

                            BRwheelPosition_CachedArray[_index] = wheelPosition_Cached;
                            BRwheelQuaternion_CachedArray[_index] = wheelQuaternion_Cached;
                        }
                        else if (j == 3)
                        {
                            currentWheelCollider = backLeftWheelColliderList[_index];
                            currentWheelCollider.motorTorque = moveAccelArray[_index];

                            currentWheelCollider.brakeTorque = moveFootBrakeArray[_index];
                            currentWheelCollider.GetWorldPose(out wheelPosition_Cached, out wheelQuaternion_Cached);

                            BLwheelPosition_CachedArray[_index] = wheelPosition_Cached;
                            BLwheelQuaternion_CachedArray[_index] = wheelQuaternion_Cached;
                        }
                    }
                }
            }
        }
        public void Set_RouteInfo(int _index, AITrafficWaypointRouteInfo routeInfo)
        {
            carAIWaypointRouteInfo[_index] = routeInfo;
        }
        public void Set_CurrentRoutePointIndexArray(int _index, int _value, AITrafficWaypoint _nextWaypoint)
        {
            currentRoutePointIndexArray[_index] = _value;
            currentWaypointList[_index] = _nextWaypoint;
            isChangingLanes[_index] = false;
        }
        public void Set_RouteProgressArray(int _index, float _value)
        {
            routeProgressArray[_index] = _value;
        }
        public void Set_SpeedLimitArray(int _index, float _value)
        {
            speedLimitArray[_index] = _value;
        }
        public void Set_WaypointDataListCountArray(int _index)
        {
            waypointDataListCountArray[_index] = waypointRouteList[_index].waypointDataList.Count;
        }
        public void Set_RoutePointPositionArray(int _index)
        {
            routePointPositionArray[_index] = waypointRouteList[_index].waypointDataList[currentRoutePointIndexArray[_index]]._transform.position;
        }
        public void SetVisibleState(int _index, bool _isVisible)
        {
            if (isVisibleArray.IsCreated) isVisibleArray[_index] = _isVisible;
        }
        public void Set_WaypointRoute(int _index, AITrafficWaypointRoute _route)
        {
            waypointRouteList[_index] = _route;
        }
        #endregion

        #region Get Array Data
        public float GetAccelerationInput(int _index)
        {
            return moveAccelArray[_index];
        }

        public float GetSteeringInput(int _index)
        {
            return moveSteerArray[_index];
        }

        public float GetCurrentSpeed(int _index)
        {
            return speedArray[_index];
        }

        public bool GetIsBraking(int _index)
        {
            return isBrakingArray[_index];
        }
        #endregion

        #region Registers
        public int RegisterCarAI(AITrafficCar carAI, AITrafficWaypointRoute route)
        {
            carAIList.Add(carAI);
            waypointRouteList.Add(route);
            currentWaypointList.Add(null);
            changeLaneCooldownTimer.Add(0);
            changeLaneTriggerTimer.Add(0);
            frontOriginList.Add(Vector3.zero);
            frontDirectionList.Add(Vector3.zero);
            frontRotationList.Add(Quaternion.identity);
            frontTransformCached.Add(carAI.frontSensorTransform);
            frontHitTransform.Add(null);
            frontPreviousHitTransform.Add(null);
            leftOriginList.Add(Vector3.zero);
            leftDirectionList.Add(Vector3.zero);
            leftRotationList.Add(Quaternion.identity);
            leftTransformCached.Add(carAI.leftSensorTransform);
            leftHitTransform.Add(null);
            leftPreviousHitTransform.Add(null);
            rightOriginList.Add(Vector3.zero);
            rightDirectionList.Add(Vector3.zero);
            rightRotationList.Add(Quaternion.identity);
            rightTransformCached.Add(carAI.rightSensorTransform);
            rightHitTransform.Add(null);
            rightPreviousHitTransform.Add(null);
            carAIWaypointRouteInfo.Add(null);
            brakeMaterial.Add(carAI.brakeMaterial);
            frontRightWheelColliderList.Add(carAI._wheels[0].collider);
            frontLefttWheelColliderList.Add(carAI._wheels[1].collider);
            backRighttWheelColliderList.Add(carAI._wheels[2].collider);
            backLeftWheelColliderList.Add(carAI._wheels[3].collider);
            Rigidbody rigidbody = carAI.GetComponent<Rigidbody>();
            rigidbodyList.Add(rigidbody);
            headLight.Add(carAI.headLight);
            Transform driveTarget = new GameObject("DriveTarget").transform;
            driveTarget.SetParent(carAI.transform);
            TransformAccessArray temp_driveTargetTransformAccessArray = new TransformAccessArray(carCount);
            for (int i = 0; i < carCount; i++)
            {
                temp_driveTargetTransformAccessArray.Add(driveTargetTransformAccessArray[i]);
            }
            temp_driveTargetTransformAccessArray.Add(driveTarget);
            carCount = carAIList.Count;
            if (carCount >= 2)
            {
                DisposeArrays();
            }
            #region allocation
            currentRoutePointIndexArray = new NativeArray<int>(carCount, Allocator.Persistent);
            waypointDataListCountArray = new NativeArray<int>(carCount, Allocator.Persistent);
            carTransformPreviousPositionArray = new NativeArray<Vector3>(carCount, Allocator.Persistent);
            carTransformPositionArray = new NativeArray<Vector3>(carCount, Allocator.Persistent);
            routePointPositionArray = new NativeArray<float3>(carCount, Allocator.Persistent);
            driveTargetTransformAccessArray = new TransformAccessArray(carCount);
            carTransformAccessArray = new TransformAccessArray(carCount);
            isChangingLanes = new NativeArray<bool>(carCount, Allocator.Persistent);
            canChangeLanes = new NativeArray<bool>(carCount, Allocator.Persistent);
            isDrivingArray = new NativeArray<bool>(carCount, Allocator.Persistent);
            isActiveArray = new NativeArray<bool>(carCount, Allocator.Persistent);
            speedArray = new NativeArray<float>(carCount, Allocator.Persistent);
            routeProgressArray = new NativeArray<float>(carCount, Allocator.Persistent);
            targetSpeedArray = new NativeArray<float>(carCount, Allocator.Persistent);
            accelArray = new NativeArray<float>(carCount, Allocator.Persistent);
            speedLimitArray = new NativeArray<float>(carCount, Allocator.Persistent);
            targetAngleArray = new NativeArray<float>(carCount, Allocator.Persistent);
            dragArray = new NativeArray<float>(carCount, Allocator.Persistent);
            angularDragArray = new NativeArray<float>(carCount, Allocator.Persistent);
            overrideDragArray = new NativeArray<bool>(carCount, Allocator.Persistent);
            localTargetArray = new NativeArray<Vector3>(carCount, Allocator.Persistent);
            moveSteerArray = new NativeArray<float>(carCount, Allocator.Persistent);
            moveAccelArray = new NativeArray<float>(carCount, Allocator.Persistent);
            moveFootBrakeArray = new NativeArray<float>(carCount, Allocator.Persistent);
            movehandBrakeArray = new NativeArray<float>(carCount, Allocator.Persistent);
            overrideInputArray = new NativeArray<bool>(carCount, Allocator.Persistent);
            overrideAccelerationPowerArray = new NativeArray<float>(carCount, Allocator.Persistent);
            overrideBrakePowerArray = new NativeArray<float>(carCount, Allocator.Persistent);
            isBrakingArray = new NativeArray<bool>(carCount, Allocator.Persistent);
            frontRightWheelTransformList = new TransformAccessArray(carCount);
            frontLeftWheelTransformList = new TransformAccessArray(carCount);
            backRightWheelTransformList = new TransformAccessArray(carCount);
            backLeftWheelTransformList = new TransformAccessArray(carCount);
            FRwheelPosition_CachedArray = new NativeArray<float3>(carCount, Allocator.Persistent);
            FRwheelQuaternion_CachedArray = new NativeArray<quaternion>(carCount, Allocator.Persistent);
            FLwheelPosition_CachedArray = new NativeArray<float3>(carCount, Allocator.Persistent);
            FLwheelQuaternion_CachedArray = new NativeArray<quaternion>(carCount, Allocator.Persistent);
            BRwheelPosition_CachedArray = new NativeArray<float3>(carCount, Allocator.Persistent);
            BRwheelQuaternion_CachedArray = new NativeArray<quaternion>(carCount, Allocator.Persistent);
            BLwheelPosition_CachedArray = new NativeArray<float3>(carCount, Allocator.Persistent);
            BLwheelQuaternion_CachedArray = new NativeArray<quaternion>(carCount, Allocator.Persistent);
            topSpeedArray = new NativeArray<float>(carCount, Allocator.Persistent);
            frontHitDistance = new NativeArray<float>(carCount, Allocator.Persistent);
            leftHitDistance = new NativeArray<float>(carCount, Allocator.Persistent);
            rightHitDistance = new NativeArray<float>(carCount, Allocator.Persistent);
            frontHit = new NativeArray<bool>(carCount, Allocator.Persistent);
            leftHit = new NativeArray<bool>(carCount, Allocator.Persistent);
            rightHit = new NativeArray<bool>(carCount, Allocator.Persistent);
            stopForTrafficLight = new NativeArray<bool>(carCount, Allocator.Persistent);
            routeIsActive = new NativeArray<bool>(carCount, Allocator.Persistent);
            isVisibleArray = new NativeArray<bool>(carCount, Allocator.Persistent);
            isDisabledArray = new NativeArray<bool>(carCount, Allocator.Persistent);
            withinLimitArray = new NativeArray<bool>(carCount, Allocator.Persistent);
            distanceToPlayerArray = new NativeArray<float>(carCount, Allocator.Persistent);
            isEnabledArray = new NativeArray<bool>(carCount, Allocator.Persistent);
            outOfBoundsArray = new NativeArray<bool>(carCount, Allocator.Persistent);
            lightIsActiveArray = new NativeArray<bool>(carCount, Allocator.Persistent);
            #endregion
            waypointDataListCountArray[carCount - 1] = waypointRouteList[carCount - 1].waypointDataList.Count;
            carAIWaypointRouteInfo[carCount - 1] = waypointRouteList[carCount - 1].routeInfo;
            for (int i = 0; i < carCount; i++)
            {
                driveTargetTransformAccessArray.Add(temp_driveTargetTransformAccessArray[i]);
                carTransformAccessArray.Add(carAIList[i].transform);
                frontRightWheelTransformList.Add(carAIList[i]._wheels[0].meshTransform);
                frontLeftWheelTransformList.Add(carAIList[i]._wheels[1].meshTransform);
                backRightWheelTransformList.Add(carAIList[i]._wheels[2].meshTransform);
                backLeftWheelTransformList.Add(carAIList[i]._wheels[3].meshTransform);
                frontHitDistance[i] = frontSensorLength;
                leftHitDistance[i] = sideSensorLength;
                rightHitDistance[i] = sideSensorLength;
                topSpeedArray[i] = carAIList[i].topSpeed;
                isActiveArray[i] = true;
                canChangeLanes[i] = true;
                isDrivingArray[i] = true;
            }
            temp_driveTargetTransformAccessArray.Dispose();
            return carCount - 1;
        }

        public int RegisterSpawnPoint(AITrafficSpawnPoint _TrafficSpawnPoint)
        {
            int index = trafficSpawnPoints.Count;
            trafficSpawnPoints.Add(_TrafficSpawnPoint);
            return index;
        }
        #endregion

        #region Gizmos
        private bool spawnPointsAreHidden;
        void OnDrawGizmos()
        {
            if (m_AITrafficDebug.showCarGizmos)
            {
                for (int i = 0; i < carTransformPositionArray.Length; i++)
                {
                    if (isActiveArray[i])
                    {
                        ///// Front Sensor Gizmo
                        Gizmos.color = frontHitDistance[i] == frontSensorLength ? m_AITrafficDebug.normalColor : m_AITrafficDebug.detectColor;
                        Vector3 offset = new Vector3(frontSensorSize.x * 2.0f, frontSensorSize.y * 2.0f, frontHitDistance[i]);
                        DrawCube(frontOriginList[i] + frontDirectionList[i] * (frontHitDistance[i] / 2), frontRotationList[i], offset);
                        if (m_AITrafficDebug.alwaysSideSensorGizmos)
                        {
                            #region Left Sensor
                            /// Left Sensor
                            leftOriginList[i] = leftTransformCached[i].position;
                            leftDirectionList[i] = leftTransformCached[i].forward;
                            leftRotationList[i] = leftTransformCached[i].rotation;
                            if (Physics.BoxCast(
                                leftOriginList[i],
                                sideSensorSize,
                                leftDirectionList[i],
                                out boxHit,
                                leftRotationList[i],
                                sideSensorLength,
                                layerMask,
                                QueryTriggerInteraction.UseGlobal))
                            {
                                leftHitTransform[i] = boxHit.transform; // cache transform lookup
                                if (leftHitTransform[i] != leftPreviousHitTransform[i])
                                {
                                    leftPreviousHitTransform[i] = leftHitTransform[i];
                                }
                                leftHitDistance[i] = boxHit.distance;
                                leftHit[i] = true;
                            }
                            else if (leftHit[i] != false) //ResetHitBox
                            {
                                leftHitDistance[i] = sideSensorLength;
                                leftHit[i] = false;
                            }
                            #endregion

                            #region Right Sensor
                            /// Right Sensor
                            rightOriginList[i] = rightTransformCached[i].position;
                            rightDirectionList[i] = rightTransformCached[i].forward;
                            rightRotationList[i] = rightTransformCached[i].rotation;
                            if (Physics.BoxCast(
                                rightOriginList[i],
                                sideSensorSize,
                                rightDirectionList[i],
                                out boxHit,
                                rightRotationList[i],
                                sideSensorLength,
                                layerMask,
                                QueryTriggerInteraction.UseGlobal))
                            {
                                rightHitTransform[i] = boxHit.transform; // cache transform lookup
                                if (rightHitTransform[i] != rightPreviousHitTransform[i])
                                {
                                    rightPreviousHitTransform[i] = rightHitTransform[i];
                                }
                                rightHitDistance[i] = boxHit.distance;
                                rightHit[i] = true;
                            }
                            else if (rightHit[i] != false) //ResetHitBox
                            {
                                rightHitDistance[i] = sideSensorLength;
                                rightHit[i] = false;
                            }
                            #endregion

                            ///// Left Sensor Gizmo
                            Gizmos.color = leftHitDistance[i] == sideSensorLength ? m_AITrafficDebug.normalColor : m_AITrafficDebug.detectColor;
                            offset = new Vector3(sideSensorSize.x * 2.0f, sideSensorSize.y * 2.0f, leftHitDistance[i]);
                            DrawCube(leftOriginList[i] + leftDirectionList[i] * (leftHitDistance[i] / 2), leftRotationList[i], offset);
                            ///// Right Sensor
                            Gizmos.color = rightHitDistance[i] == sideSensorLength ? m_AITrafficDebug.normalColor : m_AITrafficDebug.detectColor;
                            offset = new Vector3(sideSensorSize.x * 2.0f, sideSensorSize.y * 2.0f, rightHitDistance[i]);
                            DrawCube(rightOriginList[i] + rightDirectionList[i] * (rightHitDistance[i] / 2), rightRotationList[i], offset);
                        }
                        else
                        {
                            if (leftHit[i])//(isChangingLanes[i] == false && canChangeLanes[i]) || m_AITrafficDebug.alwaysSideSensorGizmos)
                            {
                                ///// Left Sensor Gizmo
                                Gizmos.color = leftHitDistance[i] == sideSensorLength ? m_AITrafficDebug.normalColor : m_AITrafficDebug.detectColor;
                                offset = new Vector3(sideSensorSize.x * 2.0f, sideSensorSize.y * 2.0f, leftHitDistance[i]);
                                DrawCube(leftOriginList[i] + leftDirectionList[i] * (leftHitDistance[i] / 2), leftRotationList[i], offset);
                            }
                            else if (rightHit[i])
                            {
                                ///// Right Sensor Gizmo
                                Gizmos.color = rightHitDistance[i] == sideSensorLength ? m_AITrafficDebug.normalColor : m_AITrafficDebug.detectColor;
                                offset = new Vector3(sideSensorSize.x * 2.0f, sideSensorSize.y * 2.0f, rightHitDistance[i]);
                                DrawCube(rightOriginList[i] + rightDirectionList[i] * (rightHitDistance[i] / 2), rightRotationList[i], offset);
                            }
                        }
                    }
                }
            }
            if (_AITrafficPool.hideSpawnPointsInEditMode && spawnPointsAreHidden == false)
            {
                spawnPointsAreHidden = true;
                AITrafficSpawnPoint[] spawnPoints = FindObjectsOfType<AITrafficSpawnPoint>();
                for (int i = 0; i < spawnPoints.Length; i++)
                {
                    spawnPoints[i].GetComponent<MeshRenderer>().enabled = false;
                }
            }
            else if (_AITrafficPool.hideSpawnPointsInEditMode == false && spawnPointsAreHidden)
            {
                spawnPointsAreHidden = false;
                AITrafficSpawnPoint[] spawnPoints = FindObjectsOfType<AITrafficSpawnPoint>();
                for (int i = 0; i < spawnPoints.Length; i++)
                {
                    spawnPoints[i].GetComponent<MeshRenderer>().enabled = true;
                }
            }
        }
        void DrawCube(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Matrix4x4 cubeTransform = Matrix4x4.TRS(position, rotation, scale);
            Matrix4x4 oldGizmosMatrix = Gizmos.matrix;
            Gizmos.matrix *= cubeTransform;
            Gizmos.DrawCube(Vector3.zero, Vector3.one);
            Gizmos.matrix = oldGizmosMatrix;
        }
        #endregion

        #region TrafficPool
        public AITrafficCar GetCarFromPool(AITrafficWaypointRoute parentRoute)
        {
            loadCar = trafficPool[0].trafficPrefab;
            isDisabledArray[trafficPool[0].assignedIndex] = false;
            rigidbodyList[trafficPool[0].assignedIndex].isKinematic = false;
            EnableCar(carAIList[trafficPool[0].assignedIndex].assignedIndex, parentRoute);
            trafficPool.RemoveAt(0);
            return loadCar;
        }

        public void EnableCar(int _index, AITrafficWaypointRoute parentRoute)
        {
            isActiveArray[_index] = true;
            carAIList[_index].gameObject.SetActive(true);
            waypointRouteList[_index] = parentRoute;
            carAIWaypointRouteInfo[_index] = parentRoute.routeInfo;
            carAIList[_index].StartDriving();
        }

        public void MoveCarToPool(int _index)
        {
            canChangeLanes[_index] = false;
            isChangingLanes[_index] = false;
            isDisabledArray[_index] = true;
            isActiveArray[_index] = false;
            carAIList[_index].StopDriving();
            carAIList[_index].gameObject.SetActive(false);
            newTrafficPoolEntry = new AITrafficPoolEntry();
            newTrafficPoolEntry.assignedIndex = _index;
            newTrafficPoolEntry.trafficPrefab = carAIList[_index];
            trafficPool.Add(newTrafficPoolEntry);
        }

        public void MoveAllCarsToPool()
        {
            for (int i = 0; i < isActiveArray.Length; i++)
            {
                if (isActiveArray[i])
                {
                    canChangeLanes[i] = false;
                    isChangingLanes[i] = false;
                    isDisabledArray[i] = true;
                    isActiveArray[i] = false;
                    carAIList[i].StopDriving();
                    carAIList[i].gameObject.SetActive(false);
                    newTrafficPoolEntry = new AITrafficPoolEntry();
                    newTrafficPoolEntry.assignedIndex = i;
                    newTrafficPoolEntry.trafficPrefab = carAIList[i];
                    trafficPool.Add(newTrafficPoolEntry);
                }
            }
        }

        void SpawnTraffic()
        {
            spawnTimer = 0f;
            availableSpawnPoints.Clear();
            for (int i = 0; i < trafficSpawnPoints.Count; i++) // Get Available Spawn Points From All Zones
            {
                distanceToSpawnPoint = Vector3.Distance(centerPosition, trafficSpawnPoints[i].transformCached.position);
                if ((distanceToSpawnPoint > _AITrafficPool.actizeZone || (distanceToSpawnPoint > _AITrafficPool.minSpawnZone && trafficSpawnPoints[i].isVisible == false))
                    && distanceToSpawnPoint < _AITrafficPool.spawnZone && trafficSpawnPoints[i].isTrigger == false)
                {
                    availableSpawnPoints.Add(trafficSpawnPoints[i]);
                }
            }
            currentDensity = carAIList.Count - trafficPool.Count;
            if (currentDensity < _AITrafficPool.density) //Spawn Traffic
            {
                currentAmountToSpawn = _AITrafficPool.density - currentDensity;
                for (int i = 0; i < currentAmountToSpawn; i++)
                {
                    if (availableSpawnPoints.Count == 0 || trafficPool.Count == 0) break;
                    randomSpawnPointIndex = UnityEngine.Random.Range(0, availableSpawnPoints.Count);
                    if (availableSpawnPoints[randomSpawnPointIndex].waypoint.onReachWaypointSettings.parentRoute.currentDensity < availableSpawnPoints[randomSpawnPointIndex].waypoint.onReachWaypointSettings.parentRoute.maxDensity)
                    {
                        availableSpawnPoints[randomSpawnPointIndex].waypoint.onReachWaypointSettings.parentRoute.currentDensity += 1;
                        spawncar = GetCarFromPool(availableSpawnPoints[randomSpawnPointIndex].waypoint.onReachWaypointSettings.parentRoute);
                        Vector3 spawnPosition = availableSpawnPoints[randomSpawnPointIndex].transformCached.position;
                        Vector3 spawnOffset = new Vector3(0, -4, 0);
                        spawnPosition += spawnOffset;
                        spawncar.transform.SetPositionAndRotation(

                            spawnPosition,
                            availableSpawnPoints[randomSpawnPointIndex].transformCached.rotation
                            );
                        spawncar.transform.LookAt(availableSpawnPoints[randomSpawnPointIndex].waypoint.onReachWaypointSettings.parentRoute.waypointDataList[availableSpawnPoints[randomSpawnPointIndex].waypoint.onReachWaypointSettings.waypointIndexnumber]._transform);
                        availableSpawnPoints.RemoveAt(randomSpawnPointIndex);
                    }
                }
            }
        }

        IEnumerator SpawnStartupTraffic()
        {
            yield return new WaitForEndOfFrame();
            availableSpawnPoints.Clear();
            for (int i = 0; i < trafficSpawnPoints.Count; i++) // Get Available Spawn Points From All Zones
            {
                distanceToSpawnPoint = Vector3.Distance(centerPosition, trafficSpawnPoints[i].transformCached.position);
                if (trafficSpawnPoints[i].isTrigger == false)
                {
                    availableSpawnPoints.Add(trafficSpawnPoints[i]);
                }
            }
            for (int i = 0; i < _AITrafficPool.density; i++) // Spawn Traffic
            {
                for (int j = 0; j < _AITrafficPool.trafficPrefabs.Length; j++)
                {
                    if (availableSpawnPoints.Count == 0) break;
                    randomSpawnPointIndex = UnityEngine.Random.Range(0, availableSpawnPoints.Count);
                    Vector3 spawnPosition = availableSpawnPoints[randomSpawnPointIndex].transformCached.position;
                    Vector3 spawnOffset = new Vector3(0, -4, 0);
                    spawnPosition += spawnOffset;
                    GameObject spawnedTrafficVehicle = Instantiate(_AITrafficPool.trafficPrefabs[j], spawnPosition, availableSpawnPoints[randomSpawnPointIndex].transformCached.rotation);
                    spawnedTrafficVehicle.GetComponent<AITrafficCar>().RegisterCar(availableSpawnPoints[randomSpawnPointIndex].waypoint.onReachWaypointSettings.parentRoute);
                    spawnedTrafficVehicle.transform.LookAt(availableSpawnPoints[randomSpawnPointIndex].waypoint.onReachWaypointSettings.parentRoute.waypointDataList[availableSpawnPoints[randomSpawnPointIndex].waypoint.onReachWaypointSettings.waypointIndexnumber]._transform);
                    availableSpawnPoints.RemoveAt(randomSpawnPointIndex);
                }
            }

            for (int i = 0; i < _AITrafficPool.density; i++)
            {
                if (carCount >= _AITrafficPool.density) break;
                for (int j = 0; j < _AITrafficPool.trafficPrefabs.Length; j++)
                {
                    if (carCount >= _AITrafficPool.density) break;
                    GameObject spawnedTrafficVehicle = Instantiate(_AITrafficPool.trafficPrefabs[j], Vector3.zero, Quaternion.identity);
                    spawnedTrafficVehicle.GetComponent<AITrafficCar>().RegisterCar(waypointRouteList[0]);
                    MoveCarToPool(spawnedTrafficVehicle.GetComponent<AITrafficCar>().assignedIndex);
                }
            }

            for (int i = 0; i < carCount; i++)
            {
                routePointPositionArray[i] = waypointRouteList[i].waypointDataList[currentRoutePointIndexArray[i]]._transform.position;
                carAIList[i].StartDriving();
            }

            isInitialized = true;
        }
        #endregion
        
    }
}