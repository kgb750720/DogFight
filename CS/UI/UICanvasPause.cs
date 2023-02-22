using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UICanvasPause : MonoBehaviour
{
    public bool Pause = false;
    public UnityEvent OpenPauseEvents = new UnityEvent();
    public UnityEvent ClosePauseEvents = new UnityEvent();

    Stack<Button> stkBack = new Stack<Button>();

    CursorLockMode _lastCursorState = CursorLockMode.None;
    // Start is called before the first frame update
    void Start()
    {
        if(Pause)
            OpenPauseEvents?.Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetPause(bool pause)
    {
        if(this.Pause!=pause)
        {
            if (pause)
            {
                OpenPauseEvents?.Invoke();
                _lastCursorState = Cursor.lockState;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                ClosePauseEvents?.Invoke();
                Cursor.lockState = _lastCursorState;
            }
        }
        Pause = pause;
    }

    public void SwitchPause()
    {
        Pause = !Pause;
        if (Pause)
            OpenPauseEvents?.Invoke();
        else
            ClosePauseEvents?.Invoke();
    }

    public void Back()
    {
        if(!Pause)
        {
            SetPause(true);
        }
        else
        {
            if (stkBack.Count > 0)
            {
                stkBack.Peek().onClick.Invoke();
                stkBack.Pop();
            }
            else
                SetPause(false);
        }
    }

    public void AddBackButton(Button btnBack)
    {
        stkBack.Push(btnBack);
    }
}
