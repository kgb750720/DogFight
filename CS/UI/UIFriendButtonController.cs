using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIFriendButtonController : MonoBehaviour
{
    public TMP_Text tmpPlayerName;
    public TMP_Text tmpPlayerStatus;
    public delegate void DestroyEvent();
    public DestroyEvent OnInstanceDestroy;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        OnInstanceDestroy?.Invoke();
    }

    public void SetPlaeyrName(string palyerName)
    {
        tmpPlayerName.text = palyerName;
    }

    public void SetPlayerStatus(string status)
    {
        tmpPlayerStatus.text = status;
    }

}
