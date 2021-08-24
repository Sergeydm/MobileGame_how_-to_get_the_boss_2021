using UnityEngine;
using System.Collections;

namespace SensorToolkit.Example
{
    [RequireComponent(typeof(CharacterControls))]
    public class PlayerInput : MonoBehaviour
    {
        public Sensor InteractionRange;

        CharacterControls cc;
        
        void Start()
        {
            cc = GetComponent<CharacterControls>();
        }

        void Update()
        {



            // Project mouse position onto worlds plane at y=0

            var camPosition = Camera.main.transform.position;
            var camForward = Camera.main.transform.forward;
            var camDepthToGroundPlane = -camPosition.y / camForward.y;

            // Pickup the pickup if its in range
            var pickup = InteractionRange.GetNearestByComponent<Holdable>();
            if (pickup != null && !pickup.IsHeld)
            {
                cc.PickUp(pickup);
            }
        }
    }
}