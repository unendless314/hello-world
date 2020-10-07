using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteDetectHandler : MonoBehaviour
{
    public bool isEnterZone = false;

    private GameObject objNote;

    public bool OnDrumHit()
    {
        if (isEnterZone && objNote != null)
        {
            HideNote();
            return true;
        }
        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.LogFormat("OnTriggerEnter:{0}", other.name);
        if (other.CompareTag("sheet"))
        {
            isEnterZone = true;
            objNote = other.gameObject;
        }

        //other.GetComponent<Rigidbody>().detectCollisions = false; //自己寫沒用到
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("sheet"))
        {
            HideNote();
        }
    }

    private void HideNote()
    {
        isEnterZone = false;
        if (objNote != null)    //  似乎不會執行?
        {
            objNote.GetComponent<Collider>().enabled = false;
            objNote.GetComponent<Renderer>().enabled = false;
            objNote = null;
        }
    }
}
