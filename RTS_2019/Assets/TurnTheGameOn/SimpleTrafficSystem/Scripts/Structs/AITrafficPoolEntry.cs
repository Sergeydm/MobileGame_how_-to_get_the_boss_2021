namespace TurnTheGameOn.SimpleTrafficSystem
{
    [System.Serializable]
    public struct AITrafficPoolEntry
    {
        public string name;
        public int assignedIndex;
        public AITrafficCar trafficPrefab;
    }
}