namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine;

    public class RandomSpeed : MonoBehaviour
    {
        public float minSpeed = 15f;
        public float maxSpeed = 35f;

        void OnEnable()
        {
            GetComponent<AITrafficCar>().topSpeed = UnityEngine.Random.Range(minSpeed, maxSpeed);
        }
    }
}