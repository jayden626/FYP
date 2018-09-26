using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK.Controllables;
using VRTK.Controllables.ArtificialBased;

public class DoorController : MonoBehaviour {

    public VRTK_BaseControllable controllable;
    public GameObject puzzleBoard;
    public GameObject door;
    public bool doorOpen = false;
    public float doorSpeed = 1;

    protected virtual void OnEnable()
    {
        controllable = (controllable == null ? GetComponent<VRTK_BaseControllable>() : controllable);
        if (controllable != null)
        {
            controllable.MaxLimitReached += MaxLimitReached;
        }
    }

    protected virtual void OnDisable()
    {
        if (controllable != null)
        {
            controllable.MaxLimitReached -= MaxLimitReached;
        }
    }

    protected virtual void MaxLimitReached(object sender, ControllableEventArgs e)
    {
        if( puzzleBoard.GetComponent<PuzzleController>().isSolved() && !doorOpen)
        {
            //Open the door
            doorOpen = true;
            StartCoroutine(RotateMe(door.transform, Vector3.up * 100, 0.8f));
        }
        else
        {
            //Return lever to original position
            this.GetComponent<VRTK_ArtificialRotator>().SetAngleTarget(0.0f, 0.1f);
        }
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
}
