using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SerializeField]
public class ChangeIDBase
{
    /// <summary>
    /// ��������ӿ�
    /// </summary>
    /// <param name="newId">��id</param>
    /// <param name="playerIdReference">��id</param>
    /// <param name="changeNameEvent">�����ص�</param>
    public virtual void DoChangeID(string newId, ref string playerIdReference, GameManager.ChangeNameEvent changeNameEvent){}

}
