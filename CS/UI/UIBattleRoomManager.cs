using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UIBattleRoomManager : MonoBehaviour
{
    public UIMainPanelManager panelManager;

    public UnityEvent ShowEvents = new UnityEvent();
    public UnityEvent CloseEvents = new UnityEvent();

    private void Awake()
    {
        if (!panelManager)
            panelManager = transform.Find("Menu Manager").GetComponent<UIMainPanelManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Show()
    {

    }
}
