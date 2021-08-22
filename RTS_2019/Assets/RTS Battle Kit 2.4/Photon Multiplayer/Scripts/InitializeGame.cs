using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if PHOTON_MULTIPLAYER
using ExitGames.Client.Photon;

public class InitializeGame : Photon.PunBehaviour {
	
	//visible in the inspector
	public string menuScene;
	
	public GameObject startUI;
	
	public Text[] localPlayerName;
	public Text[] enemyPlayerName;
	
	void Start(){
		//go back to the menu if we're not connected to the network
		if(!PhotonNetwork.connected){
			SceneManager.LoadScene(menuScene);

			return;
		}
		
		//change send rates (affects performance and game smoothness)
		PhotonNetwork.sendRateOnSerialize = 3;
		PhotonNetwork.sendRate = 30;
		
		//show the start ui
		StartCoroutine(StartUI());
	}
	
	//wait for a short time and start the game on both sides
	IEnumerator StartUI(){
		yield return new WaitForSeconds(0.7f);
		
		if(PhotonNetwork.isMasterClient)
			photonView.RPC("StartGame", PhotonTargets.All);
	}
	
	//initialize castles and show starting UI screen
	[PunRPC]
	void StartGame(){
		CastleMultiplayer[] castles = GameObject.FindObjectsOfType<CastleMultiplayer>();
		
		for(int i = 0; i < castles.Length; i++){
			castles[i].Initialize();
		}
		
		StartCoroutine(ShowStartUI());
	}
	
	//leave the room if the other player disconnects
	public override void OnPhotonPlayerDisconnected(PhotonPlayer other){
		Debug.Log("OnPlayerLeftRoom() " + other.NickName); // seen when other disconnects
		PhotonNetwork.LeaveRoom();
	}
	
	//go back to the main menu after leaving the room
	public override void OnLeftRoom(){
		if(!BattleEnded())
			LoadMenu();
	}
	
	//open the main menu
	public void LoadMenu(){
		SceneManager.LoadScene(menuScene);
	}
	
	//check if the battle has ended
	bool BattleEnded(){
		GameObject localCastle = CastleMultiplayer.LocalCastle;
		CastleMultiplayer castle = localCastle.GetComponent<CastleMultiplayer>();
		
		return castle.battleEnded;
	}
	
	//show the starting UI panels (and nicknames)
	IEnumerator ShowStartUI(){		
		for(int i = 0; i < localPlayerName.Length; i++){
			localPlayerName[i].text = PhotonNetwork.player.NickName;
		
			string enemyName = PhotonNetwork.player.GetNext().NickName;
			enemyPlayerName[i].text = enemyName;
		}
		
		startUI.GetComponent<Animator>().SetTrigger("start");
		
		yield return new WaitForSeconds(3);
		
		//deactivate after the animation has finished
		startUI.SetActive(false);
	}
}

#else

public class InitializeGame : MonoBehaviour {
	//empty monobehaviour class for when photon has not been imported
	
	public string menuScene;
	
	public GameObject startUI;
	
	public Text[] localPlayerName;
	public Text[] enemyPlayerName;
}

#endif