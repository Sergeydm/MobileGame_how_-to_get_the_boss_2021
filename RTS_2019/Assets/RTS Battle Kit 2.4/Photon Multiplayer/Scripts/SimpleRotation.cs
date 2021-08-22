using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRotation : MonoBehaviour {
	
	//rotates object using speed
	public float speed;
	
	void Update () {
		transform.Rotate(Vector3.up * Time.deltaTime * -speed);
	}
}
