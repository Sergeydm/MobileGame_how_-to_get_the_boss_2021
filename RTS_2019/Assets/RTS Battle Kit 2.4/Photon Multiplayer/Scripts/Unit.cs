using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

#if PHOTON_MULTIPLAYER
using ExitGames.Client.Photon;

public class Unit : Photon.PunBehaviour {
	
	//visible in the inspector
	public float health;
	public float damage;
	public float minAttackDistance;
	public float castleStoppingDistance;
	public GameObject dieEffect;
	
	public Image healthBar;
	public Color masterClientColor;
	public Color clientColor;
	
	public string clientCastle;
	public string masterClientCastle;
	public string clientUnit;
	public string masterClientUnit;
	
	public string unitType;
	public GameObject selectedObject;
	
	public bool wizard;
	public ParticleSystem spawnEffect;
	public int skeletonCount;
	public float skeletonDelay;
	
	//not in the inspector
	string attackTag;
	string castleAttackTag;
	
	float startHealth;
	float defaultStoppingDistance;
	
	float lastSerializeTime;
	Vector3 lastPosition;
	float clientSpeed;
	
	Transform enemyCastle;
	NavMeshAgent agent;
	Animator[] anims;
	
	[HideInInspector]
	public bool selected;
	
	[HideInInspector]
	public bool local;
	
	[HideInInspector]
	public Vector3 arrowForce;
	
	bool master;
	
	bool archer;
	bool wizardSpawns;
	public bool clientStarted;
	
	Vector3 position;
	Quaternion rotation;
	
	[HideInInspector]
	public Vector3 clickedPosition;
	
	void Start(){
		master = PhotonNetwork.isMasterClient;
		archer = GetComponent<Archer>() != null;
		anims = GetComponentsInChildren<Animator>();
		agent = GetComponent<NavMeshAgent>();	
		
		//clientSpeed = agent.speed;
		startHealth = health;
		
		if(master){			
			//if master, tell the client to set the 'local' value and set colors and tags
			defaultStoppingDistance = agent.stoppingDistance;
			
			photonView.RPC("SetLocalState", PhotonTargets.Others, !local);
			SetUnitColor();
			SetTags();
			
			//spawn skeletons if this is a wizard
			if(wizard)
				StartCoroutine(SpawnSkeletons());
		}
		else{
			//if this is the client side, initialize the position and last position and start
			position = transform.position;
			lastPosition = transform.position;
			
			StartCoroutine(StartClient());
		}
	}
	
	void Update(){
		//perform all unit updates on the master (and the first second for the client to stop it from lagging on spawn)
		if(master || !clientStarted){
			UnitUpdate();
		}
		else{
			//if this is the client side, simply take the position and rotation from the master client and smooth them out
			transform.position = Vector3.MoveTowards(transform.position, position, clientSpeed * Time.deltaTime);
			transform.rotation = Quaternion.Lerp(transform.rotation, rotation, 4f * Time.deltaTime);
		}
		
		//update the healthbar and selected object on both sides
		healthBar.fillAmount = health/startHealth;
		healthBar.transform.parent.parent.parent.LookAt(2 * transform.position - Camera.main.transform.position);
		selectedObject.SetActive(selected);
	}
	
	//checks if this unit is local (client unit on the client side or master client unit on the master client side)
	[PunRPC]
	void SetLocalState(bool local){
		this.local = local;
		
		SetUnitColor();
		SetTags();
	}
	
	//updates the healthbar color for this unit
	void SetUnitColor(){
		Color color = Color.white;
		
		if(master){
			color = local ? masterClientColor : clientColor;
		}
		else{
			color = local ? clientColor : masterClientColor;
		}
		
		healthBar.color = color;
	}
	
	//updates the tags for this unit (which units to attack, which castles to attack, etc.)
	void SetTags(){
		if(local){
			attackTag = master ? clientUnit : masterClientUnit;
			castleAttackTag = master ? clientCastle : masterClientCastle;
			
			gameObject.tag = master ? masterClientUnit : clientUnit;
		}
		else{
			attackTag = master ? masterClientUnit : clientUnit;
			castleAttackTag = master ? masterClientCastle : clientCastle;
			
			gameObject.tag = master ? clientUnit : masterClientUnit;
		}
	}
	
	//general unit update performed on the master client side
	void UnitUpdate(){	
		if(clickedPosition == Vector3.zero){
			//do regular navigation if we're not moving to a clicked position
			Navigate();
		}
		else{
			//move towards the clicked position
			agent.destination = clickedPosition;
			
			if(agent.stoppingDistance != 2f){
				agent.isStopped = false;
				agent.stoppingDistance = 2f;
			}
			
			//stop moving to the click position if we have reached it
			if(Vector3.Distance(transform.position, clickedPosition) < agent.stoppingDistance + 0.1f){
				agent.stoppingDistance = defaultStoppingDistance;
				clickedPosition = Vector3.zero;
			}		
			else if(anims[0].GetBool("Attacking")){
				SetAnim(false);
			}
		}
		
		//die if the unit doesn't have health left
		if(health <= 0)
			Die();
	}
	
	void Navigate(){
		//try to find a unit target
		GameObject targetUnit = GetTarget(attackTag);
		
		//attack the castle if there's no target unit
		if(targetUnit == null || Vector3.Distance(transform.position, targetUnit.transform.position) > minAttackDistance){
			AttackCastle();
		}
		else{
			//otherwise move to the target
			agent.destination = targetUnit.transform.position;
			
			if(agent.isStopped)
				agent.isStopped = false;
			
			//if we're close enough, attack the target
			if(Vector3.Distance(targetUnit.transform.position, transform.position) < agent.stoppingDistance + 0.1f){
				targetUnit.GetComponent<Unit>().health -= Time.deltaTime * damage;
				
				LookAt(targetUnit.transform);

				if(!anims[0].GetBool("Attacking"))
					SetAnim(true);
			}
			else if(anims[0].GetBool("Attacking")){
				SetAnim(false);
			}
			
			//calculate the arrow force so the archers know how fast to shoot their arrows
			if(archer)
				UpdateArrowForce(targetUnit.transform.position);
		}
	}
	
	void AttackCastle(){
		//if there's no enemy castle, try to find one
		if(enemyCastle == null){
			GameObject targetCastle = GetTarget(castleAttackTag);
			
			if(targetCastle != null)
				enemyCastle = targetCastle.transform;
			
			return;
		}
		
		//update the arrow force for the archers to use
		if(archer)
			UpdateArrowForce(enemyCastle.position);
		
		//attack the castle if close enough
		if(Vector3.Distance(enemyCastle.position, transform.position) < castleStoppingDistance){
			CastleMultiplayer castle = enemyCastle.parent.GetComponent<CastleMultiplayer>();
			castle.AttackCastle(enemyCastle.gameObject, damage);
			
			LookAt(enemyCastle);
			CheckState(true);
		}
		else{
			agent.destination = enemyCastle.position;
			CheckState(false);
		}
	}
	
	//wait for a second before using master client input to prevent lag right after spawning
	IEnumerator StartClient(){
		yield return new WaitForSeconds(1f);
		
		clientStarted = true;
		agent.enabled = false;
	}
	
	//calculate the arrow force using the distance to our target
	void UpdateArrowForce(Vector3 targetPosition){
		float shootingForce = Vector3.Distance(transform.position, targetPosition);
		arrowForce = new Vector3(0, shootingForce * 12 + ((targetPosition.y - transform.position.y) * 45), shootingForce * 55);
	}
	
	//find the current target (either a unit or a castle)
	GameObject GetTarget(string tag){
		if(tag == null || tag == "")
			return null;
		
		GameObject[] possibleTargets = GameObject.FindGameObjectsWithTag(tag);
		
		GameObject target = null;
		float smallestDistance = Mathf.Infinity;
		
		for(int i = 0; i < possibleTargets.Length; i++){
			float dist = Vector3.Distance(possibleTargets[i].transform.position, transform.position);
			
			if(dist < smallestDistance){
				smallestDistance = dist;
				target = possibleTargets[i];
			}
		}
		
		return target;
	}
	
	//sets the clicked position so this character will move there
	public void MoveToClick(Vector3 position){
		clickedPosition = position;
	}
	
	//destroy this unit
	public void Die(){
		string ownerTag = local ? masterClientCastle : clientCastle;
		CastleMultiplayer owner = GameObject.FindGameObjectWithTag(ownerTag).transform.parent.GetComponent<CastleMultiplayer>();
		
		owner.DestroyUnit(gameObject, unitType, dieEffect);
	}
	
	//spawn skeletons (master client)
	IEnumerator SpawnSkeletons(){
		while(true){
			//if character is not attacking
			if(!anims[0].GetBool("Attacking")){
				photonView.RPC("StartSkeletonSpawner", PhotonTargets.All);
			}
		
			//short delay before the loop starts again
			yield return new WaitForSeconds(skeletonDelay);
		}
	}
	
	//gets send to both the client and the master client so they both spawn skeletons
	[PunRPC]
	void StartSkeletonSpawner(){
		StartCoroutine(SpawnSkeletonsLocal());
	}
	
	IEnumerator SpawnSkeletonsLocal(){
		WizardSpawnState(true);
				
		//wait 0.5 seconds
		yield return new WaitForSeconds(0.5f);
		//play the spawn effect
		spawnEffect.Play();	
				
		if(master){
			//spawn the correct amount of skeletons with some delay
			for(int i = 0; i < skeletonCount; i++){
				SpawnSkeleton();
						
				yield return new WaitForSeconds(2f/skeletonCount);
			}
		}
			
		//stop playing the spawneffect
		spawnEffect.Stop();
			
		//wait for 0.5 seconds again
		yield return new WaitForSeconds(0.5f);
			
		WizardSpawnState(false);
	}
	
	void WizardSpawnState(bool state){
		//wizard is now spawning
		wizardSpawns = state;
			
		//stop the navmesh agent
		agent.isStopped = state;
			
		//start spawning animation
		anims[0].SetBool("Spawning", state);
	}
	
	void SpawnSkeleton(){
		//instantiate a skeleton character in front of the wizard via the castle
		Vector3 pos = transform.position;
		Vector3 spawnPos = pos + transform.forward * Random.Range(1f, 2f) + transform.right * Random.Range(-0.5f, 0.5f);
		CastleMultiplayer.LocalCastle.GetComponent<CastleMultiplayer>().SpawnUnit(spawnPos, local, -1);
	}
	
	//look at target
	void LookAt(Transform target){
		Vector3 pos = target.position;
		pos.y = transform.position.y;
		transform.LookAt(pos);
	}
	
	//set attacking state (animation and isStopped)
	void CheckState(bool attack){
		if(anims[0].GetBool("Attacking") != attack)
			SetAnim(attack);
			
		if(agent.isStopped != attack)
			agent.isStopped = attack;
	}
	
	//update animations on all animators (for cavalry)
	void SetAnim(bool attacking){
		for(int i = 0; i < anims.Length; i++){
			anims[i].SetBool("Attacking", attacking);
		}
	}
	
	//called multiple times per second
	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
		if(stream.isWriting){
			//the master will write data to the client
			if(!master)
				return;
			
			stream.SendNext(this.health);
			
			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);
			
			bool attacking = anims[0].GetBool("Attacking");
			stream.SendNext(attacking);
			
			if(archer)
				stream.SendNext(arrowForce);
		}
        else if(!master){
			//the client receives data from the master client
			this.health = (float)stream.ReceiveNext();
			
			this.position = (Vector3)stream.ReceiveNext();
			this.rotation = (Quaternion)stream.ReceiveNext();
			
			bool attackAnim = (bool)stream.ReceiveNext();
			
			if(anims != null && anims.Length != 0)
				SetAnim(attackAnim);
			
			if(archer)
				this.arrowForce = (Vector3)stream.ReceiveNext();
			
			//after receiving the position, it will estimate the unit speed to create smooth movement
			EstimateSpeed();
			
			//update last time and position for the speed estimation
			lastSerializeTime = Time.time;
			lastPosition = position;
        }
	}
	
	void EstimateSpeed(){		
		if(lastSerializeTime == 0)
			return;
		
		//use speed = distance/time to estimate move speed
		float distance = Vector3.Distance(position, lastPosition);
		float time = Time.time - lastSerializeTime;
		
		clientSpeed = distance/time;
	}
}

#else

public class Unit : MonoBehaviour {
	//empty monobehaviour class for when photon has not been imported
	
	public float health;
	public float damage;
	public float minAttackDistance;
	public float castleStoppingDistance;
	public GameObject dieEffect;
	
	public Image healthBar;
	public Color masterClientColor;
	public Color clientColor;
	
	public string clientCastle;
	public string masterClientCastle;
	public string clientUnit;
	public string masterClientUnit;
	
	public string unitType;
	public GameObject selectedObject;
	
	public bool wizard;
	public ParticleSystem spawnEffect;
	public int skeletonCount;
	public float skeletonDelay;
}

#endif