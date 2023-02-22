using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class LockWarring : MonoBehaviour
{

    public HashSet<WeaponLauncher> LockerLaunchers = new HashSet<WeaponLauncher>();
    public HashSet<MoverMissile> LockerMissiles = new HashSet<MoverMissile>();
    public AudioSource BeLocked;
    public AudioSource HadenBeLocked;
    // Start is called before the first frame update
    void Start()
    {
            
    }

    // Update is called once per frame
    void Update()
    {
        if (FlightView.singleton && FlightView.singleton.Target == gameObject)
        {
            BeLocked.spatialBlend = 0;
            HadenBeLocked.spatialBlend = 0;
        }
        else
        {
            BeLocked.spatialBlend = 1;
            HadenBeLocked.spatialBlend = 1;
        }
        if (BeLocked && HadenBeLocked)
        {
            if (LockerMissiles.Count > 0)
            {
                BeLocked.Stop();
                if (!HadenBeLocked.isPlaying)
                    HadenBeLocked.Play();
            }
            else if (LockerLaunchers.Count > 0 && LockerMissiles.Count == 0 && !BeLocked.isPlaying)
                BeLocked.Play();
            else if(LockerLaunchers.Count == 0 && LockerMissiles.Count == 0)
            {
                BeLocked.Stop();
                HadenBeLocked.Stop();
            }
        }
    }

    public void Locked(WeaponLauncher launcher)
    {
        if (launcher != null && !LockerLaunchers.Contains(launcher))
            LockerLaunchers.Add(launcher);
    }

    public void Locked(MoverMissile missile)
    {
        if (missile != null && !LockerMissiles.Contains(missile))
            LockerMissiles.Add(missile);
    }

    public void Unlocked(WeaponLauncher launcher)
    {
        if (LockerLaunchers.Contains(launcher))
            LockerLaunchers.Remove(launcher);
    }

    public void Unlocked(MoverMissile missile)
    {
        if (LockerMissiles.Contains(missile))
            LockerMissiles.Remove(missile);
    }

    private void OnDestroy()
    {
        WeaponLauncher []list = new WeaponLauncher[LockerLaunchers.Count];
        LockerLaunchers.CopyTo(list);
        for (int i = 0; i < list.Length; i++)
        {
            if (list[i])
                continue;
            list[i].Unlock(gameObject);
        }
    }

    public SyncList<GameObject> SyncLockerLaunchers
    {
        get 
        {
            GameObject[] temp = new GameObject[LockerLaunchers.Count];
            WeaponLauncher[] launcherTemp = new WeaponLauncher[LockerLaunchers.Count];
            LockerLaunchers.CopyTo(launcherTemp);
            for (int i = 0; i < launcherTemp.Length; i++)
                temp[i] = launcherTemp[i].gameObject;
            return new SyncList<GameObject>(temp);
        }
        set
        {
            LockerLaunchers.Clear();
            foreach (var item in value)
                LockerLaunchers.Add(item.GetComponent<WeaponLauncher>());
        }
    }

    public SyncList<GameObject> SyncLockerMissiles
    {
        get
        {
            GameObject []temp = new GameObject[LockerMissiles.Count];
            MoverMissile[] missilesTemp = new MoverMissile[LockerMissiles.Count];
            LockerMissiles.CopyTo(missilesTemp);
            for (int i = 0; i < missilesTemp.Length; i++)
                temp[i] = missilesTemp[i].gameObject;
            return new SyncList<GameObject>(temp);
        }
        set
        {
            LockerMissiles.Clear();
            foreach (var item in value)
                LockerMissiles.Add(item.GetComponent<MoverMissile>());
        }
    }
}
