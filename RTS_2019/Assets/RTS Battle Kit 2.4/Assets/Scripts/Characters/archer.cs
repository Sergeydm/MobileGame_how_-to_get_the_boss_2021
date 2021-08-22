using UnityEngine;
using System.Collections;

public class archer : MonoBehaviour {
	
	public GameObject arrow;
	public Transform arrowSpawner;
	public GameObject animationArrow;
	
	private bool shooting;
	private bool addArrowForce;
	private GameObject newArrow;
	private float shootingForce;
	private Animator animator;
	
	void Start(){
		animator = GetComponent<Animator>();
	}
	
	void Update(){
		float animationTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1;
		
		//only shoot when animation is almost done (when the character is shooting)
		if(animator.GetBool("Attacking") == true && animationTime >= 0.95f && !shooting)
			StartCoroutine(Shoot());
		
		animationArrow.SetActive(animationTime > 0.25f && animationTime < 0.95f);
	}
	
	void LateUpdate(){		
		//check if the archer shoots an arrow
		if(addArrowForce && newArrow != null && arrowSpawner != null){
			Character target = GetComponent<Character>();
			Vector3 targetPosition = target.currentTarget != null ? target.currentTarget.transform.position : target.castleAttackPosition;
			
			//create a shootingforce
			shootingForce = Vector3.Distance(transform.position, targetPosition);
			
			//add shooting force to the arrow
			Vector3 force = new Vector3(0, shootingForce * 12 + ((targetPosition.y - transform.position.y) * 45), shootingForce * 55);
			newArrow.GetComponent<Rigidbody>().AddForce(transform.TransformDirection(force));
			
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
