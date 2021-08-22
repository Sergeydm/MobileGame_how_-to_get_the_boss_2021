namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine;

    [RequireComponent(typeof(AITrafficCar))]
    public class BrakeLight : MonoBehaviour
    {
        public Light[] brakeLights;
        private AITrafficCar car;
        bool isBraking;

        private void Awake()
        {
            car = GetComponent<AITrafficCar>();
            for (int i = 0; i < brakeLights.Length; i++)
            {
                brakeLights[i].enabled = false;
            }
        }

        void Update()
        {
            if (car.IsBraking())
            {
                if (isBraking == false)
                {
                    isBraking = true;
                    for (int i = 0; i < brakeLights.Length; i++)
                    {
                        brakeLights[i].enabled = true;
                    }
                }
            }
            else
            {
                if (isBraking)
                {
                    isBraking = false;
                    for (int i = 0; i < brakeLights.Length; i++)
                    {
                        brakeLights[i].enabled = false;
                    }
                }
            }
        }
    }
}