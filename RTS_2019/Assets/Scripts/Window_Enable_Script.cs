using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Window_Enable_Script : MonoBehaviour
{
    public GameObject windows;
    void Start()
    {
      
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnWindowEnable()
    {
        windows.SetActive(!windows.activeInHierarchy);
        print("Сработала кнопка перекючения видимости окон");
    }
}
