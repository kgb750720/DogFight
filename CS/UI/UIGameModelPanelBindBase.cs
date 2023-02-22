using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGameModelPanelBindBase : MonoBehaviour
{
    protected NetworkPlayingRoomGameModelPlayer _modelPlayer;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected virtual void OnDestroy()
    {
        if(_modelPlayer)
            _modelPlayer.OnResponsePanelUpdateMessage.AddListener(SetPanelUpdateMessage);
    }

    public virtual void SetBindModelPlayer(NetworkPlayingRoomGameModelPlayer modelPlayer)
    {
        _modelPlayer = modelPlayer;
        SetPanelUpdateMessage(modelPlayer.GameModelPanelMessage);
        modelPlayer.OnResponsePanelUpdateMessage.AddListener(SetPanelUpdateMessage);
    }

    public virtual void SetPanelUpdateMessage(ModelPanelUpdateBaseMessage msg){}
}
