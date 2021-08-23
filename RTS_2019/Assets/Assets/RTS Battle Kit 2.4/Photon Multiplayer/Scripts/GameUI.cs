using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour {
	
	//UI elements already in the scene (so we can access them from the castles and units)
    public Text leftCastleStrengthText;
	public Text rightCastleStrengthText;
	public Image leftCastleStrengthBar;
	public Image rightCastleStrengthBar;
	
	public GameObject battleResultPanel;
	public GameObject battleResultTitle;
	public Text battleResultDuration;
	public Text battleResultKilledEnemies;
	public Text battleResultKilledAllies;
	
	public GameObject gamePanel;
	public GameObject selectionModeLabel;
	
	public GameObject goldWarning;
	public GameObject[] wrongSideWarnings;
	public GameObject characterList;
	public GameObject killList;
	public GameObject killLabel;
	public Button button;
	
	public Image goldBar;
	public Text goldText;
	
	public Image selectionBox;
	public Canvas selectionCanvas;
	public Image selectionButton;
	public GameObject boxSelectTarget;
	
	public Image bombLoadingBar;
	public Button bombButton;
	public GameObject bombRange;
	
	#if PHOTON_MULTIPLAYER
	
	void Awake(){
		//deactivate some UI elements
		goldWarning.SetActive(false);
		battleResultPanel.SetActive(false);
		selectionModeLabel.SetActive(false);
		bombRange.SetActive(false);
		
		for(int i = 0; i < wrongSideWarnings.Length; i++){
			wrongSideWarnings[i].SetActive(false);
		}
	}
	
	#endif
}