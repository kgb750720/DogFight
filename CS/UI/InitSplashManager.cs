using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.Shift;

[RequireComponent(typeof(SplashScreenManager))]
public class InitSplashManager : MonoBehaviour
{
    private void Awake()
    {
        if (GameManager.SkipLogin)
            GetComponent<SplashScreenManager>().enableLoginScreen = false;
        if (GameManager.SkipPressAnyKey)
            GetComponent<SplashScreenManager>().enablePressAnyKeyScreen = false;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DoSkipLogin()
    {
        GameManager.DoSkipLogin();
    }

    public void DoSkipPressAnyKey()
    {
        GameManager.DoSkipPressAnyKey();
    }
}
