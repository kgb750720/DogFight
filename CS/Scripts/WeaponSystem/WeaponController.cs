using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mirror;

public class WeaponController : MonoBehaviour
{
	public string[] TargetTag = new string[1]{"Enemy"};
	public List<WeaponLauncher> WeaponList = new List<WeaponLauncher>();
	public List<NetworkIdentity> WeaponIdentyList
    {
		get
        {
			List<NetworkIdentity> res = new List<NetworkIdentity>(WeaponList.Count);
            foreach (var item in WeaponList)
            {
				res.Add(item.GetComponent<NetworkIdentity>());
            }
			return res;
		}
    }
	public int CurrentWeaponIdx = 0;
	public bool ShowCrosshair;
	public bool InitAddAllWeapons = true;
	
	public struct SyncData
    {
		public string[] TargetTag;
		public int CurrentWeaponIdx;
		public List<NetworkIdentity> WeaponIdentyList;
		public SyncData(string[] targetTag,int currentWeaponIdx, List<NetworkIdentity> weaponIdentyList)
        {
			TargetTag = targetTag;
			CurrentWeaponIdx = currentWeaponIdx;
			WeaponIdentyList = weaponIdentyList;
        }
	}

	public SyncData Sync
    {
		get => new SyncData(TargetTag, CurrentWeaponIdx,WeaponIdentyList);
		set
        {
			TargetTag = value.TargetTag;
			if(CurrentWeaponIdx<WeaponList.Count)
				WeaponList[CurrentWeaponIdx].OnActive = false;
			CurrentWeaponIdx = value.CurrentWeaponIdx;
			WeaponList.Clear();
            foreach (var item in value.WeaponIdentyList)
            {
				if (item.gameObject)
					WeaponList.Add(item.GetComponent<WeaponLauncher>());
            }
			if (CurrentWeaponIdx < WeaponList.Count)
				WeaponList[CurrentWeaponIdx].OnActive = true;
        }
    }


	void Awake ()
	{
		
	}
	public WeaponLauncher GetCurrentWeapon(){
		if (CurrentWeaponIdx < WeaponList.Count && WeaponList [CurrentWeaponIdx] != null) {
			return WeaponList [CurrentWeaponIdx];
		}
		return null;
	}
	
	private void Start ()
	{
		// find all attached weapons.
		if (InitAddAllWeapons && this.transform.GetComponentsInChildren<WeaponLauncher>().Length > 0)
		{
			var weas = this.transform.GetComponentsInChildren<WeaponLauncher>();
			for (int i = 0; i < weas.Length; i++)
			{
				WeaponList.Add(weas[i].GetComponent<WeaponLauncher>());
				foreach (var item in TargetTag)
					WeaponList[i].TargetTag.Add(item);
			}
		}
		for (int i=0; i<WeaponList.Count; i++) {
			if (WeaponList [i] != null) {
				foreach (var item in TargetTag)
					WeaponList[i].TargetTag.Add(item);
				WeaponList [i].ShowCrosshair = ShowCrosshair;
			}
		}
	}

	private void Update ()
    {
		if(!GetComponent<SyncBase>()||NetworkServer.active)
			UpdateWeaponListState();
    }

    private void UpdateWeaponListState()
    {
        Stack<int> nullIdxs = new Stack<int>();
        for (int i = 0; i < WeaponList.Count; i++)
        {
            if (WeaponList[i] != null)
            {
                if (!GetComponent<JetSync>() || GetComponent<JetSync>().isServer)
                    WeaponList[i].OnActive = false;
            }
            else
                nullIdxs.Push(i);
        }

        while (nullIdxs.Count > 0)
        {
            if (nullIdxs.Peek() <= CurrentWeaponIdx && CurrentWeaponIdx != 0)
                CurrentWeaponIdx--;
            WeaponList.RemoveAt(nullIdxs.Pop());
        }

        if (CurrentWeaponIdx < WeaponList.Count && WeaponList[CurrentWeaponIdx] != null && (!GetComponent<JetSync>() || GetComponent<JetSync>().isServer))
        {
            WeaponList[CurrentWeaponIdx].OnActive = true;
        }
    }

    public void LaunchWeapon (int index)
	{
		CurrentWeaponIdx = index;
		if (CurrentWeaponIdx < WeaponList.Count && WeaponList [index] != null) {

			WeaponList [index].Shoot ();
		}
	}
	
	public void SwitchWeapon ()
	{
		CurrentWeaponIdx += 1;
		if (CurrentWeaponIdx >= WeaponList.Count) {
			CurrentWeaponIdx = 0;	
		}
	}
	public void LaunchWeapon ()
	{
		if (CurrentWeaponIdx < WeaponList.Count && WeaponList [CurrentWeaponIdx] != null) {
			WeaponList [CurrentWeaponIdx].Shoot ();
		}
	}

	public WeaponLauncher CurrLauncher
    {
		get
        {
			if (CurrentWeaponIdx < WeaponList.Count && CurrentWeaponIdx >= 0)
				return WeaponList[CurrentWeaponIdx];
			return null;
        }
    }

    public void AddLauncherOnceFireNub(int currentWeaponIdx, int addNub)
    {
		if(currentWeaponIdx<WeaponList.Count)
			WeaponList[currentWeaponIdx].AddFireOnceNub(addNub);
    }

    public void SwitchLauncherLockModel(int currentWeaponIdx)
    {
		if (currentWeaponIdx < WeaponList.Count)
			WeaponList[currentWeaponIdx].SwitchLockModel();
	}
}
