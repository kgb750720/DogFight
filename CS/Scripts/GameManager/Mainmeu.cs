using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class Mainmeu : MonoBehaviour {

	public GUISkin skin;
	public Texture2D Logo;

	void Start () {
	
	}
	
	void Update () {
	
	}
	
	public void OnGUI(){
		if(skin)
		GUI.skin = skin;
		
		GUI.DrawTexture(new Rect(Screen.width/2 - Logo.width /2 , Screen.height  / 2 - Logo.height * 1.2f, Logo.width   ,Logo.height ),Logo);
		
		if(GUI.Button(new Rect(Screen.width/2 - 150,Screen.height/2 ,300,40), "World War II")){
            SceneManager.LoadScene("WW2Menu");
		}
		if(GUI.Button(new Rect(Screen.width/2 - 150,Screen.height/2 + 50,300,40), "Modern War")){
            SceneManager.LoadScene("F16Menu");
		}

        if (GUI.Button(new Rect(Screen.width / 2 - 150, Screen.height / 2 + 100, 300, 40), "Exit"))
        {
            Application.Quit();
        }

        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
		GUI.Label(new Rect(0, Screen.height / 2 + 180, Screen.width,20),"Air Fighter produced by Jingcheng Yuan & Junjie Ni");
	}
}
