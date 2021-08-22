namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine;

    public class AITrafficCar : MonoBehaviour
    {
        public int assignedIndex { get; private set; } // assigned AITrafficController array index
        public float topSpeed = 25f;
        public Transform frontSensorTransform;
        public Transform leftSensorTransform;
        public Transform rightSensorTransform;
        public AITrafficCarWheels[] _wheels;
        public Material brakeMaterial;
        public Light headLight;
        public bool goToStartOnStop; // if not using pooling, car will teleport to first point of starting route and resume driving after stop driving is called.
        private AITrafficWaypointRoute startRoute;
        private Vector3 goToPointWhenStoppedVector3;
        private Rigidbody rb;

        public void RegisterCar(AITrafficWaypointRoute route)
        {
            if (brakeMaterial == null)
            {
                MeshRenderer mesh = GetComponent<MeshRenderer>();
                brakeMaterial = mesh.material;
            }
            assignedIndex = AITrafficController.Instance.RegisterCarAI(this, route);
            startRoute = route;
            rb = GetComponent<Rigidbody>();
        }

        #region Public API Methods
        /// These methods can be used to get AITrafficCar variables and call functions
        /// intended to be used by other MonoBehaviours.

        /// <summary>
        /// Returns current acceleration input as a float 0-1.
        /// </summary>
        /// <returns></returns>
        public float AccelerationInput()
        {
            return AITrafficController.Instance.GetAccelerationInput(assignedIndex);
        }

        /// <summary>
        /// Returns current steering input as a float -1 to 1.
        /// </summary>
        /// <returns></returns>
        public float SteeringInput()
        {
            return AITrafficController.Instance.GetSteeringInput(assignedIndex);
        }

        /// <summary>
        /// Returns current speed as a float.
        /// </summary>
        /// <returns></returns>
        public float CurrentSpeed()
        {
            return AITrafficController.Instance.GetCurrentSpeed(assignedIndex);
        }

        /// <summary>
        /// Returns current breaking input state as a bool.
        /// </summary>
        /// <returns></returns>
        public bool IsBraking()
        {
            return AITrafficController.Instance.GetIsBraking(assignedIndex);
        }

        /// <summary>
        /// The AITraffic car will start driving.
        /// </summary>
        [ContextMenu("StartDriving")]
        public void StartDriving()
        {
            AITrafficController.Instance.Set_IsDrivingArray(assignedIndex, true);
        }

        /// <summary>
        /// The AITraffic Car will stop driving.
        /// </summary>
        [ContextMenu("StopDriving")]
        public void StopDriving()
        {
            if (goToStartOnStop)
            {
                ChangeToRouteWaypoint(startRoute.waypointDataList[0]._waypoint.onReachWaypointSettings);
                goToPointWhenStoppedVector3 = startRoute.waypointDataList[0]._transform.position;
                goToPointWhenStoppedVector3.y += 1;
                transform.position = goToPointWhenStoppedVector3;
                transform.LookAt(startRoute.waypointDataList[1]._transform);
                rb.velocity = Vector3.zero;
            }
            else
            {
                AITrafficController.Instance.Set_IsDrivingArray(assignedIndex, false);
            }
        }

        /// <summary>
        /// Disables the AITrafficCar and returns it to the AITrafficController pool
        /// </summary>
        [ContextMenu("MoveCarToPool")]
        public void MoveCarToPool()
        {
            AITrafficController.Instance.MoveCarToPool(assignedIndex);
        }
        #endregion

        #region Waypoint Trigger Methods
        /// <summary>
        /// Callback triggered when the AITrafficCar reaches a waypoint.
        /// </summary>
        /// <param name="onReachWaypointSettings"></param>
        public void OnReachedWaypoint(AITrafficCarOnReachWaypointInfo onReachWaypointSettings)
        {
            if (onReachWaypointSettings.parentRoute == AITrafficController.Instance.waypointRouteList[assignedIndex])
            {
                onReachWaypointSettings.OnReachWaypointEvent.Invoke();
                AITrafficController.Instance.Set_SpeedLimitArray(assignedIndex, onReachWaypointSettings.speedLimit);
                AITrafficController.Instance.Set_RouteProgressArray(assignedIndex, onReachWaypointSettings.waypointIndexnumber - 1);
                AITrafficController.Instance.Set_WaypointDataListCountArray(assignedIndex);
                if (onReachWaypointSettings.newRoutePoints.Length > 0)
                {
                    int randomIndex = UnityEngine.Random.Range(0, onReachWaypointSettings.newRoutePoints.Length);
                    if (randomIndex == onReachWaypointSettings.newRoutePoints.Length) randomIndex -= 1;
                    AITrafficController.Instance.Set_WaypointRoute(assignedIndex, onReachWaypointSettings.newRoutePoints[randomIndex].onReachWaypointSettings.parentRoute);
                    AITrafficController.Instance.Set_RouteInfo(assignedIndex, onReachWaypointSettings.newRoutePoints[randomIndex].onReachWaypointSettings.parentRoute.routeInfo);
                    AITrafficController.Instance.Set_RouteProgressArray(assignedIndex, onReachWaypointSettings.newRoutePoints[randomIndex].onReachWaypointSettings.waypointIndexnumber - 1);
                    AITrafficController.Instance.Set_CurrentRoutePointIndexArray
                        (
                        assignedIndex,
                        onReachWaypointSettings.newRoutePoints[randomIndex].onReachWaypointSettings.waypointIndexnumber,
                        onReachWaypointSettings.newRoutePoints[randomIndex].onReachWaypointSettings.waypoint
                        );
                }
                else if (onReachWaypointSettings.waypointIndexnumber < onReachWaypointSettings.parentRoute.waypointDataList.Count)
                {
                    AITrafficController.Instance.Set_CurrentRoutePointIndexArray
                        (
                        assignedIndex,
                        onReachWaypointSettings.waypointIndexnumber,
                        onReachWaypointSettings.waypoint
                        );
                }
                AITrafficController.Instance.Set_RoutePointPositionArray(assignedIndex);
                if (onReachWaypointSettings.stopDriving) StopDriving();
            }
        }

        /// <summary>
        /// Used by AITrafficController to instruct the AITrafficCar to change lanes.
        /// </summary>
        /// <param name="onReachWaypointSettings"></param>
        public void ChangeToRouteWaypoint(AITrafficCarOnReachWaypointInfo onReachWaypointSettings)
        {
            onReachWaypointSettings.OnReachWaypointEvent.Invoke();
            AITrafficController.Instance.Set_SpeedLimitArray(assignedIndex, onReachWaypointSettings.speedLimit);
            AITrafficController.Instance.Set_WaypointDataListCountArray(assignedIndex);
            AITrafficController.Instance.Set_WaypointRoute(assignedIndex, onReachWaypointSettings.parentRoute);
            AITrafficController.Instance.Set_RouteInfo(assignedIndex, onReachWaypointSettings.parentRoute.routeInfo);
            AITrafficController.Instance.Set_RouteProgressArray(assignedIndex, onReachWaypointSettings.waypointIndexnumber - 1);
            AITrafficController.Instance.Set_CurrentRoutePointIndexArray
                (
                assignedIndex,
                onReachWaypointSettings.waypointIndexnumber,
                onReachWaypointSettings.waypoint
                );

            AITrafficController.Instance.Set_RoutePointPositionArray(assignedIndex);
        }
        #endregion

        #region Callbacks
        void OnBecameInvisible()
        {
            AITrafficController.Instance.SetVisibleState(assignedIndex, false);
        }

        void OnBecameVisible()
        {
            AITrafficController.Instance.SetVisibleState(assignedIndex, true);
        }
        #endregion
    }
}