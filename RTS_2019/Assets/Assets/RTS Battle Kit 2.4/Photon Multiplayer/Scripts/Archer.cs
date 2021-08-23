using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if PHOTON_MULTIPLAYER
using ExitGames.Client.Photon;

public class Archer : Photon.PunBehaviour {
	
	public GameObject arrow;
	public Transform arrowSpawner;
	public GameObject animationArrow;
	
	private bool shooting;
	private bool addArrowForce;
	private GameObject newArrow;
	private Animator animator;
	
	void Start(){
		animator = GetComponent<Animator>();
	}
	
	void Update(){
		float animationTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1;
		
		//only shoot when animation is almost done (when the character is shooting)
		if(animator.GetBool("Attacking") == true && animationTime >= 0.95f && !shooting){
			StartCoroutine(Shoot());
		}
		
		animationArrow.SetActive(animationTime > 0.25f && animationTime < 0.95f);
	}
	
	void LateUpdate(){		
		//check if the archer shoots an arrow
		if(addArrowForce && newArrow != null && arrowSpawner != null){
			Unit target = GetComponent<Unit>();
			newArrow.GetComponent<Rigidbody>().AddForce(transform.TransformDirection(target.arrowForce));
			
			addArrowForce = false;
		}
	}
	
	IEnumerator Shoot(){
		//archer is currently shooting
		shooting = true;
		
		//add a new arrow
		newArrow = Instantiate(arrow, arrowSpawner.position, arrowSpawner.rotation) as GameObject;
		newArrow.GetComponent<Arrow>().arrowOwner = this.gameObject;
		//shoot it using rigidbody addforce
		addArrowForce = true;
	
		//wait and set shooting back to false
		yield return new WaitForSeconds(0.5f);
		shooting = false;
	}
}

#else

public class Archer : MonoBehaviour {
	//empty monobehaviour class for when photon has not been imported
	
	public GameObject arrow;
	public Transform arrowSpawner;
	public GameObject animationArrow;
}

#endif
