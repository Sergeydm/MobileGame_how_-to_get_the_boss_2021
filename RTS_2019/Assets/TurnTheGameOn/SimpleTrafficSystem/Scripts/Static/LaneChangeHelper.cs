namespace TurnTheGameOn.SimpleTrafficSystem
{
    public static class LaneChangeHelper
    {
        public static void AssignLaneChangePoints(AITrafficWaypointRoute getFrom, AITrafficWaypointRoute assignTo)
        {
            for (int i = 2; i < assignTo.waypointDataList.Count; i++) // skip first 2 waypoints
            {
                if (i < assignTo.waypointDataList.Count - 3) // skip last 3 waypoints
                {
                    if (getFrom.waypointDataList.Count > i + 1)
                    {
                        assignTo.waypointDataList[i]._waypoint.onReachWaypointSettings.laneChangePoints.Add
                            (getFrom.waypointDataList[i + 1]._waypoint);
                    }
                }
            }
        }
    }
}