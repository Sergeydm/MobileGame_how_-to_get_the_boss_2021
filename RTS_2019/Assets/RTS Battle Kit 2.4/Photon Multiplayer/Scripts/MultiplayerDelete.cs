using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplayerDelete : MonoBehaviour {
	
	//lifetime
    public float lifetime;
	
	#if PHOTON_MULTIPLAYER
	
	IEnumerator Start(){
		//wait for the lifetime
		yield return new WaitForSeconds(lifetime);
		
		//then destroy this object on both sides if we're the master client
		if(PhotonNetwork.isMasterClient)
			PhotonNetwork.Destroy(gameObject);
	}
	
	#endif
}
