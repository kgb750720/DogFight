using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UIPlayerRowContentBase : MonoBehaviour
{
    public GameObject PrefabPlayerRow;

    public RectTransform ListContent;

    public NetworkPlayingRoomStolsPlayer localPlayer;
    // Start is called before the first frame update
    protected virtual void Start()
    {
        
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        
    }

    public virtual void UpdateContent(Dictionary<string, HashSet<NetworkPlayingRoomStolsPlayer>> teamGroup){}

    public virtual void ClearList()
    {
        for (int i = ListContent.childCount - 1; i >= 0; i--)
        {
            GameObject.Destroy(ListContent.GetChild(i).gameObject);
        }
    }
}
