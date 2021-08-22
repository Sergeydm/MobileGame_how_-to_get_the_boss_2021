namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using System.Collections.Generic;

    public class Editor_STSWindow : EditorWindow
    {
        #region Initialization
        [MenuItem("Tools/Simple Traffic System/STS Tools Window")]
        public static void ShowWindow()
        {
            Editor_STSWindow window = (Editor_STSWindow)GetWindow(typeof(Editor_STSWindow));
            window.minSize = new Vector2(300, 200);
            window.titleContent.text = "STS Tools";
            Color defaultTransparentWhite = new Color(1, 1, 1, 0.8f);
            Color defaultTransparentRed = new Color(1, 0, 0, 0.8f);
            window.signalConnectorHandleColor = window.GetSavedColor("signalConnectorHandleColor", defaultTransparentWhite);
            window.signalConnectorHandleActiveColor = window.GetSavedColor("signalConnectorHandleActiveColor", defaultTransparentRed);
            window.signalConnectorHandleTextColor = window.GetSavedColor("signalConnectorHandleTextColor", Color.black);
            window.signalConnectorHandleTextActiveColor = window.GetSavedColor("signalConnectorHandleTextActiveColor", Color.white);
            window.signalConnectionColor = window.GetSavedColor("signalConnectionColor", Color.red);
            window.spawnPointHandleColor = window.GetSavedColor("spawnPointHandleColor", defaultTransparentWhite);
            window.spawnPointHandleTextColor = window.GetSavedColor("spawnPointHandleTextColor", Color.black);
            window.fromHandleColor = window.GetSavedColor("fromHandleColor", defaultTransparentWhite);
            window.fromHandleActiveColor = window.GetSavedColor("fromHandleActiveColor", defaultTransparentRed);
            window.fromHandleTextColor = window.GetSavedColor("fromHandleTextColor", Color.black);
            window.fromHandleTextActiveColor = window.GetSavedColor("fromHandleTextActiveColor", Color.white);
            window.toHandleColor = window.GetSavedColor("toHandleColor", defaultTransparentWhite);
            window.toHandleActiveColor = window.GetSavedColor("toHandleActiveColor", defaultTransparentRed);
            window.toHandleTextColor = window.GetSavedColor("toHandleTextColor", Color.black);
            window.toHandleTextActiveColor = window.GetSavedColor("toHandleTextActiveColor", Color.white);
            window.laneConnectorHandleColor = window.GetSavedColor("laneConnectorHandleColor", defaultTransparentWhite);
            window.laneConnectorHandleActiveColor = window.GetSavedColor("laneConnectorHandleActiveColor", defaultTransparentRed);
            window.laneConnectorHandleTextColor = window.GetSavedColor("laneConnectorHandleTextColor", Color.black);
            window.laneConnectorHandleTextActiveColor = window.GetSavedColor("laneConnectorHandleTextActiveColor", Color.white);
            window.laneConnectionColor = window.GetSavedColor("laneConnectionColor", Color.red);
            window.handleDrawDistance = EditorPrefs.GetFloat("handleDrawDistance", 150f);
            window.Show();
        }
        #endregion

        #region WindowVariables
        public static Editor_STSWindow editorWindow;
        Vector2 scrollPos = new Vector2();
        public enum EditMode
        {
            Off,
            LaneConnector,
            RouteConnector,
            RouteEditor,
            SignalConnector,
            SpawnPoints
        }
        [SerializeField] EditMode editMode;
        bool showDebug = false;
        bool requireRepaint;
        bool showCustomizationOptions = false;
        bool showSpawnButtons = true;
        public AITrafficWaypointRoute[] _routes;
        // Signal Connector
        public AITrafficLight[] SC_lights;
        [SerializeField] public AITrafficLight SC_light;
        [SerializeField] public AITrafficWaypointRoute SC_route;
        public int SC_lightIndex = -1;
        public int SC_routeIndex = -1;
        // Route Connector
        public AITrafficWaypoint RC_fromPoint;
        public AITrafficWaypoint RC_toPoint;
        public int RC_fromRouteIndex = -1;
        public int RC_fromPointIndex = -1;
        public int RC_toRouteIndex = -1;
        public int RC_toPointIndex = -1;
        // Lane Connector
        public AITrafficWaypointRoute LC_routeA;
        public AITrafficWaypointRoute LC_routeB;
        public int LC_routeIndexA = -1;
        public int LC_routeIndexB = -1;
        // Route Editor
        public AITrafficWaypointRoute RE_route;
        public List<AITrafficWaypoint> RE_connectedWaypoints;
        public int RE_routeIndex = -1;
        public enum RouteEditorProperty
        {
            ObjectOnly,
            All,
            SpeedLimit,
            StopDriving,
            NewRoutePoints,
            LaneChangePoints,
            OnReachWaypointEvent
        }
        public RouteEditorProperty routeEditorProperty;
        // Handle Settings
        public Color signalConnectorHandleColor;
        public Color signalConnectorHandleActiveColor;
        public Color signalConnectorHandleTextColor;
        public Color signalConnectorHandleTextActiveColor;
        public Color signalConnectionColor;
        public Color spawnPointHandleColor;
        public Color spawnPointHandleTextColor;
        public Color fromHandleColor;
        public Color fromHandleActiveColor;
        public Color fromHandleTextColor;
        public Color fromHandleTextActiveColor;
        public Color toHandleColor;
        public Color toHandleActiveColor;
        public Color toHandleTextColor;
        public Color toHandleTextActiveColor;
        public Color laneConnectorHandleColor;
        public Color laneConnectorHandleActiveColor;
        public Color laneConnectorHandleTextColor;
        public Color laneConnectorHandleTextActiveColor;
        public Color laneConnectionColor;
        public float handleDrawDistance;
        float handleSize = 1f;
        float handleMaxSizeDistance = 50f;
        Vector3 handleTextoffset = new Vector3(0, 0.5f, 0);
        #endregion

        

        #region InspectorWindow GUI
        void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.MaxWidth(1000), GUILayout.MaxHeight(2000));
            SerializedObject serialObj = new SerializedObject(this);
            var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 18 };
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical("Box");
            showSpawnButtons = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), showSpawnButtons, "   Spawn", true);
            if (showSpawnButtons)
            {
                if (GUILayout.Button("AI Traffic Controller"))
                {
                    GameObject _objectToSpawn = Instantiate(Resources.Load("AITrafficController")) as GameObject;
                    _objectToSpawn.name = "AITrafficController";
                    GameObject[] newSelection = new GameObject[1];
                    newSelection[0] = _objectToSpawn;
                    Selection.objects = newSelection;
                    Undo.RegisterCreatedObjectUndo(_objectToSpawn, "SpawnAITrafficController");
                }
                if (GUILayout.Button("AI Traffic Waypoint Route"))
                {
                    GameObject _objectToSpawn = Instantiate(Resources.Load("AITrafficWaypointRoute")) as GameObject;
                    _objectToSpawn.name = "AITrafficWaypointRoute";
                    GameObject[] newSelection = new GameObject[1];
                    newSelection[0] = _objectToSpawn;
                    Selection.objects = newSelection;
                    Undo.RegisterCreatedObjectUndo(_objectToSpawn, "SpawnWaypointRoute");
                }
                if (GUILayout.Button("AI Traffic Light Manager"))
                {
                    GameObject _objectToSpawn = Instantiate(Resources.Load("AITrafficLightManager")) as GameObject;
                    _objectToSpawn.name = "AITrafficLightManager";
                    GameObject[] newSelection = new GameObject[1];
                    newSelection[0] = _objectToSpawn;
                    Selection.objects = newSelection;
                    Undo.RegisterCreatedObjectUndo(_objectToSpawn, "SpawnTrafficLightManager");
                }
                if (GUILayout.Button("Spline Route Creator"))
                {
                    GameObject _objectToSpawn = Instantiate(Resources.Load("AITrafficWaypointRouteCreator")) as GameObject;
                    _objectToSpawn.name = "AITrafficWaypointRouteCreator";
                    GameObject[] newSelection = new GameObject[1];
                    newSelection[0] = _objectToSpawn;
                    Selection.objects = newSelection;
                    Undo.RegisterCreatedObjectUndo(_objectToSpawn, "AITrafficWaypointRouteCreator");
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            EditorGUI.BeginChangeCheck();
            EditMode newEditMode = (EditMode)EditorGUILayout.EnumPopup("Configure Mode", editMode);
            if (EditorGUI.EndChangeCheck())
            {
                UndoRecordTargetObject(this, "Mode Changed");
                ClearData(true);
                editMode = newEditMode;
                SceneView.RepaintAll();
            }

            if (requireRepaint)
            {
                requireRepaint = false;
                Repaint();
            }

            switch (editMode)
            {
                case EditMode.LaneConnector:
                    #region LaneConnectorGUI
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("LANE CONNECTOR", style);
                    EditorGUILayout.Space();
                    if (_routes.Length > 0)
                    {
                        if (_routes[0] == null) ClearData(true);
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("Reload Routes", GUILayout.Width(100)))
                        {
                            UndoRecordTargetObject(this, "Load Routes");
                            _routes = FindObjectsOfType<AITrafficWaypointRoute>();
                            SceneView.RepaintAll();
                        }

                        if (GUILayout.Button("Unload Routes", GUILayout.Width(100)))
                        {
                            UndoRecordTargetObject(this, "Unload Routes");
                            ClearData(true);
                        }
                        EditorGUILayout.LabelField("Debug", EditorStyles.label, GUILayout.Width(40));
                        showDebug = EditorGUILayout.Toggle("", showDebug, GUILayout.Width(20));
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        if (GUILayout.Button("Load Routes"))
                        {
                            UndoRecordTargetObject(this, "Load Routes");
                            _routes = FindObjectsOfType<AITrafficWaypointRoute>();
                            SceneView.RepaintAll();
                        }
                    }
                    if (showDebug)
                    {
                        EditorGUILayout.Space();
                        showCustomizationOptions = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), showCustomizationOptions, "Handle Customization", true);
                        if (showCustomizationOptions)
                        {
                            EditorGUI.BeginChangeCheck();
                            laneConnectorHandleColor = EditorGUILayout.ColorField("Handle", laneConnectorHandleColor);
                            if (EditorGUI.EndChangeCheck()) SaveColor("laneConnectorHandleColor", laneConnectorHandleColor);

                            EditorGUI.BeginChangeCheck();
                            laneConnectorHandleActiveColor = EditorGUILayout.ColorField("Handle Active", laneConnectorHandleActiveColor);
                            if (EditorGUI.EndChangeCheck()) SaveColor("laneConnectorHandleActiveColor", laneConnectorHandleActiveColor);

                            EditorGUI.BeginChangeCheck();
                            laneConnectorHandleTextColor = EditorGUILayout.ColorField("Handle Text", laneConnectorHandleTextColor);
                            if (EditorGUI.EndChangeCheck()) SaveColor("laneConnectorHandleTextColor", laneConnectorHandleTextColor);

                            EditorGUI.BeginChangeCheck();
                            laneConnectorHandleTextActiveColor = EditorGUILayout.ColorField("Handle Text Active", laneConnectorHandleTextActiveColor);
                            if (EditorGUI.EndChangeCheck()) SaveColor("laneConnectorHandleTextActiveColor", laneConnectorHandleTextActiveColor);

                            EditorGUI.BeginChangeCheck();
                            laneConnectionColor = EditorGUILayout.ColorField("Connection", laneConnectionColor);
                            if (EditorGUI.EndChangeCheck()) SaveColor("laneConnectionColor", laneConnectionColor);

                            EditorGUI.BeginChangeCheck();
                            handleDrawDistance = EditorGUILayout.FloatField("Draw Distance", handleDrawDistance);
                            if (EditorGUI.EndChangeCheck()) EditorPrefs.SetFloat("LCDrawDistance", handleDrawDistance);
                        }
                        GUI.enabled = false;
                        SerializedProperty _routesProperty = serialObj.FindProperty("_routes");
                        EditorGUILayout.PropertyField(_routesProperty, true);

                        SerializedProperty LC_routeAProperty = serialObj.FindProperty("LC_routeA");
                        EditorGUILayout.PropertyField(LC_routeAProperty, true);

                        SerializedProperty LC_routeBProperty = serialObj.FindProperty("LC_routeB");
                        EditorGUILayout.PropertyField(LC_routeBProperty, true);

                        SerializedProperty LC_routeIndexAProperty = serialObj.FindProperty("LC_routeIndexA");
                        EditorGUILayout.PropertyField(LC_routeIndexAProperty, true);

                        SerializedProperty LC_routeIndexBProperty = serialObj.FindProperty("LC_routeIndexB");
                        EditorGUILayout.PropertyField(LC_routeIndexBProperty, true);
                        GUI.enabled = true;
                    }
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);



                    if (_routes.Length > 0)
                    {
                        if (LC_routeIndexA == -1 || LC_routeIndexB == -1)
                            EditorGUILayout.HelpBox("Press R handle button to select routes A/B", MessageType.Info);
                        GUI.enabled = false;
                        SerializedProperty LC_routeAProperty = serialObj.FindProperty("LC_routeA");
                        EditorGUILayout.PropertyField(LC_routeAProperty, true);
                        SerializedProperty LC_routeBProperty = serialObj.FindProperty("LC_routeB");
                        EditorGUILayout.PropertyField(LC_routeBProperty, true);
                        GUI.enabled = true;

                        if (LC_routeIndexA != -1 && LC_routeIndexB != -1)
                        {
                            if (GUILayout.Button("Setup Lane Change Points"))
                            {
                                List<Object> objList = new List<Object> { this, LC_routeA, LC_routeB };
                                for (int i = 0; i < LC_routeA.waypointDataList.Count; i++)
                                {
                                    objList.Add(LC_routeA.waypointDataList[i]._waypoint);
                                }
                                for (int i = 0; i < LC_routeB.waypointDataList.Count; i++)
                                {
                                    objList.Add(LC_routeB.waypointDataList[i]._waypoint);
                                }
                                Object[] objs = objList.ToArray();
                                UndoRecordTargetObject(objs, "Setup Lane Change Points");
                                LaneChangeHelper.AssignLaneChangePoints(LC_routeA, LC_routeB);
                                EditorUtility.SetDirty(LC_routeA);
                                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                                LaneChangeHelper.AssignLaneChangePoints(LC_routeB, LC_routeA);
                                EditorUtility.SetDirty(LC_routeB);
                                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                                ClearData(false);
                            }
                        }
                    }
                    #endregion
                    break;
                case EditMode.RouteConnector:
                    #region RouteConnectorGUI
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("ROUTE CONNECTOR", style);
                    EditorGUILayout.Space();
                    if (_routes.Length > 0)
                    {
                        if (_routes[0] == null) ClearData(true);
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("Reload Routes", GUILayout.Width(100)))
                        {
                            UndoRecordTargetObject(this, "Load Routes");
                            _routes = FindObjectsOfType<AITrafficWaypointRoute>();
                            SceneView.RepaintAll();
                        }

                        if (GUILayout.Button("Unload Routes", GUILayout.Width(100)))
                        {
                            UndoRecordTargetObject(this, "Unload Routes");
                            ClearData(true);
                        }
                        EditorGUILayout.LabelField("Debug", EditorStyles.label, GUILayout.Width(40));
                        showDebug = EditorGUILayout.Toggle("", showDebug, GUILayout.Width(20));
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        if (GUILayout.Button("Load Routes"))
                        {
                            UndoRecordTargetObject(this, "Load Routes");
                            _routes = FindObjectsOfType<AITrafficWaypointRoute>();
                            SceneView.RepaintAll();
                        }
                    }
                    if (showDebug)
                    {
                        EditorGUILayout.Space();
                        showCustomizationOptions = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), showCustomizationOptions, "Handle Customization", true);
                        if (showCustomizationOptions)
                        {
                            EditorGUI.BeginChangeCheck();
                            fromHandleColor = EditorGUILayout.ColorField("F Handle", fromHandleColor);
                            if (EditorGUI.EndChangeCheck()) SaveColor("fromHandleColor", fromHandleColor);

                            EditorGUI.BeginChangeCheck();
                            fromHandleActiveColor = EditorGUILayout.ColorField("F Handle Active", fromHandleActiveColor);
                            if (EditorGUI.EndChangeCheck()) SaveColor("fromHandleActiveColor", fromHandleActiveColor);

                            EditorGUI.BeginChangeCheck();
                            fromHandleTextColor = EditorGUILayout.ColorField("F Handle Text", fromHandleTextColor);
                            if (EditorGUI.EndChangeCheck()) SaveColor("fromHandleTextColor", fromHandleTextColor);

                            EditorGUI.BeginChangeCheck();
                            fromHandleTextActiveColor = EditorGUILayout.ColorField("F Handle Text Active", fromHandleTextActiveColor);
                            if (EditorGUI.EndChangeCheck()) SaveColor("fromHandleTextActiveColor", fromHandleTextActiveColor);

                            EditorGUI.BeginChangeCheck();
                            toHandleColor = EditorGUILayout.ColorField("T Handle", toHandleColor);
                            if (EditorGUI.EndChangeCheck()) SaveColor("toHandleColor", toHandleColor);

                            EditorGUI.BeginChangeCheck();
                            toHandleActiveColor = EditorGUILayout.ColorField("T Handle Active", toHandleActiveColor);
                            if (EditorGUI.EndChangeCheck()) SaveColor("toHandleActiveColor", toHandleActiveColor);

                            EditorGUI.BeginChangeCheck();
                            toHandleTextColor = EditorGUILayout.ColorField("T Handle Text", toHandleTextColor);
                            if (EditorGUI.EndChangeCheck()) SaveColor("toHandleTextColor", toHandleTextColor);

                            EditorGUI.BeginChangeCheck();
                            toHandleTextActiveColor = EditorGUILayout.ColorField("T Handle Text Active", toHandleTextActiveColor);
                            if (EditorGUI.EndChangeCheck()) SaveColor("toHandleTextActiveColor", toHandleTextActiveColor);

                            EditorGUI.BeginChangeCheck();
                            handleDrawDistance = EditorGUILayout.FloatField("Draw Distance", handleDrawDistance);
                            if (EditorGUI.EndChangeCheck()) EditorPrefs.SetFloat("handleDrawDistance", handleDrawDistance);
                        }
                        GUI.enabled = false;
                        SerializedProperty _routesProperty = serialObj.FindProperty("_routes");
                        EditorGUILayout.PropertyField(_routesProperty, true);

                        SerializedProperty RC_fromRouteIndexProperty = serialObj.FindProperty("RC_fromRouteIndex");
                        EditorGUILayout.PropertyField(RC_fromRouteIndexProperty, true);

                        SerializedProperty RC_fromPointIndexProperty = serialObj.FindProperty("RC_fromPointIndex");
                        EditorGUILayout.PropertyField(RC_fromPointIndexProperty, true);

                        SerializedProperty RC_toRouteIndexProperty = serialObj.FindProperty("RC_toRouteIndex");
                        EditorGUILayout.PropertyField(RC_toRouteIndexProperty, true);

                        SerializedProperty RC_toPointIndexProperty = serialObj.FindProperty("RC_toPointIndex");
                        EditorGUILayout.PropertyField(RC_toPointIndexProperty, true);
                        GUI.enabled = true;
                    }
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);



                    if (_routes.Length > 0)
                    {
                        if (RC_fromRouteIndex == -1 || RC_toRouteIndex == -1)
                            EditorGUILayout.HelpBox("Press F/T handle buttonss to select From/To points", MessageType.Info);
                        GUI.enabled = false;
                        SerializedProperty fromPointProperty = serialObj.FindProperty("RC_fromPoint");
                        EditorGUILayout.PropertyField(fromPointProperty, true);

                        SerializedProperty toPointProperty = serialObj.FindProperty("RC_toPoint");
                        EditorGUILayout.PropertyField(toPointProperty, true);
                        GUI.enabled = true;

                        if (RC_fromPoint != null && RC_toPoint != null)
                        {
                            if (GUILayout.Button("Connect Route Points"))
                            {
                                Object[] objs = new Object[] { this, RC_fromPoint };
                                UndoRecordTargetObject(objs, "Connect Route Points");

                                AITrafficWaypoint[] currentArray = new AITrafficWaypoint[RC_fromPoint.onReachWaypointSettings.newRoutePoints.Length + 1];
                                for (int i = 0; i < RC_fromPoint.onReachWaypointSettings.newRoutePoints.Length; i++)
                                {
                                    currentArray[i] = RC_fromPoint.onReachWaypointSettings.newRoutePoints[i];
                                }
                                currentArray[currentArray.Length - 1] = RC_toPoint;
                                RC_fromPoint.onReachWaypointSettings.newRoutePoints = currentArray;

                                EditorUtility.SetDirty(RC_fromPoint);
                                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

                                ClearData(false);
                            }
                        }
                    }
                    #endregion
                    break;
                case EditMode.RouteEditor:
                    #region RouteEditorGUI
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("ROUTE EDITOR", style);
                    EditorGUILayout.Space();
                    if (_routes.Length > 0)
                    {
                        if (_routes[0] == null) ClearData(true);
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("Reload Routes", GUILayout.Width(100)))
                        {
                            UndoRecordTargetObject(this, "Load Routes");
                            _routes = FindObjectsOfType<AITrafficWaypointRoute>();
                            SceneView.RepaintAll();
                        }

                        if (GUILayout.Button("Unload Routes", GUILayout.Width(100)))
                        {
                            UndoRecordTargetObject(this, "Unload Routes");
                            ClearData(true);
                        }
                        EditorGUILayout.LabelField("Debug", EditorStyles.label, GUILayout.Width(45));
                        showDebug = EditorGUILayout.Toggle("", showDebug, GUILayout.Width(20));
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        if (GUILayout.Button("Load Routes"))
                        {
                            UndoRecordTargetObject(this, "Load Routes");
                            _routes = FindObjectsOfType<AITrafficWaypointRoute>();
                            SceneView.RepaintAll();
                        }
                    }
                    if (showDebug)
                    {
                        EditorGUILayout.Space();
                        showCustomizationOptions = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), showCustomizationOptions, "Handle Customization", true);
                        if (showCustomizationOptions)
                        {

                        }
                        GUI.enabled = false;
                        SerializedProperty _routesProperty = serialObj.FindProperty("_routes");
                        EditorGUILayout.PropertyField(_routesProperty, true);

                        SerializedProperty RE_connectedWaypoints = serialObj.FindProperty("RE_connectedWaypoints");
                        EditorGUILayout.PropertyField(RE_connectedWaypoints, true);

                        SerializedProperty RE_route = serialObj.FindProperty("RE_route");
                        EditorGUILayout.PropertyField(RE_route, true);
                        GUI.enabled = true;
                    }
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    if (_routes.Length > 0)
                    {




                        if (RE_route == null)
                        {
                            EditorGUILayout.HelpBox("Press R handle button to load route waypoints", MessageType.Info);
                            EditorGUILayout.HelpBox("Button is located above final route points.", MessageType.Info);
                        }
                        else
                        {
                            if (GUILayout.Button("Unload Waypoints"))
                            {
                                UndoRecordTargetObject(this, "Unload Route Waypoints");
                                RE_route = null;
                                RE_routeIndex = -1;
                            }

                            if (RE_route != null)
                            {
                                if (GUILayout.Button("Clear All Lane Change Points"))
                                {
                                    List<Object> objList = new List<Object> { this, RE_route };
                                    for (int i = 0; i < RE_route.waypointDataList.Count; i++)
                                    {
                                        objList.Add(RE_route.waypointDataList[i]._waypoint);
                                    }
                                    Object[] objs = objList.ToArray();
                                    UndoRecordTargetObject(objs, "Clear All Lane Change Points");
                                    RE_route.ClearAllLaneChangePoints();
                                }
                                if (GUILayout.Button("Clear All New Route Points"))
                                {
                                    List<Object> objList = new List<Object> { this, RE_route };
                                    for (int i = 0; i < RE_route.waypointDataList.Count; i++)
                                    {
                                        objList.Add(RE_route.waypointDataList[i]._waypoint);
                                    }
                                    Object[] objs = objList.ToArray();
                                    UndoRecordTargetObject(objs, "Clear All New Rout Points");
                                    RE_route.ClearAllNewRoutePoints();
                                }

                                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                                SerializedProperty _routeEditorProperty = serialObj.FindProperty("routeEditorProperty");
                                EditorGUI.BeginChangeCheck();
                                EditorGUILayout.PropertyField(_routeEditorProperty, new GUIContent("Property"), true);
                                if (EditorGUI.EndChangeCheck())
                                    serialObj.ApplyModifiedProperties();

                                EditorGUILayout.Space();

                                SerializedObject routeObj = new SerializedObject(serialObj.FindProperty("RE_route").objectReferenceValue);
                                style.fontSize = 12;
                                style.alignment = TextAnchor.MiddleLeft;
                                for (int i = 0; i < RE_route.waypointDataList.Count; i++)
                                {
                                    SerializedObject routeWaypointObj = new SerializedObject(routeObj.FindProperty("waypointDataList").GetArrayElementAtIndex(i).FindPropertyRelative("_waypoint").objectReferenceValue);
                                    SerializedObject routeWaypointObject = new SerializedObject(RE_route.waypointDataList[i]._waypoint.gameObject);

                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.LabelField("WAYPOINT " + (i + 1).ToString(), style, GUILayout.Width(110));

                                    if (GUILayout.Button("Select", GUILayout.Width(45)))
                                    {
                                        UndoRecordTargetObject(this, "Select");
                                        Selection.activeObject = routeWaypointObj.targetObject;
                                        EditorGUIUtility.PingObject(routeWaypointObj.targetObject);
                                    }
                                    if (GUILayout.Button("Frame", GUILayout.Width(45)))
                                    {
                                        UndoRecordTargetObject(this, "Frame");
                                        Selection.activeObject = routeWaypointObj.targetObject;
                                        EditorGUIUtility.PingObject(routeWaypointObj.targetObject);
                                        SceneView.lastActiveSceneView.FrameSelected();
                                    }
                                    Color prevColor = GUI.color;
                                    GUI.color = Color.red;
                                    if (GUILayout.Button("Delete", GUILayout.Width(50)))
                                    {
                                        RE_GetPointsConnectedToRoutes();
                                        List<Object> objList = new List<Object> { this, RE_route };
                                        for (int j = 0; j < RE_connectedWaypoints.Count; j++)
                                        {
                                            objList.Add(RE_connectedWaypoints[j]);
                                        }
                                        Object[] objs = objList.ToArray();
                                        UndoRecordTargetObject(objs, "Delete Waypoint");
                                        Undo.RegisterFullObjectHierarchyUndo(RE_route.gameObject, "Delete Waypoint");
                                        Undo.DestroyObjectImmediate(routeWaypointObject.targetObject);//routeWaypointObj.targetObject);
                                        RE_route.waypointDataList.RemoveAt(i);
                                        for (int j = 0; j < RE_connectedWaypoints.Count; j++)
                                        {
                                            RE_connectedWaypoints[j].RemoveMissingLaneChangePoints();
                                            RE_connectedWaypoints[j].RemoveMissingNewRoutePoints();
                                        }
                                        RE_route.RefreshPointIndexes();
                                        // remove point from connection array
                                    }
                                    GUI.color = prevColor;
                                    EditorGUILayout.EndHorizontal();

                                    if (routeEditorProperty != RouteEditorProperty.ObjectOnly) EditorGUILayout.BeginVertical("box");
                                    /// Speed Limit
                                    if (routeEditorProperty == RouteEditorProperty.All || routeEditorProperty == RouteEditorProperty.SpeedLimit)
                                    {
                                        EditorGUI.BeginChangeCheck();
                                        SerializedProperty speedLimit = routeWaypointObj.FindProperty("onReachWaypointSettings").FindPropertyRelative("speedLimit");
                                        EditorGUILayout.PropertyField(speedLimit, true);
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            routeWaypointObj.ApplyModifiedProperties();
                                        }
                                    }
                                    /// Stop Driving
                                    if (routeEditorProperty == RouteEditorProperty.All || routeEditorProperty == RouteEditorProperty.StopDriving)
                                    {
                                        EditorGUI.BeginChangeCheck();
                                        SerializedProperty stopDriving = routeWaypointObj.FindProperty("onReachWaypointSettings").FindPropertyRelative("stopDriving");
                                        EditorGUILayout.PropertyField(stopDriving, true);
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            routeWaypointObj.ApplyModifiedProperties();
                                        }
                                    }
                                    /// New Route Point
                                    if (routeEditorProperty == RouteEditorProperty.All || routeEditorProperty == RouteEditorProperty.NewRoutePoints)
                                    {
                                        EditorGUI.BeginChangeCheck();
                                        SerializedProperty newRoutePoints = routeWaypointObj.FindProperty("onReachWaypointSettings").FindPropertyRelative("newRoutePoints");
                                        EditorGUILayout.PropertyField(newRoutePoints, true);
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            routeWaypointObj.ApplyModifiedProperties();
                                        }
                                    }
                                    /// Lane Change Points
                                    if (routeEditorProperty == RouteEditorProperty.All || routeEditorProperty == RouteEditorProperty.LaneChangePoints)
                                    {
                                        EditorGUI.BeginChangeCheck();
                                        SerializedProperty laneChangePoints = routeWaypointObj.FindProperty("onReachWaypointSettings").FindPropertyRelative("laneChangePoints");
                                        EditorGUILayout.PropertyField(laneChangePoints, true);
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            routeWaypointObj.ApplyModifiedProperties();
                                        }
                                    }
                                    /// On Reach Waypoint Event
                                    if (routeEditorProperty == RouteEditorProperty.All || routeEditorProperty == RouteEditorProperty.OnReachWaypointEvent)
                                    {
                                        EditorGUI.BeginChangeCheck();
                                        SerializedProperty OnReachWaypointEvent = routeWaypointObj.FindProperty("onReachWaypointSettings").FindPropertyRelative("OnReachWaypointEvent");
                                        EditorGUILayout.PropertyField(OnReachWaypointEvent, true);
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            routeWaypointObj.ApplyModifiedProperties();
                                        }
                                    }
                                    if (routeEditorProperty != RouteEditorProperty.ObjectOnly) EditorGUILayout.EndVertical();
                                    EditorGUILayout.Space();
                                }
                            }
                        }
                    }
                    #endregion
                    break;
                case EditMode.SignalConnector:
                    #region SignalConnector GUI
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("SIGNAL CONNECTOR", style);
                    EditorGUILayout.Space();
                    if (_routes.Length > 0)
                    {
                        if (_routes[0] == null) ClearData(true);
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("Reload Routes", GUILayout.Width(100)))
                        {
                            UndoRecordTargetObject(this, "Load Lights & Routes");
                            _routes = FindObjectsOfType<AITrafficWaypointRoute>();
                            SC_lights = FindObjectsOfType<AITrafficLight>();
                            SceneView.RepaintAll();
                        }

                        if (GUILayout.Button("Unload Routes", GUILayout.Width(100)))
                        {
                            UndoRecordTargetObject(this, "Unload Routes");
                            ClearData(true);
                        }
                        EditorGUILayout.LabelField("Debug", EditorStyles.label, GUILayout.Width(40));
                        showDebug = EditorGUILayout.Toggle("", showDebug, GUILayout.Width(20));
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        if (GUILayout.Button("Load Lights & Routes"))
                        {
                            UndoRecordTargetObject(this, "Load Lights & Routes");
                            _routes = FindObjectsOfType<AITrafficWaypointRoute>();
                            SC_lights = FindObjectsOfType<AITrafficLight>();
                            SceneView.RepaintAll();
                        }
                    }
                    if (showDebug)
                    {
                        EditorGUILayout.Space();
                        showCustomizationOptions = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), showCustomizationOptions, "Handle Customization", true);
                        if (showCustomizationOptions)
                        {
                            EditorGUI.BeginChangeCheck();
                            signalConnectorHandleColor = EditorGUILayout.ColorField("Handle", signalConnectorHandleColor);
                            if (EditorGUI.EndChangeCheck()) SaveColor("signalConnectorHandleColor", signalConnectorHandleColor);

                            EditorGUI.BeginChangeCheck();
                            signalConnectorHandleActiveColor = EditorGUILayout.ColorField("Handle Active", signalConnectorHandleActiveColor);
                            if (EditorGUI.EndChangeCheck()) SaveColor("signalConnectorHandleActiveColor", signalConnectorHandleActiveColor);

                            EditorGUI.BeginChangeCheck();
                            signalConnectorHandleTextColor = EditorGUILayout.ColorField("Handle Text", signalConnectorHandleTextColor);
                            if (EditorGUI.EndChangeCheck()) SaveColor("signalConnectorHandleTextColor", signalConnectorHandleTextColor);

                            EditorGUI.BeginChangeCheck();
                            signalConnectorHandleTextActiveColor = EditorGUILayout.ColorField("Handle Text Active", signalConnectorHandleTextActiveColor);
                            if (EditorGUI.EndChangeCheck()) SaveColor("signalConnectorHandleTextActiveColor", signalConnectorHandleTextActiveColor);

                            EditorGUI.BeginChangeCheck();
                            signalConnectionColor = EditorGUILayout.ColorField("Connection", signalConnectionColor);
                            if (EditorGUI.EndChangeCheck()) SaveColor("signalConnectionColor", signalConnectionColor);

                            EditorGUI.BeginChangeCheck();
                            handleDrawDistance = EditorGUILayout.FloatField("Draw Distance", handleDrawDistance);
                            if (EditorGUI.EndChangeCheck()) EditorPrefs.SetFloat("handleDrawDistance", handleDrawDistance);
                        }
                        GUI.enabled = false;
                        SerializedProperty _routesProperty = serialObj.FindProperty("_routes");
                        EditorGUILayout.PropertyField(_routesProperty, true);

                        SerializedProperty SC_lightsProperty = serialObj.FindProperty("SC_lights");
                        EditorGUILayout.PropertyField(SC_lightsProperty, true);

                        SerializedProperty SC_lightProperty = serialObj.FindProperty("SC_light");
                        EditorGUILayout.PropertyField(SC_lightProperty, true);

                        SerializedProperty SC_routeProperty = serialObj.FindProperty("SC_route");
                        EditorGUILayout.PropertyField(SC_routeProperty, true);

                        SerializedProperty SC_lightIndexProperty = serialObj.FindProperty("SC_lightIndex");
                        EditorGUILayout.PropertyField(SC_lightIndexProperty, true);

                        SerializedProperty SC_routeIndexProperty = serialObj.FindProperty("SC_routeIndex");
                        EditorGUILayout.PropertyField(SC_routeIndexProperty, true);
                        GUI.enabled = true;
                    }
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    if (_routes.Length > 0)
                    {
                        SerializedProperty SC_lightProperty = serialObj.FindProperty("SC_light");
                        if (SC_lightIndex == -1)
                            EditorGUILayout.HelpBox("Press L handle button to select a Light", MessageType.Info);
                        GUI.enabled = false;
                        EditorGUILayout.PropertyField(SC_lightProperty, true);
                        GUI.enabled = true;

                        SerializedProperty SC_routeProperty = serialObj.FindProperty("SC_route");
                        if (SC_routeIndex == -1)
                            EditorGUILayout.HelpBox("Press R handle to select a Route", MessageType.Info);
                        GUI.enabled = false;
                        EditorGUILayout.PropertyField(SC_routeProperty, true);
                        GUI.enabled = true;

                        if (SC_light && SC_route)
                        {
                            if (SC_lights[SC_lightIndex].waypointRoute == _routes[SC_routeIndex])
                            {
                                if (GUILayout.Button("Disconnect Light From Route"))
                                {
                                    Object[] objs = new Object[] { this, SC_lights[SC_lightIndex] };
                                    UndoRecordTargetObject(objs, "Disconnect Light From Route");

                                    SC_lights[SC_lightIndex].waypointRoute = null;

                                    EditorUtility.SetDirty(SC_lights[SC_lightIndex]);
                                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                                    ClearData(false);
                                }
                            }
                            else
                            {
                                if (GUILayout.Button("Connect Light To Route"))
                                {
                                    Object[] objs = new Object[] { this, SC_lights[SC_lightIndex] };
                                    UndoRecordTargetObject(objs, "Connect Light To Route");

                                    SC_lights[SC_lightIndex].waypointRoute = _routes[SC_routeIndex];

                                    EditorUtility.SetDirty(SC_lights[SC_lightIndex]);
                                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                                    ClearData(false);
                                }
                            }
                        }
                    }
                    #endregion
                    break;
                case EditMode.SpawnPoints:
                    #region SpawnPoints GUI
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("SPAWN POINTS", style);
                    EditorGUILayout.Space();
                    if (_routes.Length > 0)
                    {
                        if (_routes[0] == null) ClearData(true);
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("Reload Routes", GUILayout.Width(100)))
                        {
                            UndoRecordTargetObject(this, "Load Routes");
                            _routes = FindObjectsOfType<AITrafficWaypointRoute>();
                            SceneView.RepaintAll();
                        }

                        if (GUILayout.Button("Unload Routes", GUILayout.Width(100)))
                        {
                            UndoRecordTargetObject(this, "Unload Routes");
                            ClearData(true);
                        }
                        EditorGUILayout.LabelField("Debug", EditorStyles.label, GUILayout.Width(40));
                        showDebug = EditorGUILayout.Toggle("", showDebug, GUILayout.Width(20));
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        if (GUILayout.Button("Load Routes"))
                        {
                            UndoRecordTargetObject(this, "Load Routes");
                            _routes = FindObjectsOfType<AITrafficWaypointRoute>();
                            SceneView.RepaintAll();
                        }
                    }
                    if (showDebug)
                    {
                        EditorGUILayout.Space();
                        showCustomizationOptions = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), showCustomizationOptions, "Handle Customization", true);
                        if (showCustomizationOptions)
                        {
                            EditorGUI.BeginChangeCheck();
                            spawnPointHandleColor = EditorGUILayout.ColorField("Handle", spawnPointHandleColor);
                            if (EditorGUI.EndChangeCheck()) SaveColor("spawnPointHandleColor", spawnPointHandleColor);

                            EditorGUI.BeginChangeCheck();
                            spawnPointHandleTextColor = EditorGUILayout.ColorField("Handle Text", spawnPointHandleTextColor);
                            if (EditorGUI.EndChangeCheck()) SaveColor("spawnPointHandleTextColor", spawnPointHandleTextColor);

                            EditorGUI.BeginChangeCheck();
                            handleDrawDistance = EditorGUILayout.FloatField("Draw Distance", handleDrawDistance);
                            if (EditorGUI.EndChangeCheck()) EditorPrefs.SetFloat("handleDrawDistance", handleDrawDistance);
                        }
                        GUI.enabled = false;
                        SerializedProperty _routesProperty = serialObj.FindProperty("_routes");
                        EditorGUILayout.PropertyField(_routesProperty, true);
                        GUI.enabled = true;
                    }
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);



                    if (_routes.Length > 0)
                    {
                        if (GUILayout.Button("Align Route Waypoints"))
                        {
                            List<Object> objList = new List<Object>();
                            objList.Add(this);
                            for (int i = 0; i < _routes.Length; i++)
                            {
                                for (int j = 0; j < _routes[i].waypointDataList.Count; j++)
                                {
                                    objList.Add(_routes[i].waypointDataList[j]._transform);
                                }
                            }
                            Object[] objArray = objList.ToArray();
                            UndoRecordTargetObject(objArray, "Align Route Waypoints");
                            for (int i = 0; i < _routes.Length; i++)
                            {
                                _routes[i].AlignPoints();
                            }
                            EditorUtility.SetDirty(this);
                            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                            Repaint();
                        }
                        if (GUILayout.Button("Remove All Spawn Points"))
                        {
                            for (int i = 0; i < _routes.Length; i++)
                            {
                                string message = "removing spawn point " + i.ToString() + "/" + _routes.Length.ToString();
                                EditorUtility.DisplayProgressBar("Remove All Spawn Points", message , i / (float)_routes.Length);
                                Undo.RegisterFullObjectHierarchyUndo(_routes[i].gameObject, "Remove All Spawn Points");
                                AITrafficSpawnPoint[] spawnPoints = _routes[i].GetComponentsInChildren<AITrafficSpawnPoint>();
                                for (int j = 0; j < spawnPoints.Length; j++)
                                {
                                    Undo.DestroyObjectImmediate(spawnPoints[j].gameObject);
                                }
                            }
                            EditorUtility.SetDirty(this);
                            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                            Repaint();
                            EditorUtility.ClearProgressBar();
                        }
                        if (GUILayout.Button("Setup Random Spawn Points"))
                        {
                            GameObject waypointObject = Resources.Load("AITrafficSpawnPoint") as GameObject;
                            for (int i = 0; i < _routes.Length; i++)
                            {
                                string message = "updating route " + i.ToString() + "/" + _routes.Length.ToString();
                                EditorUtility.DisplayProgressBar("Setup Random Spawn Points", message, i / (float)_routes.Length);
                                if (_routes[i].waypointDataList.Count > 4)
                                {
                                    Undo.RegisterFullObjectHierarchyUndo(_routes[i].gameObject, "Setup Random Spawn Points");
                                    AITrafficSpawnPoint[] spawnPoints = _routes[i].GetComponentsInChildren<AITrafficSpawnPoint>();
                                    for (int j = 0; j < spawnPoints.Length; j++)
                                    {
                                        Undo.DestroyObjectImmediate(spawnPoints[j].gameObject);
                                    }

                                    int randomIndex = UnityEngine.Random.Range(0, 3);
                                    for (int j = randomIndex; j < _routes[i].waypointDataList.Count && j < _routes[i].waypointDataList.Count - 2; j += UnityEngine.Random.Range(2, 4))
                                    {
                                        GameObject loadedSpawnPoint = Instantiate(waypointObject, _routes[i].waypointDataList[j]._transform);
                                        Undo.RegisterCreatedObjectUndo(loadedSpawnPoint, "AITrafficSpawnPoint");
                                        AITrafficSpawnPoint trafficSpawnPoint = loadedSpawnPoint.GetComponent<AITrafficSpawnPoint>();
                                        trafficSpawnPoint.waypoint = trafficSpawnPoint.transform.parent.GetComponent<AITrafficWaypoint>();
                                    }
                                }
                            }
                            EditorUtility.ClearProgressBar();
                            Undo.FlushUndoRecordObjects();
                            EditorUtility.SetDirty(this);
                            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                        }
                        EditorGUILayout.HelpBox("Press S handle buttons to create spwan points", MessageType.Info);
                    }
                    #endregion
                    break;
                default:
                    break;
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
        #endregion

        void RE_GetPointsConnectedToRoutes()
        {
            RE_connectedWaypoints = new List<AITrafficWaypoint>();
            for (int i = 0; i < _routes.Length; i++)
            {
                for (int j = 0; j < _routes[i].waypointDataList.Count; j++)
                {
                    for (int k = 0; k < _routes[i].waypointDataList[j]._waypoint.onReachWaypointSettings.newRoutePoints.Length; k++)
                    {
                        if (_routes[i].waypointDataList[j]._waypoint.onReachWaypointSettings.newRoutePoints[k].onReachWaypointSettings.parentRoute == RE_route)
                        {
                            RE_connectedWaypoints.Add(_routes[i].waypointDataList[j]._waypoint);
                        }
                    }
                    for (int k = 0; k < _routes[i].waypointDataList[j]._waypoint.onReachWaypointSettings.laneChangePoints.Count; k++)
                    {
                        if (_routes[i].waypointDataList[j]._waypoint.onReachWaypointSettings.laneChangePoints[k].onReachWaypointSettings.parentRoute == RE_route)
                        {
                            RE_connectedWaypoints.Add(_routes[i].waypointDataList[j]._waypoint);
                        }

                    }
                }
            }
        }

        #region SceneView GUI
        void OnFocus()
        {
            SceneView.duringSceneGui -= this.OnSceneGUI;
            SceneView.duringSceneGui += this.OnSceneGUI;
            Undo.undoRedoPerformed -= this.OnUndoRedo;
            Undo.undoRedoPerformed += this.OnUndoRedo;
            EditorSceneManager.activeSceneChangedInEditMode -= SceneChanged;
            EditorSceneManager.activeSceneChangedInEditMode += SceneChanged;
            EditorApplication.playModeStateChanged -= PlayModeStateChangedCallback;
            EditorApplication.playModeStateChanged += PlayModeStateChangedCallback;
        }

        private void OnDestroy()
        {
            SceneView.duringSceneGui -= this.OnSceneGUI;
            Undo.undoRedoPerformed -= this.OnUndoRedo;
            EditorSceneManager.activeSceneChangedInEditMode -= SceneChanged;
            EditorApplication.playModeStateChanged -= PlayModeStateChangedCallback;
        }

        void OnSceneGUI(SceneView sceneView)
        {
            Transform sceneViewCameraTransform = Camera.current.transform;
            GUIStyle style = new GUIStyle();
            style.alignment = TextAnchor.MiddleCenter;
            style.fontStyle = FontStyle.Bold;
            style.fontSize = 12;

            switch (editMode)
            {
                case EditMode.LaneConnector:
                    #region LaneConnector
                    for (int i = 0; i < this._routes.Length; i++)
                    {
                        if (_routes[0] == null)
                        {
                            ClearData(true);
                            break;
                        }
                        if (this._routes[i].waypointDataList.Count > 0)
                        {
                            int index = this._routes[i].waypointDataList.Count - 1;
                            Vector3 pointTransformPosition = this._routes[i].waypointDataList[index]._transform.position;
                            float pointDistance = Vector3.Distance(Camera.current.transform.position, pointTransformPosition);
                            float linearPointDistance = Mathf.InverseLerp(0, handleMaxSizeDistance, pointDistance);
                            Vector3 _textPosition = pointTransformPosition + (handleTextoffset * linearPointDistance);
                            float size = linearPointDistance * handleSize;
                            if (handleDrawDistance >= Vector3.Distance(Camera.current.transform.position, pointTransformPosition))
                            {
                                if ((LC_routeIndexA == i || LC_routeIndexB == i) || (LC_routeIndexA == -1 || LC_routeIndexB == -1)) // if index is selected // if 1 or more indexes are not assigned
                                {
                                    style.normal.textColor = (this.LC_routeIndexA == i || this.LC_routeIndexB == i) ? laneConnectorHandleTextActiveColor : laneConnectorHandleTextColor;
                                    Handles.color = (this.LC_routeIndexA == i || this.LC_routeIndexB == i) ? laneConnectorHandleActiveColor : laneConnectorHandleColor;
                                    if (Handles.Button(pointTransformPosition, Quaternion.LookRotation(sceneViewCameraTransform.forward, sceneViewCameraTransform.up), size, size, Handles.DotHandleCap))
                                    {
                                        UndoRecordTargetObject(this, "Lane Connector Handle");
                                        bool updatedSelection = false;
                                        if (this.LC_routeIndexA == i) /// Check if selection is already assigned to A
                                        {
                                            this.LC_routeA = null;
                                            LC_routeIndexA = -1;
                                            updatedSelection = true;
                                        }
                                        if (this.LC_routeIndexB == i) /// Check if selection is already assigned to B
                                        {
                                            this.LC_routeB = null;
                                            LC_routeIndexB = -1;
                                            updatedSelection = true;
                                        }
                                        if (updatedSelection == false) /// Assign selection
                                        {
                                            if (this.LC_routeIndexA == -1)
                                            {
                                                this.LC_routeIndexA = i;
                                                this.LC_routeA = this._routes[i].waypointDataList[index]._waypoint.onReachWaypointSettings.parentRoute;
                                            }
                                            else if (this.LC_routeIndexB == -1)
                                            {
                                                this.LC_routeIndexB = i;
                                                this.LC_routeB = this._routes[i].waypointDataList[index]._waypoint.onReachWaypointSettings.parentRoute;
                                            }
                                        }
                                        Repaint();
                                    }
                                    Handles.Label(_textPosition, "R", style);
                                }
                            }
                        }
                    }
                    if (this.LC_routeIndexA != -1 && this.LC_routeIndexB != -1) /// Draw Line
                    {
                        Handles.color = laneConnectionColor;
                        int indexA = this._routes[LC_routeIndexA].waypointDataList.Count - 1;
                        int indexB = this._routes[LC_routeIndexB].waypointDataList.Count - 1;
                        Vector3 positionA = this._routes[LC_routeIndexA].waypointDataList[indexA]._transform.position + new Vector3(0, 0.2f, 0);
                        Vector3 positionB = this._routes[LC_routeIndexB].waypointDataList[indexB]._transform.position + new Vector3(0, 0.2f, 0);
                        Handles.DrawLine(positionA, positionB);
                    }
                    #endregion
                    break;
                case EditMode.RouteConnector:
                    #region RouteConnector
                    for (int i = 0; i < this._routes.Length; i++)
                    {
                        if (_routes[0] == null)
                        {
                            ClearData(true);
                            break;
                        }
                        for (int j = 0; j < this._routes[i].waypointDataList.Count; j++)
                        {
                            Vector3 pointTransformPosition = this._routes[i].waypointDataList[j]._transform.position;
                            float pointDistance = Vector3.Distance(Camera.current.transform.position, pointTransformPosition);
                            float linearPointDistance = Mathf.InverseLerp(0, handleMaxSizeDistance, pointDistance);
                            Vector3 _textPosition = pointTransformPosition + (handleTextoffset * linearPointDistance);
                            float size = linearPointDistance * handleSize;
                            if (handleDrawDistance >= pointDistance)
                            {
                                #region TO Handle
                                if (RC_toPointIndex == -1 || RC_toRouteIndex == i && RC_toPointIndex == j)
                                {
                                    if (RC_fromPointIndex != i && RC_fromPointIndex != j)
                                    {
                                        style.normal.textColor = (this.RC_toRouteIndex == i) && (this.RC_toPointIndex == j) ? toHandleTextActiveColor : toHandleTextColor;
                                        Handles.color = (this.RC_toRouteIndex == i) && (this.RC_toPointIndex == j) ? toHandleActiveColor : toHandleColor;
                                        if (Handles.Button(pointTransformPosition, Quaternion.LookRotation(sceneViewCameraTransform.forward, sceneViewCameraTransform.up), size, size, Handles.DotHandleCap))
                                        {
                                            UndoRecordTargetObject(this, "To Handle");
                                            if (RC_toRouteIndex == i && RC_toPointIndex == j)
                                            {
                                                this.RC_toRouteIndex = -1;
                                                this.RC_toPointIndex = -1;
                                                this.RC_toPoint = null;
                                            }
                                            else
                                            {
                                                this.RC_toRouteIndex = i;
                                                this.RC_toPointIndex = j;
                                                this.RC_toPoint = this._routes[i].waypointDataList[j]._waypoint;
                                            }
                                            Repaint();
                                        }
                                        Handles.Label(_textPosition, "T", style);
                                    }
                                }
                                #endregion

                                #region FROM Handle
                                if ( RC_fromPointIndex == -1 || (RC_fromRouteIndex == i && RC_fromPointIndex == j))
                                {
                                    if (RC_toPointIndex != i && RC_toPointIndex != j)
                                    {
                                        pointTransformPosition = this._routes[i].waypointDataList[j]._transform.position + new Vector3(0, (2.75f * linearPointDistance), 0);
                                        _textPosition = pointTransformPosition + (handleTextoffset * linearPointDistance);
                                        style.normal.textColor = (this.RC_fromRouteIndex == i) && (this.RC_fromPointIndex == j) ? fromHandleTextActiveColor : fromHandleTextColor;
                                        Handles.color = (this.RC_fromRouteIndex == i) && (this.RC_fromPointIndex == j) ? fromHandleActiveColor : fromHandleColor;
                                        if (Handles.Button(pointTransformPosition, Quaternion.LookRotation(sceneViewCameraTransform.forward, sceneViewCameraTransform.up), size, size, Handles.DotHandleCap))
                                        {
                                            UndoRecordTargetObject(this, "From Handle");
                                            if (RC_fromRouteIndex == i && RC_fromPointIndex == j)
                                            {
                                                this.RC_fromRouteIndex = -1;
                                                this.RC_fromPointIndex = -1;
                                                this.RC_fromPoint = null;
                                            }
                                            else
                                            {
                                                this.RC_fromRouteIndex = i;
                                                this.RC_fromPointIndex = j;
                                                this.RC_fromPoint = this._routes[i].waypointDataList[j]._waypoint;
                                            }
                                            Repaint();
                                        }
                                        Handles.Label(_textPosition, "F", style);
                                    }
                                }
                                #endregion
                            }
                        }
                    }
                    #endregion
                    break;
                case EditMode.RouteEditor:
                    #region RouteEditor
                    for (int i = 0; i < _routes.Length; i++)
                    {
                        if (_routes[0] == null)
                        {
                            ClearData(true);
                            break;
                        }
                        if (_routes[i].waypointDataList.Count > 0)
                        {
                            int index = _routes[i].waypointDataList.Count - 1;
                            Vector3 pointTransformPosition = _routes[i].waypointDataList[index]._transform.position;
                            float pointDistance = Vector3.Distance(Camera.current.transform.position, pointTransformPosition);
                            float linearPointDistance = Mathf.InverseLerp(0, handleMaxSizeDistance, pointDistance);
                            Vector3 _textPosition = pointTransformPosition + (handleTextoffset * linearPointDistance);
                            float size = linearPointDistance * handleSize;
                            if (handleDrawDistance >= Vector3.Distance(Camera.current.transform.position, pointTransformPosition))
                            {
                                if (RE_routeIndex == i  || RE_routeIndex == -1) // if index is selected // if index is not assigned
                                {
                                    style.normal.textColor = RE_routeIndex == i ? laneConnectorHandleTextActiveColor : laneConnectorHandleTextColor;
                                    Handles.color = RE_routeIndex == i ? laneConnectorHandleActiveColor : laneConnectorHandleColor;
                                    if (Handles.Button(pointTransformPosition, Quaternion.LookRotation(sceneViewCameraTransform.forward, sceneViewCameraTransform.up), size, size, Handles.DotHandleCap))
                                    {
                                        UndoRecordTargetObject(this, "Route Editor Handle");
                                        bool updatedSelection = false;
                                        if (RE_routeIndex == i) /// Check if selection is already assigned to A
                                        {
                                            RE_route = null;
                                            RE_routeIndex = -1;
                                            updatedSelection = true;
                                        }
                                        if (updatedSelection == false) /// Assign selection
                                        {
                                            if (RE_routeIndex == -1)
                                            {
                                                RE_routeIndex = i;
                                                RE_route = _routes[i].waypointDataList[index]._waypoint.onReachWaypointSettings.parentRoute;
                                            }
                                        }
                                        Repaint();
                                    }
                                    Handles.Label(_textPosition, "R", style);
                                }
                            }
                        }
                    }
                    //for (int i = 0; i < this._routes.Length; i++)
                    //{
                    //    for (int j = 0; j < this._routes[i].waypointDataList.Count; j++)
                    //    {
                    //        Vector3 pointTransformPosition = this._routes[i].waypointDataList[j]._transform.position;
                    //        float pointDistance = Vector3.Distance(Camera.current.transform.position, pointTransformPosition);
                    //        float linearPointDistance = Mathf.InverseLerp(0, handleMaxSizeDistance, pointDistance);
                    //        Vector3 _textPosition = pointTransformPosition + (handleTextoffset * linearPointDistance);
                    //        float size = linearPointDistance * handleSize;
                    //        if (handleDrawDistance >= pointDistance)
                    //        {
                    //            #region TO Handle
                    //            style.normal.textColor = (this.RC_toRouteIndex == i) && (this.RC_toPointIndex == j) ? toHandleTextActiveColor : toHandleTextColor;
                    //            Handles.color = (this.RC_toRouteIndex == i) && (this.RC_toPointIndex == j) ? toHandleActiveColor : toHandleColor;
                    //            if (Handles.Button(pointTransformPosition, Quaternion.LookRotation(sceneViewCameraTransform.forward, sceneViewCameraTransform.up), size, size, Handles.DotHandleCap))
                    //            {
                    //                UndoRecordTargetObject(this, "To Handle");
                    //                if (RC_toRouteIndex == i && RC_toPointIndex == j)
                    //                {
                    //                    this.RC_toRouteIndex = -1;
                    //                    this.RC_toPointIndex = -1;
                    //                    this.RC_toPoint = null;
                    //                }
                    //                else
                    //                {
                    //                    this.RC_toRouteIndex = i;
                    //                    this.RC_toPointIndex = j;
                    //                    this.RC_toPoint = this._routes[i].waypointDataList[j]._waypoint;
                    //                }
                    //                Repaint();
                    //            }
                    //            Handles.Label(_textPosition, "T", style);
                    //            #endregion

                    //            #region FROM Handle
                    //            pointTransformPosition = this._routes[i].waypointDataList[j]._transform.position + new Vector3(0, (2.75f * linearPointDistance), 0);
                    //            _textPosition = pointTransformPosition + (handleTextoffset * linearPointDistance);
                    //            style.normal.textColor = (this.RC_fromRouteIndex == i) && (this.RC_fromPointIndex == j) ? fromHandleTextActiveColor : fromHandleTextColor;
                    //            Handles.color = (this.RC_fromRouteIndex == i) && (this.RC_fromPointIndex == j) ? fromHandleActiveColor : fromHandleColor;
                    //            if (Handles.Button(pointTransformPosition, Quaternion.LookRotation(sceneViewCameraTransform.forward, sceneViewCameraTransform.up), size, size, Handles.DotHandleCap))
                    //            {
                    //                UndoRecordTargetObject(this, "From Handle");
                    //                if (RC_fromRouteIndex == i && RC_fromPointIndex == j)
                    //                {
                    //                    this.RC_fromRouteIndex = -1;
                    //                    this.RC_fromPointIndex = -1;
                    //                    this.RC_fromPoint = null;
                    //                }
                    //                else
                    //                {
                    //                    this.RC_fromRouteIndex = i;
                    //                    this.RC_fromPointIndex = j;
                    //                    this.RC_fromPoint = this._routes[i].waypointDataList[j]._waypoint;
                    //                }
                    //                Repaint();
                    //            }
                    //            Handles.Label(_textPosition, "F", style);
                    //            #endregion
                    //        }
                    //    }
                    //}
                    #endregion
                    break;
                case EditMode.SignalConnector:
                    #region SignalConnector
                    #region Route Handle
                    for (int i = 0; i < _routes.Length; i++)
                    {
                        if (_routes[0] == null)
                        {
                            ClearData(true);
                            break;
                        }
                        if (_routes[i].waypointDataList.Count > 0)
                        {
                            if (SC_routeIndex == -1 || SC_routeIndex == i)
                            {
                                int index = _routes[i].waypointDataList.Count - 1;
                                Vector3 pointTransformPosition = _routes[i].waypointDataList[index]._transform.position;
                                float pointDistance = Vector3.Distance(Camera.current.transform.position, pointTransformPosition);
                                float linearPointDistance = Mathf.InverseLerp(0, handleMaxSizeDistance, pointDistance);
                                Vector3 _textPosition = pointTransformPosition + (handleTextoffset * linearPointDistance);
                                float size = linearPointDistance * handleSize;
                                if (handleDrawDistance >= Vector3.Distance(Camera.current.transform.position, pointTransformPosition))
                                {
                                    style.normal.textColor = (SC_routeIndex == i) ? signalConnectorHandleTextActiveColor : signalConnectorHandleTextColor;
                                    Handles.color = (SC_routeIndex == i) ? signalConnectorHandleActiveColor : signalConnectorHandleColor;
                                    if (Handles.Button(pointTransformPosition, Quaternion.LookRotation(sceneViewCameraTransform.forward, sceneViewCameraTransform.up), size, size, Handles.DotHandleCap))
                                    {
                                        UndoRecordTargetObject(this, "Signal Connector Route Handle");
                                        if (this.SC_routeIndex == i)
                                        {
                                            this.SC_routeIndex = -1;
                                            this.SC_route = null;
                                        }
                                        else
                                        {
                                            this.SC_routeIndex = i;
                                            this.SC_route = this._routes[i].waypointDataList[index]._waypoint.onReachWaypointSettings.parentRoute;
                                        }
                                        Repaint();
                                    }
                                    Handles.Label(_textPosition, "R", style);
                                }
                            }
                        }
                    }
                    #endregion

                    #region Light Handle
                    for (int i = 0; i < SC_lights.Length; i++)
                    {
                        if (SC_lights[0] == null)
                        {
                            ClearData(true);
                            break;
                        }
                        if (SC_lightIndex == -1 || SC_lightIndex == i)
                        {
                            Vector3 pointTransformPosition = SC_lights[i].transform.position;
                            float pointDistance = Vector3.Distance(Camera.current.transform.position, pointTransformPosition);
                            float linearPointDistance = Mathf.InverseLerp(0, handleMaxSizeDistance, pointDistance);
                            Vector3 _textPosition = pointTransformPosition + (handleTextoffset * linearPointDistance);
                            float size = linearPointDistance * handleSize;
                            if (handleDrawDistance >= Vector3.Distance(Camera.current.transform.position, pointTransformPosition))
                            {
                                style.normal.textColor = (SC_lightIndex == i) ? signalConnectorHandleTextActiveColor : signalConnectorHandleTextColor;
                                Handles.color = (SC_lightIndex == i) ? signalConnectorHandleActiveColor : signalConnectorHandleColor;
                                if (Handles.Button(pointTransformPosition, Quaternion.LookRotation(sceneViewCameraTransform.forward, sceneViewCameraTransform.up), size, size, Handles.DotHandleCap))
                                {
                                    UndoRecordTargetObject(this, "Signal Connector Light Handle");
                                    if (SC_lightIndex == i)
                                    {
                                        SC_lightIndex = -1;
                                        SC_light = null;
                                    }
                                    else
                                    {
                                        SC_lightIndex = i;
                                        SC_light = SC_lights[i];
                                    }
                                    Repaint();
                                }
                                Handles.Label(_textPosition, "L", style);
                            }
                        }
                    }
                    #endregion

                    #region Line Handle
                    for (int i = 0; i < SC_lights.Length; i++)
                    {
                        if (SC_lights[i].waypointRoute != null)
                        {
                            int index = SC_lights[i].waypointRoute.waypointDataList.Count - 1;
                            Handles.color = signalConnectionColor;
                            Handles.DrawLine(SC_lights[i].transform.position, SC_lights[i].waypointRoute.waypointDataList[index]._transform.position);
                        }
                    }
                    #endregion

                    #endregion
                    break;
                case EditMode.SpawnPoints:
                    #region SpawnPoints
                    style.normal.textColor = spawnPointHandleTextColor;
                    Handles.color = spawnPointHandleColor;
                    for (int i = 0; i < _routes.Length; i++)
                    {
                        if (_routes[0] == null)
                        {
                            ClearData(true);
                            break;
                        }
                        if (_routes[i].waypointDataList.Count > 2) // ignore final 2 waypoints
                        {
                            for (int j = 0; j < _routes[i].waypointDataList.Count - 2; j++)
                            {
                                Vector3 pointTransformPosition = _routes[i].waypointDataList[j]._transform.position;
                                float pointDistance = Vector3.Distance(Camera.current.transform.position, pointTransformPosition);
                                float linearPointDistance = Mathf.InverseLerp(0, handleMaxSizeDistance, pointDistance);
                                Vector3 _textPosition = pointTransformPosition + (handleTextoffset * linearPointDistance);
                                float size = linearPointDistance * handleSize;
                                if (handleDrawDistance >= Vector3.Distance(Camera.current.transform.position, pointTransformPosition))
                                {
                                    if (_routes[i].waypointDataList[j]._transform.childCount == 0)
                                    {
                                        if (Handles.Button(pointTransformPosition, Quaternion.LookRotation(sceneViewCameraTransform.forward, sceneViewCameraTransform.up), size, size, Handles.DotHandleCap))
                                        {
                                            UndoRecordTargetObject(this, "Spawn Point Handle");
                                            GameObject loadedSpawnPoint = Instantiate(Resources.Load("AITrafficSpawnPoint"), this._routes[i].waypointDataList[j]._transform) as GameObject;
                                            Undo.RegisterCreatedObjectUndo(loadedSpawnPoint, "Create Spawn Point");

                                            AITrafficSpawnPoint trafficSpawnPoint = loadedSpawnPoint.GetComponent<AITrafficSpawnPoint>();
                                            trafficSpawnPoint.waypoint = trafficSpawnPoint.transform.parent.GetComponent<AITrafficWaypoint>();

                                            GameObject[] newSelection = new GameObject[1];
                                            newSelection[0] = loadedSpawnPoint;
                                            Selection.objects = newSelection;

                                            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                                            Repaint();
                                        }
                                        Handles.Label(_textPosition, "S", style);
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                    break;
            }
            SceneView.RepaintAll();
        }
        #endregion

        #region UndoRecordThisObject
        void UndoRecordTargetObject(Object targetObject, string actionName)
        {
            Undo.RegisterCompleteObjectUndo(targetObject, "STS Window Action: " + actionName);
            Undo.FlushUndoRecordObjects();
        }

        void UndoRecordTargetObject(Object[] targetObjects, string actionName)
        {
            Undo.RegisterCompleteObjectUndo(targetObjects, "STS Window Action: " + actionName);
            Undo.FlushUndoRecordObjects();
        }

        void OnUndoRedo()
        {
            requireRepaint = true;
        }
        #endregion

        #region SaveEditorPreferences
        void SaveColor(string _colorPropertyName, Color _color)
        {
            EditorPrefs.SetFloat("STS_" + _colorPropertyName + "_R", _color.r);
            EditorPrefs.SetFloat("STS_" + _colorPropertyName + "_G", _color.g);
            EditorPrefs.SetFloat("STS_" + _colorPropertyName + "_B", _color.b);
            EditorPrefs.SetFloat("STS_" + _colorPropertyName + "_A", _color.a);
        }

        public Color GetSavedColor(string _colorPropertyName, Color _defaultColor)
        {
            Color savedColor = new Color
                (
                 EditorPrefs.GetFloat("STS_" + _colorPropertyName + "_R", _defaultColor.r),
                 EditorPrefs.GetFloat("STS_" + _colorPropertyName + "_G", _defaultColor.g),
                 EditorPrefs.GetFloat("STS_" + _colorPropertyName + "_B", _defaultColor.b),
                 EditorPrefs.GetFloat("STS_" + _colorPropertyName + "_A", _defaultColor.a)
                );
            return savedColor;
        }
        #endregion

        void SceneChanged(Scene scene1, Scene scene2)
        {
            Debug.Log("STS Tools Window: A new scene was loaded, unloading window data");
            ClearData(true);
        }

        void PlayModeStateChangedCallback(PlayModeStateChange state)
        {
            Debug.Log("STS Tools Window: Edit/Play mode state has changed, unloading window data");
            ClearData(true);
        }

        void ClearData(bool clearRoutes)
        {
            if (clearRoutes)
            {
                _routes = new AITrafficWaypointRoute[0];
                SC_lights = new AITrafficLight[0];
            }
            SC_routeIndex = -1;
            SC_lightIndex = -1;
            SC_light = null;
            SC_route = null;
            RC_fromRouteIndex = -1;
            RC_fromPointIndex = -1;
            RC_toRouteIndex = -1;
            RC_toPointIndex = -1;
            RC_fromPoint = null;
            RC_toPoint = null;
            LC_routeIndexA = -1;
            LC_routeIndexB = -1;
            LC_routeA = null;
            LC_routeB = null;
            RE_route = null;
            RE_routeIndex = -1;
            showDebug = false;
            EditorUtility.SetDirty(this);
            Repaint();
        }

    }
}