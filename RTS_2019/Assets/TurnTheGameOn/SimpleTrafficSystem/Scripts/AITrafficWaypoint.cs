namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine;
    using System.Collections.Generic;

    public class AITrafficWaypoint : MonoBehaviour
    {
        public AITrafficCarOnReachWaypointInfo onReachWaypointSettings;
        private Collider m_collider;

        private void OnEnable()
        {
            onReachWaypointSettings.position = transform.position;
            if (onReachWaypointSettings.waypointIndexnumber < onReachWaypointSettings.parentRoute.waypointDataList.Count)
            {
                onReachWaypointSettings.waypoint = this;
            }
        }

        void OnTriggerEnter(Collider col)
        {
            col.transform.root.SendMessage("OnReachedWaypoint", onReachWaypointSettings, SendMessageOptions.DontRequireReceiver);
            if (onReachWaypointSettings.waypointIndexnumber == onReachWaypointSettings.parentRoute.waypointDataList.Count)
            {
                if (onReachWaypointSettings.newRoutePoints.Length == 0)
                {
                    col.transform.root.SendMessage("StopDriving", SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        public void TriggerNextWaypoint(AITrafficCar _AITrafficCar)
        {
            _AITrafficCar.OnReachedWaypoint(onReachWaypointSettings);
            if (onReachWaypointSettings.waypointIndexnumber == onReachWaypointSettings.parentRoute.waypointDataList.Count)
            {
                if (onReachWaypointSettings.newRoutePoints.Length == 0)
                {
                    _AITrafficCar.StopDriving();
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (m_collider == null) m_collider = GetComponent<Collider>();
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.DrawCube(Vector3.zero, m_collider.bounds.size);
        }

        private List<AITrafficWaypoint> newWaypointList = new List<AITrafficWaypoint>();

        public void RemoveMissingLaneChangePoints()
        {
            newWaypointList = new List<AITrafficWaypoint>();
            for (int i = 0; i < onReachWaypointSettings.laneChangePoints.Count; i++)
            {
                if (onReachWaypointSettings.laneChangePoints[i] != null)
                {
                    newWaypointList.Add(onReachWaypointSettings.laneChangePoints[i]);
                }
            }
            onReachWaypointSettings.laneChangePoints = new List<AITrafficWaypoint>(newWaypointList);
        }

        public void RemoveMissingNewRoutePoints()
        {
            newWaypointList.Clear();
            for (int i = 0; i < onReachWaypointSettings.newRoutePoints.Length; i++)
            {
                if (onReachWaypointSettings.newRoutePoints[i] != null)
                {
                    newWaypointList.Add(onReachWaypointSettings.newRoutePoints[i]);
                }
            }
            onReachWaypointSettings.newRoutePoints = newWaypointList.ToArray();
        }

    }
}