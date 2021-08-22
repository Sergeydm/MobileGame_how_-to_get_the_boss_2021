namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "AITrafficDebug", menuName = "SimpleTrafficSystem/AITrafficDebug")]
    public class AITrafficDebug : ScriptableObject
    {
        [Header("Waypoint Routes")]
        public bool alwaysDrawGizmos = true;
        public bool canUpdateGizmos;
        public bool alwaysSideSensorGizmos;
        public Color pathColor = new Color(0, 1, 0, 0.298f);
        public Color selectedPathColor = new Color(0, 1, 0, 1);
        public Color junctionColor = new Color(1, 1, 0, 0.298f);
        public Color selectedJunctionColor = new Color(1, 1, 0, 1);
        public float arrowHeadWidth = 10.0f;
        public float arrowHeadLength = 2.0f;
        public Vector3 arrowHeadScale = new Vector3(1, 0, 1);
        public float updateGizmoCoolDown = 0.1f;

        [Header("AI Cars")]
        public bool showCarGizmos;
        public Color detectColor;
        public Color normalColor;

        [Header("Job Controller")]
        public bool debugProcessTime;
    }
}
