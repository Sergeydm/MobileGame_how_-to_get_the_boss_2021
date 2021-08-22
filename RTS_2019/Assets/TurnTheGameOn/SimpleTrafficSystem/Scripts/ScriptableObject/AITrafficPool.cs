namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "AITrafficPool", menuName = "SimpleTrafficSystem/AITrafficPool")]
    public class AITrafficPool : ScriptableObject
    {
        public GameObject[] trafficPrefabs;
        public int density = 75;
        public float spawnRate = 5f;
        [Header("Zones")]
        public float minSpawnZone = 50;
        public float cullHeadLight = 100;
        
        public float actizeZone = 400;
        public float spawnZone = 600;

        [Header("Debug")]
        public bool hideSpawnPointsInEditMode;
        public bool showGizmos;
        public Color minSpawnZoneColor;
        public Color cullHeadLightZone;
        
        public Color activeZoneColor;
        public Color spawnZoneColor;
        public bool debugProcessTime;
    }
}
