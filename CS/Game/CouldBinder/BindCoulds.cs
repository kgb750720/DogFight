using Mewlist;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BindCoulds : MonoBehaviour
{
    [ResourcesMassiveCloudProfile]
    public string[] MassiveCloudsProfiles;

    MassiveClouds massiveClouds;

    private void Awake()
    {
        if (!massiveClouds)
            massiveClouds = GetComponent<MassiveClouds>();
        massiveClouds.profiles.Clear();
        foreach (var path in MassiveCloudsProfiles)
        {
            MassiveCloudsProfile profile = Resources.Load<MassiveCloudsProfile>(GameManager.RemovePathPrefixAndSuffix(path));
            massiveClouds.profiles.Add(profile);
        }
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
