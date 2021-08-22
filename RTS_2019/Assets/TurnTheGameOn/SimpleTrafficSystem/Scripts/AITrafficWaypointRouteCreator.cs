namespace TurnTheGameOn.SimpleTrafficSystem
{
    using System.Collections.Generic;
    using UnityEngine;

    [ExecuteInEditMode]
    public class AITrafficWaypointRouteCreator : MonoBehaviour
    {
        #region Base
        public Transform startControlPoint;
        public Transform endControlPoint;
        public List<Transform> controlPointsList;
        public enum Routes
        {
            One = 1,
            Two = 2,
            Three = 3,
            Four = 4,
            Five = 5,
            Six = 6,
            Seven = 7,
            Eight = 8,
            Nine = 9,
            Ten = 10
        }
        public Routes routes;
        [System.Serializable]
        public class RouteSettings
        {
            public AITrafficWaypointRoute route;
            public Vector3 offset;
        }
        public List<RouteSettings> routeSettings;

        public int spawnedPoints;
        public bool loopSpline = false;
        [Range(0.001f, 1f)] public float waypointFrequency = 0.1f;
        public bool requiresUpdate;
        public List<Vector3> previousOffset;
        public List<Vector3> defaultOffset;


        public bool isInitialized;

        private void Start()
        {
            if (!isInitialized)
            {
                Initialize();
            }
        }

        public void Initialize()
        {
            transform.position = Vector3.zero;
            startControlPoint = new GameObject("startControlPoint").transform;
            endControlPoint = new GameObject("endControlPoint").transform;
            startControlPoint.transform.SetParent(transform, true);
            endControlPoint.transform.SetParent(transform, true);
            controlPointsList = new List<Transform>();
            controlPointsList.Add(startControlPoint);
            controlPointsList.Add(endControlPoint);
            routeSettings = new List<RouteSettings>();
            spawnedPoints = 0;
            isInitialized = true;
        }
        #endregion

        public void AddWaypoint(Vector3 _position)
        {
            spawnedPoints += 1;
            GameObject newWaypoint = new GameObject("controlPoint");
            newWaypoint.transform.position = _position;
            controlPointsList.Insert(spawnedPoints, newWaypoint.transform);
            newWaypoint.transform.SetParent(transform, true);
            Refresh();
        }

        public void InsertWaypoint(Vector3 position)
        {
            spawnedPoints += 1;
            GameObject newWaypoint = new GameObject("controlPoint insert");
            newWaypoint.transform.position = position;
            bool isBetweenPoints = false;
            int insertIndex = 0;
            if (controlPointsList.Count >= 2)
            {
                for (int i = 0; i < controlPointsList.Count - 1; i++)
                {
                    Vector3 point_A = controlPointsList[i].position;
                    Vector3 point_B = controlPointsList[i + 1].position;
                    isBetweenPoints = IsCBetweenAB(point_A, point_B, position);
                    insertIndex = i + 1;
                    if (isBetweenPoints) break;
                }
            }

            if (isBetweenPoints)
            {
                controlPointsList.Insert(insertIndex, newWaypoint.transform);
            }
            else
            {
                insertIndex = spawnedPoints;
                controlPointsList.Insert(insertIndex, newWaypoint.transform);
            }

            if (spawnedPoints == 2)
            {
                controlPointsList[0].position = controlPointsList[1].position + new Vector3(0, 0, 5);
            }
            if (spawnedPoints >= 2 && (controlPointsList.Count - 2) == insertIndex)
            {
                controlPointsList[controlPointsList.Count - 1].position = controlPointsList[controlPointsList.Count - 2].position + new Vector3(0, 0, 5);
            }
            newWaypoint.transform.SetParent(transform, true);
            Refresh();
        }

        bool IsCBetweenAB(Vector3 A, Vector3 B, Vector3 C)
        {
            return (
                Vector3.Dot((B - A).normalized, (C - B).normalized) < 0f && Vector3.Dot((A - B).normalized, (C - A).normalized) < 0f &&
                Vector3.Distance(A, B) >= Vector3.Distance(A, C) &&
                Vector3.Distance(A, B) >= Vector3.Distance(B, C)
                );
        }

        public void Refresh()
        {
            UpdateRoutesEditorOnly();
            UpdateWaypoints();
            if (spawnedPoints == 2)
            {
                startControlPoint.position = controlPointsList[2].position;
                startControlPoint.parent = controlPointsList[1];
                controlPointsList[1].Rotate(new Vector3(0, -180, 0));
                startControlPoint.parent = transform;
                controlPointsList[1].Rotate(new Vector3(0, 180, 0));
            }
            if (spawnedPoints >= 2)
            {
                endControlPoint.position = controlPointsList[controlPointsList.Count - 3].position;
                endControlPoint.parent = controlPointsList[controlPointsList.Count - 2];
                controlPointsList[controlPointsList.Count - 2].Rotate(new Vector3(0, -180, 0));
                endControlPoint.parent = transform;
                controlPointsList[controlPointsList.Count - 2].Rotate(new Vector3(0, 180, 0));
            }
        }

        void UpdateRoutesEditorOnly()
        {
            for (int i = 0; i < routeSettings.Count; i++)
            {
                if (routeSettings[i].route != null)
                    DestroyImmediate(routeSettings[i].route.gameObject); // use Destroy for runtime
            }
            previousOffset = new List<Vector3>();
            for (int i = 0; i < routeSettings.Count; i++)
            {
                previousOffset.Add(routeSettings[i].offset);
            }
            routeSettings.Clear();
            for (int i = 0; i < (int)routes; i++)
            {
                GameObject objectToSpawn = Instantiate(Resources.Load("AITrafficWaypointRoute")) as GameObject;
                objectToSpawn.name = "AITrafficWaypointRoute";

                RouteSettings _routeSettings = new RouteSettings();
                _routeSettings.route = objectToSpawn.GetComponent<AITrafficWaypointRoute>();
                if (previousOffset.Count - 1 >= i)
                    _routeSettings.offset = previousOffset[i];
                else if (defaultOffset.Count - 1 >= i)
                    _routeSettings.offset = defaultOffset[i];
                routeSettings.Add(_routeSettings);
            }
        }

        void UpdateWaypoints()
        {
            Vector3[] controlPoints = new Vector3[controlPointsList.Count];
            for (int i = 0; i < controlPointsList.Count; i++)
            {
                controlPoints[i] = controlPointsList[i].position;
            }
            for (int i = 0; i < routeSettings.Count; i++)
            {
                if (waypointFrequency >= 0.0001f)
                {
                    float progress = 0f;
                    while (progress <= 1 && waypointFrequency <= 1)
                    {
                        Vector3 spawnPoint = new Vector3();
                        Transform newPoint = routeSettings[i].route.ClickToSpawnNextWaypoint(spawnPoint);
                        GetPointOnSpline(progress, controlPoints, routeSettings[i].offset, newPoint, routeSettings[i].route.transform);
                        progress += waypointFrequency;
                    }
                    Vector3 spawnPointFinal = new Vector3();
                    Transform newPointFinal = routeSettings[i].route.ClickToSpawnNextWaypoint(spawnPointFinal);
                    GetPointOnSpline(progress, controlPoints, routeSettings[i].offset, newPointFinal, routeSettings[i].route.transform);
                }
            }
        }

        public static Vector3 GetPointOnSpline(float percentage, Vector3[] cPoints, Vector3 offset, Transform waypoint, Transform originalParent)//, GameObject testPositionObject, Vector3 offset)
        {
            if (cPoints.Length >= 4)
            {
                //Convert the input range (0 to 1) to range (0 to numSections)
                int numSections = cPoints.Length - 3;
                int curPoint = Mathf.Min(Mathf.FloorToInt(percentage * (float)numSections), numSections - 1);
                float t = percentage * (float)numSections - (float)curPoint;

                //Get the 4 control points around the location to be sampled.
                Vector3 p0 = cPoints[curPoint];
                Vector3 p1 = cPoints[curPoint + 1];
                Vector3 p2 = cPoints[curPoint + 2];
                Vector3 p3 = cPoints[curPoint + 3];

                //The Catmull-Rom spline can be written as:
                // 0.5 * (2*P1 + (-P0 + P2) * t + (2*P0 - 5*P1 + 4*P2 - P3) * t^2 + (-P0 + 3*P1 - 3*P2 + P3) * t^3)
                //Variables P0 to P3 are the control points.
                //Variable t is the position on the spline, with a range of 0 to numSections.
                //C# way of writing the function. Note that f means float (to force precision).
                Vector3 result = .5f * (2f * p1 + (-p0 + p2) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * (t * t) + (-p0 + 3f * p1 - 3f * p2 + p3) * (t * t * t));

                Transform parentForRotation = new GameObject().transform;
                parentForRotation.position = new Vector3(result.x, result.y, result.z);
                waypoint.position = new Vector3(result.x, result.y, result.z);
                waypoint.SetParent(parentForRotation, true);

                Transform lookPoint = new GameObject().transform;
                lookPoint.position = GetPointOnSplineToLookAt(percentage + 0.001f, cPoints);
                parentForRotation.LookAt(lookPoint);
                waypoint.localPosition = new Vector3(offset.x, offset.y, offset.z);
                waypoint.SetParent(originalParent, true);

                DestroyImmediate(lookPoint.gameObject);
                DestroyImmediate(parentForRotation.gameObject);

                Vector3 offsetPos = new Vector3(result.x + offset.x, result.y + offset.y, result.z + offset.z);
                return offsetPos;
            }
            else
            {
                return new Vector3(0, 0, 0);
            }
        }

        public static Vector3 GetControlPointOnSpline(float percentage, Vector3[] cPoints)
        {
            if (cPoints.Length >= 4)
            {
                //Convert the input range (0 to 1) to range (0 to numSections)
                int numSections = cPoints.Length - 3;
                int curPoint = Mathf.Min(Mathf.FloorToInt(percentage * (float)numSections), numSections - 1);
                float t = percentage * (float)numSections - (float)curPoint;

                //Get the 4 control points around the location to be sampled.
                Vector3 p0 = cPoints[curPoint];
                Vector3 p1 = cPoints[curPoint + 1];
                Vector3 p2 = cPoints[curPoint + 2];
                Vector3 p3 = cPoints[curPoint + 3];

                //The Catmull-Rom spline can be written as:
                // 0.5 * (2*P1 + (-P0 + P2) * t + (2*P0 - 5*P1 + 4*P2 - P3) * t^2 + (-P0 + 3*P1 - 3*P2 + P3) * t^3)
                //Variables P0 to P3 are the control points.
                //Variable t is the position on the spline, with a range of 0 to numSections.
                //C# way of writing the function. Note that f means float (to force precision).
                Vector3 result = .5f * (2f * p1 + (-p0 + p2) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * (t * t) + (-p0 + 3f * p1 - 3f * p2 + p3) * (t * t * t));
                return result;
            }
            else
            {
                return new Vector3(0, 0, 0);
            }
        }


        public static Vector3 GetPointOnSplineToLookAt(float percentage, Vector3[] cPoints)
        {
            if (cPoints.Length >= 4)
            {

                //Convert the input range (0 to 1) to range (0 to numSections)
                int numSections = cPoints.Length - 3;
                int curPoint = Mathf.Min(Mathf.FloorToInt(percentage * (float)numSections), numSections - 1);
                float t = percentage * (float)numSections - (float)curPoint;

                //Get the 4 control points around the location to be sampled.
                Vector3 p0 = cPoints[curPoint];
                Vector3 p1 = cPoints[curPoint + 1];
                Vector3 p2 = cPoints[curPoint + 2];
                Vector3 p3 = cPoints[curPoint + 3];

                //The Catmull-Rom spline can be written as:
                // 0.5 * (2*P1 + (-P0 + P2) * t + (2*P0 - 5*P1 + 4*P2 - P3) * t^2 + (-P0 + 3*P1 - 3*P2 + P3) * t^3)
                //Variables P0 to P3 are the control points.
                //Variable t is the position on the spline, with a range of 0 to numSections.
                //C# way of writing the function. Note that f means float (to force precision).
                Vector3 result = .5f * (2f * p1 + (-p0 + p2) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * (t * t) + (-p0 + 3f * p1 - 3f * p2 + p3) * (t * t * t));

                return new Vector3(result.x, result.y, result.z);
            }

            else
            {
                return new Vector3(0, 0, 0);
            }
        }

    }
}