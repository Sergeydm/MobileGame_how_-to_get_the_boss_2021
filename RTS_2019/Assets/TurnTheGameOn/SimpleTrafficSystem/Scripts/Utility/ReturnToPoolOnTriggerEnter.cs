namespace TurnTheGameOn.SimpleTrafficSystem
{
    /// Gets a reference to the AITrafficCar script to get the car's assignedIndex
    /// then use that to call AITrafficController.MoveCarToPool(assignedIndex)

    /// Create a cube
    /// Set BoxCollider IsTrigger to true
    /// Attach this script
    /// Then use it as a trigger zone that returns cars to the pool
    using UnityEngine;

    public class ReturnToPoolOnTriggerEnter : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "AITrafficCar")
            {
                AITrafficCar _AITrafficCar = other.GetComponent<AITrafficCar>();
                int _assignedIndex = _AITrafficCar.assignedIndex;
                AITrafficController.Instance.MoveCarToPool(_assignedIndex);
            }
        }
    }
}