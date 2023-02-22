using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.Shift;
using UnityEngine.Events;

public class UIMainPanelManager : MonoBehaviour,IMainPanelManager
{
    [Header("PANEL LIST")]
    public List<PanelItem> panels = new List<PanelItem>();

    [Header("SETTINGS")]
    public int currentPanelIndex = 0;
    protected int currentButtonIndex = 0;
    public int newPanelIndex;

    public GameObject currentPanel { get; protected set; }
    protected GameObject nextPanel;
    protected GameObject currentButton;
    protected GameObject nextButton;

    protected Animator currentPanelAnimator;
    protected Animator nextPanelAnimator;
    protected Animator currentButtonAnimator;
    protected Animator nextButtonAnimator;

    public string panelFadeIn { get; } = "Panel In";
    public string panelFadeOut { get; } = "Panel Out";
    public string buttonFadeIn { get; } = "Normal to Pressed";
    public string buttonFadeOut { get; } = "Pressed to Dissolve";
    public string buttonFadeNormal { get; } = "Pressed to Normal";

    [System.Serializable]
    public class PanelItem
    {
        public string panelName;
        public GameObject panelObject;
        public GameObject buttonObject;
    }

    [System.Serializable]
    public class UIEventItem
    {
        public string panelName;
        public UnityEvent UIOpenEvent;
        public UnityEvent UICloseEvent;
    }

    [SerializeField]
    protected List<UIEventItem> UIEventList = new List<UIEventItem>();

    protected Dictionary<string,UIEventItem> eventHash = new Dictionary<string,UIEventItem>();

    protected void Start()
    {
        currentButton = panels[currentPanelIndex].buttonObject;
        currentButtonAnimator = currentButton.GetComponent<Animator>();
        currentButtonAnimator.Play(buttonFadeIn);

        currentPanel = panels[currentPanelIndex].panelObject;
        currentPanelAnimator = currentPanel.GetComponent<Animator>();
        currentPanelAnimator.Play(panelFadeIn);
        //base.Start();
        foreach (var item in UIEventList)
        {
            eventHash.Add(item.panelName,item);
        }

        
    }

    public void OpenFirstTab()
    {
        if (eventHash.ContainsKey(panels[currentPanelIndex].panelName))
            eventHash[panels[currentPanelIndex].panelName].UICloseEvent.Invoke();
        if (currentPanelIndex != 0)
        {
            currentPanel = panels[currentPanelIndex].panelObject;
            currentPanelAnimator = currentPanel.GetComponent<Animator>();
            currentPanelAnimator.Play(panelFadeOut);

            currentButton = panels[currentPanelIndex].buttonObject;
            currentButtonAnimator = currentButton.GetComponent<Animator>();
            currentButtonAnimator.Play(buttonFadeNormal);

            currentPanelIndex = 0;
            currentButtonIndex = 0;

            currentPanel = panels[currentPanelIndex].panelObject;
            currentPanelAnimator = currentPanel.GetComponent<Animator>();
            currentPanelAnimator.Play(panelFadeIn);

            currentButton = panels[currentButtonIndex].buttonObject;
            currentButtonAnimator = currentButton.GetComponent<Animator>();
            currentButtonAnimator.Play(buttonFadeIn);
        }

        else if (currentPanelIndex == 0)
        {
            currentPanel = panels[currentPanelIndex].panelObject;
            currentPanelAnimator = currentPanel.GetComponent<Animator>();
            currentPanelAnimator.Play(panelFadeIn);

            currentButton = panels[currentButtonIndex].buttonObject;
            currentButtonAnimator = currentButton.GetComponent<Animator>();
            currentButtonAnimator.Play(buttonFadeIn);
        }
        if (eventHash.ContainsKey(panels[currentPanelIndex].panelName))
            eventHash[panels[currentPanelIndex].panelName].UIOpenEvent.Invoke();
    }

    public void OpenPanel(string newPanel)
    {
        if (eventHash.ContainsKey(panels[currentPanelIndex].panelName))
            eventHash[panels[currentPanelIndex].panelName].UICloseEvent.Invoke();
        for (int i = 0; i < panels.Count; i++)
        {
            if (panels[i].panelName == newPanel)
                newPanelIndex = i;
        }

        if (newPanelIndex != currentPanelIndex)
        {
            currentPanel = panels[currentPanelIndex].panelObject;
            currentPanelIndex = newPanelIndex;
            nextPanel = panels[currentPanelIndex].panelObject;

            currentPanelAnimator = currentPanel.GetComponent<Animator>();
            nextPanelAnimator = nextPanel.GetComponent<Animator>();

            currentPanelAnimator.Play(panelFadeOut);
            nextPanelAnimator.Play(panelFadeIn);

            currentButton = panels[currentButtonIndex].buttonObject;
            currentButtonIndex = newPanelIndex;
            nextButton = panels[currentButtonIndex].buttonObject;

            currentButtonAnimator = currentButton.GetComponent<Animator>();
            nextButtonAnimator = nextButton.GetComponent<Animator>();

            currentButtonAnimator.Play(buttonFadeOut);
            nextButtonAnimator.Play(buttonFadeIn);
        }
        if (eventHash.ContainsKey(panels[currentPanelIndex].panelName))
            eventHash[panels[currentPanelIndex].panelName].UIOpenEvent.Invoke();
    }

    public void NextPage()
    {
        if (eventHash.ContainsKey(panels[currentPanelIndex].panelName))
            eventHash[panels[currentPanelIndex].panelName].UICloseEvent.Invoke();
        if (currentPanelIndex <= panels.Count - 2)
        {
            currentPanel = panels[currentPanelIndex].panelObject;
            currentButton = panels[currentButtonIndex].buttonObject;
            nextButton = panels[currentButtonIndex + 1].buttonObject;

            currentPanelAnimator = currentPanel.GetComponent<Animator>();
            currentButtonAnimator = currentButton.GetComponent<Animator>();

            currentButtonAnimator.Play(buttonFadeNormal);
            currentPanelAnimator.Play(panelFadeOut);

            currentPanelIndex += 1;
            currentButtonIndex += 1;
            nextPanel = panels[currentPanelIndex].panelObject;

            nextPanelAnimator = nextPanel.GetComponent<Animator>();
            nextButtonAnimator = nextButton.GetComponent<Animator>();
            nextPanelAnimator.Play(panelFadeIn);
            nextButtonAnimator.Play(buttonFadeIn);
        }
        if (eventHash.ContainsKey(panels[currentPanelIndex].panelName))
            eventHash[panels[currentPanelIndex].panelName].UIOpenEvent.Invoke();
    }

    public void PrevPage()
    {
        if (eventHash.ContainsKey(panels[currentPanelIndex].panelName))
            eventHash[panels[currentPanelIndex].panelName].UICloseEvent.Invoke();
        if (currentPanelIndex >= 1)
        {
            currentPanel = panels[currentPanelIndex].panelObject;
            currentButton = panels[currentButtonIndex].buttonObject;
            nextButton = panels[currentButtonIndex - 1].buttonObject;

            currentPanelAnimator = currentPanel.GetComponent<Animator>();
            currentButtonAnimator = currentButton.GetComponent<Animator>();

            currentButtonAnimator.Play(buttonFadeNormal);
            currentPanelAnimator.Play(panelFadeOut);

            currentPanelIndex -= 1;
            currentButtonIndex -= 1;
            nextPanel = panels[currentPanelIndex].panelObject;

            nextPanelAnimator = nextPanel.GetComponent<Animator>();
            nextButtonAnimator = nextButton.GetComponent<Animator>();
            nextPanelAnimator.Play(panelFadeIn);
            nextButtonAnimator.Play(buttonFadeIn);
        }
        if (eventHash.ContainsKey(panels[currentPanelIndex].panelName))
            eventHash[panels[currentPanelIndex].panelName].UIOpenEvent.Invoke();
    }

    //public void CloseCurrentUIProcess()
    //{
    //    GameObject currentPanel = panels[currentPanelIndex].panelObject;
    //    Animator currentPanelAnimator = currentPanel.GetComponent<Animator>();
    //    currentPanelAnimator.Play(panelFadeOut);

    //    GameObject currentButton = panels[currentPanelIndex].buttonObject;
    //    Animator currentButtonAnimator = currentButton.GetComponent<Animator>();
    //    currentButtonAnimator.Play(buttonFadeNormal);
    //}
}
