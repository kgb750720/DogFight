using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Flight;

public class GameUI : MonoBehaviour
{

	public GUISkin skin;
	public Texture2D Logo;
	public int Mode;
	private GameManager game;
	private PlayerController play;
	private WeaponController weapon;
	private FlightView view;
	private ItemUse item;
	
	void Start ()
    {
        NewFlightUIInit();
    }

    public void NewFlightUIInit()
    {
        game = (GameManager)GameObject.FindObjectOfType<GameManager>();
        play = (PlayerController)GameObject.FindObjectOfType<PlayerController>();
        ResetWeapon();
        // define player
        view = GameObject.FindObjectOfType<FlightView>();
        if (view && view.Target)
            item = view.Target.GetComponent<ItemUse>();
    }

    private void ResetWeapon()
    {
        if (play)
            weapon = play.GetComponent<WeaponController>();
    }

    private void Update()
    {
        if(view&&view.Target!=null&&!weapon)
        {
			NewFlightUIInit();
        }
    }

    public void OnGUI ()
	{
		if (play)
		{
			//隐藏光标
			Cursor.visible = false;
			if (skin)
				GUI.skin = skin;
			//自定义字体
			GUIStyle fontStyle1 = new GUIStyle();
			fontStyle1.normal.background = null;    //设置背景填充  
			fontStyle1.normal.textColor = new Color(0, 1, 0);   //设置字体颜色  
			fontStyle1.fontSize = 16;       //字体大小  

			//自定义字体
			GUIStyle WarningStyle = new GUIStyle();
			WarningStyle.normal.background = null;    //设置背景填充  
			WarningStyle.normal.textColor = new Color(1, 0, 0);   //设置字体颜色  
			WarningStyle.fontSize = 25;       //字体大小 
			WarningStyle.alignment = TextAnchor.MiddleCenter;


			switch (Mode)
			{
				case 0:
					//if (Input.GetKeyDown (KeyCode.Escape)) {
					//	Mode = 2;	
					//}

					if (play&&play.flight)
					{

						play.Active = true;

						if (play.flight.Stall) //失速警告
						{
							GUI.Label(new Rect(Screen.width / 2, Screen.height / 2 - 100, 200, 50), "失速!", WarningStyle);
						}



						GUI.skin.label.alignment = TextAnchor.UpperLeft;
						GUI.skin.label.fontSize = 16;
						GUI.Label(new Rect(20, 20, 200, 50), "Kills " + game.Killed.ToString(), fontStyle1);
						GUI.Label(new Rect(20, 40, 200, 50), "Score " + game.Score.ToString(), fontStyle1);

						GUI.skin.label.alignment = TextAnchor.UpperRight;
						GUI.Label(new Rect(Screen.width - 110, 20, 200, 50), "结构完整度 " + play.GetComponent<DamageManager>().HP, fontStyle1);
						GUI.Label(new Rect(Screen.width - 110, 40, 200, 50), "油门 " + ((int)(play.flight.AfterBurner * play.flight.throttle * 100)).ToString() + "%", fontStyle1);
						GUI.Label(new Rect(Screen.width - 110, 60, 200, 50), "速度 " + ((int)play.flight.Speed).ToString(), fontStyle1);

						if (item != null)
						{
							ItemInterfere interfere = item as ItemInterfere;
							if (interfere)
								GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height - 50, 200, 50), "剩余" + item.ItemName + " " + interfere.CurrInterfereNub,fontStyle1);
						}


						GUI.skin.label.fontSize = 16;
						// Draw Weapon system
						if (weapon != null && weapon.WeaponList.Count > 0)
						{
							if (weapon.WeaponList[weapon.CurrentWeaponIdx].Icon)
								GUI.DrawTexture(new Rect(Screen.width - 100, Screen.height - 100, 80, 80), weapon.WeaponList[weapon.CurrentWeaponIdx].Icon);

							GUI.skin.label.alignment = TextAnchor.UpperRight;
							GUI.Label(new Rect(Screen.width - 230, Screen.height - 180, 200, 30), "锁定模式：" + (weapon.CurrLauncher.MultiLockModel ? "多重锁定" : "普通锁定"));
							GUI.Label(new Rect(Screen.width - 230, Screen.height - 150, 200, 30), "开火发射数量："+weapon.CurrLauncher.FireOnceOutBulletNub);
							//if (weapon.WeaponList [weapon.CurrentWeapon].Ammo <= 0 && weapon.WeaponList [weapon.CurrentWeapon].CoolingProcess > 0) {
							if (weapon.WeaponList[weapon.CurrentWeaponIdx].Overheating && weapon.WeaponList[weapon.CurrentWeaponIdx].CoolingProcess > 0)
							{
								if (!weapon.WeaponList[weapon.CurrentWeaponIdx].InfinityAmmo)
									GUI.Label(new Rect(Screen.width - 230, Screen.height - 120, 200, 30), "冷却 " + Mathf.Floor((1 - weapon.WeaponList[weapon.CurrentWeaponIdx].CoolingProcess) * 100) + "%");
							}
							else
							{
								if (!weapon.WeaponList[weapon.CurrentWeaponIdx].InfinityAmmo)
									GUI.Label(new Rect(Screen.width - 230, Screen.height - 120, 200, 30), weapon.WeaponList[weapon.CurrentWeaponIdx].Ammo.ToString());
							}
						}
						//else{
						//weapon = play.GetComponent<WeaponController> ();
						//}

						GUI.skin.label.alignment = TextAnchor.UpperLeft;
						//GUI.Label (new Rect (20, Screen.height - 50, 250, 30), "R Mouse : Switch Guns C : Change Camera");

					}
					else
					{
						play = (PlayerController)GameObject.FindObjectOfType(typeof(PlayerController));
						weapon = play.GetComponent<WeaponController>();
					}
					break;
				case 1:
					if (play)
						play.Active = false;

					MouseLock.MouseLocked = false;

					GUI.skin.label.alignment = TextAnchor.MiddleCenter;
					GUI.Label(new Rect(0, Screen.height / 2 + 10, Screen.width, 30), "Game Over");

					GUI.DrawTexture(new Rect(Screen.width / 2 - Logo.width / 2, Screen.height / 2 - Logo.height * 1.2f, Logo.width, Logo.height), Logo);

					if (GUI.Button(new Rect(Screen.width / 2 - 150, Screen.height / 2 + 50, 300, 40), "Restart"))
					{
						SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

					}
					if (GUI.Button(new Rect(Screen.width / 2 - 150, Screen.height / 2 + 100, 300, 40), "Main menu"))
					{
						SceneManager.LoadScene("Mainmenu");
					}
					break;

				case 2:
					if (play)
						play.Active = false;

					MouseLock.MouseLocked = false;
					Time.timeScale = 0;
					GUI.skin.label.alignment = TextAnchor.MiddleCenter;
					GUI.Label(new Rect(0, Screen.height / 2 + 10, Screen.width, 30), "Pause");

					GUI.DrawTexture(new Rect(Screen.width / 2 - Logo.width / 2, Screen.height / 2 - Logo.height * 1.2f, Logo.width, Logo.height), Logo);

					if (GUI.Button(new Rect(Screen.width / 2 - 150, Screen.height / 2 + 50, 300, 40), "Resume"))
					{
						Mode = 0;
						Time.timeScale = 1;
					}
					if (GUI.Button(new Rect(Screen.width / 2 - 150, Screen.height / 2 + 100, 300, 40), "Main menu"))
					{
						Time.timeScale = 1;
						Mode = 0;
						SceneManager.LoadScene("Mainmenu");
					}
					break;

			}
		}
	}
}
