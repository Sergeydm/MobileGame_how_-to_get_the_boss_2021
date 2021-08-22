namespace TurnTheGameOn.SimpleTrafficSystem
{
    using UnityEngine;
    using UnityEditor;

    [CustomEditor(typeof(AITrafficCar))]
    public class Editor_AITrafficCar : Editor
    {
        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour((AITrafficCar)target), typeof(AITrafficCar), false);
            GUI.enabled = true;

            AITrafficCar vehicleAI = (AITrafficCar)target;

            SerializedProperty goToStartOnStop = serializedObject.FindProperty("goToStartOnStop");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(goToStartOnStop, true);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();

            SerializedProperty topSpeed = serializedObject.FindProperty("topSpeed");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(topSpeed, true);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();

            SerializedProperty brakeMaterial = serializedObject.FindProperty("brakeMaterial");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(brakeMaterial, true);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();

            SerializedProperty frontSensorTransform = serializedObject.FindProperty("frontSensorTransform");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(frontSensorTransform, true);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();

            SerializedProperty leftSensorTransform = serializedObject.FindProperty("leftSensorTransform");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(leftSensorTransform, true);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();

            SerializedProperty rightSensorTransform = serializedObject.FindProperty("rightSensorTransform");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(rightSensorTransform, true);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();

            SerializedProperty headLight = serializedObject.FindProperty("headLight");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(headLight, true);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();

            SerializedProperty _wheels = serializedObject.FindProperty("_wheels");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_wheels, true);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Align Wheel Colliders"))
            {
                AlignWheelColliders();
            }
        }

        public void AlignWheelColliders()
        {
            AITrafficCar driveSystem = (AITrafficCar)target;
            Transform defaultColliderParent = driveSystem._wheels[0].collider.transform.parent; // make a reference to the colliders original parent

            driveSystem._wheels[0].collider.transform.parent = driveSystem._wheels[0].mesh.transform;// move colliders to the reference positions
            driveSystem._wheels[1].collider.transform.parent = driveSystem._wheels[1].mesh.transform;
            driveSystem._wheels[2].collider.transform.parent = driveSystem._wheels[2].mesh.transform;
            driveSystem._wheels[3].collider.transform.parent = driveSystem._wheels[3].mesh.transform;

            driveSystem._wheels[0].collider.transform.position = new Vector3(driveSystem._wheels[0].mesh.transform.position.x,
                driveSystem._wheels[0].collider.transform.position.y, driveSystem._wheels[0].mesh.transform.position.z); //adjust the wheel collider positions on x and z axis to match the new wheel position
            driveSystem._wheels[1].collider.transform.position = new Vector3(driveSystem._wheels[1].mesh.transform.position.x,
                driveSystem._wheels[1].collider.transform.position.y, driveSystem._wheels[1].mesh.transform.position.z);
            driveSystem._wheels[2].collider.transform.position = new Vector3(driveSystem._wheels[2].mesh.transform.position.x,
                driveSystem._wheels[2].collider.transform.position.y, driveSystem._wheels[2].mesh.transform.position.z);
            driveSystem._wheels[3].collider.transform.position = new Vector3(driveSystem._wheels[3].mesh.transform.position.x,
                driveSystem._wheels[3].collider.transform.position.y, driveSystem._wheels[3].mesh.transform.position.z);

            driveSystem._wheels[0].collider.transform.parent = defaultColliderParent; // move colliders back to the original parent
            driveSystem._wheels[1].collider.transform.parent = defaultColliderParent;
            driveSystem._wheels[2].collider.transform.parent = defaultColliderParent;
            driveSystem._wheels[3].collider.transform.parent = defaultColliderParent;
        }
    }
}