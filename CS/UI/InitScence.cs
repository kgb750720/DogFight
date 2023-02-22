using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitScence : MonoBehaviour
{
    public bool InitOpenShowHanger = true;
    public bool InitShowHangerArround = true;

    UIHangerManger UIHanger;

    // Start is called before the first frame update
    void Start()
    {
        if (InitOpenShowHanger)
        {
            UIHanger = GameObject.FindObjectOfType<UIHangerManger>();
            if (UIHanger)
            {
                UIHanger.OpenShowHanger(null);
                if (InitShowHangerArround)
                    UIHanger.Around = true;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        if (UIHanger)
        {

            UIHanger.Touch = false;
            UIHanger.Around = false;
            UIHanger.EquipmentUIShow = false;
            UIHanger.CloseShowHanger();
        }
    }

    public void EnableHangerArround(bool enable)
    {
        UIHanger.Around = enable;
    }
}
