using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISpawner : MonoBehaviour
{

    public bool Enabled = true;

    [Serializable]
    public struct WeaponInfo
    {
        public GameObject prefabWeapon;
        public string[] RocketPosName;
    }

    [Serializable]
    public struct JetAIInfo
    {
        public GameObject AIJetPrefab;
        public WeaponInfo[] WeaponPrefabList;
    }

    public List<JetAIInfo> Objectman=new List<JetAIInfo>(); // object to spawn
    public float timeSpawn = 3;
    public int spawnObjsCount = 10;
    public int radius = 10;
    public string Tag = "Enemy";
    public string[] AntagonistTags = { "Player" };
    private float timetemp = 0;
    private int indexSpawn;

    HashSet<GameObject> spawnObjs = new HashSet<GameObject>();

    public static string AIDeadEventPrefix { get; } = "AIDeadEvent";
    public string AIDeadEventTag { get=> AIDeadEventPrefix + Tag; }

    // Start is called before the first frame update
    void Start()
    {
        indexSpawn = UnityEngine.Random.Range(0, Objectman.Count);
        timetemp = Time.time;
        EventsManager<GameObject>.AddListener(AIDeadEventTag, OnAIDead);
    }

    private void OnAIDead(GameObject aiObj)
    {
        spawnObjs.Remove(aiObj);
        //EventsManager<GameObject>.Invoke(GameModelCtrl.DeadEvet, aiObj);
    }

    // Update is called once per frame
    void Update()
    {
        if (!Enabled)
            return;

        if (spawnObjs.Count < spawnObjsCount && Time.time > timetemp + timeSpawn)
        {
            timetemp = Time.time;
            GameObject JetObj = (GameObject)GameObject.Instantiate(Objectman[indexSpawn].AIJetPrefab, transform.position + new Vector3(UnityEngine.Random.Range(-radius, radius), 0, UnityEngine.Random.Range(-radius, radius)), transform.rotation);
            JetObj.tag = Tag;
            SyncBase sync = JetObj.GetComponent<SyncBase>();
            if (sync)
            {
                sync.SCallbackSetSyncTag(Tag);
            }
            JetObj.GetComponent<AIController>().TargetTag = AntagonistTags;
            spawnObjs.Add(JetObj);
            if (NetworkServer.active)
                NetworkServer.Spawn(JetObj);
            foreach (var weapon in Objectman[indexSpawn].WeaponPrefabList)
            {
                GameObject weaponObj = Instantiate(weapon.prefabWeapon, JetObj.transform.position, transform.rotation, JetObj.transform);
                WeaponLauncher launcher = weaponObj.GetComponent<WeaponLauncher>();
                List<string> posName = new List<string>(weapon.RocketPosName);
                launcher.MissileOuterPosName = posName;
                launcher.AmmoMax = posName.Count;
                weaponObj.transform.parent = JetObj.transform;
                if(NetworkServer.active)
                    NetworkServer.Spawn(weaponObj);
            }
            indexSpawn = UnityEngine.Random.Range(0, Objectman.Count);
        }
    }

    private void OnDestroy()
    {
        EventsManager<GameObject>.RemoveListener(AIDeadEventTag, OnAIDead);
    }
}
