namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine;
    using UnityEditor;

    [CustomEditor(typeof(AITrafficController))]
    public class Editor_AITrafficController : Editor
    {
        static bool isInitialized;
        public static void Initialize()
        {
            if (!isInitialized)
            {
                isInitialized = true;
                SceneView.duringSceneGui += CustomOnSceneGUI;
            }
        }

        private void OnEnable()
        {
            if (!isInitialized)
            {
                isInitialized = true;
                SceneView.duringSceneGui += CustomOnSceneGUI;
            }
        }
        private void OnDisable()
        {
            SceneView.duringSceneGui += CustomOnSceneGUI;
            isInitialized = false;
        }

        static void CustomOnSceneGUI(SceneView sceneview)
        {
            if (Application.isPlaying)
            {
                if (AITrafficController.Instance.m_AITrafficDebug.showCarGizmos)
                {
                    for (int i = 0; i < AITrafficController.Instance.carTransformPositionArray.Length; i++)
                    {
                        if (AITrafficController.Instance.isDisabledArray[i] == false)
                        {
                            Handles.DrawBezier(
                                AITrafficController.Instance.carTransformPositionArray[i],
                                AITrafficController.Instance.driveTargetTransformAccessArray[i].position,
                                AITrafficController.Instance.carTransformPositionArray[i],
                                AITrafficController.Instance.driveTargetTransformAccessArray[i].position,
                                Color.green,
                                null,
                                3);
                        }
                    }
                }
            }
        }
    }

    [InitializeOnLoad]
    public static class PlayModeStateChangedExample
    {
        static PlayModeStateChangedExample()
        {
            EditorApplication.playModeStateChanged += LogPlayModeState;
        }

        private static void LogPlayModeState(PlayModeStateChange state)
        {
            if (state.ToString() == "EnteredPlayMode" && AITrafficController.Instance)
            {
                Editor_AITrafficController.Initialize();
            }
        }

        [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
        static void DrawHandles(AITrafficController _AITrafficController, GizmoType gizmoType)
        {
            if (EditorApplication.isPlaying && _AITrafficController.usePooling && _AITrafficController._AITrafficPool.showGizmos)
            {
                

                

                

                // spawn zone
                Handles.color = _AITrafficController._AITrafficPool.spawnZoneColor;
                Handles.DrawSolidDisc
                    (
                    _AITrafficController.centerPosition,
                Vector3.up,
                _AITrafficController._AITrafficPool.spawnZone
                );

                // active zone
                Handles.color = _AITrafficController._AITrafficPool.activeZoneColor;
                Handles.DrawSolidDisc
                    (
                    _AITrafficController.centerPosition,
                Vector3.up,
                _AITrafficController._AITrafficPool.actizeZone
                );

                // cull headlight zone
                Handles.color = _AITrafficController._AITrafficPool.cullHeadLightZone;
                Handles.DrawSolidDisc
                    (
                    _AITrafficController.centerPosition,
                Vector3.up,
                _AITrafficController._AITrafficPool.cullHeadLight
                );

                // min spawn zone
                Handles.color = _AITrafficController._AITrafficPool.minSpawnZoneColor;
                Handles.DrawSolidDisc
                    (
                    _AITrafficController.centerPosition,
                Vector3.up,
                _AITrafficController._AITrafficPool.minSpawnZone
                );
            }
        }
    }

}