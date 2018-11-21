using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController : MonoBehaviour {

    public GameObject headsetFade;

    // Use this for initialization
    void Start () {
        headsetFade.GetComponent<VRTK.VRTK_HeadsetFade>().Fade(Color.black, 0f);
        headsetFade.GetComponent<VRTK.VRTK_HeadsetFade>().Unfade(5f);
    }
	
	// Update is called once per frame
	void Update () {
		if( Input.GetKeyDown(KeyCode.F))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Dungeon");
        }
	}
}
