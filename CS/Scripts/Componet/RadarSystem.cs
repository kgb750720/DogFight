/// <summary>
/// This is Radar System. using to detection an objects and showing on minimap by Tags[]
/// </summary>
using UnityEngine;
using System.Collections;
using HWRcomponent;
using System.Collections.Generic;

namespace HWRcomponent
{
	public enum Alignment
	{
		None,
		LeftTop,
		RightTop,
		LeftBot,
		RightBot ,
		MiddleTop ,
		MiddleBot
	}

}
public class RadarSystem : MonoBehaviour
{

	private Vector2 inposition;
	public float Size = 100; // size of minimap
	public float Distance = 2000;// maximum distance of objects
	public float Alpha = 0.5f;
	public bool MarkEnemyOnScreem = true;
	//public Texture2D[] Navtexture;// textutes list	//Navtexture idx0：敌军 Navtexture idx1：友军
	public Texture2D EnemyTexture;
	public Texture2D FriendTexture;
	public Texture2D MissileTexture;
	public Texture2D ScreenEnemyTexture;
	//public string[] EnemyTag;// object tags list
	public HashSet<string> EnemyTag=new HashSet<string>();
	public Texture2D NavCompass;// compass texture
	public Texture2D NavBG;// background texture
	public Vector2 PositionOffset = new Vector2 (0, 0);// minimap position offset
	public float Scale = 1;// mini map scale ( Scale < 1 = zoom in , Scale > 1 = zoom out)
	public Alignment PositionAlignment = Alignment.None;// position alignment
	public bool MapRotation;
	public GameObject Player;
	public bool Show = true;
	public Color ColorMult = Color.white;

	//扫描间隔时间
	public float ScanningIntervalTime = 0.5f;
	private float lastScanningTime;
	public GameObject[] ScanningObjs;
	public List<GameObject> ScanningFriends = new List<GameObject>();
	public List<GameObject> ScanningEnemys = new List<GameObject>();
	public List<GameObject> ScanningMissiles = new List<GameObject>();

	private FlightView view;

	private void Awake()
    {
		lastScanningTime -= ScanningIntervalTime;
    }

    void Start ()
	{
		ResetEnemyTag();
		view = FindObjectOfType<FlightView>();
	}

	public void ResetEnemyTag()
	{
		GameManager manager = GameManager.Manager;//GameObject.FindObjectOfType<GameManager>();
		if (manager.TargetTags.ContainsKey(transform.tag))
		{
			string[] tags = manager.TargetTags[transform.tag];
			EnemyTag.Clear();
			foreach (var item in tags)
			{
				EnemyTag.Add(item);
			}
		}
	}

	void Update ()
	{
		if (!Player) {
			Player = this.gameObject;
		}

        if (CurrentCamera == null)
        {

            CurrentCamera = Camera.main;

            if (CurrentCamera == null)
                CurrentCamera = Camera.current;
        }
        if (Camera.current != null)
        {
            if (CurrentCamera != Camera.current)
            {
                CurrentCamera = Camera.current;
            }
        }


        if (Scale <= 0) {
			Scale = 1;
		}
		// define the position
		switch (PositionAlignment) {
		case Alignment.None:
			inposition = PositionOffset;
			break;
		case Alignment.LeftTop:
			inposition = Vector2.zero + PositionOffset;
			break;
		case Alignment.RightTop:
			inposition = new Vector2 (Screen.width - Size, 0) + PositionOffset;
			break;
		case Alignment.LeftBot:
			inposition = new Vector2 (0, Screen.height - Size) + PositionOffset;
			break;
		case Alignment.RightBot:
			inposition = new Vector2 (Screen.width - Size, Screen.height - Size) + PositionOffset;
			break;
		case Alignment.MiddleTop:
			inposition = new Vector2 ((Screen.width / 2) - (Size / 2), Size) + PositionOffset;
			break;
		case Alignment.MiddleBot:
			inposition = new Vector2 ((Screen.width / 2) - (Size / 2), Screen.height - Size) + PositionOffset;
			break;
		}

		if (Time.time - lastScanningTime > ScanningIntervalTime)
		{
			lastScanningTime = Time.time;
			int t = LayerMask.GetMask(new string[] { "Scanning", "RocketScanning" });
			Collider[] hits = Physics.OverlapSphere(transform.position, Distance, LayerMask.GetMask(new string[] { "Scanning", "RocketScanning" }));
			ScanningObjs = new GameObject[hits.Length];
			ScanningEnemys.Clear();
			ScanningFriends.Clear();
			ScanningMissiles.Clear();
			for (int i = 0; i < hits.Length; i++)
			{
				ScanningObjs[i] = hits[i].gameObject;
				if (ScanningObjs[i].tag == gameObject.tag && ScanningObjs[i] != gameObject)
					ScanningFriends.Add(ScanningObjs[i]);
				else if (EnemyTag.Contains(ScanningObjs[i].tag))
					ScanningEnemys.Add(ScanningObjs[i]);
				else if (ScanningObjs[i].GetComponent<MoverMissile>())
					ScanningMissiles.Add(ScanningObjs[i]);
			}
		}

	}
	// convert 3D position to 2D position
	Vector2 ConvertToNavPosition (Vector3 pos)
	{
		Vector2 res = Vector2.zero;
		if (Player) {
            res.x = inposition.x + (((pos.x - Player.transform.position.x) / Distance * 2* Size  + (Size * Scale)/2f ) / Scale);
            res.y = inposition.y + ((-(pos.z - Player.transform.position.z) / Distance * 2 * Size + (Size * Scale)/2f ) / Scale);
            //res.x = inposition.x + (((pos.x - Player.transform.position.x) / Distance * Size  + (Size * Scale)/2f ) / Scale);
            //res.y = inposition.y + ((-(pos.z - Player.transform.position.z) / Distance * Size + (Size * Scale)/2f ) / Scale);
            //res.x = inposition.x + (pos.x - Player.transform.position.x) / Distance * Size ;
            //res.y = inposition.x + (pos.y - Player.transform.position.x) / Distance * Size;

        }
		return res;
	}

	void DrawNav (GameObject[] drawElementsList, Texture2D navtexture)
	{
		if (Player) {
			for (int i=0; i<drawElementsList.Length; i++) {
				if (drawElementsList[i]!=null&&Vector3.Distance (Player.transform.position, drawElementsList [i].transform.position) <= (Distance * Scale)) {
					Vector2 pos = ConvertToNavPosition (drawElementsList [i].transform.position);
                    
      //              if (Vector2.Distance (pos, (inposition + new Vector2 (Size / 2f, Size / 2f))) < (Size / 2f))
      //              {// + (navtexture.width / 2)
      //                  float navscale = Scale;
						//if (navscale < 1) {
						//	navscale = 1;
						//}
      //                  GUI.DrawTexture (new Rect (pos.x  - (navtexture.width / navscale)/2 , pos.y - (navtexture.height / navscale)/2 , navtexture.width / navscale , navtexture.height / navscale ), navtexture);
      //              }

					float navscale = Scale;
					if (navscale < 1) 
					{
							navscale = 1;
					}
                    if (Vector2.Distance (pos, (inposition + new Vector2 (Size / 2f, Size / 2f))) < (Size / 2f))	//雷达显示边缘内
                    {// + (navtexture.width / 2)
                        
                        GUI.DrawTexture (new Rect (pos.x  - (navtexture.width / navscale)/2 , pos.y - (navtexture.height / navscale)/2 , navtexture.width / navscale , navtexture.height / navscale ), navtexture);
                    }
                    else //超出部分在雷达界面边缘显示
                    {
						pos = (inposition + new Vector2(Size / 2f, Size / 2f)) + (pos - (inposition + new Vector2(Size / 2f, Size / 2f))).normalized * Size / 2f;  //显示边缘的坐标
						GUI.DrawTexture(new Rect(pos.x - (navtexture.width / navscale) / 2, pos.y - (navtexture.height / navscale) / 2, navtexture.width / navscale, navtexture.height / navscale), navtexture);
					}
				}
			}
		}
	}

	float[] list;
    private Camera CurrentCamera;

    void OnGUI ()
	{

		if(MarkEnemyOnScreem&&CurrentCamera)
        {
            foreach (var item in ScanningEnemys)
            {
				if (item)
				{
					Vector3 screenPoint = CurrentCamera.WorldToScreenPoint(item.transform.position);
					if (ScreenEnemyTexture&&screenPoint.z>0)
						GUI.DrawTexture(new Rect(screenPoint.x - ScreenEnemyTexture.width / 2, Screen.height - screenPoint.y - ScreenEnemyTexture.height / 2, ScreenEnemyTexture.width, ScreenEnemyTexture.height), ScreenEnemyTexture);
				}
            }
        }

		if (!Show)
			return;
		
		GUI.color = new Color (ColorMult.r, ColorMult.g, ColorMult.b, Alpha);
		if (MapRotation) {
			GUIUtility.RotateAroundPivot (-(this.transform.eulerAngles.y), inposition + new Vector2 (Size / 2f, Size / 2f)); 
		}

		

		if (ScanningFriends.Count > 0)
			DrawNav(ScanningFriends.ToArray(), FriendTexture);
		if(ScanningEnemys.Count>0)
			DrawNav(ScanningEnemys.ToArray(), EnemyTexture);
		if (ScanningMissiles.Count > 0)
			DrawNav(ScanningMissiles.ToArray(), MissileTexture);

		//for (int i=0; i<EnemyTag.Length; i++) {
		//	GameObject []objs = GameObject.FindGameObjectsWithTag(EnemyTag[i]);
		//	DrawNav (GameObject.FindGameObjectsWithTag (EnemyTag [i]), Navtexture [i]);
		//}


		if (NavBG)
			GUI.DrawTexture (new Rect (inposition.x, inposition.y, Size, Size), NavBG);
		GUIUtility.RotateAroundPivot ((this.transform.eulerAngles.y), inposition + new Vector2 (Size / 2f, Size / 2f)); 
		if (NavCompass)
			GUI.DrawTexture (new Rect (inposition.x + (Size / 2f) - (NavCompass.width / 2f), inposition.y + (Size / 2f) - (NavCompass.height / 2f), NavCompass.width, NavCompass.height), NavCompass);

	}
}




