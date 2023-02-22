using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewChangeBackSwitchCam : ViewChangeBack
{
    public ViewChangeBackSwitchCam(GameObject originCam, GameObject changeCam) : base(originCam, changeCam)
    {
    }

    public override void Canceled()
    {
        if (m_changeCam && m_changeCam.GetComponent<Camera>())
            m_changeCam.GetComponent<Camera>().enabled = false;
        if (m_changeCam && m_changeCam.GetComponent<AudioListener>())
            m_changeCam.GetComponent<AudioListener>().enabled = false;

        if (m_originCam && m_originCam.GetComponent<Camera>())
            m_originCam.GetComponent<Camera>().enabled = true;
        if (m_originCam && m_originCam.GetComponent<AudioListener>())
            m_originCam.GetComponent<AudioListener>().enabled = true;
    }

    public override void Close()
    {
        base.Close();
    }

    public override void Started()
    {
        if (m_originCam && m_originCam.GetComponent<Camera>())
            m_originCam.GetComponent<Camera>().enabled = false;
        if (m_originCam && m_originCam.GetComponent<AudioListener>())
            m_originCam.GetComponent<AudioListener>().enabled = false;

        if (m_changeCam && m_changeCam.GetComponent<Camera>())
            m_changeCam.GetComponent<Camera>().enabled = true;
        if (m_changeCam && m_changeCam.GetComponent<AudioListener>())
            m_changeCam.GetComponent<AudioListener>().enabled = true;
    }

    public override void Update()
    {
    }
}
