using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
	// basic game score
	public int Score = 0;
	public int Killed = 0;

	public Dictionary<string, string[]> TargetTags = new Dictionary<string, string[]>();



	


	[SerializeField]
	private string playerId = "";
	public string PlayerId 
	{
		get
		{
			return playerId;
		}
		set
        {
			ChangeIdSC.DoChangeID(value,ref playerId, OnChangePlayerIdEvents);
        }
	}

	public delegate void ChangeNameEvent(string newName);
	public static ChangeNameEvent OnChangePlayerIdEvents;

	//[SerializeField]
	public ChangeIDBase ChangeIdSC;

	static bool skipLogin = false;
	static bool skipPressAnyKey = false;
	public static bool SkipLogin { get => skipLogin; }
	public static bool SkipPressAnyKey { get => skipPressAnyKey; }

	public static void DoSkipLogin()
    {
		skipLogin = true;
    }

	public static void DoSkipPressAnyKey()
	{
		skipPressAnyKey = true;
	}

	private static GameManager manager = null;

	//public static string ManagerPrefabPath = "Camera";

	public GameObject HangerPrefab;
	private static GameObject HangerInstanceObj;
	public static UIHangerManger Hanger { get => HangerInstanceObj.GetComponent<UIHangerManger>(); }

	public delegate void SpawnPrefabChangedCallBack(string newSpawnObjResourcesPath);

	public static string PathPrefix { get=>"Assets/Resources/"; }
	public static string PathSuffix { get => ".prefab"; }
	public static string RemovePathPrefixAndSuffix(string PrefabResourcesPath)
    {
		if (PathPrefix.Length + PathSuffix.Length > PrefabResourcesPath.Length)
			return "";
		return PrefabResourcesPath.Substring(PathPrefix.Length, PrefabResourcesPath.IndexOf('.') - PathPrefix.Length);

	}

	[SerializeField]
	[ResourcesPrefab]
	private string ResourcesSpawn;

	[ResourcesPrefab]
	private static string spawnPrefabResourcesPath;

	public static string SpawnPrefabResourcesPath
	{ 
		get => spawnPrefabResourcesPath;
		set
        {
			if (spawnPrefabResourcesPath != value)
			{
				spawnPrefabResourcesPath = value;
				if (!spawnWeaponPathList.ContainsKey(spawnPrefabResourcesPath))
					spawnWeaponPathList[spawnPrefabResourcesPath] = new SpawnPrefabPathWeaponListData();
				SpawnPrefabChanged.Invoke(value);
			}
        }
	}
	public static SpawnPrefabChangedCallBack SpawnPrefabChanged;

	

	[ResourcesPrefab]
	public List<string> WeaponList = new List<string>();
	//武器列表信息
	[Serializable]
	public class SpawnPrefabPathWeaponListData
	{
		public delegate void WeaponListChangedCallBack(string[] newList);
		public SpawnPrefabPathWeaponListData() { }
		public SpawnPrefabPathWeaponListData(List<string> weaponResourcesPathList)
        {
			this.weaponResourcesPathList = weaponResourcesPathList;
        }
		private List<string> weaponResourcesPathList = new List<string>();
		public string[] WeaponResourcesPathList
		{
			get => weaponResourcesPathList.ToArray();
			set
			{
				if (!Enumerable.SequenceEqual(weaponResourcesPathList.ToArray(), value))
				{
					weaponResourcesPathList.Clear();
					weaponResourcesPathList.AddRange(value);
					WeaponListChanged?.Invoke(value);
				}
			}
		}
		[JsonIgnore]
		public WeaponListChangedCallBack WeaponListChanged;
	}

	
	//各机体武器列表总表信息<机体预制体路径,对应的机体装备列表>
	static Dictionary<string, SpawnPrefabPathWeaponListData> spawnWeaponPathList = new Dictionary<string, SpawnPrefabPathWeaponListData>();

	private string GetSpawnWeaponPathListToJson()
    {
		//KeyValuePair<string, SpawnPrefabPathWeaponListData>[] pairsList = new KeyValuePair<string, SpawnPrefabPathWeaponListData>[spawnWeaponPathList.Count];
		//int i = 0;
		//      foreach (var item in spawnWeaponPathList)
		//      {
		//	pairsList[i] = item;
		//	i++;
		//      }
		//XmlSerializer xmlSerializer = new XmlSerializer(typeof(KeyValuePair<string, SpawnPrefabPathWeaponListData>[]));
		//MemoryStream ms = new MemoryStream();
		//xmlSerializer.Serialize(ms, pairsList);
		//return Encoding.UTF8.GetString(ms.ToArray());
		return JsonConvert.SerializeObject(spawnWeaponPathList);
	}

    private void SetSpawnWeaponPathListFromJson(string Json)
    {
		Dictionary<string, SpawnPrefabPathWeaponListData.WeaponListChangedCallBack> cache = new Dictionary<string, SpawnPrefabPathWeaponListData.WeaponListChangedCallBack>(spawnWeaponPathList.Count);
        foreach (var item in spawnWeaponPathList)
        {
			cache[item.Key] = item.Value.WeaponListChanged;
        }
        spawnWeaponPathList = JsonConvert.DeserializeObject<Dictionary<string, SpawnPrefabPathWeaponListData>>(Json);
        foreach (var item in cache)
        {
			if(spawnWeaponPathList.ContainsKey(item.Key))
            {
				spawnWeaponPathList[item.Key].WeaponListChanged = item.Value;
            }
        }
    }

    public static string[] GetWeaponResourcesPathList()
    {
		if (!spawnWeaponPathList.ContainsKey(spawnPrefabResourcesPath))
			spawnWeaponPathList.Add(spawnPrefabResourcesPath, new SpawnPrefabPathWeaponListData());
        return spawnWeaponPathList[spawnPrefabResourcesPath].WeaponResourcesPathList;
    }

    public static string[] GetWeaponResourcesPathList(string spawnPrefabPath)
    {
		if(spawnWeaponPathList.ContainsKey(spawnPrefabPath))
			return spawnWeaponPathList[spawnPrefabPath].WeaponResourcesPathList;
		return null;
	}

	public static void SetWeaponResourcesPathList(string[] weaponListPath)
	{
		spawnWeaponPathList[spawnPrefabResourcesPath].WeaponResourcesPathList = weaponListPath;
	}

	public static void SetWeaponResourcesPathList(string spawnPrefabPath, string[] weaponListPath)
	{
		if (spawnWeaponPathList.ContainsKey(spawnPrefabPath))
			spawnWeaponPathList[spawnPrefabPath].WeaponResourcesPathList = weaponListPath;
	}

	public static void RemoveWeaponListChanged(string spawnPrefabPath,SpawnPrefabPathWeaponListData.WeaponListChangedCallBack callback)
    {
		if(spawnWeaponPathList.ContainsKey(spawnPrefabPath))
        {
			spawnWeaponPathList[spawnPrefabPath].WeaponListChanged -= callback;
        }
    }

	public static void AddWeaponListChanged(string spawnPrefabPath, SpawnPrefabPathWeaponListData.WeaponListChangedCallBack callback)
	{
		if (!spawnWeaponPathList.ContainsKey(spawnPrefabPath))
			spawnWeaponPathList[spawnPrefabPath] = new SpawnPrefabPathWeaponListData();
		spawnWeaponPathList[spawnPrefabPath].WeaponListChanged += callback;
	}

	//装备配置信息
	[Serializable]
	public class SpawnPrefabConfigData
	{
		public delegate void EquipmentConfigChangeCallBack(string equipmentPrefab, string syncDataJons);
		public delegate void EquipmentConfigRemoveCallBack(string equipmentPrefab);

		public struct EquipmentConfigCallbackCache
        {
			public EquipmentConfigChangeCallBack changeCallback;
			public EquipmentConfigRemoveCallBack removeCallback;
		}
		[JsonIgnore]
		public EquipmentConfigChangeCallBack EquipmentConfChanged;
		[JsonIgnore]
		public EquipmentConfigRemoveCallBack EquipmentConfRemoved;
		//<装备预制体路径,Sync同步Json字符串>
		public Dictionary<string, string> equipmentConfig = new Dictionary<string, string>();
		public Dictionary<string, string> EquipmentConf { get => new Dictionary<string, string>(equipmentConfig); }
		public void AddEquimentConfig(string equipmentPrefab, string syncDataJons)
		{
			if (!equipmentConfig.ContainsKey(equipmentPrefab))
			{
				equipmentConfig.Add(equipmentPrefab, syncDataJons);
				EquipmentConfChanged.Invoke(equipmentPrefab, syncDataJons);
			}
			else
			{
				equipmentConfig[equipmentPrefab] = syncDataJons;
				EquipmentConfChanged.Invoke(equipmentPrefab, syncDataJons);
			}
		}

		public void RemoveEquimentConfig(string equipmentPrefab)
		{
			if (equipmentConfig.ContainsKey(equipmentPrefab))
			{
				equipmentConfig.Remove(equipmentPrefab);
				EquipmentConfRemoved.Invoke(equipmentPrefab);
			}
		}
	}
	//各机体配置储存总表<机体预制体路径，机体对应的装备配置信息>
	private static Dictionary<string, SpawnPrefabConfigData> spawnEquipmentConfig = new Dictionary<string, SpawnPrefabConfigData>();

	private string GetSpawnEquipmentConfigToJosn()
	{
		Dictionary<string, string> SpawnEquipmentConfigJson = new Dictionary<string, string>(spawnEquipmentConfig.Count);
        foreach (var item in spawnEquipmentConfig)
        {
			SpawnEquipmentConfigJson[item.Key] = JsonConvert.SerializeObject(item.Value);
		}
		return JsonConvert.SerializeObject(SpawnEquipmentConfigJson);
	}

	private void SetSpawnEquipmentConfigFormJson(string Json)
	{
		Dictionary<string, SpawnPrefabConfigData.EquipmentConfigCallbackCache> cache = new Dictionary<string, SpawnPrefabConfigData.EquipmentConfigCallbackCache>(spawnEquipmentConfig.Count);
        foreach (var item in spawnEquipmentConfig)
        {
			cache.Add(item.Key, new SpawnPrefabConfigData.EquipmentConfigCallbackCache { changeCallback = item.Value.EquipmentConfChanged, removeCallback = item.Value.EquipmentConfRemoved });
        }
		Dictionary<string,string> SpawnEquipmentConfig= JsonConvert.DeserializeObject<Dictionary<string, string>>(Json);
		spawnEquipmentConfig.Clear();
        foreach (var item in SpawnEquipmentConfig)
		{
			spawnEquipmentConfig[item.Key] = JsonConvert.DeserializeObject<SpawnPrefabConfigData>(item.Value);
		}
        foreach (var item in cache)
        {
			if (spawnEquipmentConfig.ContainsKey(item.Key))
			{
				spawnEquipmentConfig[item.Key].EquipmentConfChanged = item.Value.changeCallback;
				spawnEquipmentConfig[item.Key].EquipmentConfRemoved = item.Value.removeCallback;
			}
        }
	}

	public static void SpawnPrefabAddEquimentConfig(string spawnPrefab,string equipmentPrefab, string syncDataJons)
	{
		spawnEquipmentConfig[spawnPrefab].AddEquimentConfig(equipmentPrefab, syncDataJons);
	}

	public static void SpawnPrefabRemoveEquimentConfig(string spawnPrefab, string equipmentPrefab)
	{
		if (spawnEquipmentConfig.ContainsKey(spawnPrefab))
			spawnEquipmentConfig[spawnPrefab].RemoveEquimentConfig(equipmentPrefab);
	}

	public static Dictionary<string, string> GetSpawnEpuimentConfig(string spawnPrefab)
    {
		if (spawnEquipmentConfig.ContainsKey(spawnPrefab))
			return spawnEquipmentConfig[spawnPrefab].EquipmentConf;
		return new Dictionary<string, string>();
    }

	public static Dictionary<string, string> GetSpawnEpuimentConfig()
	{
		if (spawnEquipmentConfig.ContainsKey(spawnPrefabResourcesPath))
			return spawnEquipmentConfig[spawnPrefabResourcesPath].EquipmentConf;
		return new Dictionary<string, string>();
	}

	public static void  AddSpawnEquipmentConfChanged(string spawnPrefab, SpawnPrefabConfigData.EquipmentConfigChangeCallBack callback)
    {
		if (!spawnEquipmentConfig.ContainsKey(spawnPrefab))
			spawnEquipmentConfig[spawnPrefab] = new SpawnPrefabConfigData();
		spawnEquipmentConfig[spawnPrefab].EquipmentConfChanged += callback;
		//return null;
    }
	public static void RemoveSpawnEquipmentConfChanged(string spawnPrefab, SpawnPrefabConfigData.EquipmentConfigChangeCallBack callback)
    {
		if (!spawnEquipmentConfig.ContainsKey(spawnPrefab))
			spawnEquipmentConfig[spawnPrefab] = new SpawnPrefabConfigData();
		spawnEquipmentConfig[spawnPrefab].EquipmentConfChanged -= callback;
		//return null;
    }

	public static void AddSpawnEquipmentConfRemoved(string spawnPrefab, SpawnPrefabConfigData.EquipmentConfigRemoveCallBack callback)
    {
		if (!spawnEquipmentConfig.ContainsKey(spawnPrefab))
			spawnEquipmentConfig[spawnPrefab] = new SpawnPrefabConfigData();
		spawnEquipmentConfig[spawnPrefab].EquipmentConfRemoved += callback;
		//return null;
    }
	public static void RemoveSpawnEquipmentConfRemoved(string spawnPrefab, SpawnPrefabConfigData.EquipmentConfigRemoveCallBack callback)
    {
		if (!spawnEquipmentConfig.ContainsKey(spawnPrefab))
			spawnEquipmentConfig[spawnPrefab] = new SpawnPrefabConfigData();
		spawnEquipmentConfig[spawnPrefab].EquipmentConfRemoved -= callback;
		//return null;
    }

	static bool _isOnline = false;
	static public bool isOnline 
	{ 
		get=>_isOnline; 
		set
		{
			_isOnline = value;
			if (_isOnline)
				GameManager.Manager.LocalWriteJetConfigInfo();
			else
				GameManager.Manager.LocalReadJetConfigInfo();
		} 
	}

	public static GameManager Manager
    {
		get
        {
			if (manager != null)
				return manager;
			//if (ManagerPrefabPath != null)
			//{
			//	GameObject obj = Resources.Load(ManagerPrefabPath) as GameObject;
			//	if (obj.GetComponent<GameManager>())
			//	{
			//		GameObject.Instantiate(obj);
			//		return manager;
			//	}
			//}
			return new GameObject("GameManager").AddComponent<GameManager>();
        }
    }

    private void Awake()
    {
		if (manager == null)
        {
            TargetTags.Add("Player", new string[] { "Enemy", "Interfere" });
            TargetTags.Add("Enemy", new string[] { "Player", "Interfere" });
            TargetTags.Add("Team1", new string[] { "Team2", "Interfere" });
            TargetTags.Add("Team2", new string[] { "Team1", "Interfere" });
            manager = this;
            GameObject.DontDestroyOnLoad(gameObject);
            if (!HangerInstanceObj)
            {
                HangerInstanceObj = Instantiate(HangerPrefab, new Vector3(0, -1000, 0), Quaternion.identity);
                GameObject.DontDestroyOnLoad(HangerInstanceObj);
            }


            //初始化装备信息
            LocalReadJetConfigInfo();

            //初始化内存池(内存池改写为独立的PoolManager模块)
            //SceneObjetsPool = new Dictionary<Scene, Dictionary<string, Pool>>();

            //初始化改名方式
            ChangeIdSC = new ChangeIDStandAlone();
        }
    }

    private void LocalReadJetConfigInfo()
    {
        if (!PlayerPrefs.HasKey("ResourcesSpawn"))
            spawnPrefabResourcesPath = ResourcesSpawn;
        else
            spawnPrefabResourcesPath = PlayerPrefs.GetString("ResourcesSpawn");
        //SpawnPrefabChanged += delegate
        //{
        //    if (!isOnline)
        //        PlayerPrefs.SetString("ResourcesSpawn", spawnPrefabResourcesPath);
        //};

        if (PlayerPrefs.HasKey("SpawnEquipmentConfig"))
        {
            SetSpawnEquipmentConfigFormJson(PlayerPrefs.GetString("SpawnEquipmentConfig"));
        }
        //void funcTemp()
        //spawnEquipmentConfig[spawnPrefabResourcesPath].EquipmentConfChanged

        if (!PlayerPrefs.HasKey("SpawnWeaponPathList"))
            spawnWeaponPathList[spawnPrefabResourcesPath] = new SpawnPrefabPathWeaponListData(WeaponList);
        else
            SetSpawnWeaponPathListFromJson(PlayerPrefs.GetString("SpawnWeaponPathList"));
    }

    void Start () {
		Score = 0;
		Killed = 0;
	}
	
	// Update is called once per frame
	void Update () 
	{
		//GameManager manager = this;
		//string temp = GetSpawnWeaponPathListToJson();
		//print(temp);
	}


    private void OnDestroy()
    {
		if (!isOnline)
        {
            LocalWriteJetConfigInfo();
        }
    }

    private void LocalWriteJetConfigInfo()
    {
        PlayerPrefs.SetString("ResourcesSpawn", spawnPrefabResourcesPath);
        PlayerPrefs.SetString("SpawnWeaponPathList", GetSpawnWeaponPathListToJson());
		string s = GetSpawnEquipmentConfigToJosn();

		PlayerPrefs.SetString("SpawnEquipmentConfig", s);
    }

    // add score function
    public void AddScore(int score){
		Score += score;
		Killed +=1;
	}
	
	void OnGUI(){
		//GUI.Label(new Rect(20,20,300,30),"Kills "+Score);
	}
	// game over fimction
	public void PlayerDeadEvent(){
		//GameUI menu = (GameUI)GameObject.FindObjectOfType(typeof(GameUI));
		//if(menu){
		//	menu.Mode = 1;	
		//}
	}

	public static XmlElement LoadMXL(string LoadXMLName)
    {
		TextAsset text = Resources.Load<TextAsset>("XML/"+LoadXMLName);
		XmlDocument xml = new XmlDocument();
		xml.LoadXml(text.ToString().Trim());
		return xml.DocumentElement;
	}

	public static XmlNodeList LoadMxlNodeList(string LoadXMLName,string Info)
    {
		XmlElement root = LoadMXL(LoadXMLName);
		XmlElement infoList = root.SelectSingleNode(Info) as XmlElement;
		return infoList.ChildNodes;
	}
}
