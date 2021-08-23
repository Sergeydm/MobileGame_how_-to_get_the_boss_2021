using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

#if PHOTON_MULTIPLAYER
using ExitGames.Client.Photon;

public class CastleMultiplayer : Photon.PunBehaviour, IPunObservable {
	
	//visible in the inspector
	public Troop[] troops;
	public CastlePart[] castleParts;
	public GameObject skeleton;
	public float maxGold;
	public float goldDelay;
	public float goldAmount;
	public GameObject spawnEffect;
	public bool ownHalfOnly;
	public bool highlightSelectedButton;
	public Color buttonHighlight;
	
	public float bombLoadingSpeed;
	public float bombRange;
	public GameObject bombExplosion;
	
	public string clientCastle;
	public string masterClientCastle;
	public string clientUnit;
	public string masterClientUnit;
	
	public Color clientColor;
	public Color masterClientColor;
	public Color loseColor;
	public Color victoryColor;
	public KeyCode toggleSelectionMode;
	public KeyCode clearSelection;
	public GameObject spawnCircle;
    
	//not visible in the inspector
	public static GameObject LocalCastle;
	
	bool master;
	
	GameUI gameUI;
	BoxSelectUnits boxSelection;
	
	float masterCastleStrengthStart;
	float clientCastleStrengthStart;
	float masterCastleStrength;
	float clientCastleStrength;
	
	float lastMasterCastleStrength;
	float lastClientCastleStrength;
	float strengthbarSpeedMaster;
	float strengthbarSpeedClient;
	float lastSerializeTime;
	
	int selectedUnit;
	float gold;
	
	float battleDuration;
	int masterKills;
	int clientKills;

	float clickTime;
	
	float bombProgress;
	
	[HideInInspector]
	public bool battleEnded;
	
	[HideInInspector]
	public bool selectionMode;
	
	void Awake(){
		//initialize the camera, boxselection and tags
		boxSelection = GetComponent<BoxSelectUnits>();
		boxSelection.enabled = false;
		
		if(photonView.isMine){
            LocalCastle = gameObject;
		}
		else{
			GetComponentInChildren<CamController>().gameObject.SetActive(false);
		}
		
		master = PhotonNetwork.isMasterClient;
		
		foreach(Transform castlePart in transform){
			if(castlePart.GetComponent<Camera>())
				continue;
			
			if(master){
				castlePart.gameObject.tag = photonView.isMine ? masterClientCastle : clientCastle;
			}
			else{
				castlePart.gameObject.tag = photonView.isMine ? clientCastle : masterClientCastle;
			}
		}
		
		DontDestroyOnLoad(gameObject);
	}
	
	public void Initialize(){
		//once the castle reached the game scene, it will use this to initialize flag colors and game ui
		gameUI = GameObject.FindObjectOfType<GameUI>();
		
		gameUI.leftCastleStrengthBar.color = master ? masterClientColor : clientColor;
		gameUI.rightCastleStrengthBar.color = master ? clientColor : masterClientColor;
		
		if(master && photonView.isMine)
			CollectCastleStrengths(true);
		
		SetFlagColors();
		
		//if this is the local castle, add gold, add character buttons and set the buttons
		if(photonView.isMine){
			InvokeRepeating("AddGold", 1.0f, goldDelay);
			AddCharacterButtons();
			
			boxSelection.Initialize(gameUI, this);
			
			gameUI.selectionButton.gameObject.GetComponent<Button>().onClick.AddListener(() => { 
				ChangeSelectionMode(); 
			});
			
			gameUI.bombButton.GetComponent<Button>().onClick.AddListener(() => { 
				PlaceBomb(); 
			});
			
			gameUI.bombRange.GetComponent<Light>().spotAngle = bombRange;
			gameUI.boxSelectTarget.SetActive(false);
		}
	}
	
	void Update(){		
		if(gameUI == null || battleEnded)
			return;
		
		//keep track of the battle duration
		battleDuration += Time.deltaTime;
		
		//update the castle if this is the local castle
		if(photonView.isMine)
			CastleUpdate();
		
		//update the UI if this is the castle that belongs to the master client
		if((photonView.isMine && master) || (!photonView.isMine && !master))
			UIUpdate();
	}
	
	void CastleUpdate(){
		//check if we want to spawn units
		if(Input.GetMouseButtonDown(0) && !(GameObject.Find("Mobile multiplayer") && MobileMultiplayer.selectionModeMove)){
			TrySpawn();
		}
		else if((Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(0)) && boxSelection.enabled){
			//otherwise, check for the click position so we can move units around
			if(!gameUI.boxSelectTarget.activeSelf)
				gameUI.boxSelectTarget.SetActive(true);
			
			Vector3 clickPosition = MousePositionInWorld(true);
		
			if(clickPosition != Vector3.zero){
				gameUI.boxSelectTarget.transform.position = clickPosition;
				
				List<Unit> units = SelectedUnits();
				
				if(master){
					MoveSelected(clickPosition, units);
				}
				else{
					int[] viewIds = new int[units.Count];
					
					for(int i = 0; i < units.Count; i++){
						viewIds[i] = units[i].GetComponent<PhotonView>().viewID;
					}
					
					photonView.RPC("MoveSelectedRPC", PhotonTargets.MasterClient, clickPosition, viewIds);
				}
			}
		}
		
		//show a label/warning if the user is in selection mode but tries to spawn units
		CheckSelectionModeLabel();
		
		//check if we want to update the bomb range, otherwise update the bomb loading bar
		if(gameUI.bombRange.activeSelf){
			UpdateBombRange();
		}
		else{
			Color blue = new Color(0, 1, 1, 1);
			Image bombLoadingBar = gameUI.bombLoadingBar;
			
			if(bombProgress < 1f){
				bombProgress += Time.deltaTime * bombLoadingSpeed;
				
				if(bombLoadingBar.color != Color.red)
					bombLoadingBar.color = Color.red;
			}
			else if(bombLoadingBar.color != blue){
				bombProgress = 1f;
				bombLoadingBar.color = blue;
			}
		
			bombLoadingBar.fillAmount = bombProgress;
		}
		
		//update the goldbar and gold text
		gameUI.goldBar.fillAmount = gold/maxGold;
		gameUI.goldText.text = "" + gold;
		
		//check if the player wants to clear selected units
		if(Input.GetKeyDown(toggleSelectionMode)){
			ChangeSelectionMode();
		}
		else if(Input.GetKeyDown(clearSelection)){
			ClearSelection();
		}
	}
	
	void UIUpdate(){
		//update healthbars and texts if this is the castle that belongs to the master client (so the master client can sync with the client)
		if(master){
			CollectCastleStrengths(false);
		}
		else{
			masterCastleStrength -= Time.deltaTime * strengthbarSpeedMaster;
			clientCastleStrength -= Time.deltaTime * strengthbarSpeedClient;
		}
		
		gameUI.leftCastleStrengthText.text = master ? "" + (int)masterCastleStrength : "" + (int)clientCastleStrength;
		gameUI.rightCastleStrengthText.text = master ? "" + (int)clientCastleStrength : "" + (int)masterCastleStrength;
		
		float clientFillAmount = clientCastleStrength/clientCastleStrengthStart;
		float masterFillAmount = masterCastleStrength/masterCastleStrengthStart;
		
		gameUI.rightCastleStrengthBar.fillAmount = master ? clientFillAmount : masterFillAmount;
		gameUI.leftCastleStrengthBar.fillAmount = master ? masterFillAmount : clientFillAmount;
	}
	
	//change button colors for units that are not available
	void UpdateUnitButtons(){
		for(int i = 0; i < troops.Length; i++){
			if(troops[i].troopCosts <= gold){
				troops[i].button.GetComponent<Image>().color = Color.white;
			}
			else{
				Color grey = new Color(0.7f, 0.7f, 0.7f, 1);
				troops[i].button.GetComponent<Image>().color = grey;
			}
		}
	}
	
	//change the bomb range position
	void UpdateBombRange(){
		Vector3 hitPoint = MousePositionInWorld(false);
		
		if(hitPoint == Vector3.zero)
			return;
			
		gameUI.bombRange.transform.position = new Vector3(hitPoint.x, 75, hitPoint.z);
	}
	
	//update the selection mode label
	void CheckSelectionModeLabel(){
		if(boxSelection.enabled){
			if(Input.GetMouseButtonDown(0)){
				clickTime = 0;
			}
			else if(clickTime < 0.3f && Input.GetMouseButtonUp(0)){
				if(!gameUI.selectionModeLabel.activeSelf)
					StartCoroutine(SelectionModeLabel());
			}
		}
		
		clickTime += Time.deltaTime;
	}
	
	//show the selection mode label for a second
	IEnumerator SelectionModeLabel(){
		gameUI.selectionModeLabel.SetActive(true);
		
		yield return new WaitForSeconds(1);
		
		gameUI.selectionModeLabel.SetActive(false);
	}
	
	//find all selected units
	List<Unit> SelectedUnits(){
		List<Unit> units = new List<Unit>();
		Unit[] allUnits = GameObject.FindObjectsOfType<Unit>();
		
		for(int i = 0; i < allUnits.Length; i++){
			if(allUnits[i].selected)
				units.Add(allUnits[i]);
		}
		
		return units;
	}
	
	//move the selected units
	[PunRPC]
	void MoveSelectedRPC(Vector3 targetPosition, int[] ids){
		List<Unit> units = new List<Unit>();
		
		for(int i = 0; i < ids.Length; i++){
			units.Add(PhotonView.Find(ids[i]).gameObject.GetComponent<Unit>());
		}
		
		MoveSelected(targetPosition, units);
	}
	
	//set a click position on the selected units so they will move
	void MoveSelected(Vector3 targetPosition, List<Unit> units){
		for(int i = 0; i < units.Count; i++){
			units[i].MoveToClick(targetPosition);
		}
	}
	
	//add some gold so the player can spawn new units
	void AddGold(){
		if(gold >= maxGold)
			return;
		
		if(gold + goldAmount > maxGold){
			gold += maxGold - gold;
		}
		else{
			gold += goldAmount;
		}
		
		//check which buttons should be available
		UpdateUnitButtons();
	}
	
	//tries to spawn a unit
	void TrySpawn(){
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		Physics.Raycast(ray, out hit);
		
		//check if we clicked the battle ground and we can spawn
		if(hit.collider != null && hit.collider.gameObject.CompareTag("Battle ground") && CanSpawn(hit.point.x)){
			//check if we have enough gold
			if(gold < troops[selectedUnit].troopCosts){
				StartCoroutine(GoldWarning());
				return;
			}
			
			//spawn an effect and then spawn the unit
			GameObject circle = Instantiate(spawnCircle);
			circle.transform.position = hit.point;
		
			if(master){
				SpawnUnit(hit.point + Vector3.up * 0.25f, true, selectedUnit);
			}
			else{
				photonView.RPC("ClientSpawn", PhotonTargets.MasterClient, hit.point + Vector3.up * 0.25f, selectedUnit);
			}
			
			//also decrease the gold
			gold -= troops[selectedUnit].troopCosts;
			UpdateUnitButtons();
		}
		else if(gameUI.bombRange.activeSelf){
			//otherwise we might want to place the bomb
			if(!EventSystem.current.IsPointerOverGameObject()){
				if(hit.collider != null && CompareTag(hit.collider.gameObject.tag))
					SpawnBombMaster(hit.point);
			}
			else if(!GameObject.Find("Mobile multiplayer")){
				gameUI.bombRange.SetActive(false);
			}
		}
	}
	
	//check if we clicked the battle ground or one of the units
	bool CompareTag(string tag){
		if(tag == "Battle ground" || tag == clientUnit || tag == masterClientUnit)
			return true;
		
		return false;
	}
	
	//spawn a bomb or send a message to the master client so he can spawn a bomb
	void SpawnBombMaster(Vector3 position){
		bombProgress = 0;
		gameUI.bombRange.SetActive(false);
		
		if(master){
			SpawnBomb(position, clientUnit);
		}
		else{
			photonView.RPC("BombRPC", PhotonTargets.MasterClient, position);
		}
	}
	
	//spawn a bomb locally
	[PunRPC]
	void BombRPC(Vector3 position){
		SpawnBomb(position, masterClientUnit);
	}
	
	//actually spawn the bomb
	void SpawnBomb(Vector3 spawnPosition, string targetTag){
		PhotonNetwork.Instantiate(bombExplosion.name, spawnPosition, Quaternion.identity, 0);
		
		GameObject[] enemies = GameObject.FindGameObjectsWithTag(targetTag);
		
		for(int i = 0; i < enemies.Length; i++){
			if(Vector3.Distance(enemies[i].transform.position, spawnPosition) <= bombRange/2f)
				enemies[i].GetComponent<Unit>().Die();	
		}
	}
	
	//get the mouse position in 3d view using raycasting
	Vector3 MousePositionInWorld(bool groundOnly){
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		Physics.Raycast(ray, out hit);
		
		if(hit.collider == null || !CompareTag(hit.collider.gameObject.tag))
			return Vector3.zero;
		
		if(groundOnly && !hit.collider.gameObject.CompareTag("Battle ground"))
			return Vector3.zero;
		
		return hit.point;
	}
	
	//check everything to make sure it's possible to spawn a unit
	bool CanSpawn(float xPos){
		if(boxSelection.enabled || gameUI.bombRange.activeSelf || EventSystem.current.IsPointerOverGameObject())
			return false;
		
		if(GameObject.Find("Mobile multiplayer") && !MobileMultiplayer.deployMode)
			return false;
		
		if(!ownHalfOnly)
			return true;
		
		if((transform.position.x < 0 && xPos > -2) || (transform.position.x > 0 && xPos < 2)){
			int side = transform.position.x < 0 ? 1 : 0;
			StartCoroutine(EnemySide(side));
			return false;
		}
		
		return true;
	}
	
	//the client tells the master client to spawn
	[PunRPC]
	void ClientSpawn(Vector3 position, int selected){
		SpawnUnit(position, false, selected);
	}
	
	//on the master client side, a unit is spawned
	public void SpawnUnit(Vector3 position, bool masterClient, int selected){
		Quaternion spawnRotation = transform.rotation * Quaternion.Euler(0, -90, 0);
		string unit = selected == -1 ? skeleton.name : troops[selected].deployableTroops.name;
		GameObject newUnit = PhotonNetwork.Instantiate(unit, position, spawnRotation, 0);
		PhotonNetwork.Instantiate(spawnEffect.name, position, Quaternion.identity, 0);
		
		if(masterClient)
			newUnit.GetComponent<Unit>().local = true;
	}
	
	//show a warning if we try to spawn on the wrong side
	IEnumerator EnemySide(int side){
		GameObject warning = gameUI.wrongSideWarnings[side];
		warning.SetActive(true);
		
		yield return new WaitForSeconds(1);
		
		warning.SetActive(false);
	}
	
	//show a warning if we don't have enough gold
	IEnumerator GoldWarning(){
		if(!gameUI.goldWarning.activeSelf){
			gameUI.goldWarning.SetActive(true);
	
			yield return new WaitForSeconds(2);
			gameUI.goldWarning.SetActive(false);
		}
	}
	
	//get the flag colors for all flags in the scene using their position
	void SetFlagColors(){
		Color color = Color.white;
		
		if(photonView.isMine){
			color = master ? Color.red : Color.blue;
		}
		else{
			color = master ? Color.blue : Color.red;
		}
		
		GameObject[] flags = GameObject.FindGameObjectsWithTag("Flag");
		
		for(int i = 0; i < flags.Length; i++){
			float posNormalized = transform.position.x/Mathf.Abs(transform.position.x);
			float flagPosNormalized = flags[i].transform.position.x/Mathf.Abs(flags[i].transform.position.x);
			
			if(posNormalized == flagPosNormalized)
				flags[i].GetComponent<Renderer>().material.color = color;
		}
	}
	
	//add all character buttons so we can select them
	void AddCharacterButtons(){
		for(int i = 0; i < troops.Length; i++){
			Button newButton = Instantiate(gameUI.button);
			RectTransform rectTransform = newButton.GetComponent<RectTransform>();
			rectTransform.SetParent(gameUI.characterList.transform, false);
			
			newButton.gameObject.GetComponent<Outline>().effectColor = buttonHighlight;
			newButton.gameObject.GetComponent<Image>().sprite = troops[i].buttonImage;
			newButton.GetComponent<Outline>().enabled = i == 0 && highlightSelectedButton;
			newButton.transform.name = "" + i;
		
			newButton.GetComponent<Button>().onClick.AddListener(() => { 
				SelectUnit(int.Parse(newButton.transform.name)); 
			});
			
			newButton.GetComponentInChildren<Text>().text = "Price: " + troops[i].troopCosts + 
			"\n Damage: " + troops[i].deployableTroops.GetComponentInChildren<Unit>().damage + 
			"\n Health: " + troops[i].deployableTroops.GetComponentInChildren<Unit>().health;
			
			troops[i].button = newButton.gameObject;
		}
	}
	
	//remove a unit
	public void DestroyUnit(GameObject unit, string type, GameObject dieEffect){
		PhotonView view = unit.GetComponent<PhotonView>();
		
		if(view == null || !view.isMine)
			return;
		
		PhotonNetwork.Instantiate(dieEffect.name, unit.transform.position + Vector3.up * 2, unit.transform.rotation, 0);
		
		UnitKilled(unit.tag, type);
		PhotonNetwork.Destroy(view);
	}
	
	//notify the player when he kills a unit
	void UnitKilled(string tag, string type){
		if(tag == clientUnit){
			CastleMultiplayer localCastle = CastleMultiplayer.LocalCastle.GetComponent<CastleMultiplayer>();
			localCastle.masterKills++;
			
			KillNotification(type);
		}
		else{
			clientKills++;
			photonView.RPC("ClientKillNotification", PhotonTargets.Others, type);
		}
	}
	
	//notify the player when he kills a unit
	[PunRPC]
	void ClientKillNotification(string type){
		KillNotification(type);
	}
	
	//locally show the kill notification
	void KillNotification(string unitType){
		GameObject newLabel = Instantiate(gameUI.killLabel);
		RectTransform rectTransform = newLabel.GetComponent<RectTransform>();
		rectTransform.SetParent(gameUI.killList.transform, false);
		newLabel.GetComponentInChildren<Text>().text = "ENEMY " + unitType.ToUpper() + " KILLED";
	}
	
	//find the matching castle part and attack it
	public void AttackCastle(GameObject castlePart, float damage){
		for(int i = 0; i < castleParts.Length; i++){
			if(castleParts[i].castlePart == castlePart){
				castleParts[i].strength -= Time.deltaTime * damage;
				
				if(castleParts[i].strength < 0)
					castleParts[i].strength = 0;
				
				CheckCastle(i);
			}
		}
	}
	
	//select a new unit
	public void SelectUnit(int unit){
		if(highlightSelectedButton){
			for(int i = 0; i < troops.Length; i++){
				troops[i].button.GetComponent<Outline>().enabled = false;	
			}
		
			troops[unit].button.GetComponent<Outline>().enabled = true;
		}
	
		selectedUnit = unit;
	}
	
	//place a bomb
	public void PlaceBomb(){
		if(bombProgress < 1f)
			return;
		
		gameUI.bombRange.SetActive(true);
	}
	
	//change the selection mode (if we can box select)
	public void ChangeSelectionMode(){
		boxSelection.enabled = !boxSelection.enabled;
		selectionMode = boxSelection.enabled;
		
		gameUI.selectionButton.color = selectionMode ? Color.red : Color.white;
		
		if(!selectionMode){
			ClearSelection();
			gameUI.boxSelectTarget.SetActive(false);
		}
		
		if(GameObject.Find("Mobile multiplayer")){
			if(selectionMode && MobileMultiplayer.deployMode)
				GameObject.Find("Mobile multiplayer").GetComponent<MobileMultiplayer>().ToggleDeployMode();
				
			MobileMultiplayer.camEnabled = !selectionMode;
		}
	}
	
	//deselect all units
	void ClearSelection(){
		Unit[] units = GameObject.FindObjectsOfType<Unit>();
		
		for(int i = 0; i < units.Length; i++){
			units[i].selected = false;
		}
	}
	
	//check if there is a castle part that needs to be destroyed
	void CheckCastle(int castlePartIndex){
		if(castleParts[castlePartIndex].strength <= 0)
			photonView.RPC("DestroyCastlePart", PhotonTargets.All, castlePartIndex);
	}
	
	//destroy a castle part
	[PunRPC]
	void DestroyCastlePart(int castlePartIndex){
		if(castleParts[castlePartIndex].destroyed)
			return;
		
		GameObject castlePart = castleParts[castlePartIndex].castlePart;
		
		Instantiate(castleParts[castlePartIndex].fractured, castlePart.transform.position, castlePart.transform.rotation);
		
		castleParts[castlePartIndex].destroyed = true;
		Destroy(castlePart);
	}
	
	//collect the total of all castle strengths
	void CollectCastleStrengths(bool initialization){
		CastleMultiplayer[] castles = GameObject.FindObjectsOfType<CastleMultiplayer>();
			
		for(int i = 0; i < castles.Length; i++){
			float totalStrength = castles[i].TotalStrength();
			
			if(castles[i] == this){
				if(initialization)
					masterCastleStrengthStart = totalStrength;
				
				masterCastleStrength = totalStrength;
			}
			else{
				if(initialization)
					clientCastleStrengthStart = totalStrength;
				
				clientCastleStrength = totalStrength;
			}
			
			if(totalStrength <= 0)
				EndBattle(castles[i] != this);
		}
	}
	
	//end a battle
	void EndBattle(bool masterWon){
		EndBattleLocally(battleDuration, masterWon, masterKills, clientKills);
		photonView.RPC("EndBattleClient", PhotonTargets.Others, battleDuration, masterWon, masterKills, clientKills);
		
		photonView.RPC("Disconnect", PhotonTargets.All);
	}
	
	//let the client know he needs to end the battle too
	[PunRPC]
	void EndBattleClient(float battleDuration, bool masterWon, int masterKills, int clientKills){
		EndBattleLocally(battleDuration, masterWon, masterKills, clientKills);
	}
	
	//locally end the battle (both the client and the master client)
	void EndBattleLocally(float duration, bool masterWon, int masterKills, int clientKills){
		bool wonBattle = (masterWon && master) || (!masterWon && !master);
		
		gameUI.battleResultPanel.GetComponent<Image>().color = wonBattle ? victoryColor : loseColor;
		gameUI.battleResultTitle.GetComponent<Text>().text = wonBattle ? "VICTORY" : "DEFEAT";
			
		gameUI.battleResultKilledEnemies.text = "" + (master ? masterKills : clientKills);
		gameUI.battleResultKilledAllies.text = "" + (master ? clientKills : masterKills);
		
		string time = (((int)(duration/60) < 10) ? "0" : "") + "" + (int)(duration/60) + ((duration % 60 < 10) ? ":0" : ":") + (int)(duration % 60);
		gameUI.battleResultDuration.text = time;
		
		gameUI.gamePanel.SetActive(false);
		gameUI.battleResultPanel.SetActive(true);
		
		CastleMultiplayer[] castles = GameObject.FindObjectsOfType<CastleMultiplayer>();
		for(int i = 0; i < castles.Length; i++){
			castles[i].battleEnded = true;
		}
	}
	
	//leave the photon room
	[PunRPC]
	void Disconnect(){
		PhotonNetwork.LeaveRoom();
	}
	
	//get the total castle strength
	public float TotalStrength(){
		float total = 0;
		
		for(int i = 0; i < castleParts.Length; i++){
			total += castleParts[i].strength;
		}
		
		return total;
	}
	
	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
		if(stream.isWriting){
			//if this is the master client and the local master client castle, write strength data to the client
			if(photonView.isMine && master){
				stream.SendNext(this.masterCastleStrength);
				stream.SendNext(this.clientCastleStrength);
			}
		}
        else{
			//if this is the master client castle on the client side, receive strength data
			if(!photonView.isMine && !master){
				float masterStrength = (float)stream.ReceiveNext();
				float clientStrength = (float)stream.ReceiveNext();
				
				//get the starting strength values for the healthbar
				if(this.masterCastleStrengthStart == 0){
					this.masterCastleStrengthStart = masterStrength;
					this.clientCastleStrengthStart = clientStrength;
					
					this.masterCastleStrength = masterStrength;
					this.clientCastleStrength = clientStrength;
				}
				else{
					//smoothly update the healthbar by estimating health
					EstimateCastleStrength(masterStrength, clientStrength);
				}
				
				//update some values so we can estimate the strength correctly
				lastSerializeTime = Time.time;
				lastClientCastleStrength = clientStrength;
				lastMasterCastleStrength = masterStrength;
			}
        }
	}
	
	//estimates health bar strengths
	void EstimateCastleStrength(float strengthMasterCastle, float strengthClientCastle){
		if(lastSerializeTime == 0)
			return;
		
		strengthbarSpeedMaster = Speed(strengthMasterCastle, lastMasterCastleStrength);
		strengthbarSpeedClient = Speed(strengthClientCastle, lastClientCastleStrength);
	}
	
	//gets the health bar speed
	float Speed(float castleStrength, float lastCastleStrength){
		float distance = lastCastleStrength - castleStrength;
		float time = Time.time - lastSerializeTime;
		
		return distance/time;
	}
}

#else

public class CastleMultiplayer : MonoBehaviour {
	//empty monobehaviour class for when photon has not been imported
	
	public Troop[] troops;
	public CastlePart[] castleParts;
	public GameObject skeleton;
	public float maxGold;
	public float goldDelay;
	public float goldAmount;
	public GameObject spawnEffect;
	public bool ownHalfOnly;
	public bool highlightSelectedButton;
	public Color buttonHighlight;
	
	public float bombLoadingSpeed;
	public float bombRange;
	public GameObject bombExplosion;
	
	public string clientCastle;
	public string masterClientCastle;
	public string clientUnit;
	public string masterClientUnit;
	
	public Color clientColor;
	public Color masterClientColor;
	public Color loseColor;
	public Color victoryColor;
	public KeyCode toggleSelectionMode;
	public KeyCode clearSelection;
	public GameObject spawnCircle;
}

#endif

[System.Serializable]
public struct CastlePart {
	public GameObject castlePart;
	public GameObject fractured;
	public float strength;
	
	[HideInInspector]
	public bool destroyed;
}