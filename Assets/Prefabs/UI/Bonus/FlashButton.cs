using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashButton : MonoBehaviour
{
    [SerializeField] GameObject m_flasher;

    public void Flash()
    {
        m_flasher.GetComponent<Animator>().SetTrigger("Flash");
    }
}
