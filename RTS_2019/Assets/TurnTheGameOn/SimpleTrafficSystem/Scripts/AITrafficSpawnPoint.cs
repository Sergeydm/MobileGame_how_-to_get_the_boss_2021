namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine;
    using System.Collections;

    public class AITrafficSpawnPoint : MonoBehaviour
    {
        public bool isTrigger { get; private set; }
        public bool isVisible { get; private set; }
        public Transform transformCached { get; private set; }
        public int assignedIndex { get; private set; }
        public AITrafficWaypoint waypoint;
        public Material runtimeMaterial;

        private void OnEnable()
        {
            GetComponent<MeshRenderer>().sharedMaterial = runtimeMaterial;
        }

        private void Awake()
        {
            transformCached = transform;
            StartCoroutine(RegisterSpawnPoint());
        }

        IEnumerator RegisterSpawnPoint()
        {
            while (AITrafficController.Instance == null)
            {
                yield return new WaitForEndOfFrame();
            }
            assignedIndex = AITrafficController.Instance.RegisterSpawnPoint(this);
        }

        void OnBecameInvisible()
        {
            isVisible = false;
        }

        void OnBecameVisible()
        {
            isVisible = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            isTrigger = true;
        }

        private void OnTriggerStay(Collider other)
        {
            isTrigger = true;
        }

        private void OnTriggerExit(Collider other)
        {
            isTrigger = false;
        }

        public bool CanSpawn()
        {
            if (!isVisible && !isTrigger)
                return true;
            else
                return false;
        }
    }
}