namespace TurnTheGameOn.SimpleTrafficSystem
{
    using System.Collections;
    using UnityEngine;

    public class AITrafficLightManager : MonoBehaviour
    {
        [System.Serializable]
        public struct TrafficLightCycle
        {
            public float greenTimer;
            public float yellowTimer;
            public float redtimer;
            public AITrafficLight[] trafficLights;
        }
        public TrafficLightCycle[] trafficLightCycles;

        private void Start()
        {
            EnableRedLights();
            StartCoroutine(StartTrafficLightCycles());
        }

        private void EnableRedLights()
        {
            for (int i = 0; i < trafficLightCycles.Length; i++)
            {
                for (int j = 0; j < trafficLightCycles[i].trafficLights.Length; j++)
                {
                    trafficLightCycles[i].trafficLights[j].EnableRedLight();
                }
            }
        }

        IEnumerator StartTrafficLightCycles()
        {
            while (true)
            {
                for (int i = 0; i < trafficLightCycles.Length; i++)
                {
                    for (int j = 0; j < trafficLightCycles[i].trafficLights.Length; j++)
                    {
                        trafficLightCycles[i].trafficLights[j].EnableGreenLight();
                    }
                    yield return new WaitForSeconds(trafficLightCycles[i].greenTimer);
                    for (int j = 0; j < trafficLightCycles[i].trafficLights.Length; j++)
                    {
                        trafficLightCycles[i].trafficLights[j].EnableYellowLight();
                    }
                    yield return new WaitForSeconds(trafficLightCycles[i].yellowTimer);
                    for (int j = 0; j < trafficLightCycles[i].trafficLights.Length; j++)
                    {
                        trafficLightCycles[i].trafficLights[j].EnableRedLight();
                    }
                    yield return new WaitForSeconds(trafficLightCycles[i].redtimer);
                }
            }
        }

    }
}