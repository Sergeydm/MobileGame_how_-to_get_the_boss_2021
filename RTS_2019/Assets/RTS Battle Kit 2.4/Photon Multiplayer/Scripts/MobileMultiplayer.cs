using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MobileMultiplayer : MonoBehaviour {
	
	//not visible in the inspector
	private bool moveRight;
	private bool moveLeft;
	private bool moveUp;
	private bool moveDown;
	
	private GameObject buttons;
	private GameObject switchSelectionModeButton;
	
	public static bool camEnabled = true;
	public static bool deployMode;
	public static bool selectionModeMove;
	
	//visible in the inspector
	public Sprite moveUnits;
	public Sprite selectUnits;
	
	#if PHOTON_MULTIPLAYER
	
	CastleMultiplayer localCastle;
	InitializeGame gameInitializer;
	GameUI gameUI;
	
	void Start(){
		//find gameobjects
		switchSelectionModeButton = GameObject.Find("Switch selection mode button");
		buttons = GameObject.Find("Buttons");
		
		GameObject local = CastleMultiplayer.LocalCastle;
		
		if(local != null)
			localCastle = local.GetComponent<CastleMultiplayer>();
		
		gameInitializer = GameObject.FindObjectOfType<InitializeGame>();
		gameUI = GameObject.FindObjectOfType<GameUI>();
	}
	
	void Update(){		
		//check if you can move the camera
		if(camEnabled){	
			//use the move booleans to know when to move the camera
			if(moveLeft)
				Camera.main.transform.Translate(Vector3.right * Time.deltaTime * -CamController.movespeed);
			
			if(moveRight)
				Camera.main.transform.Translate(Vector3.right * Time.deltaTime * CamController.movespeed);
			
			if(moveUp)
				Camera.main.transform.Translate(Vector3.up * Time.deltaTime * CamController.movespeed);
			
			if(moveDown)
				Camera.main.transform.Translate(Vector3.up * Time.deltaTime * -CamController.movespeed);
		}
		
		//turn the buttons off when any of the menu's is active
		buttons.SetActive(!Settings.settingsMenu.activeSelf && !gameInitializer.startUI.activeSelf && gameUI.gamePanel.activeSelf);
		
		//Set mobile selection mode button active/not active
		if(localCastle != null)
			switchSelectionModeButton.SetActive(localCastle.selectionMode);
	}
	
	//start moving the camera based on the direction
	public void MoveCameraButtonDown(string direction){
		if(direction == "right")
			moveRight = true;
		
		if(direction == "left")
			moveLeft = true;
		
		if(direction == "up")
			moveUp = true;
		
		if(direction == "down")
			moveDown = true;
	}
	
	//stop moving the camera based on direction
	public void MoveCameraButtonUp(string direction){
		if(direction == "right")
			moveRight = false;
	
		if(direction == "left")
			moveLeft = false;
		
		if(direction == "up")
			moveUp = false;
		
		if(direction == "down")
			moveDown = false;
	}
	
	//toggle deploymode on/off
	public void ToggleDeployMode(){
		deployMode = !deployMode;
		if(deployMode){
			//set button color to red
			GameObject.Find("Deploy units button").GetComponent<Image>().color = Color.red;
			
			if(localCastle.selectionMode){
				//switch selection mode off
				localCastle.ChangeSelectionMode();
			}
		}
		else{
			//set button color to white
			GameObject.Find("Deploy units button").GetComponent<Image>().color = Color.white;
		}
		
		camEnabled = !deployMode;
	}
	
	//switch between selection modes (box select & move units mode)
	public void SwitchSelectionMode(){
		selectionModeMove = !selectionModeMove;
		
		//change the sprite displayed
		if(selectionModeMove){
			switchSelectionModeButton.GetComponent<Image>().sprite = moveUnits;
		}
		else{
			switchSelectionModeButton.GetComponent<Image>().sprite = selectUnits;
		}
	}
	
	#endif
}