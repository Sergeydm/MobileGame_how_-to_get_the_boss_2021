namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine;
    using UnityEditor;

    [CustomEditor(typeof(AITrafficWaypoint)), CanEditMultipleObjects]
    public class Editor_AITrafficWaypoint : Editor
    {
        void OnSceneGUI()
        {
            AITrafficWaypoint waypoint = (AITrafficWaypoint)target;

            Handles.Label
                (
                waypoint.transform.position + new Vector3(0, 0.25f, 0),
            "    Waypoint Number: " + waypoint.onReachWaypointSettings.waypointIndexnumber.ToString() + "\n"
            );
        }
    }
}