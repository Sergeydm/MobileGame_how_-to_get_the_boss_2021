using UnityEngine;

public class FakeWheel : MonoBehaviour
{
    /// A simple fake wheel implementation, useful for vehicles that have more than 4 wheels.
    /// Attach it to a dummy wheel mesh
    /// Assign a reference wheel mesh
    /// The fake wheel have the same rotation and y position offset as the reference wheel.
    public Transform referenceWheel;
    private Transform fakeWheel;
    private Vector3 newPosition;

    private void Awake()
    {
        fakeWheel = transform;
    }

    private void Update()
    {
        newPosition = fakeWheel.position;
        newPosition.y = referenceWheel.position.y;
        fakeWheel.transform.SetPositionAndRotation(newPosition, referenceWheel.transform.rotation);
    }
}