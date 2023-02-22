using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class UIScoreboardPanel : MonoBehaviour
{
    public RectTransform ScoreboardPanelTrans;
    [Header("KEY")]
    [SerializeField]
    public KeyCode hotkey;
    public bool SwitchShowPanel = false;
    public UnityEvent OpenPanelEvent = new UnityEvent();
    public UnityEvent ClosePanelEvent = new UnityEvent();


    public TMP_Text PersonScore;
    public TMP_Text PersonKAD;
    public TMP_Text PersonKills;
    public TMP_Text PersonDead;
    public TMP_Text PersonKD;

    // Start is called before the first frame update
    void Start()
    {
        ScoreboardPanelTrans.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(SwitchShowPanel&&Input.GetKeyDown(hotkey))
        {
            if(ScoreboardPanelTrans.gameObject.activeSelf)
            {
                ScoreboardPanelTrans.gameObject.SetActive(false);
                ClosePanelEvent.Invoke();
            }
            else
            {
                ScoreboardPanelTrans.gameObject.SetActive(true);
                OpenPanelEvent.Invoke();
            }
        }
        else if(Input.GetKey(hotkey))
        {
            ScoreboardPanelTrans.gameObject.SetActive(true);
            OpenPanelEvent.Invoke();
        }
        else
        {
            ScoreboardPanelTrans.gameObject.SetActive(false);
            ClosePanelEvent.Invoke();
        }
    }

    public void UpdatePersonScoreInfo(PlayerScoresInfo scorebardInfo)
    {
        PersonScore.text = scorebardInfo.Score.ToString();
        PersonKills.text = scorebardInfo.Kills.ToString();
        PersonDead.text = scorebardInfo.Dead.ToString();
        string[] pointNubs = ((float)scorebardInfo.Score / 100 / Mathf.Max(scorebardInfo.Dead, 1)).ToString().Split('.');
        PersonKAD.text = pointNubs[0] + "." + (pointNubs.Length > 1 ? pointNubs[1].Substring(0, Mathf.Min(pointNubs[1].Length,2)) : "0");
        pointNubs = ((float)scorebardInfo.Kills / Mathf.Max(scorebardInfo.Dead, 1)).ToString().Split('.');
        PersonKD.text = pointNubs[0] + "." + (pointNubs.Length > 1 ? pointNubs[1].Substring(0, Mathf.Min(pointNubs[1].Length, 2)) : "0");
    }

    public void UpdateScorebroad(PlayerScoresInfo[] playersScoresInfo)
    {
    }
}
