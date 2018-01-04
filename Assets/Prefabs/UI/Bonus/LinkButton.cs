using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkButton : MonoBehaviour
{
    [SerializeField] GameObject m_liveObject;

    public void Toggle(bool on)
    {
        m_liveObject.SetActive(on);
    }
}
