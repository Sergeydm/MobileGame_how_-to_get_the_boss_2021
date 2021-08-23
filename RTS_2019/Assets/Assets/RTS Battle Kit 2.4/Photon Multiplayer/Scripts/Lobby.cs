using UnityEngine;
using UnityEngine.SceneManagement;

#if PHOTON_MULTIPLAYER
using ExitGames.Client.Photon; 

public class Lobby : Photon.PunBehaviour {
	
	//visible in the inspector
	public string menuScene;
	public string gameScene;
	
	[SerializeField]
	private GameObject castlePrefab;
	
	//not in the inspector
	bool master;
   
    void Start(){
		//go back to the menu if we're not connected
	    if(!PhotonNetwork.connected){
			SceneManager.LoadScene(menuScene);
			return;
		}
		
		master = PhotonNetwork.isMasterClient;
		
		//load the castles and position them correctly (they will be moved to the game scene once both players have connected)
		if(castlePrefab != null && CastleMultiplayer.LocalCastle == null){
			Vector3 originalPosition = castlePrefab.transform.position;
			Vector3 newPosition = master ? originalPosition : new Vector3(-originalPosition.x, originalPosition.y, originalPosition.z);
			float angle = master ? 180 : 0;
			Quaternion rotation = Quaternion.Euler(new Vector3(0, angle, 0));
			PhotonNetwork.Instantiate(this.castlePrefab.name, newPosition, rotation, 0);
		}
    }
   
    //if the second player connects, load the game scene
    public override void OnPhotonPlayerConnected(PhotonPlayer other){
		Debug.Log("Second player entered (" + other.NickName + ")");

		if(master)
			PhotonNetwork.LoadLevel(gameScene);
	}
	
	//go back to the menu after leaving
	public override void OnLeftRoom(){
		SceneManager.LoadScene(menuScene);
	}
	
	//leave the photon room
	public void LeaveRoom(){
		PhotonNetwork.LeaveRoom();
	}
}

#else

public class Lobby : MonoBehaviour {
	//empty monobehaviour class for when photon has not been imported
	
	public string menuScene;
	public string gameScene;
	
	[SerializeField]
	private GameObject castlePrefab;
}

#endif