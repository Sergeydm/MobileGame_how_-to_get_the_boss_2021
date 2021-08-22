using UnityEngine;
using System.Collections;

public class Arrow : MonoBehaviour {
	
	[HideInInspector]
	public GameObject arrowOwner;
	
	private string archerTag;
	private Rigidbody rb;
	
	void Start(){
		if(arrowOwner != null)
			archerTag = arrowOwner.tag;
		
		rb = GetComponent<Rigidbody>();
		
		//destroy arrow after 5 seconds
		Destroy(gameObject, 5);
	}
	
	void Update(){
		transform.LookAt(transform.position + rb.velocity);
	}

	void OnTriggerEnter(Collider other){
		//freeze arrow when it hits an enemy and parent it to the enemy to move with it
		if((other.gameObject.tag == "Enemy" || other.gameObject.tag == "Knight") && other.gameObject.tag != archerTag){
			rb.velocity = Vector3.zero;
			rb.isKinematic = true;
			transform.parent = other.gameObject.transform;
		}
		else if(other.gameObject.tag == "Battle ground"){
			//destroy arrow when it hits the ground
			Destroy(gameObject);	
		}
	}
}
