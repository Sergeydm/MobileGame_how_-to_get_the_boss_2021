namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine;
    using UnityEditor;
    using UnityEditorInternal;
    using UnityEditor.SceneManagement;
    using System.Collections.Generic;

    [CanEditMultipleObjects]
    [CustomEditor(typeof(AITrafficWaypointRoute))]
    public class Editor_AITrafficWaypointRoute : Editor
    {
        AITrafficWaypointRoute circuit;
        ReorderableList reorderableList;
        float lineHeight;
        float lineHeightSpace;
        private bool showWaypoints;

        void OnEnable()
        {
            lineHeight = EditorGUIUtility.singleLineHeight;
            lineHeightSpace = lineHeight + 0;
            circuit = (AITrafficWaypointRoute)target;
            reorderableList = new ReorderableList(serializedObject, serializedObject.FindProperty("waypointDataList"), false, true, false, true);
            reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                SerializedProperty element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);

                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), element.FindPropertyRelative("_name").stringValue);
                EditorGUI.indentLevel = 7;
                GUI.enabled = false;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + (lineHeightSpace * 0), rect.width, lineHeight), element.FindPropertyRelative("_transform"));
                GUI.enabled = true;
                EditorGUI.indentLevel = 0;
                reorderableList.elementHeightCallback = (int _index) => lineHeightSpace * 2;
            };
            reorderableList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "Waypoints");
            };
            reorderableList.onRemoveCallback = (ReorderableList list) =>
            {
                GameObject go = circuit.waypointDataList[reorderableList.index]._transform.gameObject;
                circuit.waypointDataList.RemoveAt(reorderableList.index);
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.BeginVertical(EditorStyles.inspectorFullWidthMargins);
            EditorStyles.label.wordWrap = true;

            EditorGUILayout.HelpBox("Shift + Left Click in scene view on a Collider to add new points to the route", MessageType.None);
            EditorGUILayout.HelpBox("Shift + Ctrl + Left Click in scene view on a Collider to insert new points to the route", MessageType.None);

            SerializedProperty m_AITrafficGizmoSettings = serializedObject.FindProperty("m_AITrafficGizmoSettings");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_AITrafficGizmoSettings, true);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();

            showWaypoints = EditorGUILayout.Foldout(showWaypoints, "Waypoints", true);
            if (showWaypoints)
            {
                EditorGUILayout.BeginVertical("Box");
                EditorGUI.BeginChangeCheck();
                reorderableList.DoLayoutList();
                if (EditorGUI.EndChangeCheck())
                    serializedObject.ApplyModifiedProperties();
                EditorGUILayout.EndVertical();
            }

            SerializedProperty spawnTrafficVehicles = serializedObject.FindProperty("spawnTrafficVehicles");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(spawnTrafficVehicles, true);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();

            SerializedProperty useSpawnPoints = serializedObject.FindProperty("useSpawnPoints");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(useSpawnPoints, true);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();

            SerializedProperty maxDensity = serializedObject.FindProperty("maxDensity");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(maxDensity, true);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();

            EditorGUILayout.EndVertical();

            if (GUILayout.Button("Reverse Waypoints"))
            {
                circuit.ReversePoints();
                EditorUtility.SetDirty(this);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }

            if (GUILayout.Button("Align Waypoints"))
            {
                circuit.AlignPoints();
                EditorUtility.SetDirty(this);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }

            if (GUILayout.Button("Setup Random Spawn Points"))
            {
                
                if (circuit.waypointDataList.Count > 4)
                {
                    Undo.RegisterFullObjectHierarchyUndo(circuit.gameObject, "Remove All Spawn Points");
                    AITrafficSpawnPoint[] spawnPoints = circuit.GetComponentsInChildren<AITrafficSpawnPoint>();
                    for (int i = 0; i < spawnPoints.Length; i++)
                    {
                        string message = "removing old spawn point " + i.ToString() + "/" + spawnPoints.Length.ToString();
                        EditorUtility.DisplayProgressBar("Setup Random Spawn Points", message, i / (float)spawnPoints.Length);
                        Undo.DestroyObjectImmediate(spawnPoints[i].gameObject);
                    }
                    int randomIndex = UnityEngine.Random.Range(0, 3);
                    for (int i = randomIndex; i < circuit.waypointDataList.Count && i < circuit.waypointDataList.Count - 2; i += UnityEngine.Random.Range(2, 4))
                    {
                        string message = "updating route point " + i.ToString() + "/" + circuit.waypointDataList.Count.ToString();
                        EditorUtility.DisplayProgressBar("Setup Random Spawn Points", message, i / (float)circuit.waypointDataList.Count);
                        GameObject loadedSpawnPoint = Instantiate(Resources.Load("AITrafficSpawnPoint"), circuit.waypointDataList[i]._transform) as GameObject;
                        Undo.RegisterCreatedObjectUndo(loadedSpawnPoint, "AITrafficSpawnPoint");
                        AITrafficSpawnPoint trafficSpawnPoint = loadedSpawnPoint.GetComponent<AITrafficSpawnPoint>();
                        trafficSpawnPoint.waypoint = trafficSpawnPoint.transform.parent.GetComponent<AITrafficWaypoint>();
                    }
                    Undo.FlushUndoRecordObjects();
                    EditorUtility.SetDirty(this);
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                    EditorUtility.ClearProgressBar();
                }
            }

            if (GUILayout.Button("Remove All Spawn Points"))
            {
                Undo.RegisterFullObjectHierarchyUndo(circuit.gameObject, "Remove All Spawn Points");
                AITrafficSpawnPoint[] spawnPoints = circuit.GetComponentsInChildren<AITrafficSpawnPoint>();
                for (int i = 0; i < spawnPoints.Length; i++)
                {
                    string message = "removing spawn point " + i.ToString() + "/" + spawnPoints.Length.ToString();
                    EditorUtility.DisplayProgressBar("Remove All Spawn Points", message, i / (float)spawnPoints.Length);
                    Undo.DestroyObjectImmediate(spawnPoints[i].gameObject);
                }
                Undo.FlushUndoRecordObjects();
                EditorUtility.SetDirty(this);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                EditorUtility.ClearProgressBar();
            }
        }

        void OnSceneGUI()
        {
            AITrafficWaypointRoute _AITrafficWaypointRoute = (AITrafficWaypointRoute)target;

            for (int i = 0; i < _AITrafficWaypointRoute.waypointDataList.Count; i++)
            {
                if (_AITrafficWaypointRoute.waypointDataList[i]._waypoint)
                {
                    GUIStyle style = new GUIStyle();
                    string target = "";
                    style.normal.textColor = Color.green;
                    Handles.Label(_AITrafficWaypointRoute.waypointDataList[i]._waypoint.transform.position + new Vector3(0, 0.25f, 0),
                    "    Waypoint:   " + _AITrafficWaypointRoute.waypointDataList[i]._waypoint.onReachWaypointSettings.waypointIndexnumber.ToString() + "\n" +
                    "    SpeedLimit: " + _AITrafficWaypointRoute.waypointDataList[i]._waypoint.onReachWaypointSettings.speedLimit + "\n" +
                    target,
                    style
                    );
                }
            }

            Event e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && e.shift && e.button == 0 && e.control)
            {
                int controlId = GUIUtility.GetControlID(FocusType.Passive);

                Ray worldRay = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                RaycastHit hitInfo;
                if (Physics.Raycast(worldRay, out hitInfo))
                {
                    ClickToInsertSpawnNextWaypoint(_AITrafficWaypointRoute, hitInfo.point);
                }

                GUIUtility.hotControl = controlId;
                e.Use();
            }
            else if (e.type == EventType.MouseDown && e.button == 0 && e.shift)
            {
                int controlId = GUIUtility.GetControlID(FocusType.Passive);

                Ray worldRay = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                RaycastHit hitInfo;
                if (Physics.Raycast(worldRay, out hitInfo))
                {
                    ClickToSpawnNextWaypoint(_AITrafficWaypointRoute, hitInfo.point);
                }

                GUIUtility.hotControl = controlId;
                e.Use();
            }
        }

        public Transform ClickToSpawnNextWaypoint(AITrafficWaypointRoute __AITrafficWaypointRoute, Vector3 _position)
        {
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("ClickToSpawnNextWaypoint");
            var undoGroupIndex = Undo.GetCurrentGroup();
            Undo.RegisterCompleteObjectUndo(__AITrafficWaypointRoute, "ClickToSpawnNextWaypoint");
            GameObject newWaypoint = Instantiate(Resources.Load("AITrafficWaypoint"), _position, Quaternion.identity, __AITrafficWaypointRoute.transform) as GameObject;
            CarAIWaypointInfo newPoint = new CarAIWaypointInfo();
            newPoint._name = newWaypoint.name = "AITrafficWaypoint " + (__AITrafficWaypointRoute.waypointDataList.Count + 1);
            newPoint._transform = newWaypoint.transform;
            newPoint._waypoint = newWaypoint.GetComponent<AITrafficWaypoint>();
            newPoint._waypoint.onReachWaypointSettings.waypointIndexnumber = __AITrafficWaypointRoute.waypointDataList.Count + 1;
            newPoint._waypoint.onReachWaypointSettings.parentRoute = __AITrafficWaypointRoute;
            newPoint._waypoint.onReachWaypointSettings.speedLimit = 25f;
            __AITrafficWaypointRoute.waypointDataList.Add(newPoint);
            Undo.RegisterCreatedObjectUndo(newWaypoint, "ClickToSpawnNextWaypoint");
            Undo.RegisterCompleteObjectUndo(__AITrafficWaypointRoute, "ClickToSpawnNextWaypoint");
            Undo.CollapseUndoOperations(undoGroupIndex);
            return newPoint._transform;
        }

        public void ClickToInsertSpawnNextWaypoint(AITrafficWaypointRoute __AITrafficWaypointRoute, Vector3 _position)
        {
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("ClickToInsertSpawnNextWaypoint");
            var undoGroupIndex = Undo.GetCurrentGroup();
            Undo.RegisterCompleteObjectUndo(__AITrafficWaypointRoute, "ClickToInsertSpawnNextWaypoint");
            List<Object> waypointObjectList = new List<Object>();
            for (int i = 0; i < __AITrafficWaypointRoute.waypointDataList.Count; i++)
            {
                waypointObjectList.Add(__AITrafficWaypointRoute.waypointDataList[i]._waypoint);
            }
            Object[] waypointObjectArray = waypointObjectList.ToArray();
            Undo.RegisterCompleteObjectUndo(waypointObjectArray, "ClickToSpawnNextWaypoint");
            bool isBetweenPoints = false;
            int insertIndex = 0;
            if (__AITrafficWaypointRoute.waypointDataList.Count >= 2)
            {
                for (int i = 0; i < __AITrafficWaypointRoute.waypointDataList.Count - 1; i++)
                {
                    Vector3 point_A = __AITrafficWaypointRoute.waypointDataList[i]._transform.position;
                    Vector3 point_B = __AITrafficWaypointRoute.waypointDataList[i + 1]._transform.position;
                    isBetweenPoints = IsCBetweenAB(point_A, point_B, _position);
                    insertIndex = i + 1;
                    if (isBetweenPoints) break;
                }
            }

            GameObject newWaypoint = Instantiate(Resources.Load("AITrafficWaypoint"), _position, Quaternion.identity, __AITrafficWaypointRoute.transform) as GameObject;
            CarAIWaypointInfo newPoint = new CarAIWaypointInfo();
            newPoint._transform = newWaypoint.transform;
            newPoint._waypoint = newWaypoint.GetComponent<AITrafficWaypoint>();
            newPoint._waypoint.onReachWaypointSettings.parentRoute = __AITrafficWaypointRoute;
            newPoint._waypoint.onReachWaypointSettings.speedLimit = 25f;
            if (isBetweenPoints)
            {
                newPoint._transform.SetSiblingIndex(insertIndex);
                newPoint._name = newWaypoint.name = "AITrafficWaypoint " + (insertIndex + 1);
                __AITrafficWaypointRoute.waypointDataList.Insert(insertIndex, newPoint);
                for (int i = 0; i < __AITrafficWaypointRoute.waypointDataList.Count; i++)
                {
                    int newIndexName = i + 1;
                    __AITrafficWaypointRoute.waypointDataList[i]._transform.gameObject.name = "AITrafficWaypoint " + newIndexName;
                    __AITrafficWaypointRoute.waypointDataList[i]._waypoint.onReachWaypointSettings.waypointIndexnumber = i + 1;
                }
            }
            else
            {
                newPoint._name = newWaypoint.name = "AITrafficWaypoint " + (__AITrafficWaypointRoute.waypointDataList.Count + 1);
                newPoint._waypoint.onReachWaypointSettings.waypointIndexnumber = __AITrafficWaypointRoute.waypointDataList.Count + 1;
                __AITrafficWaypointRoute.waypointDataList.Add(newPoint);
            }
            Undo.RegisterCreatedObjectUndo(newWaypoint, "ClickToInsertSpawnNextWaypoint");
            Undo.RegisterCompleteObjectUndo(__AITrafficWaypointRoute, "ClickToInsertSpawnNextWaypoint");
            Undo.CollapseUndoOperations(undoGroupIndex);
        }

        bool IsCBetweenAB(Vector3 A, Vector3 B, Vector3 C)
        {
            return (
                Vector3.Dot((B - A).normalized, (C - B).normalized) < 0f && Vector3.Dot((A - B).normalized, (C - A).normalized) < 0f &&
                Vector3.Distance(A, B) >= Vector3.Distance(A, C) &&
                Vector3.Distance(A, B) >= Vector3.Distance(B, C)
                );
        }

    }
}