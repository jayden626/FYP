using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class ControllerController : MonoBehaviour {
    public GameObject LeftControllerRaycast;
    public GameObject RightControllerRaycast;

    public GameObject LeftControllerHands;
    public GameObject RightControllerHands;

    public bool usingHands;

    // Use this for initialization
    void Start () {
		if(usingHands)
        {
            VRTK_SDKManager.instance.scriptAliasLeftController = LeftControllerHands;
            VRTK_SDKManager.instance.scriptAliasRightController = RightControllerHands;
        }
        else
        {
            VRTK_SDKManager.instance.scriptAliasLeftController = LeftControllerRaycast;
            VRTK_SDKManager.instance.scriptAliasRightController = RightControllerRaycast;
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
