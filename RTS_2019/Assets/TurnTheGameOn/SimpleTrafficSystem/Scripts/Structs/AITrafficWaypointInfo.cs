namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine;

    [System.Serializable]
    public struct CarAIWaypointInfo
    {
        public string _name;
        public Transform _transform;
        public AITrafficWaypoint _waypoint;
    }
}