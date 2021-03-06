﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK.Controllables;
using VRTK.Controllables.ArtificialBased;

public class DoorController : MonoBehaviour {

    public VRTK_BaseControllable controllable;
    public GameObject puzzleBoard;
    public GameObject door;
    public GameObject headsetFade;
    public bool doorOpen = false;
    public float doorSpeed = 1;
    private bool leverReset = true;

    public CoinSpawner[] coinSpawner;

    protected virtual void OnEnable()
    {
        controllable = (controllable == null ? GetComponent<VRTK_BaseControllable>() : controllable);
        if (controllable != null)
        {
            controllable.MaxLimitReached += MaxLimitReached;
            controllable.MinLimitReached += MinLimitReached;
        }
    }

    protected virtual void OnDisable()
    {
        if (controllable != null)
        {
            controllable.MaxLimitReached -= MaxLimitReached;
            controllable.MinLimitReached -= MinLimitReached;
        }
    }

    protected virtual void MaxLimitReached(object sender, ControllableEventArgs e)
    {
        if(leverReset)
        {
            this.GetComponent<AudioSource>().Play();
            if(!doorOpen && puzzleBoard.GetComponent<PuzzleController>().isComplete())
            {
                //Open the door
                doorOpen = true;
                StartCoroutine(RotateMe(door.transform, Vector3.up * 75, 1.5f));
                door.GetComponent<AudioSource>().Play();
                for( int i = 0; i<coinSpawner.Length; i++)
                {
                    coinSpawner[i].StartCoinSpawning();
                }
                Invoke("FadeAndRestart", 60f);
            }
        }
        leverReset = false;
    }

    protected virtual void MinLimitReached(object sender, ControllableEventArgs e)
    {
        leverReset = true;
    }

    IEnumerator RotateMe(Transform transform, Vector3 byAngles, float inTime)
    {
        var fromAngle = transform.rotation;
        var toAngle = Quaternion.Euler(transform.eulerAngles + byAngles);
        for (var t = 0f; t < 1; t += Time.deltaTime / inTime)
        {
            transform.rotation = Quaternion.Slerp(fromAngle, toAngle, t);
            yield return null;
        }
    }

    void FadeAndRestart()
    {
        headsetFade.GetComponent<VRTK.VRTK_HeadsetFade>().Fade(Color.black, 10f);
        Invoke("Restart", 10f);
    }

    void Restart()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Dungeon");
    }
}
