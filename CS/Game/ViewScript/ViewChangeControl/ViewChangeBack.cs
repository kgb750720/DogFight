using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewChangeBack : IViewChange
{
    protected GameObject m_originCam;
    public GameObject OriginCamera{get => m_originCam;}
    protected GameObject m_changeCam;
    public GameObject ChangeCamera { get => m_changeCam; }
    public ViewChangeBack(GameObject originCam,GameObject changeCam)
    {
        m_originCam = originCam;
        m_changeCam = changeCam;
    }
    public virtual void Canceled()
    {
    }

    public virtual void Close()
    {
        if (m_originCam && m_originCam.GetComponent<Camera>())
            m_originCam.GetComponent<Camera>().enabled = false;
        if (m_originCam && m_originCam.GetComponent<AudioListener>())
            m_originCam.GetComponent<AudioListener>().enabled = false;

        if (m_changeCam && m_changeCam.GetComponent<Camera>())
            m_changeCam.GetComponent<Camera>().enabled = false;
        if (m_changeCam && m_changeCam.GetComponent<AudioListener>())
            m_changeCam.GetComponent<AudioListener>().enabled = false;
    }

    public virtual void Open()
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

    public virtual void Started()
    {
    }

    public virtual void Update()
    {
    }
}
