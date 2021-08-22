using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if PHOTON_MULTIPLAYER
using ExitGames.Client.Photon;

public class MainMenu : Photon.PunBehaviour {
    
	//visible in the inspector
	public Text nameLabel;
	public GameObject nameScreen;
	public InputField nameInput;
	public GameObject mainUI;
	public string inbetweenScene;
	
	bool isConnecting;
	string gameVersion = "1";
	
	void Awake(){
		//sync scenes to correctly match them on both sides
		PhotonNetwork.automaticallySyncScene = true;
	}
	
	void Start(){
		//check if we have a username, and otherwise ask for one
		string name = PlayerPrefs.GetString("Username");
		
		if(name != ""){
			nameLabel.text = name;
			nameScreen.SetActive(false);
			
			PhotonNetwork.player.NickName = name;
		}
		else{
			nameScreen.SetActive(true);
		}
	}
	
	void Update(){
		//delete playerprefs using p (to delete usernames)
		if(Input.GetKeyDown("p"))
			PlayerPrefs.DeleteAll();
	}
	
	//save the new username/nickname
	public void SubmitName(){
		string name = nameInput.text;
		
		if(name == "")
			return;
		
		PhotonNetwork.player.NickName = name;
		
		PlayerPrefs.SetString("Username", name);
		nameLabel.text = name;
		
		nameScreen.SetActive(false);
	}
	
	//battle (try to join a room or create one)
	public void Battle(){
		isConnecting = true;

		mainUI.SetActive(false);

		// we check if we are connected or not, we join if we are , else we initiate the connection to the server.
		if(PhotonNetwork.connected){
			//#Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
			PhotonNetwork.JoinRandomRoom();
		}
		else{
			//#Critical, we must first and foremost connect to Photon Online Server.
			PhotonNetwork.gameVersion = this.gameVersion;
			PhotonNetwork.ConnectUsingSettings(gameVersion);
		}
	}
	
	//join a random room
	public override void OnConnectedToMaster(){
        if(isConnecting){
			//#Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnJoinRandomFailed()
			PhotonNetwork.JoinRandomRoom();
		}
	}
	
	//create a new room if there are none available
	public override void OnPhotonRandomJoinFailed(object[] codeAndMsg){
		// #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
		PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = 2}, null);
	}
	
	//show the main UI again if we somehow disconnected
	public override void OnDisconnectedFromPhoton(){
		isConnecting = false;
		mainUI.SetActive(true);
	}
	
	//after joining a room load the inbetween scene
	public override void OnJoinedRoom(){
		// #Critical: We only load if we are the first player, else we rely on  PhotonNetwork.AutomaticallySyncScene to sync our instance scene.
		if(PhotonNetwork.room.PlayerCount == 1){
			// Load the Room Level. 
			PhotonNetwork.LoadLevel(inbetweenScene);
		}
	}
}

#else

public class MainMenu : MonoBehaviour {
	//empty monobehaviour class for when photon has not been imported
	
	public Text nameLabel;
	public GameObject nameScreen;
	public InputField nameInput;
	public GameObject mainUI;
	public string inbetweenScene;
}

#endif
