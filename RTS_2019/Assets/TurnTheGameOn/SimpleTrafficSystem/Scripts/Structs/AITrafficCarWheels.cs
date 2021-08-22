namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine;

    [System.Serializable]
    public struct AITrafficCarWheels
    {
        public string name;
        public GameObject mesh;
        public Transform meshTransform;
        public WheelCollider collider;
    }
}