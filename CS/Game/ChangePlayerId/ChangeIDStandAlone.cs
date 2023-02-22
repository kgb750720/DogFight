using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SerializeField]
public class ChangeIDStandAlone : ChangeIDBase
{
    public override void DoChangeID(string newId, ref string playerIdReference, GameManager.ChangeNameEvent changeNameEvent)
    {
        playerIdReference = newId;
        changeNameEvent?.Invoke(playerIdReference);
    }

}
