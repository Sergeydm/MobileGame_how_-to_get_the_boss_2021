namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine;

    public class AITrafficLight : MonoBehaviour
    {
        public MeshRenderer redMesh;
        public MeshRenderer yellowMesh;
        public MeshRenderer greenMesh;
        public AITrafficWaypointRoute waypointRoute;

        public void EnableRedLight()
        {
            if (waypointRoute) waypointRoute.StopForTrafficlight(true);
            redMesh.enabled = true;
            yellowMesh.enabled = false;
            greenMesh.enabled = false;
        }

        public void EnableYellowLight()
        {
            if (waypointRoute) waypointRoute.StopForTrafficlight(true);
            redMesh.enabled = false;
            yellowMesh.enabled = true;
            greenMesh.enabled = false;
        }

        public void EnableGreenLight()
        {
            if (waypointRoute) waypointRoute.StopForTrafficlight(false);
            redMesh.enabled = false;
            yellowMesh.enabled = false;
            greenMesh.enabled = true;
        }

        public void DisableAllLights()
        {
            if (waypointRoute) waypointRoute.StopForTrafficlight(true);
            redMesh.enabled = false;
            yellowMesh.enabled = false;
            greenMesh.enabled = false;
        }

    }
}