using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class F16Menu : MonoBehaviour {

	public GUISkin skin;
	public Texture2D Logo;

	void Start () {
	
	}
	
	void Update () {
	
	}
	
	public void OnGUI(){
		if(skin)
		GUI.skin = skin;
		
		GUI.DrawTexture(new Rect(Screen.width * 4 / 5 - Logo.width / 2, Screen.height /2 - Logo.height / 2, Logo.width, Logo.height), Logo);
		
		if(GUI.Button(new Rect(Screen.width / 5 - 100, Screen.height / 2 - 75, 200,30), "Free Flight")){
            SceneManager.LoadScene("FreeFlightF16");
		}
		if(GUI.Button(new Rect(Screen.width / 5 - 100, Screen.height / 2 - 25, 200, 30), "1V1")){
            SceneManager.LoadScene("Modern");
		}
		if(GUI.Button(new Rect(Screen.width / 5 - 100, Screen.height / 2 + 25, 200, 30), "5V5")){
            SceneManager.LoadScene("ModernMultiPlayer");
		}

        if (GUI.Button(new Rect(Screen.width / 5 - 100, Screen.height / 2 + 75, 200, 30), "Main Menu"))
        {
            SceneManager.LoadScene("MainMenu");
        }
        //GUI.skin.label.alignment = TextAnchor.MiddleCenter;
        //GUI.Label(new Rect(0,Screen.height-90,Screen.width,50),"Air Fighter by Jingcheng Yuan & Junjie Ni");
    }
}
