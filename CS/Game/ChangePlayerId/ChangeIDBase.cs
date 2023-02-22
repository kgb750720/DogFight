using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SerializeField]
public class ChangeIDBase
{
    /// <summary>
    /// 改名基类接口
    /// </summary>
    /// <param name="newId">新id</param>
    /// <param name="playerIdReference">老id</param>
    /// <param name="changeNameEvent">改名回调</param>
    public virtual void DoChangeID(string newId, ref string playerIdReference, GameManager.ChangeNameEvent changeNameEvent){}

}
