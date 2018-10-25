using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabletSoundController : MonoBehaviour {

    void OnCollisionEnter()
    {
        GetComponent<AudioSource>().Play();
    }
}
