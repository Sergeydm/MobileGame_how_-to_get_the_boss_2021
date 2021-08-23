using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoxSelectUnits : MonoBehaviour {
	
	#if PHOTON_MULTIPLAYER
	//not visible in the inspector
    Image box;
	Canvas canvas;
	
	Vector3 startPos;
    RectTransform boxTransform;
    bool isSelecting;
	
	string targetTag;
	
	//initialize as soon as the castle has reached the game scene
    public void Initialize(GameUI gameUI, CastleMultiplayer castle){
		box = gameUI.selectionBox;
		canvas = gameUI.selectionCanvas;
		
        boxTransform = box.GetComponent<RectTransform>();
        boxTransform.pivot = Vector2.one * 0.5f;
        boxTransform.anchorMin = Vector2.one * 0.5f;
        boxTransform.anchorMax = Vector2.one * 0.5f;
		
		box.gameObject.SetActive(false);
		
		bool masterCastle = castle.castleParts[0].castlePart.CompareTag(castle.masterClientCastle);
		targetTag = masterCastle ? castle.masterClientUnit : castle.clientUnit;
    }
 
    void Update(){
		//save the starting position for the box
        if(Input.GetMouseButtonDown(0) && !Moving()){
			startPos = Input.mousePosition;
			
            isSelecting = true;
			ChangeBoxState();
        }
        else if(Input.GetMouseButtonUp(0)){
			//stop the box select
            isSelecting = false;
			ChangeBoxState();
        }
 
        if(isSelecting){
			//update the box and selected units
			Vector3 center = Vector3.Lerp(startPos, Input.mousePosition, 0.5f);
			
            Bounds bounds = new Bounds();
            bounds.center = center;
            
			float xRectSize = Mathf.Abs(startPos.x - Input.mousePosition.x);
			float yRectSize = Mathf.Abs(startPos.y - Input.mousePosition.y);
			
			bounds.size = new Vector3(xRectSize, yRectSize, 0);
 
            boxTransform.position = center;
            boxTransform.sizeDelta = canvas.transform.InverseTransformVector(bounds.size);
			
			Unit[] units = GameObject.FindObjectsOfType<Unit>();
			
            foreach(Unit unit in units){
                Vector3 screenPos = Camera.main.WorldToScreenPoint(unit.gameObject.transform.position);
                screenPos.z = 0;
				
				if(bounds.Contains(screenPos) && unit.gameObject.CompareTag(targetTag))
					unit.selected = true;
            }
        }
    }
	
	//activate or deactivate the box
	void ChangeBoxState(){
		box.gameObject.SetActive(isSelecting);
	}
	
	//check if we're moving units in a mobile game
	bool Moving(){
		return GameObject.Find("Mobile multiplayer") && MobileMultiplayer.selectionModeMove;
	}
	
	#endif
}