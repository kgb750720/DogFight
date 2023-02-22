using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial struct ItemUseSyncData
{
}

public class ItemUse: MonoBehaviour
{
    public virtual bool Used { get; set; }
    public virtual string ItemName { get; }

    

    public virtual ItemUseSyncData Sync { get; set; }

    public virtual void UseItem() { }
}
