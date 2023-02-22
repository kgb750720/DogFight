using Michsky.UI.Shift;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDoOpenExitPanel : MonoBehaviour
{

    public ModalWindowManager ModalWinmanager;
    public BlurManager BlurManager;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TryOpen()
    {
        if(!ModalWinmanager.isOn&&!BlurManager.isOn)
        {
            ModalWinmanager.ModalWindowIn();
            BlurManager.BlurInAnim();
        }

    }

    public void TryClose()
    {
        if (ModalWinmanager.isOn&&BlurManager.isOn)
        {
            ModalWinmanager.ModalWindowOut();
            BlurManager.BlurOutAnim();
        }
    }
}
