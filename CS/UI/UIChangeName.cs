using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIChangeName : MonoBehaviour
{
    public TMP_Text []tmp_NameTexts;
    // Start is called before the first frame update
    void Start()
    {
        OnChangeName(GameManager.Manager.PlayerId);
        GameManager.OnChangePlayerIdEvents += OnChangeName;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        GameManager.OnChangePlayerIdEvents -= OnChangeName;
    }

    public void DoChangeName(TMP_InputField inputField)
    {
        GameManager.Manager.PlayerId = inputField.text;
    }

    private void OnChangeName(string newName)
    {
        foreach (var item in tmp_NameTexts)
        {
            item.text = newName;
        }
    }
}
