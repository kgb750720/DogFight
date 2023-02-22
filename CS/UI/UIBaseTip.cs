using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBaseTip : MonoBehaviour
{
    public Transform Tip;
    Text tip;
    float count;
    protected virtual void Awake()
    {
        if (!Tip)
            Tip = transform.Find("Tip");
        tip = Tip.GetComponentInChildren<Text>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (count > 0)
        {
            Tip.gameObject.SetActive(true);
            count -= Time.deltaTime;
        }
        else
        {
            Tip.gameObject.SetActive(false);
            count = 0;
        }
    }
    public void SetTipForTime(string tip, float second)
    {
        count = second;
        this.tip.text = tip;
    }
}
