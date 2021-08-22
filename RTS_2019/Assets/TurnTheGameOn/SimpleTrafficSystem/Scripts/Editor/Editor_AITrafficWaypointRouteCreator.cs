namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine;
    using UnityEditor;
    using System.Collections.Generic;

    [CustomEditor(typeof(AITrafficWaypointRouteCreator))]
    public class Editor_AITrafficWaypointRouteCreator : Editor
    {
        bool isInitialized;

        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour((AITrafficWaypointRouteCreator)target), typeof(AITrafficWaypointRouteCreator), false);
            GUI.enabled = true;

            AITrafficWaypointRouteCreator routeCreator = (AITrafficWaypointRouteCreator)target;

            for (int i = 0; i < routeCreator.routeSettings.Count; i++)
            {
                if (routeCreator.routeSettings[i] == null)
                {
                    routeCreator.routeSettings.Clear();
                }
            }

            UpdateRoutes(routeCreator);
            UpdateWaypoints(routeCreator);

            if (isInitialized)
            {
                if (GUILayout.Button("Start New"))
                {
                    List<Transform> oldCPs = new List<Transform>();
                    oldCPs = routeCreator.controlPointsList;
                    GUIUtility.hotControl = 0;
                    GUIUtility.keyboardControl = 0;
                    for (int i = 0; i < routeCreator.routeSettings.Count; i++)
                    {
                        routeCreator.routeSettings[i] = null;
                    }
                    routeCreator.Initialize();

                    routeCreator.requiresUpdate = true;
                    EditorUtility.SetDirty(this);
                    Repaint();
                    for (int i = 0; i < oldCPs.Count; i++)
                    {
                        DestroyImmediate(oldCPs[i].gameObject);
                    }
                }
                EditorGUILayout.HelpBox("This tool creates a spline and generates AITrafficWaypointRoutes with waypoints. Changing inspector settings will regenerate the route. Delete this object when you are done generating the route.", MessageType.None);
                EditorGUILayout.HelpBox("Shift + Left Click    in scene view on a Collider to add new points to the route", MessageType.None);
                EditorGUILayout.HelpBox("Shift + Ctrl + Left Click    in scene view on a Collider to add new points to the route", MessageType.None);

                if (GUILayout.Button("Refresh"))
                {
                    GUIUtility.hotControl = 0;
                    GUIUtility.keyboardControl = 0;
                    isInitialized = false;
                    routeCreator.requiresUpdate = true;
                    EditorUtility.SetDirty(this);
                    Repaint();

                }

                if (isInitialized)
                {
                    SerializedProperty routes = serializedObject.FindProperty("routes");
                    EditorGUILayout.PropertyField(routes, true);

                    SerializedProperty waypointFrequency = serializedObject.FindProperty("waypointFrequency");
                    EditorGUILayout.PropertyField(waypointFrequency, true);

                    SerializedProperty defaultOffset = serializedObject.FindProperty("defaultOffset");
                    EditorGUILayout.PropertyField(defaultOffset, true);

                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                    for (int i = 0; i < routeCreator.routeSettings.Count; i++)
                    {
                        EditorGUILayout.LabelField("   " + (i + 1).ToString(), EditorStyles.boldLabel);

                        SerializedProperty route = serializedObject.FindProperty("routeSettings").GetArrayElementAtIndex(i).FindPropertyRelative("route");
                        EditorGUILayout.PropertyField(route, true);

                        SerializedProperty offset = serializedObject.FindProperty("routeSettings").GetArrayElementAtIndex(i).FindPropertyRelative("offset");
                        EditorGUILayout.PropertyField(offset, true);

                        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
            for (int i = 0; i < routeCreator.controlPointsList.Count; i++)
            {
                routeCreator.controlPointsList[i].transform.SetSiblingIndex(i);
            }
        }

        void UpdateRoutes(AITrafficWaypointRouteCreator routeCreator)
        {
            if (routeCreator.requiresUpdate)
            {
                for (int i = 0; i < routeCreator.routeSettings.Count; i++)
                {
                    if (routeCreator.routeSettings[i].route != null)
                        DestroyImmediate(routeCreator.routeSettings[i].route.gameObject);
                }
                routeCreator.previousOffset = new List<Vector3>();
                for (int i = 0; i < routeCreator.routeSettings.Count; i++)
                {
                    routeCreator.previousOffset.Add(routeCreator.routeSettings[i].offset);
                }
                routeCreator.routeSettings.Clear();
                for (int i = 0; i < (int)routeCreator.routes; i++)
                {
                    GameObject objectToSpawn = Instantiate(Resources.Load("AITrafficWaypointRoute")) as GameObject;
                    objectToSpawn.name = "AITrafficWaypointRoute";

                    AITrafficWaypointRouteCreator.RouteSettings _routeSettings = new AITrafficWaypointRouteCreator.RouteSettings();
                    _routeSettings.route = objectToSpawn.GetComponent<AITrafficWaypointRoute>();
                    if (routeCreator.previousOffset.Count - 1 >= i)
                        _routeSettings.offset = routeCreator.previousOffset[i];
                    else if (routeCreator.defaultOffset.Count - 1 >= i)
                        _routeSettings.offset = routeCreator.defaultOffset[i];
                    routeCreator.routeSettings.Add(_routeSettings);
                }
            }
        }

        void UpdateWaypoints(AITrafficWaypointRouteCreator routeCreator)
        {
            if (routeCreator.requiresUpdate)
            {
                Vector3[] controlPoints = new Vector3[routeCreator.controlPointsList.Count];
                for (int i = 0; i < routeCreator.controlPointsList.Count; i++)
                {
                    controlPoints[i] = routeCreator.controlPointsList[i].position;
                }
                for (int i = 0; i < routeCreator.routeSettings.Count; i++)
                {
                    if (routeCreator.waypointFrequency >= 0.0001f)
                    {
                        float progress = 0f;
                        while (progress <= 1 && routeCreator.waypointFrequency <= 1)
                        {
                            Vector3 spawnPoint = new Vector3();
                            Transform newPoint = routeCreator.routeSettings[i].route.ClickToSpawnNextWaypoint(spawnPoint);
                            GetPointOnSpline(progress, controlPoints, routeCreator.routeSettings[i].offset, newPoint, routeCreator.routeSettings[i].route.transform);
                            progress += routeCreator.waypointFrequency;
                        }
                        Vector3 spawnPointFinal = new Vector3();
                        Transform newPointFinal = routeCreator.routeSettings[i].route.ClickToSpawnNextWaypoint(spawnPointFinal);
                        GetPointOnSpline(progress, controlPoints, routeCreator.routeSettings[i].offset, newPointFinal, routeCreator.routeSettings[i].route.transform);
                    }
                }
                routeCreator.requiresUpdate = false;
            }
        }

        void OnSceneGUI()
        {
            SerializedObject serialObj = new SerializedObject(this);
            AITrafficWaypointRouteCreator routeCreator = (AITrafficWaypointRouteCreator)target;

            #region Spawn Control Points
            Event e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && e.shift && e.button == 0 && e.control)
            {
                int controlId = GUIUtility.GetControlID(FocusType.Passive);

                Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                RaycastHit hitInfo;
                if (Physics.Raycast(worldRay, out hitInfo))
                {
                    InsertWaypoint(hitInfo.point, true);
                }

                GUIUtility.hotControl = controlId;
                e.Use();
            }
            else if (e.type == EventType.MouseDown && e.button == 0 && e.shift)
            {
                int controlId = GUIUtility.GetControlID(FocusType.Passive);

                Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                RaycastHit hitInfo;
                if (Physics.Raycast(worldRay, out hitInfo))
                {
                    AddWaypoint(hitInfo.point, true);
                }

                GUIUtility.hotControl = controlId;
                e.Use();
            }
            #endregion

            #region Handles
            if (routeCreator.controlPointsList.Count >= 4)
            {
                Handles.color = Color.white;
                for (int i = 0; i < routeCreator.controlPointsList.Count; i++) //Draw the Catmull-Rom spline between the points
                {
                    //Cant draw between the endpoints Neither do we need to draw from the second to the last endpoint
                    //...if we are not making a looping line
                    if ((i == 0 || i == routeCreator.controlPointsList.Count - 2 || i == routeCreator.controlPointsList.Count - 1) && !routeCreator.loopSpline)
                    {
                        continue;
                    }
                    DisplayCatmullRomSpline(i, routeCreator);
                }
                Handles.color = Color.red;
                if (routeCreator.spawnedPoints >= 2)
                {
                    Handles.DrawLine(routeCreator.controlPointsList[0].position, routeCreator.controlPointsList[1].position);
                    Handles.DrawLine(routeCreator.controlPointsList[routeCreator.controlPointsList.Count - 1].position, routeCreator.controlPointsList[routeCreator.controlPointsList.Count - 2].position);
                }
                SceneView.RepaintAll();
            }
            for (int i = 0; i < routeCreator.controlPointsList.Count; i++)
            {
                if (i == 0 || i == routeCreator.controlPointsList.Count - 1) Handles.color = Color.red;
                else Handles.color = Color.yellow;
                Handles.SphereHandleCap(0, routeCreator.controlPointsList[i].position, Quaternion.identity, 1f, EventType.Repaint);
                routeCreator.controlPointsList[i].position = Handles.PositionHandle(routeCreator.controlPointsList[i].position, Quaternion.identity);
            }
            #endregion

            serialObj.ApplyModifiedProperties();
        }

        protected virtual void OnEnable()
        {
            EditorApplication.update += OnEditorUpdate;
        }

        protected virtual void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }

        float timer = 0f;

        protected virtual void OnEditorUpdate()
        {
            if (timer >= 0.35f)
            {
                timer = 0f;
                isInitialized = true;
            }
            else if (!isInitialized)
            {
                timer += 1f * Time.deltaTime;
            }
        }

        void Refresh()
        {
            AITrafficWaypointRouteCreator routeCreator = (AITrafficWaypointRouteCreator)target;
            UpdateRoutes(routeCreator);
            UpdateWaypoints(routeCreator);
            if (routeCreator.spawnedPoints == 2)
            {
                routeCreator.startControlPoint.position = routeCreator.controlPointsList[2].position;
                routeCreator.startControlPoint.parent = routeCreator.controlPointsList[1];
                routeCreator.controlPointsList[1].Rotate(new Vector3(0, -180, 0));
                routeCreator.startControlPoint.parent = routeCreator.transform;
                routeCreator.controlPointsList[1].Rotate(new Vector3(0, 180, 0));
            }
            if (routeCreator.spawnedPoints >= 2)
            {
                routeCreator.endControlPoint.position = routeCreator.controlPointsList[routeCreator.controlPointsList.Count - 3].position;
                routeCreator.endControlPoint.parent = routeCreator.controlPointsList[routeCreator.controlPointsList.Count - 2];
                routeCreator.controlPointsList[routeCreator.controlPointsList.Count - 2].Rotate(new Vector3(0, -180, 0));
                routeCreator.endControlPoint.parent = routeCreator.transform;
                routeCreator.controlPointsList[routeCreator.controlPointsList.Count - 2].Rotate(new Vector3(0, 180, 0));
            }
        }

        bool IsCBetweenAB(Vector3 A, Vector3 B, Vector3 C)
        {
            return (
                Vector3.Dot((B - A).normalized, (C - B).normalized) < 0f && Vector3.Dot((A - B).normalized, (C - A).normalized) < 0f &&
                Vector3.Distance(A, B) >= Vector3.Distance(A, C) &&
                Vector3.Distance(A, B) >= Vector3.Distance(B, C)
                );
        }

        void AddWaypoint(Vector3 _position, bool _requireUpdate)
        {
            AITrafficWaypointRouteCreator routeCreator = (AITrafficWaypointRouteCreator)target;

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("AddWaypoint");
            var undoGroupIndex = Undo.GetCurrentGroup();
            Undo.RegisterCompleteObjectUndo(routeCreator, "AddWaypoint");


            if (_requireUpdate) routeCreator.requiresUpdate = true;
            routeCreator.spawnedPoints += 1;
            GameObject newWaypoint = new GameObject("controlPoint");
            newWaypoint.transform.position = _position;
            routeCreator.controlPointsList.Insert(routeCreator.spawnedPoints, newWaypoint.transform);
            //if (routeCreator.spawnedPoints == 2)
            //{
            //    routeCreator.controlPointsList[0].position = routeCreator.controlPointsList[1].position + new Vector3(0, 0, 25);
            //}
            //if (routeCreator.spawnedPoints >= 2)
            //{
            //    routeCreator.controlPointsList[routeCreator.controlPointsList.Count - 1].position = routeCreator.controlPointsList[routeCreator.controlPointsList.Count - 2].position + new Vector3(0, 0, 25);
            //}
            Refresh();
            newWaypoint.transform.SetParent(routeCreator.transform, true);
            EditorUtility.SetDirty(this);
            Repaint();

            Undo.RegisterCreatedObjectUndo(newWaypoint, "AddWaypoint");
            Undo.RegisterCompleteObjectUndo(routeCreator, "AddWaypoint");
            Undo.CollapseUndoOperations(undoGroupIndex);
        }

        void InsertWaypoint(Vector3 position, bool _requireUpdate)
        {
            AITrafficWaypointRouteCreator routeCreator = (AITrafficWaypointRouteCreator)target;

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("InsertWaypoint");
            var undoGroupIndex = Undo.GetCurrentGroup();
            Undo.RegisterCompleteObjectUndo(routeCreator, "InsertWaypoint");

            if (_requireUpdate) routeCreator.requiresUpdate = true;
            routeCreator.spawnedPoints += 1;
            GameObject newWaypoint = new GameObject("controlPoint insert");
            newWaypoint.transform.position = position;
            bool isBetweenPoints = false;
            int insertIndex = 0;
            if (routeCreator.controlPointsList.Count >= 2)
            {
                for (int i = 0; i < routeCreator.controlPointsList.Count - 1; i++)
                {
                    Vector3 point_A = routeCreator.controlPointsList[i].position;
                    Vector3 point_B = routeCreator.controlPointsList[i + 1].position;
                    isBetweenPoints = IsCBetweenAB(point_A, point_B, position);
                    insertIndex = i + 1;
                    if (isBetweenPoints) break;
                }
            }

            if (isBetweenPoints)
            {
                routeCreator.controlPointsList.Insert(insertIndex, newWaypoint.transform);
            }
            else
            {
                insertIndex = routeCreator.spawnedPoints;
                routeCreator.controlPointsList.Insert(insertIndex, newWaypoint.transform);
            }

            //if (routeCreator.spawnedPoints == 2)
            //{
            //    routeCreator.controlPointsList[0].position = routeCreator.controlPointsList[1].position + new Vector3(0, 0, 5);
            //}
            //if (routeCreator.spawnedPoints >= 2 && (routeCreator.controlPointsList.Count - 2) == insertIndex)
            //{
            //    routeCreator.controlPointsList[routeCreator.controlPointsList.Count - 1].position = routeCreator.controlPointsList[routeCreator.controlPointsList.Count - 2].position + new Vector3(0, 0, 5);
            //}
            Refresh();
            newWaypoint.transform.SetParent(routeCreator.transform, true);
            EditorUtility.SetDirty(this);
            Repaint();

            Undo.RegisterCreatedObjectUndo(newWaypoint, "InsertWaypoint");
            Undo.RegisterCompleteObjectUndo(routeCreator, "InsertWaypoint");
            Undo.CollapseUndoOperations(undoGroupIndex);
        }

        void DisplayCatmullRomSpline(int pos, AITrafficWaypointRouteCreator routeCreator) //Display a spline between 2 points derived with the Catmull-Rom spline algorithm
        {
            //The 4 points we need to form a spline between p1 and p2
            Vector3 p0 = routeCreator.controlPointsList[ClampListPos(pos - 1, routeCreator)].position;
            Vector3 p1 = routeCreator.controlPointsList[pos].position;
            Vector3 p2 = routeCreator.controlPointsList[ClampListPos(pos + 1, routeCreator)].position;
            Vector3 p3 = routeCreator.controlPointsList[ClampListPos(pos + 2, routeCreator)].position;
            Vector3 lastPos = p1; //The start position of the line
            float resolution = 0.2f; //Make sure it's is adding up to 1, so 0.3 will give a gap, but 0.2 will work
            int loops = Mathf.FloorToInt(1f / resolution); //How many times should we loop?
            for (int i = 1; i <= loops; i++)
            {
                float t = i * resolution; //Which t position are we at?
                Vector3 newPos = GetCatmullRomPosition(t, p0, p1, p2, p3); //Find the coordinate between the end points with a Catmull-Rom spline
                Handles.DrawLine(lastPos, newPos);
                lastPos = newPos; //Save this pos so we can draw the next line segment
            }
        }

        int ClampListPos(int pos, AITrafficWaypointRouteCreator routeCreator) //Clamp the list positions to allow looping
        {
            if (pos < 0)
            {
                pos = routeCreator.controlPointsList.Count - 1;
            }

            if (pos > routeCreator.controlPointsList.Count)
            {
                pos = 1;
            }
            else if (pos > routeCreator.controlPointsList.Count - 1)
            {
                pos = 0;
            }

            return pos;
        }

        Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3) //Returns a position between 4 Vector3 with Catmull-Rom spline algorithm
        {
            //The coefficients of the cubic polynomial (except the 0.5f * which I added later for performance)
            Vector3 a = 2f * p1;
            Vector3 b = p2 - p0;
            Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
            Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;
            //The cubic polynomial: a + b * t + c * t^2 + d * t^3
            Vector3 pos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));
            return pos;
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